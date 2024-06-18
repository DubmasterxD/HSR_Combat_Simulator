using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect Extension", menuName = "Effect Extension", order = 0)]
public class EffectExtension : Effect
{
    [Header("Extension")]
    [SerializeField] public bool changeDuration;
    [SerializeField] public bool changeReduceDurationOnTurnStart;
    [SerializeField] public bool changeRemoveOnTurnStart;
    [SerializeField] public bool changeMaxStacks;
}
