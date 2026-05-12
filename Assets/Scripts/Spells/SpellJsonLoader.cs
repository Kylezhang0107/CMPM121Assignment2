using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class SpellJsonLoader
{
    public static Dictionary<string, SpellData> LoadSpells(
        string resourcePath = "spells"
    )
    {
        TextAsset jsonAsset =
            Resources.Load<TextAsset>(resourcePath);

        if (jsonAsset == null)
        {
            throw new InvalidOperationException(
                $"Could not find Resources/{resourcePath}.json"
            );
        }

        Dictionary<string, SpellData> spells =
            JsonConvert.DeserializeObject<Dictionary<string, SpellData>>(
                jsonAsset.text
            );

        return spells ?? new Dictionary<string, SpellData>();
    }
}
