namespace Project.Scripts.Game
{
    /// <summary>
    /// Інтерфейс для будь-яких об'єктів, які можуть отримувати шкоду (вороги, ящики, мішені).
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }
}