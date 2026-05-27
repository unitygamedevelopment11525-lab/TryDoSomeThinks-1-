using Project.Scripts.Game.Inventory;
using Project.Scripts.Game.Player;
using UnityEngine;

namespace Project.Scripts.Game.Weapon
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
    public class WeaponItem : Item
    {
        [Header("Weapon Stats")]
        [Tooltip("Шкода від одного пострілу")]
        public float damage = 20f;
        
        [Tooltip("Затримка між пострілами в секундах (наприклад, 0.2f — це 5 пострілів на секунду)")]
        public float fireRate = 0.5f;
        
        [Tooltip("Максимальна дальність польоту кулі")]
        public float range = 50f;
        
        [Tooltip("Максимальна кількість набоїв в магазині")]
        public int maxAmmo = 30;
        
        [Tooltip("Час перезарядки в секундах")]
        public float reloadTime = 2.0f;

        [Header("Visuals")]
        [Tooltip("Префаб 3D-моделі зброї, яка буде створюватись у руках гравця")]
        public GameObject weaponPrefab;

        // Перевизначаємо метод Use для екіпірування цієї зброї
        public override void Use(GameObject player = null)
        {
            GameObject targetPlayer = player;

            // Якщо гравець не був переданий прямо в метод, шукаємо його на сцені за тегом "Player"
            if (targetPlayer == null)
            {
                targetPlayer = GameObject.FindWithTag("Player");
            }

            if (targetPlayer != null)
            {
                // Отримуємо контролер зброї з гравця
                WeaponController weaponController = targetPlayer.GetComponent<WeaponController>();
                if (weaponController != null)
                {
                    weaponController.EquipWeapon(this);
                }
                else
                {
                    Debug.LogWarning($"[WeaponItem] На об'єкті {targetPlayer.name} не знайдено компонент WeaponController!");
                }
            }
            else
            {
                Debug.LogError("[WeaponItem] Не вдалося знайти гравця на сцені для використання зброї! Переконайся, що на гравці встановлено тег 'Player'.");
            }
        }
    }
}