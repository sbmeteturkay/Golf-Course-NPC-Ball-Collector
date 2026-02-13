using System;
using UnityEngine;

namespace Game.Core
{
    public class HealthSystem : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float healthDrainRate = 1f; // per second
    
        private float currentHealth;
    
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
    
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
    
        private void Start()
        {
            currentHealth = maxHealth;
        }
    
        private void Update()
        {
            if (!IsAlive) return;
        
            DrainHealth(healthDrainRate * Time.deltaTime);
        }
    
        public void DrainHealth(float amount)
        {
            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth);
        
            if (currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }
    
        public void RestoreHealth(float amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth);
        }
    }
}
