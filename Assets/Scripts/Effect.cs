using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Effect", order = 0)]
public class Effect : ScriptableObject
{
    [HideInInspector] public Unit owner;
    [SerializeField] public string effectID;
    [SerializeField] public int duration;
    [HideInInspector] public int turnsLeft;
    [SerializeField] public bool reduceDurationOnTurnStart = false;
    [SerializeField] public bool removeOnTurnStart = false;
    [SerializeField] public int maxStacks = 1;
    [HideInInspector] public int currentStacks = 1;

    [Header("Stats")]
    [SerializeField] public float HPBoost;
    [SerializeField] public float flatHP;
    [SerializeField] public float DefBoost;
    [SerializeField] public float flatDef;
    [SerializeField] public float AtkBoost;
    [SerializeField] public float flatAtk;
    [SerializeField] public float spdBoost;
    [SerializeField] public float flatSpd;
    [SerializeField] public float critRate;
    [SerializeField] public float critDmg;
    [SerializeField] public float breakEffect;
    [SerializeField] public float effectHitRate;
    [SerializeField] public float effectRes;
    [SerializeField] public float energyRegen;

    [Header("Resists")]
    [SerializeField] public float resBoost;
    [SerializeField] public Unit.Element resBoostElement;

    [Header("Resists Penetration")]
    [SerializeField] public float resPenBoost;
    [SerializeField] public Unit.Element resPenBoostElement;

    [Header("Dmg Boost")]
    [SerializeField] public float typeDmgBoost;
    [SerializeField] public Unit.DmgType dmgBoostType;

    [SerializeField] public float elementalDmgBoost;
    [SerializeField] public Unit.Element dmgBoostElement;

    [Header("Vulnerability")]
    [SerializeField] public float typeDmgTaken;
    [SerializeField] public Unit.DmgType dmgTakenType;

    [SerializeField] public float elementalDmgTaken;
    [SerializeField] public Unit.Element dmgTakenElement;

    [Header("Misc")]
    [SerializeField] public float dmgMitigation;
    [SerializeField] public float weakenBoost;
    [SerializeField] public float actionAdvance;
    [SerializeField] public float typeCritDmgBoost;
    [SerializeField] public Unit.DmgType critDmgBoostType;

    public void Extend(EffectExtension effectExtension)
    {
        if (effectExtension != null)
        {
            if (effectExtension.changeDuration)
            {
                duration = effectExtension.duration;
            }
            if (effectExtension.changeMaxStacks)
            {
                maxStacks = effectExtension.maxStacks;
            }
            if (effectExtension.changeReduceDurationOnTurnStart)
            {
                reduceDurationOnTurnStart = effectExtension.changeReduceDurationOnTurnStart;
            }
            if (effectExtension.changeRemoveOnTurnStart)
            {
                removeOnTurnStart = effectExtension.changeRemoveOnTurnStart;
            }
            HPBoost += effectExtension.HPBoost;
            flatHP += effectExtension.flatHP;
            DefBoost += effectExtension.DefBoost;
            flatDef += effectExtension.flatDef;
            AtkBoost += effectExtension.AtkBoost;
            flatAtk += effectExtension.flatAtk;
            spdBoost += effectExtension.spdBoost;
            flatSpd += effectExtension.flatSpd;
            critRate += effectExtension.critRate;
            critDmg += effectExtension.critDmg;
            energyRegen += effectExtension.energyRegen;
            resBoost += effectExtension.resBoost;
            resBoostElement |= effectExtension.resBoostElement;
            resPenBoost += effectExtension.resPenBoost;
            resPenBoostElement |= effectExtension.resPenBoostElement;
            typeDmgBoost += effectExtension.typeDmgBoost;
            dmgBoostType |= effectExtension.dmgBoostType;
            elementalDmgBoost += effectExtension.elementalDmgBoost;
            dmgBoostElement |= effectExtension.dmgBoostElement;
            typeDmgTaken += effectExtension.typeDmgTaken;
            dmgTakenType |= effectExtension.dmgTakenType;
            elementalDmgTaken += effectExtension.elementalDmgTaken;
            dmgTakenElement |= effectExtension.dmgTakenElement;
            dmgMitigation += effectExtension.dmgMitigation;
            weakenBoost += effectExtension.weakenBoost;
            actionAdvance += effectExtension.actionAdvance;
        }
    }
}
