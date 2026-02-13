using System.Collections.Generic;
using UnityEngine;

namespace Game.Feature.Behaviors
{
    public interface ICollectionStrategy
    {
        ICollectable SelectTarget(List<ICollectable> availableCollectables, Vector3 npcPosition, float currentHealth);
    }

    public interface ICollectable
    {
        public void Collect();
        public Vector3 WorldPosition();
    }
}