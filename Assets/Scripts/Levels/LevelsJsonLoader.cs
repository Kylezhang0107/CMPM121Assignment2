using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class LevelsJsonLoader
{
    public static List<Level> LoadLevels(string resourcePath = "levels")
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
        if (jsonAsset == null)
        {
            throw new InvalidOperationException($"Could not find Resources/{resourcePath}.json");
        }

        List<Level> levels = JsonConvert.DeserializeObject<List<Level>>(jsonAsset.text) ?? new List<Level>();

        // Clean invalid entries (same philosophy as Enemy loader)
        List<Level> validLevels = new List<Level>();

        foreach (Level level in levels)
        {
            if (level == null || string.IsNullOrWhiteSpace(level.name))
            {
                continue;
            }

            // ensure spawns list is not null
            if (level.spawns == null)
            {
                level.spawns = new List<Spawn>();
            }

            validLevels.Add(level);
        }

        return validLevels;
    }
}
