using UnityEngine;

namespace Project.Scripts.Game.Inventory
{ 
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class Item : ScriptableObject
    {
        public string itemName;
        public Sprite itemSprite;
        public bool isStackable;
        public int maxStackSize = 1;
        public float itemWeight = 1;

        public void Use()
        {
            Debug.Log("Item be used " +   itemName);
        }
    }
}
