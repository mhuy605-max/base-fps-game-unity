using UnityEngine;

namespace FPSGame
{
    [RequireComponent(typeof(FPSHealth))]
    public class FPSEnemy : MonoBehaviour
    {
        public float moveSpeed = 2.5f;
        public float attackRange = 1.5f;
        public float attackCooldown = 1f;
        public int attackDamage = 10;

        Transform _player;
        FPSHealth _health;
        float _nextAttackTime;

        void Awake()
        {
            _health = GetComponent<FPSHealth>();
            FindPlayer();
        }

        void Update()
        {
            if (_health.IsDead)
                return;

            if (_player == null)
            {
                FindPlayer();

                if (_player == null)
                    return;
            }

            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;

            float distance = toPlayer.magnitude;

            if (distance > attackRange)
            {
                Vector3 dir = toPlayer.normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;

                if (dir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
            else if (Time.time >= _nextAttackTime)
            {
                FPSHealth playerHealth = _player.GetComponent<FPSHealth>();

                if (playerHealth != null && !playerHealth.IsDead)
                {
                    playerHealth.TakeDamage(attackDamage);
                    _nextAttackTime = Time.time + attackCooldown;
                }
            }
        }

        void FindPlayer()
        {
            foreach (FPSHealth health in FindObjectsByType<FPSHealth>(FindObjectsSortMode.None))
            {
                if (health.isPlayer)
                {
                    _player = health.transform;
                    return;
                }
            }
        }
    }
}