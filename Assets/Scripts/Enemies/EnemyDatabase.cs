using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDatabase : MonoBehaviour
{
    [SerializeField] private string resourcePath = "enemies";

    private Dictionary<string, Enemy> enemiesByType = new Dictionary<string, Enemy>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, Enemy> EnemiesByType => enemiesByType;

    private void Awake()
    {
        enemiesByType = EnemyJsonLoader.LoadFromResources(resourcePath);
    }

    public bool TryGetEnemy(string enemyType, out Enemy enemy)
    {
        if (string.IsNullOrWhiteSpace(enemyType))
        {
            enemy = null;
            return false;
        }

        return enemiesByType.TryGetValue(enemyType, out enemy);
    }
}
