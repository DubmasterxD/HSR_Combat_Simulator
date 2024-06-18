using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Unity.VisualScripting;

public class Unit : MonoBehaviour
{
    protected List<Effect> activeEffects;

    [SerializeField] public bool isCharacter = true;
    [SerializeField] public string displayName = "";
    [SerializeField] public Element characterElement;
    [SerializeField] public int level { get; private set; } = 80;
    [SerializeField] protected int eidolon = 0;
    [SerializeField] protected LightCone equippedLCPrefab;
    protected LightCone equippedLC;

    //character stats
    [SerializeField] protected float baseHP;
    [SerializeField] protected float baseDef;
    [SerializeField] protected float baseAtk;
    [SerializeField] protected float baseSpd;
    [SerializeField] protected float baseCritDmg;
    [SerializeField] protected float baseCritRate;
    [SerializeField] protected float maxEnergy;
    [SerializeField] protected Effect minorTraces;

    //artifacts
    [SerializeField] protected Effect artifactsStats;
    [SerializeField] protected Effect artifactsSet1;
    [SerializeField] protected Effect artifactsSet2;
    [SerializeField] protected Effect artifactsSet3;

    [SerializeField] public bool useTechnique = true;

    protected float currentEnergy = 0;
    protected bool canUlt = false;
    public float currentAV = 0;
    public bool isWeaknessBroken = false;

    public event Action<Unit> onSkillUsed;
    public event Action<Unit> onBasicUsed;
    public event Action<Unit> onUltiUsed;
    public event Action<float> onSpeedChanged;
    public event Action<Unit, Unit, float> onDmgTaken;

    float currentMaxHP = 0;
    float currentDef = 0;
    float currentAtk = 0;
    float currentSpd = 0;
    float currentCritDmg = 0;
    float currentCritRate = 0;

    public enum StatScaling
    {
        HP = 0,
        Def,
        Atk,
        BreakEffect
    }

    [Flags]
    public enum Element
    {
        None = 0,
        Physical = 1 << 0,
        Fire = 1 << 1,
        Lightning = 1 << 2,
        Ice = 1 << 3,
        Wind = 1 << 4,
        Quantum = 1 << 5,
        Imaginery = 1 << 6
    }

    [Flags]
    public enum DmgType
    {
        None = 0,
        Basic = 1 << 0,
        Skill = 1 << 1,
        Ultimate = 1 << 2,
        Break = 1 << 3,
        DoT = 1 << 4,
        FuA = 1 << 5
    }

    private void Awake()
    {
        if (equippedLCPrefab != null)
        {
            equippedLC = Instantiate(equippedLCPrefab, transform);
        }
    }

    public void AddEffect(Effect effect)
    {
        bool hasEffect = false;
        Effect activeEffect = null;
        if(activeEffects==null)
        {
            activeEffects = new List<Effect>();
        }
        if (activeEffects.Count > 0)
        {
            IEnumerable<Effect> sameEffects = activeEffects.Where(x => x.effectID == effect.effectID);
            if (sameEffects.Count() > 0)
            {
                foreach (var eff in sameEffects)
                {
                    if (eff.owner == effect.owner)
                    {
                        hasEffect = true;
                        activeEffect = eff;
                    }
                }
            }
        }

        if(!hasEffect)
        {
            if (effect.spdBoost != 0 || effect.flatSpd != 0)
            {
                float prevSpd = GetCurrentSpeed();
                activeEffects.Add(effect);
                float currentSpd = GetCurrentSpeed();
                currentAV = MathSim.GetNewAV(currentAV, prevSpd, currentSpd);
                onSpeedChanged?.Invoke(currentSpd);
            }
            else
            {
                activeEffects.Add(effect);
            }
            activeEffect = effect;
        }
        else
        {
            if(activeEffect.currentStacks < activeEffect.maxStacks)
            {
                activeEffect.currentStacks++;
            }
        }
        activeEffect.turnsLeft = activeEffect.duration;
    }

    public void RemoveEffect(Effect effect)
    {
        bool hasEffect = false;
        Effect activeEffect = null;
        if (activeEffects.Count > 0)
        {
            IEnumerable<Effect> sameEffects = activeEffects.Where(x => x.effectID == effect.effectID);
            if (sameEffects.Count() > 0)
            {
                foreach (var eff in sameEffects)
                {
                    if (eff.owner == effect.owner)
                    {
                        hasEffect = true;
                        activeEffect = eff;
                    }
                }
            }
        }
        if (!hasEffect)
        {
            return;
        }

        if (activeEffect.spdBoost != 0 || activeEffect.flatSpd!=0)
        {
            float prevSpd = GetCurrentSpeed();
            activeEffects.Remove(activeEffect);
            float currentSpd = GetCurrentSpeed();
            currentAV = MathSim.GetNewAV(currentAV, prevSpd, currentSpd);
            onSpeedChanged?.Invoke(currentSpd);
        }
        else
        {
            activeEffects.Remove(activeEffect);
        }
    }

    public Effect GetEffect(string effectID, Unit owner)
    {
        Effect activeEffect = null;
        if (activeEffects.Count > 0)
        {
            IEnumerable<Effect> sameEffects = activeEffects.Where(x => x.effectID == effectID);
            if (sameEffects.Count() > 0)
            {
                foreach (var eff in sameEffects)
                {
                    if (eff.owner == owner)
                    {
                        activeEffect = eff;
                    }
                }
            }
        }
        return activeEffect;
    }

    float GetCurrentHP()
    {
        float effectsHPBoost = 0;
        float effectsFlatHP = 0;
        foreach(var effect in activeEffects)
        {
            effectsHPBoost += effect.HPBoost * effect.currentStacks;
            effectsFlatHP += effect.flatHP * effect.currentStacks;
        }

        float HP = 0;
        if (isCharacter)
        {
            HP = baseHP + equippedLC.baseHP;
        }
        else
        {
            HP = baseHP;
        }

        return HP * (1 + effectsHPBoost) + effectsFlatHP;
    }

    float GetCurrentDef()
    {
        float effectsDefBoost = 0;
        float effectsFlatDef = 0;
        foreach (var effect in activeEffects)
        {
            effectsDefBoost += effect.DefBoost * effect.currentStacks;
            effectsFlatDef += effect.flatDef * effect.currentStacks;
        }

        float Def = 0;
        if (isCharacter)
        {
            Def = baseDef + equippedLC.baseDef;
        }
        else
        {
            Def = 200 + 10 * level;
        }

        return Def * (1 + effectsDefBoost) + effectsFlatDef;
    }

    float GetCurrentAtk()
    {
        float effectsAtkBoost = 0;
        float effectsFlatAtk = 0;
        foreach (var effect in activeEffects)
        {
            effectsAtkBoost += effect.AtkBoost * effect.currentStacks;
            effectsFlatAtk += effect.flatAtk * effect.currentStacks;
        }

        float Atk = 0;
        if (isCharacter)
        {
            Atk = baseAtk + equippedLC.baseAtk;
        }
        else
        {
            Atk = baseAtk;
        }

        return Atk * (1 + effectsAtkBoost) + effectsFlatAtk;
    }

    public float GetCurrentSpeed()
    {
        float effectSpdBoost = 0;
        float effectsFlatSpd = 0;
        foreach (var effect in activeEffects)
        {
            effectSpdBoost += effect.spdBoost * effect.currentStacks;
            effectsFlatSpd += effect.flatSpd * effect.currentStacks;
        }

        return baseSpd * (1 + effectSpdBoost) + effectsFlatSpd;
    }

    float GetCritRate()
    {
        float effectsCritRate = 0;
        foreach (var effect in activeEffects)
        {
            effectsCritRate += effect.critRate * effect.currentStacks;
        }

        return baseCritRate + effectsCritRate;
    }

    float GetCritDmg()
    {
        return GetCritDmg(0);
    }

    float GetCritDmg(DmgType dmgType)
    {
        float effectsCritDmg = 0;
        foreach (var effect in activeEffects)
        {
            effectsCritDmg += effect.critDmg * effect.currentStacks;
            if((dmgType & effect.critDmgBoostType) != 0)
            {
                effectsCritDmg += effect.typeCritDmgBoost * effect.currentStacks;
            }
        }

        return baseCritDmg + effectsCritDmg;
    }

    float GetEffectsDmgBoost(Element element, DmgType dmgType)
    {
        float dmgBoost = 0;

        if (activeEffects != null)
        {
            foreach (var effect in activeEffects)
            {
                if ((element & effect.dmgBoostElement) != 0)
                {
                    dmgBoost += effect.elementalDmgBoost * effect.currentStacks;
                }

                if ((dmgType & effect.dmgBoostType) != 0)
                {
                    dmgBoost += effect.typeDmgBoost * effect.currentStacks;
                }
            }
        }
        return dmgBoost;
    }

    public virtual void InitializeForCombat()
    {
        minorTraces.owner = this;
        artifactsStats.owner = this;
        AddEffect(minorTraces);
        AddEffect(artifactsStats);
        if (artifactsSet1 != null)
        {
            artifactsSet1.owner = this;
            AddEffect(artifactsSet1);
        }
        if (artifactsSet2 != null)
        {
            artifactsSet2.owner = this;
            AddEffect(artifactsSet2);
        }
        if (artifactsSet3 != null)
        {
            artifactsSet3.owner = this;
            AddEffect(artifactsSet3);
        }
        isWeaknessBroken = false;
        canUlt = false;
        currentEnergy = maxEnergy/2;
        currentAV = MathSim.GetAV(GetCurrentSpeed());
        if (equippedLC != null)
        {
            equippedLC.InitializeForCombat(this);
        }
        currentMaxHP = GetCurrentHP();
        currentDef = GetCurrentDef();
        currentAtk = GetCurrentAtk();
        currentSpd = GetCurrentSpeed();
        currentCritDmg = GetCritDmg();
        currentCritRate = GetCritRate();
    }

    public virtual void EndCombat()
    {
        ClearEffects();
        if (equippedLC != null)
        {
            equippedLC.EndCombat();
        }
        onSkillUsed = null;
        onBasicUsed = null;
        onUltiUsed = null;
        onDmgTaken = null;
        onSpeedChanged = null;
    }

    void ClearEffects()
    {
        if (activeEffects == null)
        {
            activeEffects = new List<Effect>();
        }
        else if (activeEffects.Count > 0)
        {
            IEnumerable<Effect> effects = activeEffects.Reverse<Effect>();
            foreach (Effect effect in effects)
            {
                RemoveEffect(effect);
            }
        }
    }

    public virtual void BeginTurn()
    {
        if (activeEffects.Count>0)
        {
            IEnumerable<Effect> effects = activeEffects.Reverse<Effect>();
            foreach (Effect effect in effects)
            {
                if (effect.duration < 0)
                {
                    continue;
                }
                if (effect.reduceDurationOnTurnStart)
                {
                    effect.turnsLeft--;
                    if (effect.actionAdvance != 0)
                    {
                        BoostAV(effect.actionAdvance * effect.currentStacks);
                    }
                }
                if (effect.removeOnTurnStart)
                {
                    if (effect.turnsLeft == 0)
                    {
                        RemoveEffect(effect);
                    }
                }
            }
        }
        currentMaxHP = GetCurrentHP();
        currentDef = GetCurrentDef();
        currentAtk = GetCurrentAtk();
        currentSpd = GetCurrentSpeed();
        currentCritDmg = GetCritDmg();
        currentCritRate = GetCritRate();
    }

    public virtual void EndTurn()
    {
        if (activeEffects.Count>0)
        {
            IEnumerable<Effect> effects = activeEffects.Reverse<Effect>();
            foreach (Effect effect in effects)
            {
                if (effect.duration < 0)
                {
                    continue;
                }
                if (!effect.reduceDurationOnTurnStart)
                {
                    effect.turnsLeft--;
                    if (effect.actionAdvance != 0)
                    {
                        BoostAV(effect.actionAdvance * effect.currentStacks);
                    }
                }
                if (!effect.removeOnTurnStart)
                {
                    if (effect.turnsLeft == 0)
                    {
                        RemoveEffect(effect);
                    }
                }
            }
        }
        currentMaxHP = GetCurrentHP();
        currentDef = GetCurrentDef();
        currentAtk = GetCurrentAtk();
        currentSpd = GetCurrentSpeed();
        currentCritDmg = GetCritDmg();
        currentCritRate = GetCritRate();
    }

    public virtual void DoTurn()
    {
        currentAV = MathSim.GetAV(GetCurrentSpeed());
    }

    protected virtual void UseBasicAttack()
    {
        onBasicUsed?.Invoke(this);
    }

    protected virtual void UseSkill()
    {
        onSkillUsed?.Invoke(this);
    }

    protected virtual void UseUltimate()
    {
        currentEnergy = 0;
        canUlt = false;
        onUltiUsed?.Invoke(this);
    }

    public void BoostAV(float boost)
    {
        currentAV -= MathSim.GetAV(GetCurrentSpeed()) * boost;
        if (currentAV < 0)
        {
            currentAV = 0;
        }
    }

    protected void GainEnergy(float energyGain)
    {
        float energyRegen = 1;
        foreach(Effect effect in activeEffects)
        {
            energyRegen += effect.energyRegen * effect.currentStacks;
        }
        currentEnergy += energyGain * energyRegen;
        if(currentEnergy >= maxEnergy)
        {
            currentEnergy = maxEnergy;
            canUlt = true;
        }
    }

    public void ReceiveDmg(float finalDmg, DmgType dmgType, Unit attacker)
    {
        onDmgTaken?.Invoke(this, attacker, finalDmg);
    }
    
    public void DealDmg(DmgType dmgType, float abilityDmg, StatScaling statScaling, float flatDmg, Unit target, float[] dmgSplit = null)
    {
        float finalDmg = 0;
        if(dmgSplit == null)
        {
            dmgSplit = new float[1] { 1 };
        }
        for (int i = 0; i < dmgSplit.Length; i++)
        {
            float baseDmg = (abilityDmg * GetScalingValue(statScaling) + flatDmg) * dmgSplit[i];

            float critMultiplier = GetCritMultiplier(GetCritRate(), GetCritDmg(dmgType));

            float dmgMultiplier = 1 + GetEffectsDmgBoost(characterElement, dmgType);

            float weakenMultiplier = Mathf.Clamp01(1 - GetWeakenMultipliers());

            float defMultiplier = target.GetDefMultiplier(this);

            float resMultiplier = 1 - target.GetRes(characterElement) + GetResPen(characterElement);

            float vulnerabilityMultiplier = 1 + target.GetVulnerabilityMultiplier(characterElement, dmgType);

            float dmgMitigationMultiplier = target.GetDmgMitigationMultipier();

            float brokenMultiplier = target.isWeaknessBroken ? 1 : 0.9f;

            finalDmg += baseDmg * critMultiplier * dmgMultiplier * weakenMultiplier * defMultiplier * resMultiplier * vulnerabilityMultiplier * dmgMitigationMultiplier * brokenMultiplier;
        }
        target.ReceiveDmg(finalDmg, dmgType, this);
    }

    private float GetScalingValue(StatScaling statScaling)
    {
        float scalingValue = 0;
        switch (statScaling)
        {
            case StatScaling.HP:
                scalingValue = GetCurrentHP();
                break;
            case StatScaling.Def:
                scalingValue = GetCurrentDef();
                break;
            case StatScaling.Atk:
                scalingValue = GetCurrentAtk();
                break;
            case StatScaling.BreakEffect:
                break;
            default:
                break;
        }

        return scalingValue;
    }

    float GetCritMultiplier(float critRate, float critDmg)
    {
        float critMultiplier = 1;
        if (UnityEngine.Random.Range(0f, 1f) < critRate)
        {
            critMultiplier += critDmg;
        }
        return critMultiplier;
    }

    float GetWeakenMultipliers()
    {
        float weakenMultipliers = 0;
        if (activeEffects != null)
        {
            foreach (Effect effect in activeEffects)
            {
                weakenMultipliers += effect.weakenBoost * effect.currentStacks;
            }
        }
        return weakenMultipliers;
    }

    public float GetDefMultiplier(Unit attacker)
    {
        float totalDef = GetCurrentDef();
        return 1 - totalDef / (totalDef + 200 + 10 * attacker.level);
    }

    public float GetRes(Element element)
    {
        float totalRes = 0;
        if(activeEffects != null)
        {
            foreach(Effect effect in activeEffects)
            {
                if ((element & effect.resBoostElement) != 0)
                {
                    totalRes += effect.resBoost * effect.currentStacks;
                }
            }
        }
        return totalRes;
    }

    public float GetResPen(Element element)
    {
        float totalResPen = 0;
        if (activeEffects != null)
        {
            foreach (Effect effect in activeEffects)
            {
                if((element & effect.resPenBoostElement) != 0)
                {
                    totalResPen += effect.resPenBoost * effect.currentStacks;
                }
            }
        }
        return totalResPen;
    }

    public float GetVulnerabilityMultiplier(Element element, DmgType dmgType)
    {
        float totalVulnerability = 0;
        if(activeEffects != null)
        {
            foreach(Effect effect in activeEffects)
            {
                if ((element & effect.dmgTakenElement)!=0)
                {
                    totalVulnerability += effect.elementalDmgTaken * effect.currentStacks;
                }

                if((dmgType & effect.dmgTakenType) != 0)
                {
                    totalVulnerability += effect.typeDmgTaken * effect.currentStacks;
                }
            }
        }
        return totalVulnerability;
    }

    public float GetDmgMitigationMultipier()
    {
        float dmgMitigationMultipier = 1;
        if(activeEffects!=null)
        { 
            foreach(Effect effect in activeEffects)
            {
                dmgMitigationMultipier *= 1 - effect.dmgMitigation * effect.currentStacks;
            }
        }
        return dmgMitigationMultipier;
    }
}
