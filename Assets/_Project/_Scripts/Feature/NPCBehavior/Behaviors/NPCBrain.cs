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
        public event Action<string> OnStrategyChanged;

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

        private ICollectionStrategy currentStrategy;
        private ICollectionStrategy greedyStrategy;
        private ICollectionStrategy balancedStrategy;
        private ICollectionStrategy safetyStrategy;

        private ICollectable targetCollectable;
        private ICollectable currentCollectable;
        private List<ICollectable> availableTargets = new();

        private float lastSearchTime;

        private enum State
        {
            SearchingForBall,
            MovingToBall,
            ReturningToCart
        }

        private State currentState = State.SearchingForBall;

        private void Start()
        {
            greedyStrategy = new GreedyStrategy();
            balancedStrategy = new BalancedStrategy(golfCart);
            safetyStrategy = new SafetyFirstStrategy(golfCart);

            currentStrategy = balancedStrategy;
            string strategyName = GetStrategyDisplayName(currentStrategy);
            OnStrategyChanged?.Invoke(strategyName);

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

        private void UpdateStrategy()
        {
            float healthPercentage = (healthSystem.CurrentHealth / healthSystem.MaxHealth) * 100f;

            ICollectionStrategy newStrategy;

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

            if (newStrategy != currentStrategy)
            {
                currentStrategy = newStrategy;

                string strategyName = GetStrategyDisplayName(currentStrategy);
                OnStrategyChanged?.Invoke(strategyName);

                // ⭐ RE-EVALUATE when strategy changes mid-journey
                if (currentState == State.MovingToBall && targetCollectable != null)
                {
                    ReEvaluateCurrentTarget();
                }

                Debug.Log($"🔄 Strategy Changed: {strategyName} (Health: {healthPercentage:F0}%)");
            }
        }

        /// <summary>
        /// Re-evaluates current target when strategy changes mid-journey
        /// HYBRID: Only switches if critically necessary (emergency mode)
        /// </summary>
        private void ReEvaluateCurrentTarget()
        {
            // Calculate predicted health at current target completion
            float predictedHealth = PredictHealthAtTargetCompletion(targetCollectable);

            // CRITICAL CHECK: Will we survive this trip?
            float criticalThreshold = 15f; // Below 15% health is critical

            if (predictedHealth < criticalThreshold)
            {
                // EMERGENCY: Current target is suicidal
                Debug.LogWarning($"⚠️ EMERGENCY! Predicted health ({predictedHealth:F0}) too low. Abandoning target.");

                var newTarget = currentStrategy.SelectTarget(
                    availableTargets,
                    transform.position,
                    healthSystem.CurrentHealth
                );

                if (newTarget != targetCollectable)
                {
                    Debug.Log(
                        $"🔄 Emergency re-route: {targetCollectable?.GameObject().name} → {newTarget?.GameObject().name}");

                    // Return old target to pool
                    if (targetCollectable != null && !availableTargets.Contains(targetCollectable))
                    {
                        availableTargets.Add(targetCollectable);
                    }

                    targetCollectable = newTarget;

                    if (targetCollectable != null)
                    {
                        agent.SetDestination(targetCollectable.WorldPosition());
                    }
                    else
                    {
                        // No safe option - return to cart immediately
                        Debug.Log("🛡️ No safe target! Emergency return to cart.");
                        currentState = State.ReturningToCart;
                        if (golfCart != null)
                        {
                            agent.SetDestination(golfCart.position);
                        }
                    }
                }
            }
            else
            {
                // NON-CRITICAL: Strategy changed but current target is still viable
                Debug.Log($"✅ Strategy changed but committed to current target (Predicted HP: {predictedHealth:F0})");
            }
        }

        /// <summary>
        /// Predicts health after completing CURRENT target trip
        /// </summary>
        private float PredictHealthAtTargetCompletion(ICollectable target)
        {
            if (target == null) return 0f;

            float healthDrainRate = 1f;
            float averageSpeed = 3.5f;

            // Distance: Current Position → Target → Cart
            float distanceToTarget = Vector3.Distance(transform.position, target.WorldPosition());
            float distanceToCart = Vector3.Distance(target.WorldPosition(), golfCart.position);
            float totalDistance = distanceToTarget + distanceToCart;

            float estimatedTime = totalDistance / averageSpeed;
            float predictedLoss = estimatedTime * healthDrainRate;

            return healthSystem.CurrentHealth - predictedLoss;
        }
    

    private void FindAllBalls()
        {
            availableTargets.Clear();
            availableTargets.AddRange(FindObjectsOfType<GolfBall>());
        }
    
        private void SearchForTargetBall()
        {
            if (Time.time - lastSearchTime < searchCooldown) return;
            lastSearchTime = Time.time;
            
            if (availableTargets.Count == 0) FindAllBalls();

            targetCollectable = currentStrategy.SelectTarget(availableTargets, transform.position, healthSystem.CurrentHealth);
        
            if (targetCollectable != null)
            {
                currentState = State.MovingToBall;
                agent.SetDestination(targetCollectable.WorldPosition());
                Debug.Log($"🎯 Target: {targetCollectable.GameObject().name} (Level {targetCollectable.Level()}, {targetCollectable.PointValue()} pts)");
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

            NavMeshPath path = new NavMeshPath();
            if (!agent.CalculatePath(targetCollectable.WorldPosition(), path) || path.status == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogWarning($"⚠️ Cannot reach: {targetCollectable.GameObject().name}");
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
            Debug.Log($"✅ Collected: {ball.GameObject().name} (+{ball.PointValue()} pts)");
            
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
                
                animator.SetTrigger(Drop);
                scoreSystem.AddScore(currentCollectable.PointValue());
                
                StartCoroutine(DropAndSearch(currentCollectable));
                currentCollectable = null;
            }
        }

        private IEnumerator DropAndSearch(ICollectable ball)
        {
            yield return new WaitForSeconds(.55f);
            animator.ResetTrigger(Drop);

            ball.Drop();
            
            healthSystem.RestoreHealth(healthRestoreAmount);
            Debug.Log($"💚 Health restored (+{healthRestoreAmount})");
            
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