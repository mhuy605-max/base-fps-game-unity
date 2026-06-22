using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FPSGame
{
    public class FPSWeapon : MonoBehaviour
    {
        [Header("Weapon")]
        public Camera aimCamera;
        public int damage = 25;
        public float range = 100f;
        public float fireRate = 0.12f;
        public int magazineSize = 30;
        public float reloadTime = 1.4f;

        [Header("Effects")]
        public Color hitColor = Color.red;
        public float hitFlashDuration = 0.05f;

        int _ammo;
        float _nextFireTime;
        float _reloadEndTime;
        bool _reloading;
        LineRenderer _line;

        void Awake()
        {
            _ammo = magazineSize;

            if (aimCamera == null)
                aimCamera = GetComponentInChildren<Camera>();

            _line = gameObject.AddComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.startWidth = 0.02f;
            _line.endWidth = 0.02f;
            _line.enabled = false;
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.startColor = Color.yellow;
            _line.endColor = Color.yellow;
        }

        void Update()
        {
            if (_reloading)
            {
                if (Time.time >= _reloadEndTime)
                {
                    _ammo = magazineSize;
                    _reloading = false;
                }
                return;
            }
        }

#if ENABLE_INPUT_SYSTEM
        public void OnShoot(InputAction.CallbackContext context)
        {
            if (context.performed)
                TryFire();
        }

        public void OnReload(InputAction.CallbackContext context)
{
    if (context.performed)
        StartReload();
}
#endif

        void TryFire()
        {
            if (_reloading || Time.time < _nextFireTime)
                return;

            if (_ammo <= 0)
            {
                StartReload();
                return;
            }

            _nextFireTime = Time.time + fireRate;
            _ammo--;
            FPSGameManager.Instance?.NotifyAmmoChanged(_ammo, magazineSize);

            if (aimCamera == null)
                return;

            Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
            Vector3 end = ray.origin + ray.direction * range;

            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                end = hit.point;
                FPSHealth health = hit.collider.GetComponentInParent<FPSHealth>();
                if (health != null && !health.isPlayer)
                    health.TakeDamage(damage);
            }

            ShowTracer(ray.origin, end);
        }

        void StartReload()
        {
            if (_reloading || _ammo >= magazineSize)
                return;

            _reloading = true;
            _reloadEndTime = Time.time + reloadTime;
        }

        void ShowTracer(Vector3 start, Vector3 end)
        {
            _line.enabled = true;
            _line.SetPosition(0, start);
            _line.SetPosition(1, end);
            _line.startColor = hitColor;
            _line.endColor = hitColor;
            CancelInvoke(nameof(HideTracer));
            Invoke(nameof(HideTracer), hitFlashDuration);
        }

        void HideTracer()
        {
            _line.enabled = false;
        }

        public int CurrentAmmo => _ammo;
        public bool IsReloading => _reloading;
    }
}
