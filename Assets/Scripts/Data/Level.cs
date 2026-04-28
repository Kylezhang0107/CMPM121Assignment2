using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Level
{
    public string name;
    public int waves;              // may be 0 if missing
    public List<Spawn> spawns;
}
