using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bandit : MonoBehaviour, IGoap
{
    NavMeshAgent agent;
    Vector3 previousDestination;

    private void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent.hasPath)
        {
            Vector3 toTarget = agent.steeringTarget - this.transform.position;
            float turnAngle = Vector3.Angle(this.transform.forward, toTarget);
            agent.acceleration = turnAngle * agent.speed;
        }
    }

    public HashSet<KeyValuePair<string, object>> GetWorldState()
    {
        HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();
        worldData.Add(new KeyValuePair<string, object>("Test", true));
        return worldData;
    }

    public HashSet<KeyValuePair<string, object>> CreateGoalState()
    {
        HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();
        goal.Add(new KeyValuePair<string, object>("Dodge", true));

        return goal;
    }

    public bool MoveAgent(GoapAction nextAction)
    {

        //if we don't need to move anywhere
        if (previousDestination == nextAction.target.transform.position)
        {
            nextAction.SetInRange(true);
            return true;
        }

        agent.SetDestination(nextAction.target.transform.position);

        if (agent.hasPath && agent.remainingDistance < 2)
        {
            nextAction.SetInRange(true);
            previousDestination = nextAction.target.transform.position;
            return true;
        }
        else
            return false;
    }

    public void ActionsFinished()
    {

    }

    public void PlanAborted(GoapAction aborter)
    {

    }

    public void PlanFailed(HashSet<KeyValuePair<string, object>> failedGoal)
    {

    }

    public void PlanFound(HashSet<KeyValuePair<string, object>> goal, Stack<GoapAction> actions)
    {

    }
}
