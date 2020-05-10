using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GoapAction", menuName = "ScriptableObjects/Test")]
public class Test : GoapAction
{
    bool completed = false;
    float startTime = 0;

    public override void Initialize()
    {
        AddPrecondition("Test", true);
        AddEffect("canMove", true);
        name = "Test";
    }

    public override bool CheckProceduralPrecondition(GameObject agent)
    {
        return true;
    }

    public override bool IsDone()
    {
        return completed;
    }

    public override bool Perform(GameObject agent)
    {
        completed = true;
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
