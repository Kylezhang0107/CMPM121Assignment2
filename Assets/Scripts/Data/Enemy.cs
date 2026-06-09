using System;

[Serializable]
public class Enemy
{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;
    public float scale = 1f;

    public string movement = "chase";
    public string damageType = "PHYSICAL";
}