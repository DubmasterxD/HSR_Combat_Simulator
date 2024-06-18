using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InTheNight : LightCone
{
    [SerializeField] public Effect passiveCR;
    [SerializeField] public Effect passiveDmgBoost;

    public override void InitializeForCombat(Unit owner)
    {
        base.InitializeForCombat(owner);
        passiveCR.owner = owner;
        passiveDmgBoost.owner = owner;
        owner.AddEffect(passiveCR);
        OnOwnerSpeedChange(owner.GetCurrentSpeed());
        owner.onSpeedChanged += OnOwnerSpeedChange;
    }

    public override void EndCombat()
    {
        base.EndCombat();
        if (owner != null)
        { 
            owner.onSpeedChanged -= OnOwnerSpeedChange; 
        }
    }

    public void OnOwnerSpeedChange(float newSpeed)
    {
        Effect activePassiveDmgBoost = owner.GetEffect(passiveDmgBoost.effectID, owner);
        if (activePassiveDmgBoost != null)
        {
            owner.RemoveEffect(passiveDmgBoost);
        }
        Effect newPassiveDmgBoost = Instantiate(passiveDmgBoost);
        newPassiveDmgBoost.currentStacks = Mathf.FloorToInt(Mathf.Clamp((newSpeed - 100) / 10, 0, passiveDmgBoost.maxStacks));
        owner.AddEffect(newPassiveDmgBoost);
    }
}
