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
        public float destroyDelayAfterDeath = 3f;

        Transform _player;
        FPSHealth _health;
        Animator _animator;
        Collider _collider;
        float _nextAttackTime;
        bool _dead;

        void Awake()
        {
            _health = GetComponent<FPSHealth>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider>();

            _health.OnDied += HandleDeath;

            FindPlayer();
        }

        void OnDestroy()
        {
            if (_health != null)
                _health.OnDied -= HandleDeath;
        }

        void Update()
        {
            if (_dead || _health.IsDead)
            {
                SetMoving(false);
                return;
            }

            if (_player == null)
            {
                FindPlayer();

                if (_player == null)
                {
                    SetMoving(false);
                    return;
                }
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

                SetMoving(true);
            }
            else
            {
                SetMoving(false);

                if (Time.time >= _nextAttackTime)
                {
                    FPSHealth playerHealth = _player.GetComponent<FPSHealth>();

                    if (playerHealth != null && !playerHealth.IsDead)
                    {
                        _animator?.SetTrigger("Attack");
                        playerHealth.TakeDamage(attackDamage);
                        _nextAttackTime = Time.time + attackCooldown;
                    }
                }
            }
        }

        void HandleDeath(FPSHealth health)
        {
            if (_dead)
                return;

            _dead = true;
            SetMoving(false);

            if (_collider != null)
                _collider.enabled = false;

            _animator?.SetTrigger("Die");

            FPSGameManager.Instance?.RegisterKill();
            Destroy(gameObject, destroyDelayAfterDeath);
        }

        void SetMoving(bool moving)
        {
            if (_animator != null)
                _animator.SetBool("isMoving", moving);
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