using UnityEngine;

namespace Project.Scripts.Game.Weapon
{
    /// <summary>
    /// Цей скрипт вішається безпосередньо на префаб 3D-моделі зброї.
    /// </summary>
    public class WeaponModel : MonoBehaviour
    {
        [Header("Weapon Transforms")]
        [Tooltip("Точка виліту кулі на дулі цієї конкретної моделі зброї (об'єкт Muzzle)")]
        [SerializeField] private Transform bulletSpawnTransform;
        
        /// <summary>
        /// Публічна властивість для отримання точки виліту кулі.
        /// </summary>
        public Transform BulletSpawnTransform => bulletSpawnTransform;
    }
}