using System.Collections.Generic;
using UnityEngine;
using System;

namespace Project.Scripts.Game.Inventory
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance;
        public event Action OnItemChanged;

        [Header("Inventory Capacity Settings")]
        public int space = 10;
        public float maxWeight = 50f;
        
        [Header("Inventory Contents")]
        public List<Item> items = new List<Item>();

        [Header("Live Status (Read Only)")]
        [SerializeField] private int currentItemCount;
        [SerializeField] private float currentWeight;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            UpdateStatus();
        }
        
        public float GetCurrentWeight()
        {
            float totalWeight = 0f;
            foreach (Item item in items)
            {
                if (item != null)
                {
                    totalWeight += item.itemWeight; // Додаємо вагу кожного предмету в списку
                }
            }
            return totalWeight;
        }

        private void UpdateStatus()
        {
            currentItemCount = items.Count;
            currentWeight = GetCurrentWeight();
        }

        public bool AddItem(Item item)
        {
            if (item == null)
            {
                Debug.LogWarning("[Inventory] Спроба підібрати пустий предмет (null)!");
                return false;
            }

            if (items.Count >= space) 
            {
                Debug.LogWarning($"[Inventory] Неможливо підібрати '{item.itemName}': немає вільних слотів! (Зайнято: {items.Count}/{space})");
                return false;
            }

            float newWeight = GetCurrentWeight() + item.itemWeight;
            if (newWeight > maxWeight) 
            {
                Debug.LogWarning($"[Inventory] Неможливо підібрати '{item.itemName}': забагато ваги! " +
                                 $"(Спроба додати: {item.itemWeight} кг. Поточна вага: {GetCurrentWeight():F1}/{maxWeight} кг)");
                return false;
            }
            
            items.Add(item);
            UpdateStatus();
            
            Debug.Log($"<color=green>[Inventory] ПРЕДМЕТ ПІДІБРАНО:</color> '{item.itemName}' (Вага: {item.itemWeight} кг). " +
                      $"Слоти: {currentItemCount}/{space}. Вага: {currentWeight:F1}/{maxWeight} кг.");
            
            OnItemChanged?.Invoke();
            return true;
        }
        
        public void Remove(Item item)
        {
            if (item == null) return;
            
            items.Remove(item);
            UpdateStatus();
            
            Debug.Log($"<color=orange>[Inventory] ПРЕДМЕТ ВИДАЛЕНО:</color> '{item.itemName}'. " +
                      $"Слоти: {currentItemCount}/{space}. Вага: {currentWeight:F1}/{maxWeight} кг.");
            
            OnItemChanged?.Invoke();
        }
    }
}