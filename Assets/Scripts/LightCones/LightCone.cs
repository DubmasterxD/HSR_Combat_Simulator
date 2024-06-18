using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightCone : MonoBehaviour
{
    protected Unit owner;

    [SerializeField] public float baseHP;
    [SerializeField] public float baseDef;
    [SerializeField] public float baseAtk;

    public virtual void InitializeForCombat(Unit owner)
    {
        this.owner = owner;
    }

    public virtual void EndCombat()
    {

    }
}
