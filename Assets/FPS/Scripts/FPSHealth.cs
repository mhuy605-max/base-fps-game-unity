using System;
using UnityEngine;

namespace FPSGame
{
    public class FPSHealth : MonoBehaviour
    {
        public int maxHealth = 100;
        public bool destroyOnDeath = true;
        public bool isPlayer;

        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public event Action<FPSHealth> OnDied;
        public event Action<int, int> OnHealthChanged;

        void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (IsDead)
                Die();
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        void Die()
        {
            OnDied?.Invoke(this);

            if (isPlayer)
            {
                FPSGameManager.Instance?.OnPlayerDied();
                return;
            }

            FPSGameManager.Instance?.RegisterKill();
            if (destroyOnDeath)
                Destroy(gameObject);
        }
    }
}
