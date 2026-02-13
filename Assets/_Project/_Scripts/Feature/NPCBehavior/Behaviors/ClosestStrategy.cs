using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Feature.Behaviors
{
    public class ClosestStrategy : ICollectionStrategy
    {
        public ICollectable SelectTarget(List<ICollectable> availableCollectables, Vector3 npcPosition, float currentHealth)
        {
            if (availableCollectables == null || availableCollectables.Count == 0)
                return null;
        
            return availableCollectables
                .OrderBy(collectable => Vector3.Distance(npcPosition, collectable.WorldPosition()))
                .FirstOrDefault();
        }
    }
}