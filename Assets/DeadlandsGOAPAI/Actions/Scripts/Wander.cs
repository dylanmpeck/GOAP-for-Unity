using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoapAction", menuName = "ScriptableObjects/Wander")]
public class Wander : GoapAction
{
    bool completed = false;
    float startTime = 0;

    public override void Initialize()
    {
        AddPrecondition("canMove", true);
        AddEffect("Dodge", true);
        name = "Wander";
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        return true;
    }

    public override bool IsDone()
    {
        return IsInRange();
    }

    public override bool Perform(GameObject agent)
    {
        Debug.Log("Performing " + name);
        return true;
    }

    public override bool RequiresInRange()
    {
        return requiresInRange;
    }

    public override void Reset()
    {
        completed = false;
        startTime = 0;
    }
}
