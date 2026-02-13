using System.Collections.Generic;
using Game.Core;
using Game.Feature.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Feature.Behaviors
{
    public class NPCBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private Transform golfCart;
        [SerializeField] private NavMeshAgent agent;
    
        [Header("Settings")]
        [SerializeField] private float collectionDistance = 1.5f;
    
        private ICollectionStrategy currentStrategy;
        private GolfBall targetBall;
        private GolfBall currentBall;
        private List<ICollectable> availableTargets = new();
    
        private enum State { SearchingForBall, MovingToBall, ReturningToCart }
        private State currentState = State.SearchingForBall;
    
        private void Start()
        {
            currentStrategy = new ClosestStrategy();
            
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            
            agent.stoppingDistance = collectionDistance; 

            FindAllBalls();
        }
    
        private void Update()
        {
            if (!healthSystem.IsAlive) 
            {
                if (agent.enabled) agent.isStopped = true;
                return;
            }
        
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
    
        private void FindAllBalls()
        {
            availableTargets.Clear();
            availableTargets.AddRange(FindObjectsOfType<GolfBall>());
        }
    
        private void SearchForTargetBall()
        {
            if (availableTargets.Count == 0) FindAllBalls();

            targetBall = currentStrategy.SelectTarget(availableTargets, transform.position, healthSystem.CurrentHealth) as GolfBall;
        
            if (targetBall != null)
            {
                currentState = State.MovingToBall;
                agent.SetDestination(targetBall.transform.position);
            }
        }

        private void MoveTowardsBall()
        {
            if (targetBall == null)
            {
                currentState = State.SearchingForBall;
                return;
            }

            agent.SetDestination(targetBall.transform.position);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                CollectBall(targetBall);
            }
        }

        private void CollectBall(GolfBall ball)
        {
            if (ball)
            {
                currentBall = ball;
            }
            
            availableTargets.Remove(ball);
            ball.Collect();
            targetBall = null;
            
            currentState = State.ReturningToCart;
            if (golfCart != null)
            {
                agent.SetDestination(golfCart.position);
            }
        }
    
        private void ReturnToCart()
        {
            if (golfCart == null) 
            {
                currentState = State.SearchingForBall;
                return;
            }

            agent.SetDestination(golfCart.position);

            if (!agent.pathPending && agent.remainingDistance <= 3f)
            {
                if (currentBall!=null)
                {
                    scoreSystem.AddScore(currentBall.PointValue);
                }
                agent.ResetPath();
                currentState = State.SearchingForBall;
            }
        }
    }
}