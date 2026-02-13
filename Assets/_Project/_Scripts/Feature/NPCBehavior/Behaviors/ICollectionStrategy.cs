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
        public void Collect(Transform parent);
        public void Drop();
        public Vector3 WorldPosition();
        public GameObject GameObject();
        public int Level();
        public int PointValue();
    }
}