using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class EnemyJsonLoader
{
    public static Dictionary<string, Enemy> LoadEnemies(string resourcePath = "enemies")
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
        if (jsonAsset == null)
        {
            throw new InvalidOperationException($"Could not find Resources/{resourcePath}.json");
        }

        List<Enemy> enemies = JsonConvert.DeserializeObject<List<Enemy>>(jsonAsset.text) ?? new List<Enemy>();
        Dictionary<string, Enemy> byName = new Dictionary<string, Enemy>(StringComparer.OrdinalIgnoreCase);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || string.IsNullOrWhiteSpace(enemy.name))
            {
                continue;
            }

            byName[enemy.name] = enemy;
        }

        return byName;
    }
}