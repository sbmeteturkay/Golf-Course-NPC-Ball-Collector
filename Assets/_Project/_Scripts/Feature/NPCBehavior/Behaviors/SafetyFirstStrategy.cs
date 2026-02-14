using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Feature.Behaviors
{
    public class SafetyFirstStrategy : ICollectionStrategy
    {
        private Transform golfCartTransform;
        private float maxSafeDistance = 15f;
        
        public SafetyFirstStrategy(Transform cartTransform)
        {
            golfCartTransform = cartTransform;
        }
        
        public ICollectable SelectTarget(List<ICollectable> availableCollectables, Vector3 npcPosition, float currentHealth)
        {
            if (availableCollectables == null || availableCollectables.Count == 0)
                return null;
            
            var safeBalls = availableCollectables
                .Where(ball => Vector3.Distance(ball.WorldPosition(), golfCartTransform.position) < maxSafeDistance)
                .ToList();
            
            if (safeBalls.Count == 0)
            {
                Debug.LogWarning("🛡️ Safety: No safe balls! Fallback to closest.");
                
                return availableCollectables
                    .OrderBy(ball => Vector3.Distance(npcPosition, ball.WorldPosition()))
                    .FirstOrDefault();
            }
            
            var target = safeBalls
                .OrderBy(ball => Vector3.Distance(npcPosition, ball.WorldPosition()))
                .FirstOrDefault();
            
            if (target != null)
            {
                float distToCart = Vector3.Distance(target.WorldPosition(), golfCartTransform.position);
                Debug.Log($"🛡️ Safety: {target.GameObject().name} ({distToCart:F1}m from cart)");
            }
            
            return target;
        }
    }
}