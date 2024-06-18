using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Seele : Unit
{
    [SerializeField] Effect basicEffectPrefab;
    [SerializeField] Effect skillEffectPrefab;
    [SerializeField] Effect talentEffectPrefab;
    Effect basicEffect;
    Effect skillEffect;
    Effect talentEffect;
    [SerializeField] Effect eidolon1;
    [SerializeField] EffectExtension eidolon2;
    [SerializeField] float basicScaling;
    [SerializeField] float skillScaling;
    [SerializeField] float ultimateScaling;
    [SerializeField] float[] basicDmgSplit = new float[2] { 0.3f, 0.7f };
    [SerializeField] float[] skillDmgSplit = new float[4] { 0.2f, 0.1f, 0.1f, 0.6f };

    public override void InitializeForCombat()
    {
        base.InitializeForCombat();
        basicEffect = Instantiate(basicEffectPrefab);
        basicEffect.owner = this;
        skillEffect = Instantiate(skillEffectPrefab);
        skillEffect.owner = this;
        talentEffect = Instantiate(talentEffectPrefab);
        talentEffect.owner = this;
        eidolon1.owner = this;
        if(eidolon>=1)
        {
            AddEffect(eidolon1);
        }
        if(eidolon>=2)
        {
            skillEffect.Extend(eidolon2);
        }
        if (useTechnique)
        {
            AddEffect(talentEffect);
        }
    }

    public override void DoTurn()
    {
        base.DoTurn();
        if (canUlt)
        {
            UseUltimate();
        }
        Effect activeSkillEffect = GetEffect(skillEffect.effectID, this);
        if (HasSparkleBuff() || activeSkillEffect==null || activeSkillEffect.turnsLeft == 0 || activeSkillEffect.currentStacks<activeSkillEffect.maxStacks)
        {
            UseSkill();
        }
        else
        {
            UseBasicAttack();
        }
    }

    protected override void UseBasicAttack()
    {
        base.UseBasicAttack();
        DealDmg(DmgType.Basic, basicScaling, StatScaling.Atk, 0, SimController.instance.GetRandomEnemy(this), basicDmgSplit);
        AddEffect(basicEffect);
        GainEnergy(20);
    }

    protected override void UseSkill()
    {
        AddEffect(skillEffect);
        base.UseSkill();
        DealDmg(DmgType.Skill, skillScaling, StatScaling.Atk, 0, SimController.instance.GetRandomEnemy(this), skillDmgSplit);
        GainEnergy(30);
    }

    protected override void UseUltimate()
    {
        AddEffect(talentEffect);
        base.UseUltimate();
        DealDmg(DmgType.Ultimate, ultimateScaling, StatScaling.Atk, 0, SimController.instance.GetRandomEnemy(this));
        GainEnergy(5);
    }

    bool HasSparkleBuff()
    {
        if (activeEffects.Count > 0 && (activeEffects.Where(x => x.effectID.Contains("Sparkle")).Count() > 0))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
