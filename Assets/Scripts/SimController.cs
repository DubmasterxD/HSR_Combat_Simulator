using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class SimController : MonoBehaviour
{
    public static SimController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this);
            return;
        }
        unitsInBattle = new List<Unit>();
        totalDmgDealt = new Dictionary<Unit, float>();
        foreach(var unit in unitsPrefabs)
        {
            unitsInBattle.Add(Instantiate(unit, unitsContainer));
            totalDmgDealt.Add(unit, 0);
        }
    }

    public TMPro.TextMeshProUGUI resultDisplay;

    [SerializeField] Transform unitsContainer;
    [SerializeField] List<Unit> unitsPrefabs;
    List<Unit> unitsInBattle;
    Dictionary<Unit, float> totalDmgDealt;

    List<Unit> unitsActionQ = new List<Unit>();
    List<Color> unitsColors = new List<Color>
    {
        new Color(1,0,0),
        new Color(0,1,0),
        new Color(0,0,1),
        new Color(1,1,0),
        new Color(0,1,1),
        new Color(1,0,1)
    };

    float AVpassed = 0;
    int MoCCycles = 3;
    int maxAV = 0;
    const int AVCycle0 = 150;
    const int AVperCycle = 100;

    public bool waitForClick = true;
    bool doNextStep = false;
    bool nextLocked = false;
    bool combatEnded = false;

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            doNextStep = true;
        }
    }

    public void StartSimulation()
    {
        EndCombat();
        StartCoroutine(Simulation());
    }

    IEnumerator Simulation()
    {
        InitializeSimulationVariables();

        while (!combatEnded)
        {
            if (!nextLocked && (doNextStep || !waitForClick))
            {
                DoMove();
                doNextStep = false;
            }
            yield return null;
        }

        EndCombat();
    }

    void InitializeSimulationVariables()
    {
        combatEnded = false;
        doNextStep = false;
        nextLocked = false;
        unitsActionQ.Clear();
        foreach (var unit in unitsInBattle)
        {
            unit.InitializeForCombat();
            unit.onSkillUsed += UnitUsedSkill;
            unit.onBasicUsed += UnitUsedBasic;
            unit.onUltiUsed += UnitUsedUlti;
            unit.onDmgTaken += UnitGotHit;
            if (totalDmgDealt.ContainsKey(unit))
            {
                totalDmgDealt[unit] = 0;
            }
            else
            {
                totalDmgDealt.Add(unit, 0);
            }
            unitsActionQ.Add(unit);
        }
        unitsActionQ = unitsActionQ.OrderBy(x => x.currentAV).ToList();

        AVpassed = 0;
        maxAV = AVCycle0 + MoCCycles * AVperCycle;
        resultDisplay.text = "";
    }

    void DoMove()
    {
        nextLocked = true;
        float AVtoPass = unitsActionQ[0].currentAV;

        foreach (var unit in unitsInBattle)
        {
            unit.currentAV -= AVtoPass;
        }

        AVpassed += AVtoPass;
        resultDisplay.text += string.Format("\n{0:00.00} AV passed.", AVtoPass);
        if (AVpassed <= maxAV)
        {
            unitsActionQ[0].BeginTurn();
            unitsActionQ[0].DoTurn();
            unitsActionQ[0].EndTurn();
            unitsActionQ = unitsActionQ.OrderBy(x => x.currentAV).ToList();
        }
        else
        {
            resultDisplay.text += "\n" + MoCCycles + " cycles passed.";
            foreach (var unit in unitsInBattle)
            {
                if (totalDmgDealt.ContainsKey(unit) && totalDmgDealt[unit] != 0)
                {
                    resultDisplay.text += " <color=#" + GetUnitColor(unit) + ">" + unit.displayName + " dealt total of " + totalDmgDealt[unit] + " dmg";
                    combatEnded = true;
                }
            }
        }
        nextLocked = false;
    }

    void EndCombat()
    {
        foreach (var unit in unitsInBattle)
        {
            unit.EndCombat();
        }
    }

    private string GetUnitColor(int i)
    {
        return ColorUtility.ToHtmlStringRGB(unitsColors[i]);
    }

    private string GetUnitColor(Unit unit)
    {
        int id = 0;
        if (unitsInBattle.Contains(unit))
        {
            id = unitsInBattle.IndexOf(unit);
        }
        return ColorUtility.ToHtmlStringRGB(unitsColors[id]);
    }

    void UnitUsedSkill(Unit unit)
    {
        resultDisplay.text += " <color=#" + GetUnitColor(unit) + ">" + unit.displayName + "</color> uses Skill.";
    }

    void UnitUsedBasic(Unit unit)
    {
        resultDisplay.text += " <color=#" + GetUnitColor(unit) + ">" + unit.displayName + "</color> uses Basic.";
    }

    void UnitUsedUlti(Unit unit)
    {
        resultDisplay.text += " <color=#" + GetUnitColor(unit) + ">" + unit.displayName + "</color> uses Ultimate.";
    }

    void UnitGotHit(Unit receiver, Unit attacker, float dmgDealt)
    {
        resultDisplay.text += " dealt " + dmgDealt + " dmg to <color=#" + GetUnitColor(receiver) + ">" + receiver.displayName + "</color>.";
        if (totalDmgDealt.ContainsKey(attacker))
        {
            totalDmgDealt[attacker] += dmgDealt;
        }
        else
        {
            totalDmgDealt.Add(attacker, dmgDealt);
        }
    }

    public void MoCCyclesChanged(string newCycles)
    {
        MoCCycles = Convert.ToInt32(newCycles);
    }

    public Unit GetStrongestAlly(Unit attacker)
    {
        Unit ally = null;
        if(attacker!=null && attacker.isCharacter)
        {
            ally = unitsInBattle[0];
        }
        return ally;
    }

    public Unit GetRandomEnemy(Unit attacker)
    {
        Unit enemy = null;
        List<Unit> enemies = new List<Unit>();
        if(unitsInBattle!=null)
        {
            foreach(Unit unit in unitsInBattle)
            {
                if(unit.isCharacter != attacker.isCharacter)
                {
                    enemies.Add(unit);
                }
            }
        }
        if(enemies.Count > 0)
        {
            enemy = enemies[UnityEngine.Random.Range(0, enemies.Count)];
        }
        return enemy;
    }
}
