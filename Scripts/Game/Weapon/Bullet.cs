using UnityEngine;
using Project.Scripts.Game; // Для доступу до інтерфейсу IDamageable

namespace Project.Scripts.Game.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Data Asset")]
        [Tooltip("Асет налаштувань цієї кулі (ScriptableObject)")]
        [SerializeField] private BulletData bulletData;
        public BulletData BulletData => bulletData;

        private LayerMask _hitLayers;
        private bool _hasHit;
        private float _overrideDamage = -1f;

        /// <summary>
        /// Метод ініціалізації кулі з налаштуванням шарів зіткнення та можливим перевизначенням шкоди.
        /// </summary>
        public void Initialize(LayerMask hitLayers, float overrideDamage = -1f)
        {
            _hitLayers = hitLayers;
            _hasHit = false;
            _overrideDamage = overrideDamage;

            // Час автоматичного знищення зчитуємо безпосередньо з асету даних кулі
            float lifetime = bulletData != null ? bulletData.Lifetime : 5f;
            Destroy(gameObject, lifetime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Запобігаємо подвійному спрацьовуванню в одному кадрі
            if (_hasHit) return;

            // Перевіряємо, чи входить об'єкт у дозволені шари для взаємодії (hitLayers)
            if (((1 << collision.gameObject.layer) & _hitLayers) != 0)
            {
                _hasHit = true;

                Debug.Log($"[Bullet] Фізичне влучання в: {collision.gameObject.name}");

                // Визначаємо шкоду: якщо передано override (від зброї), беремо її, інакше — базову шкоду кулі
                float finalDamage = _overrideDamage > 0f ? _overrideDamage : (bulletData != null ? bulletData.BaseDamage : 0f);

                // Шукаємо інтерфейс IDamageable на об'єкті зіткнення
                IDamageable damageable = collision.collider.GetComponent<IDamageable>();
                if (damageable == null)
                {
                    // На випадок, якщо колайдер на дочірньому об'єкті, а скрипт шкоди вище в ієрархії
                    damageable = collision.collider.GetComponentInParent<IDamageable>();
                }

                if (damageable != null)
                {
                    damageable.TakeDamage(finalDamage);
                }

                // Спавнимо ефект влучання у точці контакту з асету кулі
                GameObject effectPrefab = bulletData != null ? bulletData.HitEffectPrefab : null;
                if (effectPrefab != null && collision.contacts.Length > 0)
                {
                    ContactPoint contact = collision.contacts[0];
                    // Повертаємо ефект обличчям до нормалі поверхні
                    Instantiate(effectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
                }

                // Знищуємо об'єкт фізичної кулі після першого ж влучання
                Destroy(gameObject);
            }
        }
    }
}