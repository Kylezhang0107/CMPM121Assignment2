using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Spawn
{
    public string enemy;
    public string count;

    public List<int> sequence;
    public string delay;         // RPN string
    public string location;

    public string hp;
    public string speed;
    public string damage;
}
