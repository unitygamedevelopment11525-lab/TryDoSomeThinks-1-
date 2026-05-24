using UnityEngine;

namespace Project.Scripts.Game.Inventory
{
    public class ItemObject : MonoBehaviour
    {
        // Сюди в інспекторі ми перетягнемо створений ScriptableObject предмету
        public Item itemData; 

        // Метод, який викликається, коли гравець підбирає цей предмет
        public void PickUp()
        {
            // Намагаємося додати предмет в інвентар через наш Синглтон
            bool wasPickedUp = Inventory.Instance.AddItem(itemData);

            // Якщо в інвентарі було місце і предмет успішно додався
            if (wasPickedUp)
            {
                Debug.Log($"Підібрано предмет: {itemData.itemName}");
                Destroy(gameObject); // Видаляємо об'єкт зі сцени
            }
        }
    }
}