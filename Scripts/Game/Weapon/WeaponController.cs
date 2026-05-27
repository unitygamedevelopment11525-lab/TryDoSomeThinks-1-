using UnityEngine;
using UnityEngine.InputSystem;
using Project.Scripts.Game.Inventory; // Додаємо, щоб контролер бачив ваш ScriptableObject WeaponItem

namespace Project.Scripts.Game.Weapon
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("Контейнер для зброї під камерою гравця (наприклад, CameraHolder)")]
        [SerializeField] private Transform weaponHolder;
        
        [Tooltip("Шари, по яким можна стріляти (наприклад, Default, World тощо)")]
        [SerializeField] private LayerMask hitLayers;

        [Header("Simple Bullet Settings")]
        [Tooltip("Префаб кулі (звичайна сфера з Rigidbody та скриптом Bullet)")]
        [SerializeField] private GameObject bulletPrefab;

        [Tooltip("Швидкість польоту кулі")]
        [SerializeField] private float bulletSpeed = 50f;

        private InputSystem_Actions _inputActions;
        private WeaponItem _currentWeaponData;
        private GameObject _spawnedWeaponModel;

        private float _nextFireTime;
        private int _currentAmmo;
        
        // Таймер перезарядки в Update (без корутин)
        private bool _isReloading;
        private float _reloadTimer;

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

            // Підписка на перезарядку (клавіша R)
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

        private void Update()
        {
            // Обробка таймера перезарядки
            if (_isReloading)
            {
                _reloadTimer -= Time.deltaTime;
                if (_reloadTimer <= 0f)
                {
                    _currentAmmo = _currentWeaponData.maxAmmo;
                    _isReloading = false;
                    Debug.Log($"<color=green>[Weapon] Перезарядка завершена!</color> Набої: {_currentAmmo}/{_currentWeaponData.maxAmmo}");
                }
            }
        }

        public void EquipWeapon(WeaponItem newWeapon)
        {
            if (_isReloading) return;

            UnequipCurrentWeapon();

            _currentWeaponData = newWeapon;
            _currentAmmo = newWeapon.maxAmmo;

            if (newWeapon.weaponPrefab != null && weaponHolder != null)
            {
                // ВИПРАВЛЕНО: Прибрали (Object), тепер префаб створюється безпосередньо як GameObject
                _spawnedWeaponModel = Instantiate(newWeapon.weaponPrefab, weaponHolder);
                _spawnedWeaponModel.transform.localPosition = Vector3.zero;
                _spawnedWeaponModel.transform.localRotation = Quaternion.identity;
            }

            Debug.Log($"<color=cyan>[Weapon] Екіпіровано:</color> {newWeapon.itemName}. Набої: {_currentAmmo}/{newWeapon.maxAmmo}");
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
            if (_currentWeaponData == null || _isReloading || _currentAmmo == _currentWeaponData.maxAmmo) return;

            StartReload();
        }

        private void StartReload()
        {
            _isReloading = true;
            _reloadTimer = _currentWeaponData.reloadTime;
            Debug.Log($"[Weapon] Перезарядка... Потрібно зачекати {_currentWeaponData.reloadTime} сек.");
        }

        private void Shoot()
        {
            if (_currentAmmo <= 0)
            {
                Debug.Log("[Weapon] Клац! Набої закінчилися. Натисни R для перезарядки.");
                return;
            }

            _currentAmmo--;
            _nextFireTime = Time.time + _currentWeaponData.fireRate;

            Transform cameraTransform = Camera.main != null ? Camera.main.transform : transform;

            if (bulletPrefab != null)
            {
                // 1. Визначаємо, куди саме дивиться гравець (центр екрана)
                Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
                Vector3 targetPoint;
                
                if (Physics.Raycast(ray, out RaycastHit hit, _currentWeaponData.range, hitLayers))
                {
                    targetPoint = hit.point; // Влучили в перешкоду
                }
                else
                {
                    targetPoint = ray.GetPoint(_currentWeaponData.range); // Постріл у повітря
                }

                // 2. Визначаємо точку спавну кулі (шукаємо дуло Muzzle)
                Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * 0.6f;
                if (_spawnedWeaponModel != null)
                {
                    Transform muzzle = _spawnedWeaponModel.transform.Find("Muzzle");
                    if (muzzle != null)
                    {
                        spawnPosition = muzzle.position;
                    }
                }

                // 3. Розраховуємо вектор польоту від ствола до цілі
                Vector3 shootDirection = (targetPoint - spawnPosition).normalized;
                Quaternion bulletRotation = Quaternion.LookRotation(shootDirection);

                // Створюємо кулю
                GameObject bulletInstance = Instantiate(bulletPrefab, spawnPosition, bulletRotation);
                Rigidbody rb = bulletInstance.GetComponent<Rigidbody>();
                Bullet bulletScript = bulletInstance.GetComponent<Bullet>();

                if (bulletScript != null)
                {
                    // ВИПРАВЛЕНО: Поміняли аргументи місцями. Спочатку йде hitLayers, потім шкода зброї.
                    bulletScript.Initialize(hitLayers, _currentWeaponData.damage);
                }

                if (rb != null)
                {
                    // Використовуємо лінійну швидкість для нових версій Unity (як у вас і було налаштовано)
                    rb.linearVelocity = shootDirection * bulletSpeed;
                }
            }
            else
            {
                Debug.LogWarning("[WeaponController] Не призначено Bullet Prefab в інспекторі!");
            }
        }
    }
}