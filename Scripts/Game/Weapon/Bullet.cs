using UnityEngine;

namespace Project.Scripts.Game.Weapon
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
        /// Метод ініціалізації кулі з налаштуванням шарів зіткнення та шкоди.
        /// </summary>
        public void Initialize(LayerMask hitLayers, float overrideDamage = -1f)
        {
            _hitLayers = hitLayers;
            _hasHit = false;
            _overrideDamage = overrideDamage;

            // Час автоматичного знищення зчитуємо з асету кулі
            float lifetime = bulletData != null ? bulletData.Lifetime : 4f;
            Destroy(gameObject, lifetime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_hasHit) return;

            // Перевіряємо шар зіткнення
            if (((1 << collision.gameObject.layer) & _hitLayers) != 0)
            {
                _hasHit = true;

                Debug.Log($"[Bullet] Влучання в: {collision.gameObject.name}");

                // Визначаємо шкоду: якщо передано override (від зброї), беремо її, інакше — базову
                float finalDamage = _overrideDamage > 0f ? _overrideDamage : (bulletData != null ? bulletData.BaseDamage : 10f);

                // Шукаємо тестову мішень SimpleTarget
                SimpleTarget target = collision.collider.GetComponent<SimpleTarget>();
                if (target == null)
                {
                    target = collision.collider.GetComponentInParent<SimpleTarget>();
                }

                if (target != null)
                {
                    target.TakeDamage(finalDamage);
                }

                // Спавнимо ефект влучання у точці контакту
                GameObject effectPrefab = bulletData != null ? bulletData.HitEffectPrefab : null;
                if (effectPrefab != null && collision.contacts.Length > 0)
                {
                    ContactPoint contact = collision.contacts[0];
                    Instantiate(effectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
                }

                // Видаляємо фізичну кулю
                Destroy(gameObject);
            }
        }
    }
}