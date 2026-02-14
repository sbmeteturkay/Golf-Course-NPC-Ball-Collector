using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Feature.Behaviors
{
    public class GreedyStrategy : ICollectionStrategy
    {
        public ICollectable SelectTarget(List<ICollectable> availableCollectables, Vector3 npcPosition, float currentHealth)
        {
            if (availableCollectables == null || availableCollectables.Count == 0)
                return null;
            
            var target = availableCollectables
                .OrderByDescending(ball => ball.PointValue())
                .ThenBy(ball => Vector3.Distance(npcPosition, ball.WorldPosition()))
                .FirstOrDefault();
            
            if (target != null)
            {
                Debug.Log($"💰 Greedy: {target.GameObject().name} ({target.PointValue()} pts)");
            }
            
            return target;
        }
    }
}