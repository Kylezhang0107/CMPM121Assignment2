using System;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyJsonLoader
{
    [Serializable]
    private class EnemyListWrapper
    {
        public List<Enemy> enemies;
    }

    public static Dictionary<string, Enemy> LoadFromResources(string resourcePath = "enemies")
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);

        if (jsonAsset == null)
        {
            throw new InvalidOperationException($"Could not find Resources/{resourcePath}.json");
        }

        return ParseDictionary(jsonAsset.text);
    }

    public static Dictionary<string, Enemy> ParseDictionary(string json)
    {
        List<Enemy> enemies = ParseList(json);
        Dictionary<string, Enemy> byType = new Dictionary<string, Enemy>(StringComparer.OrdinalIgnoreCase);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || string.IsNullOrWhiteSpace(enemy.name))
            {
                continue;
            }

            byType[enemy.name] = enemy;
        }

        return byType;
    }

    public static List<Enemy> ParseList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Enemy>();
        }

        string trimmed = json.Trim();

        // Unity JsonUtility cannot parse top-level arrays, so wrap if needed.
        if (trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            trimmed = "{\"enemies\":" + trimmed + "}";
        }

        EnemyListWrapper wrapper = JsonUtility.FromJson<EnemyListWrapper>(trimmed);

        return wrapper?.enemies ?? new List<Enemy>();
    }
}
