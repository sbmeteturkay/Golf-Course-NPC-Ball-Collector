using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Feature.Behaviors
{
    public class BalancedStrategy : ICollectionStrategy
    {
        private Transform _dropPoint;

        public BalancedStrategy(Transform dropPoint)
        {
            _dropPoint = dropPoint;
        }

        public ICollectable SelectTarget(List<ICollectable> availableBalls, Vector3 npcPosition, float currentHealth)
        {
            if (availableBalls == null || availableBalls.Count == 0)
                return null;

            var scoredBalls = availableBalls.Select(ball => new
            {
                ball,
                score = CalculateUtilityScore(ball, npcPosition, currentHealth)
            }).ToList();

            // Debug için en iyi 3'ü göster
            var top3 = scoredBalls.OrderByDescending(x => x.score).Take(3);
            foreach (var item in top3)
            {
                var obj = item.ball.GameObject();
                Debug.Log(
                    "Ball "+obj.name+" Score="+item.score+"Level="+item.ball.Level() +"Distance="+Vector3.Distance(npcPosition, obj.transform.position));
            }

            return scoredBalls
                .OrderByDescending(x => x.score)
                .FirstOrDefault()?.ball;
        }

        private float CalculateUtilityScore(ICollectable collectable, Vector3 npcPosition, float currentHealth)
        {
            // Mesafe hesapla
            float distanceToBall = Vector3.Distance(npcPosition, collectable.GameObject().transform.position);
            float distanceToCart = Vector3.Distance(collectable.GameObject().transform.position, _dropPoint.position);
            float totalDistance = distanceToBall + distanceToCart;

            // Health cost (her 1 birim mesafe = 0.5 health)
            float healthCostRate = 0.5f;
            float estimatedHealthCost = totalDistance * healthCostRate;

            // Can bu yolculuğu karşılayabilir mi?
            if (estimatedHealthCost >= currentHealth)
            {
                return -1000f; // İmkansız, bu topu seçme
            }

            // Utility = (Kazanılan Puan) / (Harcanan Health + Mesafe Maliyeti)
            float pointValue = collectable.PointValue();
            float healthMultiplier = currentHealth / 100f; // Düşük canda daha temkinli

            float utilityScore = (pointValue * healthMultiplier) / (estimatedHealthCost + 1f);

            return utilityScore;
        }
    }
}
