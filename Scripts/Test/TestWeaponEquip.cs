using Project.Scripts.Game.Weapon;
using UnityEngine;

namespace Project.Scripts.Test
{
    /// <summary>
    /// Тимчасовий скрипт для швидкого тестування зброї без необхідності створювати UI інвентарю.
    /// Вішається на гравця поруч із WeaponController.
    /// </summary>
    public class TestWeaponEquip : MonoBehaviour
    {
        [Header("Test Settings")]
        [Tooltip("Перетягніть сюди ваш ScriptableObject зброї (наприклад, RPG.asset)")]
        [SerializeField] private WeaponItem weaponToTest;

        private WeaponController _weaponController;

        private void Start()
        {
            _weaponController = GetComponent<WeaponController>();

            if (_weaponController == null)
            {
                Debug.LogError("[TestWeaponEquip] Не знайдено WeaponController на цьому об'єкті! Скрипт тестування має бути на Гравці.");
                return;
            }

            if (weaponToTest != null)
            {
                // Автоматично екіпіруємо зброю при старті гри
                _weaponController.EquipWeapon(weaponToTest);
                Debug.Log($"<color=yellow>[Test] Авто-екіпірування:</color> Зброю '{weaponToTest.itemName}' успішно видано для тесту!");
            }
            else
            {
                Debug.LogWarning("[TestWeaponEquip] Будь ласка, перетягніть вашу зброю (наприклад, RPG.asset) у поле 'Weapon To Test' в інспекторі гравця!");
            }
        }
    }
}