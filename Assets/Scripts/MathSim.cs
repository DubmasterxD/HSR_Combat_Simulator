using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathSim
{
    static readonly float maxDistance = 10000;

    public static float GetAV(float speed)
    {
        return maxDistance / speed;
    }

    public static float GetNewAV(float prevAV, float prevSpd, float currSpd)
    {
        return prevAV * prevSpd / currSpd;
    }
}
