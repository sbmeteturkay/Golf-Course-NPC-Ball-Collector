using Game.Feature.Behaviors;
using UnityEngine;

namespace Game.Feature.Entities
{
    public class GolfBall : MonoBehaviour,ICollectable
    {
        [Header("Ball Properties")]
        [SerializeField] private int level = 1; // 1, 2, or 3

        [SerializeField] private int pointValue = 10;

        public int Level => level;
        public int PointValue => pointValue;

        private void OnValidate()
        {
            // Auto-calculate points based on level
            pointValue = level * 10;
        }

        public void Collect()
        {
            // Play effect, sound, etc.
            Destroy(gameObject);
        }

        public Vector3 WorldPosition()
        {
            return gameObject.transform.position;
        }
    }
}
