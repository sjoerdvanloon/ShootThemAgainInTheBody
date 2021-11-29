using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DamageType { Falling, Shot, Punched }

public class Damage
{
    public float Amount;
    public DamageType Type;
}
