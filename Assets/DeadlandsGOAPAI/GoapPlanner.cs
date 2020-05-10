using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Currently checking for world action matches at each link - if I end up not needing that I could optimize it out

public class GoapPlanner
{
    List<Node> graph = new List<Node>();

    public void Initialize(GameObject agent, List<GoapAction> availableActions)
    {
        List<GoapAction> usableActions = new List<GoapAction>();
        foreach (GoapAction a in availableActions)
        {
            if (a.CheckProceduralPrecondition(agent))
                usableActions.Add(a);
        }

        foreach (GoapAction action in usableActions)
        {
            graph.Add(new Node(action.cost, action));
        }

        for (int i = 0; i < graph.Count; i++)
        {
            var effects = graph[i].action.Effects;
            for (int j = 0; j < graph.Count; j++)
            {
                if (graph[i] == graph[j])
                    continue;

                var preconditions = graph[j].action.Preconditions;

                bool canBeLinked = false;
                foreach (KeyValuePair<string, object> kvp in preconditions)
                {
                    if (effects.Contains(kvp))
                        canBeLinked = true;
                }

                if (canBeLinked)
                    graph[i].links.Add(graph[j]);
            }
        }
    }

    public Stack<GoapAction> Plan(HashSet<KeyValuePair<string, object>> worldState, HashSet<KeyValuePair<string, object>> goal)
    {
        Stack<GoapAction> finalPlan = null;
        int minDistPath = int.MaxValue;

        foreach (Node node in graph)
        {
            bool isValidStartingPoint = true;
            var preconditions = node.action.Preconditions;
            foreach (KeyValuePair<string, object> kvp in preconditions)
            {
                if (!worldState.Contains(kvp))
                {
                    isValidStartingPoint = false;
                    break;
                }
            }
            if (isValidStartingPoint)
            {
                int dist = PlotShortestPathToGoal(node, out Stack<GoapAction> currentPlan, worldState, goal);
                ResetGraph();
                if (dist < 0) // couldn't reach goal
                    continue;
                else if (dist < minDistPath)
                {
                    minDistPath = dist;
                    finalPlan = currentPlan;
                }
            }
        }
        return finalPlan;
    }

    int PlotShortestPathToGoal(Node start, out Stack<GoapAction> plan, HashSet<KeyValuePair<string, object>> worldState, HashSet<KeyValuePair<string, object>> goal)
    {
        PriorityQueue<Node> pq = new PriorityQueue<Node>();
        Node currentNode = start;
        Node goalNode = null;
        int goalDistance = -1;
        currentNode.distance = 0;

        if (FoundGoal(currentNode, worldState, goal)) // starting node fulfills goal - we don't need to do any pathfinding
        {
            plan = RetracePath(currentNode);
            return currentNode.distance;
        }

        foreach (Node link in currentNode.links)
        {
            if (PreconditionsAreMet(currentNode, link, worldState))
            {
                link.previous = currentNode;
                link.distance = currentNode.distance + link.cost;
                pq.Enqueue(link.cost, link);
            }
        }

        while (pq.Count > 0)
        {
            currentNode = pq.Dequeue();
            if (FoundGoal(currentNode, worldState, goal))
            {
                goalNode = currentNode;
                goalDistance = currentNode.distance;
            }

            foreach (Node link in currentNode.links)
            {
                if (PreconditionsAreMet(currentNode, link, worldState) && link.distance < currentNode.distance + link.cost)
                {
                    link.previous = currentNode;
                    link.distance = currentNode.distance + link.cost;
                    pq.Enqueue(link.cost, link);
                }
            }
        }

        if (goalNode != null)
            plan = RetracePath(goalNode);
        else
            plan = null;
        return goalDistance;
    }

    Stack<GoapAction> RetracePath(Node node)
    {
        Stack<GoapAction> path = new Stack<GoapAction>();
        while (node != null)
        {
            path.Push(node.action);
            node = node.previous;
        }
        return path;
    }

    bool PreconditionsAreMet(Node from, Node to, HashSet<KeyValuePair<string, object>> worldState)
    {
        HashSet<KeyValuePair<string, object>> preconditions = to.action.Preconditions;
        foreach (KeyValuePair<string, object> kvp in preconditions)
        {
            // if the precondition isn't satisfied by either the effects of the previous action or world state then it can't be done
            if (!from.action.Effects.Contains(kvp) && !worldState.Contains(kvp))
                return false;
        }
        return true;
    }

    bool FoundGoal(Node node, HashSet<KeyValuePair<string, object>> worldState, HashSet<KeyValuePair<string, object>> goal)
    {
        foreach (KeyValuePair<string, object> kvp in goal)
        {
            if (!node.action.Effects.Contains(kvp) && !worldState.Contains(kvp))
                return false;
        }
        return true;
    }

    void ResetGraph()
    {
        foreach (Node node in graph)
        {
            node.previous = null;
            node.distance = int.MaxValue;
        }
    }
}

class Node : IComparable<Node>
{
    public List<Node> links = new List<Node>();
    public Node previous; // used to trace path
    public int cost;
    public int distance;
    public GoapAction action;

    public Node(int _cost, GoapAction _action)
    {
        cost = _cost;
        distance = int.MaxValue;
        previous = null;
        action = _action;
    }

    public int CompareTo(Node other)
    {
        return this.cost.CompareTo(other.cost);
    }
}
