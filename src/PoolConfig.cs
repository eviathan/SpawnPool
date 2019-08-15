using System;
using UnityEngine;

[Serializable]
public class PoolConfig
{
    public Type Type;
    public int Amount;
    public int RunningLowAmount;
    public Spawnable Object;
}
