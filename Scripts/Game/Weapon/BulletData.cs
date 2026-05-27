using UnityEngine;

namespace Project.Scripts.Game.Player
{
    [CreateAssetMenu(fileName = "New Bullet Data", menuName = "Weapons/Bullet Data")]
    public class BulletData : ScriptableObject
    {
        [Header("Bullet Physical Settings")]
        [Tooltip("Швидкість польоту кулі")]
        [SerializeField] private float speed = 40f;
        public float Speed => speed;

        [Tooltip("Час існування кулі в секундах до її автоматичного знищення")]
        [SerializeField] private float lifetime = 5f;
        public float Lifetime => lifetime;

        [Header("Combat Settings")]
        [Tooltip("Базова шкода кулі (якщо вона не переписується шкодою від самої зброї)")]
        [SerializeField] private float baseDamage = 20f;
        public float BaseDamage => baseDamage;

        [Header("Visual Effects")]
        [Tooltip("Префаб ефекту влучання (іскри, пил тощо)")]
        [SerializeField] private GameObject hitEffectPrefab;
        public GameObject HitEffectPrefab => hitEffectPrefab;
    }
}