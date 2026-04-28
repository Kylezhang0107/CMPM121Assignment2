using System;
using System.Collections.Generic;
using UnityEngine;

public static class LevelJsonLoader
{
    [Serializable]
    private class LevelListWrapper
    {
        public List<Level> levels;
    }

    public static Dictionary<string, Level> LoadFromResources(string resourcePath = "levels")
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);

        if (jsonAsset == null)
        {
            throw new InvalidOperationException($"Could not find Resources/{resourcePath}.json");
        }

        return ParseDictionary(jsonAsset.text);
    }

    public static Dictionary<string, Level> ParseDictionary(string json)
    {
        List<Level> levels = ParseList(json);
        Dictionary<string, Level> byName = new Dictionary<string, Level>(StringComparer.OrdinalIgnoreCase);

        foreach (Level level in levels)
        {
            if (level == null || string.IsNullOrWhiteSpace(level.name))
            {
                continue;
            }

            level.spawns ??= new List<Spawn>();
            byName[level.name] = level;
        }

        return byName;
    }

    public static List<Level> ParseList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Level>();
        }

        string trimmed = json.Trim();

        // Unity JsonUtility cannot parse top-level arrays, so wrap if needed.
        if (trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            trimmed = "{\"levels\":" + trimmed + "}";
        }

        LevelListWrapper wrapper = JsonUtility.FromJson<LevelListWrapper>(trimmed);

        return wrapper?.levels ?? new List<Level>();
    }
}
