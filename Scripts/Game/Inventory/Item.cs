using UnityEngine;

namespace Project.Scripts.Game.Inventory
{ 
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        [Header("Base Item Settings")]
        public string itemName;
        public Sprite itemSprite;
        public bool isStackable;
        public int maxStackSize = 1;
        public float itemWeight = 1;

        // Метод робимо віртуальним (virtual), щоб спадкові класи могли його перевизначати.
        // Додаємо опціональний параметр player = null, щоб не зламати виклики Use() без параметрів,
        // але мати можливість передати гравця, коли це необхідно.
        public virtual void Use(GameObject player = null)
        {
            Debug.Log("Item be used " + itemName);
        }
    }
}