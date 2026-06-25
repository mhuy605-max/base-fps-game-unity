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

[Header("Vision")]
public float detectionRange = 15f;
public float fieldOfView = 90f;
public LayerMask visionMask;

[Header("Shooting")]
public Transform firePoint;
public float shootingRange = 12f;
public float fireCooldown = 0.8f;
public int bulletDamage = 10;
public ParticleSystem muzzleFlash;

[Header("Patrol")]
public Transform[] patrolPoints;
public float patrolReachDistance = 0.5f;

enum EnemyState
{
    Patrol,
    Chase,
    Attack,
    Search
}

EnemyState _state = EnemyState.Patrol;
Vector3 _lastKnownPlayerPosition;
int _patrolIndex;
float _nextFireTime;
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
        FindPlayer();

    bool canSeePlayer = CanSeePlayer();

    if (canSeePlayer)
    {
        _lastKnownPlayerPosition = _player.position;

        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance <= shootingRange)
            _state = EnemyState.Attack;
        else
            _state = EnemyState.Chase;
    }
    else
    {
        if (_state == EnemyState.Chase || _state == EnemyState.Attack)
            _state = EnemyState.Search;
    }

    switch (_state)
    {
        case EnemyState.Patrol:
            Patrol();
            break;

        case EnemyState.Chase:
            MoveTo(_player.position);
            break;

        case EnemyState.Attack:
            ShootAtPlayer();
            break;

        case EnemyState.Search:
            MoveTo(_lastKnownPlayerPosition);

            if (Vector3.Distance(transform.position, _lastKnownPlayerPosition) <= patrolReachDistance)
                _state = EnemyState.Patrol;

            break;
    }
}
bool CanSeePlayer()
{
    if (_player == null)
        return false;

    Vector3 eyePos = transform.position + Vector3.up * 1.5f;
    Vector3 targetPos = _player.position + Vector3.up * 1.2f;

    Vector3 toPlayer = targetPos - eyePos;

    if (toPlayer.magnitude > detectionRange)
        return false;

    Vector3 flatToPlayer = toPlayer;
    flatToPlayer.y = 0f;

    float angle = Vector3.Angle(transform.forward, flatToPlayer.normalized);

    if (angle > fieldOfView * 0.5f)
        return false;

    if (Physics.Raycast(eyePos, toPlayer.normalized, out RaycastHit hit, detectionRange, visionMask))
    {
        FPSHealth health = hit.collider.GetComponentInParent<FPSHealth>();
        return health != null && health.isPlayer;
    }

    return false;
}
void MoveTo(Vector3 target)
{
    Vector3 toTarget = target - transform.position;
    toTarget.y = 0f;

    if (toTarget.magnitude <= 0.1f)
    {
        SetMoving(false);
        return;
    }

    Vector3 dir = toTarget.normalized;
    transform.position += dir * moveSpeed * Time.deltaTime;
    transform.rotation = Quaternion.LookRotation(dir);

    SetMoving(true);
}
void Patrol()
{
    if (patrolPoints == null || patrolPoints.Length == 0)
    {
        SetMoving(false);
        return;
    }

    Transform point = patrolPoints[_patrolIndex];

    if (point == null)
        return;

    Vector3 toPoint = point.position - transform.position;
    toPoint.y = 0f;

    if (toPoint.magnitude <= patrolReachDistance)
    {
        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
        return;
    }

    MoveTo(point.position);
}
void ShootAtPlayer()
{
    SetMoving(false);

    if (_player == null)
        return;

    Vector3 lookPos = new Vector3(_player.position.x, transform.position.y, _player.position.z);
    transform.LookAt(lookPos);

    if (Time.time < _nextFireTime)
        return;

    _animator?.SetTrigger("Attack");

    if (muzzleFlash != null)
        muzzleFlash.Play();

    Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.4f;
    Vector3 target = _player.position + Vector3.up * 1.2f;
    Vector3 dir = (target - origin).normalized;

    if (Physics.Raycast(origin, dir, out RaycastHit hit, shootingRange, visionMask))
    {
        FPSHealth health = hit.collider.GetComponentInParent<FPSHealth>();

        if (health != null && health.isPlayer && !health.IsDead)
            health.TakeDamage(bulletDamage);
    }

    _nextFireTime = Time.time + fireCooldown;
}
void OnDrawGizmosSelected()
{
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectionRange);

    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, shootingRange);

    Vector3 left = Quaternion.Euler(0f, -fieldOfView * 0.5f, 0f) * transform.forward;
    Vector3 right = Quaternion.Euler(0f, fieldOfView * 0.5f, 0f) * transform.forward;

    Gizmos.color = Color.cyan;
    Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, left * detectionRange);
    Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, right * detectionRange);

    if (patrolPoints != null)
    {
        Gizmos.color = Color.magenta;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);

            int next = (i + 1) % patrolPoints.Length;

            if (patrolPoints[next] != null)
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
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