using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Feature.Behaviors
{
    public class BalancedStrategy : ICollectionStrategy
    {
        private Transform golfCartTransform;
        private float healthDrainRate = 1f;
        private float averageSpeed = 3.5f;
        
        public BalancedStrategy(Transform cartTransform)
        {
            golfCartTransform = cartTransform;
        }
        
        public ICollectable SelectTarget(List<ICollectable> availableCollectables, Vector3 npcPosition, float currentHealth)
        {
            if (availableCollectables == null || availableCollectables.Count == 0)
                return null;
            
            var scoredBalls = availableCollectables.Select(ball => new
            {
                ball,
                score = CalculateUtilityScore(ball, npcPosition, currentHealth),
                predictedHealth = PredictHealthAtCompletion(ball, npcPosition, currentHealth)
            })
            .Where(x => x.score > 0 && x.predictedHealth > 0)
            .ToList();
            
            if (scoredBalls.Count == 0) 
            {
                Debug.LogWarning("⚠️ Balanced: No safe targets!");
                return null;
            }
            
            var best = scoredBalls.OrderByDescending(x => x.score).First();
            
            Debug.Log($"🎯 Balanced: {best.ball.GameObject().name} | HP: {currentHealth:F0} → {best.predictedHealth:F0} | Utility: {best.score:F2}");
            
            return best.ball;
        }
        
        /// <summary>
        /// PREDICTIVE: Forecasts health at trip completion
        /// </summary>
        private float PredictHealthAtCompletion(ICollectable ball, Vector3 npcPosition, float currentHealth)
        {
            float distanceToBall = Vector3.Distance(npcPosition, ball.WorldPosition());
            float distanceToCart = Vector3.Distance(ball.WorldPosition(), golfCartTransform.position);
            float totalDistance = distanceToBall + distanceToCart;
            
            float estimatedTime = totalDistance / averageSpeed;
            float predictedHealthLoss = estimatedTime * healthDrainRate;
            
            return currentHealth - predictedHealthLoss;
        }
        
        private float CalculateUtilityScore(ICollectable ball, Vector3 npcPosition, float currentHealth)
        {
            float distanceToBall = Vector3.Distance(npcPosition, ball.WorldPosition());
            float distanceToCart = Vector3.Distance(ball.WorldPosition(), golfCartTransform.position);
            float totalDistance = distanceToBall + distanceToCart;
            
            float estimatedTime = totalDistance / averageSpeed;
            float estimatedHealthCost = estimatedTime * healthDrainRate;
            
            if (estimatedHealthCost >= currentHealth)
            {
                return -1f;
            }
            
            // Safety margin: 20% health minimum
            float safetyMargin = 20f;
            float finalHealth = currentHealth - estimatedHealthCost;
            
            if (finalHealth < safetyMargin)
            {
                return -1f;
            }
            
            float healthMultiplier = currentHealth / 100f;
            float utilityScore = (ball.PointValue() * healthMultiplier) / (estimatedHealthCost + 1f);
            
            return utilityScore;
        }
    }
}