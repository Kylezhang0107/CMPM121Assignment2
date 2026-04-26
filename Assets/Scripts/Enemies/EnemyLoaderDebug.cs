using UnityEngine;

public class EnemyLoaderDebug : MonoBehaviour
{
    [SerializeField] private EnemyDatabase enemyDatabase;

    private void Start()
    {
        if (enemyDatabase == null)
        {
            enemyDatabase = FindFirstObjectByType<EnemyDatabase>();
        }

        if (enemyDatabase == null)
        {
            Debug.LogWarning("EnemyLoaderDebug: no EnemyDatabase found in scene.");
            return;
        }

        Debug.Log($"EnemyLoaderDebug: loaded {enemyDatabase.EnemiesByType.Count} enemy types.");

        if (enemyDatabase.TryGetEnemy("zombie", out Enemy zombie))
        {
            Debug.Log($"Zombie base stats from enemies.json -> hp={zombie.hp}, speed={zombie.speed}, damage={zombie.damage}, sprite={zombie.sprite}");
        }
    }
}
