using UnityEngine;

namespace Project.Scripts.Game.Weapon
{
    public class SimpleTarget : MonoBehaviour
    {
        [Header("Target Health")]
        [SerializeField] private float hp = 100f;

        public void TakeDamage(float amount)
        {
            hp -= amount;
            Debug.Log($"<color=orange>[Target Damage]</color> {gameObject.name} отримав {amount} шкоди. Залишилось HP: {hp}");

            if (hp <= 0)
            {
                Debug.Log($"<color=red>[Target Destroyed]</color> {gameObject.name} знищено!");
                Destroy(gameObject);
            }
        }
    }
}