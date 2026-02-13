using System;
using System.Collections;
using System.Collections.Generic;
using Game.Core;
using Game.Feature.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Feature.Behaviors
{
    public class NPCBrain : MonoBehaviour
    {
        public event Action<string> OnStrategyChanged; // (strategyName)

        private static readonly int PickUp = Animator.StringToHash("PickUp");
        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private static readonly int Drop = Animator.StringToHash("Drop");

        [Header("System References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private Transform golfCart;
        
        [Header("References")]
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform itemHolder;
    
        [Header("Settings")]
        [SerializeField] private float collectionDistance = 1.5f;
        [SerializeField] private float searchCooldown = 0.5f;
        
        [Header("Strategy Settings")]
        [SerializeField] private float greedyHealthThreshold = 70f;
        [SerializeField] private float safetyHealthThreshold = 30f;
        [SerializeField] private float healthRestoreAmount = 10f;
    
        // Strategy instances
        private ICollectionStrategy currentStrategy;
        private ICollectionStrategy greedyStrategy;
        private ICollectionStrategy balancedStrategy;
        private ICollectionStrategy safetyStrategy;
        
        // Target tracking
        private ICollectable targetCollectable;
        private ICollectable currentCollectable;
        private List<ICollectable> availableTargets = new();
        
        // Timing
        private float lastSearchTime;
    
        private enum State { SearchingForBall, MovingToBall, ReturningToCart }
        private State currentState = State.SearchingForBall;
    
        private void Start()
        {
            // Initialize strategies
            greedyStrategy = new GreedyStrategy();
            balancedStrategy = new BalancedStrategy(golfCart);
            safetyStrategy = new SafetyFirstStrategy(golfCart);
            
            currentStrategy = balancedStrategy; // Start with balanced
            string strategyName = GetStrategyDisplayName(currentStrategy);
            OnStrategyChanged?.Invoke(strategyName);
            // Setup agent
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            agent.stoppingDistance = collectionDistance;

            FindAllBalls();
        }
    
        private void Update()
        {
            if (!healthSystem.IsAlive) 
            {
                if (agent.enabled) agent.isStopped = true;
                animator.SetBool(IsWalking, false);
                animator.ResetTrigger(Drop);
                animator.ResetTrigger(PickUp);
                return;
            }
            
            // Dynamic strategy switching based on health
            UpdateStrategy();
        
            switch (currentState)
            {
                case State.SearchingForBall:
                    SearchForTargetBall();
                    break;
                case State.MovingToBall:
                    MoveTowardsBall();
                    break;
                case State.ReturningToCart:
                    ReturnToCart();
                    break;
            }
        }
        
        /// <summary>
        /// Dynamically switches strategy based on current health
        /// </summary>
        private void UpdateStrategy()
        {
            float healthPercentage = (healthSystem.CurrentHealth / healthSystem.MaxHealth) * 100f;
            
            ICollectionStrategy newStrategy = null;
            
            if (healthPercentage > greedyHealthThreshold)
            {
                newStrategy = greedyStrategy;
            }
            else if (healthPercentage < safetyHealthThreshold)
            {
                newStrategy = safetyStrategy;
            }
            else
            {
                newStrategy = balancedStrategy;
            }
            
            // Log strategy change
            if (newStrategy != currentStrategy)
            {
                string strategyName = GetStrategyDisplayName(newStrategy);
                OnStrategyChanged?.Invoke(strategyName);
                currentStrategy = newStrategy;
                Debug.Log($"🔄 Strategy Changed: {currentStrategy.GetType().Name} (Health: {healthPercentage:F0}%)");
            }
        }
    
        private void FindAllBalls()
        {
            availableTargets.Clear();
            availableTargets.AddRange(FindObjectsOfType<GolfBall>());
        }
    
        private void SearchForTargetBall()
        {
            // Cooldown to avoid recalculating every frame
            if (Time.time - lastSearchTime < searchCooldown) return;
            lastSearchTime = Time.time;
            
            if (availableTargets.Count == 0) FindAllBalls();
            

            targetCollectable = currentStrategy.SelectTarget(availableTargets, transform.position, healthSystem.CurrentHealth);
        
            if (targetCollectable != null)
            {
                currentState = State.MovingToBall;
                agent.SetDestination(targetCollectable.WorldPosition());
                Debug.Log("🎯 Target Selected: "+targetCollectable.GameObject().name+" Level: "+targetCollectable.Level()+" Points: "+targetCollectable.PointValue());
            }
        }

        private void MoveTowardsBall()
        {
            if (currentCollectable != null) return;
            
            if (targetCollectable == null)
            {
                currentState = State.SearchingForBall;
                return;
            }

            // Validate NavMesh path
            NavMeshPath path = new NavMeshPath();
            if (!agent.CalculatePath(targetCollectable.WorldPosition(), path) || path.status == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogWarning($"⚠️ Cannot reach ball: {targetCollectable.GameObject().name}");
                availableTargets.Remove(targetCollectable);
                targetCollectable = null;
                currentState = State.SearchingForBall;
                return;
            }

            animator.SetBool(IsWalking, true);
            agent.SetDestination(targetCollectable.WorldPosition());

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;
                StartCoroutine(CollectTarget(targetCollectable));
            }
        }

        private IEnumerator CollectTarget(ICollectable ball)
        {
            currentCollectable = ball;
            availableTargets.Remove(ball);
            targetCollectable = null;
            
            animator.SetBool(IsWalking, false);
            animator.SetTrigger(PickUp);
            
            yield return new WaitForSeconds(.5f);
            animator.ResetTrigger(PickUp);
            
            ball.Collect(itemHolder);
            Debug.Log("✅ Collected:"+ball.GameObject().name+" "+ball.PointValue()+" points)");
            
            yield return new WaitForSeconds(1f);
            
            currentState = State.ReturningToCart;
            if (golfCart != null)
            {
                agent.SetDestination(golfCart.position);
                agent.isStopped = false;
            }
        }
    
        private void ReturnToCart()
        {
            if (currentCollectable == null) return;
            
            if (golfCart == null) 
            {
                currentState = State.SearchingForBall;
                return;
            }

            animator.SetBool(IsWalking, true);
            agent.SetDestination(golfCart.position);

            if (!agent.pathPending && agent.remainingDistance <= 3f)
            {
                agent.isStopped = true;
                agent.ResetPath();
                
                if (currentCollectable != null)
                {
                    animator.SetTrigger(Drop);
                    scoreSystem.AddScore(currentCollectable.PointValue());
                }
                StartCoroutine(DropAndSearch(currentCollectable));
                currentCollectable = null;
            }
        }

        private IEnumerator DropAndSearch(ICollectable ball)
        {
            yield return new WaitForSeconds(.55f);
            animator.ResetTrigger(Drop);

            ball.Drop();
            
            // Restore health at cart
            healthSystem.RestoreHealth(healthRestoreAmount);
            Debug.Log($"💚 Returned to cart! Health restored (+{healthRestoreAmount})");
            
            agent.isStopped = false;
            yield return new WaitForSeconds(.4f);

            currentState = State.SearchingForBall;
        }
        
        private string GetStrategyDisplayName(ICollectionStrategy strategy)
        {
            return strategy switch
            {
                GreedyStrategy => "Greedy",
                BalancedStrategy => "Balanced",
                SafetyFirstStrategy => "Survival",
                _ => "UNKNOWN"
            };
        }
    }
}