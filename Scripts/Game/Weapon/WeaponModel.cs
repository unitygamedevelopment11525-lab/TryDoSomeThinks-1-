using UnityEngine;

namespace Project.Scripts.Game.Player
{
    /// <summary>
    /// Цей скрипт вішається на сам префаб 3D-моделі зброї.
    /// Дозволяє вказати унікальні точки (наприклад, дуло) для кожного типу зброї.
    /// </summary>
    public class WeaponModel : MonoBehaviour
    {
        [Header("Weapon Transforms")]
        [Tooltip("Точка виліту кулі на дулі цієї конкретної моделі зброї")]
        [SerializeField] private Transform bulletSpawnTransform;
        
        /// <summary>
        /// Публічна властивість для отримання точки виліту кулі
        /// </summary>
        public Transform BulletSpawnTransform => bulletSpawnTransform;
    }
}