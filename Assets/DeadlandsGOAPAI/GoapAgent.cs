using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GoapAgent : MonoBehaviour
{
    private FiniteStateMachine stateMachine;

    private FiniteStateMachine.State idleState; // finds something to do
    private FiniteStateMachine.State moveToState; // moves to a target
    private FiniteStateMachine.State performActionState; // performs an action

    [SerializeField] private List<GoapAction> availableActions;
    private Stack<GoapAction> currentActions;

    public IGoap enemyTypeDataProvider; // this is the implementing class that provides our world data and listens to feedback on planning

    private GoapPlanner planner;

    // Start is called before the first frame update
    void Start()
    {
        stateMachine = new FiniteStateMachine();
        currentActions = new Stack<GoapAction>();
        planner = new GoapPlanner();
        FindEnemyTypeComponent();
        InitializeActions();
        planner.Initialize(gameObject, availableActions);
        CreateIdleState();
        CreateMoveToState();
        CreatePerformActionState();
        stateMachine.PushState(idleState);
    }

    private void Update()
    {
        stateMachine.InvokeCurrentState(gameObject);
    }

    private bool HasActionPlan()
    {
        return currentActions.Count > 0;
    }

    private void CreateIdleState()
    {
        idleState = (fsm, gameObj) => {
            // GOAP planning

            // get the world state and the goal we want to plan for
            HashSet<KeyValuePair<string, object>> worldState = enemyTypeDataProvider.GetWorldState();
            HashSet<KeyValuePair<string, object>> goal = enemyTypeDataProvider.CreateGoalState();

            // Plan
            Stack<GoapAction> plan = planner.Plan(worldState, goal);
            if (plan != null)
            {
                // we have a plan, hooray!
                currentActions = plan;
                enemyTypeDataProvider.PlanFound(goal, plan);

                fsm.PopState(); // switch to PerformAction state
                fsm.PushState(performActionState);

            }
            else
            {
                // ugh, we couldn't get a plan
                Debug.Log("Failed Plan: " + goal);
                enemyTypeDataProvider.PlanFailed(goal);
                fsm.PopState(); // switch back to IdleAction state
                fsm.PushState(idleState);
            }

        };
    }

    private void CreateMoveToState()
    {
        moveToState = (fsm, gameObj) => {
            // move the game object

            GoapAction action = currentActions.Peek();
            if (action.RequiresInRange() && action.target == null)
            {
                Debug.Log("Fatal error: Action requires a target but has none. Planning failed. You did not assign the target in your Action.checkProceduralPrecondition()");
                fsm.PopState(); // move
                fsm.PopState(); // perform
                fsm.PushState(idleState);
                return;
            }

            // get the agent to move itself
            Debug.Log("Move to do: " + action.name);
            if (enemyTypeDataProvider.MoveAgent(action))
            {
                fsm.PopState();
            }
        };
    }

    private void CreatePerformActionState()
    {

        performActionState = (fsm, gameObj) => {
            // perform the action

            if (!HasActionPlan())
            {
                // no actions to perform
                Debug.Log("<color=red>Done actions</color>");
                fsm.PopState();
                fsm.PushState(idleState);
                enemyTypeDataProvider.ActionsFinished();
                return;
            }

            GoapAction action = currentActions.Peek();
            if (action.IsDone())
            {
                // the action is done. Remove it so we can perform the next one
                currentActions.Pop();
            }

            if (HasActionPlan())
            {
                // perform the next action
                action = currentActions.Peek();
                bool inRange = action.RequiresInRange() ? action.IsInRange() : true;

                if (inRange)
                {
                    // we are in range, so perform the action
                    bool success = action.Perform(gameObj);

                    if (!success)
                    {
                        // action failed, we need to plan again
                        fsm.PopState();
                        fsm.PushState(idleState);
                        enemyTypeDataProvider.PlanAborted(action);
                    }
                }
                else
                {
                    // we need to move there first
                    // push moveTo state
                    fsm.PushState(moveToState);
                }

            }
            else
            {
                // no actions left, move to Plan state
                fsm.PopState();
                fsm.PushState(idleState);
                enemyTypeDataProvider.ActionsFinished();
            }

        };
    }

    private void FindEnemyTypeComponent()
    {
        foreach (Component comp in gameObject.GetComponents(typeof(Component)))
        {
            if (typeof(IGoap).IsAssignableFrom(comp.GetType()))
            {
                enemyTypeDataProvider = (IGoap)comp;
                return;
            }
        }
    }

    private void InitializeActions()
    {
        foreach (GoapAction action in availableActions)
        {
            action.Initialize();
        }
    }
}
