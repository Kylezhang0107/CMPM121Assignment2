using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class RelicJsonLoader
{
    public static List<PlayerController.RelicData> LoadRelics()
    {
        TextAsset file = Resources.Load<TextAsset>("relics");

        if (file == null)
        {
            Debug.LogError("relics.json not found");
            return new List<PlayerController.RelicData>();
        }

        return JsonConvert.DeserializeObject<List<PlayerController.RelicData>>(file.text);
    }
}