using System.Collections.Generic;
using System.Linq;
using Game.Feature.Entities;
using UnityEngine;

namespace Game.Feature.Behaviors
{
    public class SafetyFirstStrategy:ICollectionStrategy
    {
        private Transform _dropPoint;
        private float maxSafeDistance = 10f; // Cart'a 10 birim yakın toplar
    
        public SafetyFirstStrategy(Transform dropPoint)
        {
            _dropPoint = dropPoint;
        }
    
        public ICollectable SelectTarget(List<ICollectable> availableCollectables, Vector3 npcPosition, float currentHealth)
        {
            if (availableCollectables == null || availableCollectables.Count == 0)
                return null;
        
            // Sadece cart'a yakın topları seç
            var safeBalls = availableCollectables
                .Where(collectable => Vector3.Distance(collectable.WorldPosition(), _dropPoint.position) < maxSafeDistance)
                .ToList();
        
            if (safeBalls.Count == 0)
            {
                // Güvenli top yoksa, en yakın topu al
                return availableCollectables
                    .OrderBy(collectable => Vector3.Distance(npcPosition, collectable.WorldPosition()))
                    .FirstOrDefault();
            }
        
            // Güvenli toplar içinde en yakınını seç
            return safeBalls
                .OrderBy(collectable => Vector3.Distance(npcPosition, collectable.WorldPosition()))
                .FirstOrDefault();
        }
    }
}