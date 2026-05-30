using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance;
    private readonly List<RuntimeRelic> relics = new();

    private void Awake()
    {
        Instance = this;
    }

    public void AddRelic(RuntimeRelic relic)
    {
        relics.Add(relic);
        relic.Register();
    }

    public void ClearRelics()
    {
        foreach (RuntimeRelic relic in relics)
        {
            relic.Unregister();
        }
        relics.Clear();
    }
}