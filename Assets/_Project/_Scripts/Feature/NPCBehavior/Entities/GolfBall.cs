using Game.Feature.Behaviors;
using UnityEngine;

namespace Game.Feature.Entities
{
    public class GolfBall : MonoBehaviour,ICollectable
    {
        [Header("Ball Properties")]
        [SerializeField] Rigidbody rigidBody;
        [Header("Ball Properties")]
        [SerializeField] private int level = 1;

        [SerializeField] private int pointValue = 10;

        public int Level => level;
        public int PointValue => pointValue;

        private void OnValidate()
        {
            pointValue = level * 10;
        }

        public void Collect(Transform itemHolder)
        {
            rigidBody.isKinematic = true;
            transform.SetParent(itemHolder);
            rigidBody.transform.localPosition = Vector3.zero;
        }

        public void Drop()
        {
            gameObject.transform.parent = null;
            rigidBody.isKinematic = false;
        }

        public Vector3 WorldPosition()
        {
            return gameObject.transform.position;
        }

        public GameObject GameObject()
        {
            return gameObject;
        }

        int ICollectable.Level()
        {
            return level;
        }

        int ICollectable.PointValue()
        {
            return pointValue;
        }
    }
}
