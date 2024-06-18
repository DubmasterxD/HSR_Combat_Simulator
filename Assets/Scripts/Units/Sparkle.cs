using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sparkle : Unit
{
    readonly float skillAFBoost = 0.5f;
    [SerializeField] Effect skillEffect;
    [SerializeField] float basicScaling;

    public override void InitializeForCombat()
    {
        base.InitializeForCombat();
        skillEffect.owner = this;
    }

    public override void DoTurn()
    {
        base.DoTurn();
        UseSkill();
    }

    protected override void UseBasicAttack()
    {
        base.UseBasicAttack();
        DealDmg(DmgType.Basic, basicScaling, StatScaling.Atk, 0, SimController.instance.GetRandomEnemy(this));
    }

    protected override void UseSkill()
    {
        base.UseSkill();
        Unit target = SimController.instance.GetStrongestAlly(this);
        target.AddEffect(skillEffect);
        target.BoostAV(skillAFBoost);
    }
}
