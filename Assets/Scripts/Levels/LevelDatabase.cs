using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelDatabase : MonoBehaviour
{
    [SerializeField] private string resourcePath = "levels";

    private Dictionary<string, Level> levelsByName = new Dictionary<string, Level>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, Level> LevelsByName => levelsByName;

    private void Awake()
    {
        levelsByName = LevelJsonLoader.LoadFromResources(resourcePath);
    }

    public bool TryGetLevel(string levelName, out Level level)
    {
        if (string.IsNullOrWhiteSpace(levelName))
        {
            level = null;
            return false;
        }

        return levelsByName.TryGetValue(levelName, out level);
    }
}
