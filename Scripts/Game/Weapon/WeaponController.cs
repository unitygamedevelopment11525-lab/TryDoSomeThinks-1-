using UnityEngine;
using UnityEngine.InputSystem;
using Project.Scripts.Game.Inventory;
using System.Collections;
using Project.Scripts.Game.Weapon;

namespace Project.Scripts.Game.Player
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("Об'єкт-контейнер для зброї (повинен бути дочірнім до CameraHolder, щоб рухатися разом із поглядом)")]
        [SerializeField] private Transform weaponHolder;
        
        [Tooltip("Шари, по яким можна стріляти (наприклад, Default, World, Enemies тощо)")]
        [SerializeField] private LayerMask hitLayers;

        [Header("Bullet Settings")]
        [Tooltip("Префаб фізичної кулі (має містити компоненти Rigidbody та Bullet)")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("Резервна точка виліту кулі (якщо на моделі зброї не знайдено компонент WeaponModel)")]
        [SerializeField] private Transform fallbackBulletSpawnPoint;

        private InputSystem_Actions _inputActions;
        private WeaponItem _currentWeaponData;
        private GameObject _spawnedWeaponModel;

        private float _nextFireTime;
        private int _currentAmmo;
        private bool _isReloading;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
            
            if (_inputActions.Player.Attack != null)
            {
                _inputActions.Player.Attack.performed += OnAttackPerformed;
            }

            try
            {
                var reloadAction = _inputActions.Player.GetType().GetProperty("Reload")?.GetValue(_inputActions.Player) as InputAction;
                if (reloadAction != null)
                {
                    reloadAction.performed += OnReloadPerformed;
                }
            }
            catch { }
        }

        private void OnDisable()
        {
            if (_inputActions.Player.Attack != null)
            {
                _inputActions.Player.Attack.performed -= OnAttackPerformed;
            }
            
            try
            {
                var reloadAction = _inputActions.Player.GetType().GetProperty("Reload")?.GetValue(_inputActions.Player) as InputAction;
                if (reloadAction != null)
                {
                    reloadAction.performed -= OnReloadPerformed;
                }
            }
            catch { }

            _inputActions.Player.Disable();
        }

        public void EquipWeapon(WeaponItem newWeapon)
        {
            if (_isReloading) return;

            UnequipCurrentWeapon();

            _currentWeaponData = newWeapon;
            _currentAmmo = newWeapon.maxAmmo; // Змінено на маленьку літеру

            if (newWeapon.weaponPrefab != null && weaponHolder != null) // Змінено на маленьку літеру
            {
                _spawnedWeaponModel = Instantiate(newWeapon.weaponPrefab, weaponHolder); // Змінено на маленьку літеру
                _spawnedWeaponModel.transform.localPosition = Vector3.zero;
                _spawnedWeaponModel.transform.localRotation = Quaternion.identity;
            }

            Debug.Log($"<color=cyan>[Weapon]</color> Екіпіровано: {newWeapon.itemName}. Набої: {_currentAmmo}/{newWeapon.maxAmmo}"); // Змінено на маленькі літери
        }

        public void UnequipCurrentWeapon()
        {
            if (_spawnedWeaponModel != null)
            {
                Destroy(_spawnedWeaponModel);
            }
            _currentWeaponData = null;
            _isReloading = false;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (_currentWeaponData == null || _isReloading) return;

            if (Time.time >= _nextFireTime)
            {
                Shoot();
            }
        }

        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            if (_currentWeaponData == null || _isReloading || _currentAmmo == _currentWeaponData.maxAmmo) return; // Змінено на маленьку літеру

            StartCoroutine(ReloadCoroutine());
        }

        private void Shoot()
        {
            if (_currentAmmo <= 0)
            {
                Debug.Log("[Weapon] Клац! Немає набоїв.");
                return;
            }

            _currentAmmo--;
            _nextFireTime = Time.time + _currentWeaponData.fireRate; // Змінено на маленьку літеру

            Transform cameraTransform = Camera.main != null ? Camera.main.transform : transform;

            if (bulletPrefab != null)
            {
                // 1. Визначаємо точну точку, куди дивиться гравець
                Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
                Vector3 targetPoint;
                float maxRange = _currentWeaponData != null ? _currentWeaponData.range : 100f; // Змінено на маленьку літеру

                if (Physics.Raycast(ray, out RaycastHit hit, maxRange, hitLayers))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = ray.GetPoint(maxRange);
                }

                // 2. Визначаємо точку вильоту кулі з дула моделі
                Transform spawnTransform = null;

                if (_spawnedWeaponModel != null)
                {
                    // Намагаємося отримати унікальну точку вильоту з поточної моделі зброї
                    WeaponModel weaponModel = _spawnedWeaponModel.GetComponent<WeaponModel>();
                    if (weaponModel != null)
                    {
                        spawnTransform = weaponModel.BulletSpawnTransform;
                    }
                }

                Vector3 spawnPosition;
                if (spawnTransform != null)
                {
                    spawnPosition = spawnTransform.position;
                }
                else if (fallbackBulletSpawnPoint != null)
                {
                    spawnPosition = fallbackBulletSpawnPoint.position;
                }
                else
                {
                    spawnPosition = cameraTransform.position + cameraTransform.forward * 0.5f;
                }

                // 3. Розраховуємо вектор польоту кулі
                Vector3 shootDirection = (targetPoint - spawnPosition).normalized;
                Quaternion spawnRotation = Quaternion.LookRotation(shootDirection);

                // Створюємо кулю
                GameObject bulletInstance = Instantiate(bulletPrefab, spawnPosition, spawnRotation);
                Bullet bulletScript = bulletInstance.GetComponent<Bullet>();
                Rigidbody rb = bulletInstance.GetComponent<Rigidbody>();

                if (bulletScript != null)
                {
                    bulletScript.Initialize(hitLayers, _currentWeaponData.damage); // Змінено на маленьку літеру
                    
                    if (rb != null && bulletScript.BulletData != null)
                    {
                        rb.velocity = shootDirection * bulletScript.BulletData.Speed;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[WeaponController] Не встановлено Bullet Prefab!");
            }
        }

        private IEnumerator ReloadCoroutine()
        {
            _isReloading = true;
            Debug.Log("[Weapon] Перезарядка...");

            yield return new WaitForSeconds(_currentWeaponData.reloadTime); // Змінено на маленьку літеру

            _currentAmmo = _currentWeaponData.maxAmmo; // Змінено на маленьку літеру
            _isReloading = false;
            Debug.Log($"[Weapon] Перезарядка завершена! Набої: {_currentAmmo}/{_currentWeaponData.maxAmmo}"); // Змінено на маленьку літеру
        }

        public void TriggerReload()
        {
            if (_currentWeaponData != null && !_isReloading && _currentAmmo < _currentWeaponData.maxAmmo) // Змінено на маленьку літеру
            {
                StartCoroutine(ReloadCoroutine());
            }
        }
    }
}