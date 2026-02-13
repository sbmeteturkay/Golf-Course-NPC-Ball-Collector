using System.Collections.Generic;
using Game.Core;
using Game.Feature.Entities;
using UnityEngine;

namespace Game.Feature.Behaviors
{
    public class NPCBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private Transform golfCart;
    
        [Header("Settings")]
        [SerializeField] private float searchRadius = 50f;
        [SerializeField] private float collectionDistance = 2f;
    
        private ICollectionStrategy currentStrategy;
        private GolfBall targetBall;
        private List<ICollectable> availableTargets = new();
    
        private enum State { SearchingForBall, MovingToBall, ReturningToCart }
        private State currentState = State.SearchingForBall;
    
        private void Start()
        {
            currentStrategy = new ClosestStrategy();
            FindAllBalls();
        }
    
        private void Update()
        {
            if (!healthSystem.IsAlive) return;
        
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
            targetBall = currentStrategy.SelectTarget(availableTargets, transform.position, healthSystem.CurrentHealth) as GolfBall;
        
            if (targetBall != null)
            {
                currentState = State.MovingToBall;
                Debug.Log($"Target selected: {targetBall.name} (Level {targetBall.Level})");
            }
        }
    
        private void MoveTowardsBall()
        {
            if (targetBall == null)
            {
                currentState = State.SearchingForBall;
                return;
            }
        
            Vector3 direction = (targetBall.transform.position - transform.position).normalized;
            transform.position += direction * 5f * Time.deltaTime;
        
            if (Vector3.Distance(transform.position, targetBall.transform.position) < collectionDistance)
            {
                CollectBall(targetBall);
            }
        }
    
        private void CollectBall(GolfBall ball)
        {
            scoreSystem.AddScore(ball.PointValue);
            availableTargets.Remove(ball);
            ball.Collect();
        
            targetBall = null;
            currentState = State.ReturningToCart;
        }
    
        private void ReturnToCart()
        {
            Vector3 direction = (golfCart.position - transform.position).normalized;
            transform.position += direction * 5f * Time.deltaTime;
        
            if (Vector3.Distance(transform.position, golfCart.position) < 3f)
            {
                Debug.Log("Returned to cart!");
                currentState = State.SearchingForBall;
            }
        }
    }
}