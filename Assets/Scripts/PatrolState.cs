using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IState
{
    GuardAI ai;
    int waypointIndex;

    public PatrolState(GuardAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.SetAttack(false);
        ai.agent.isStopped = false;

        waypointIndex = 0;

        if (ai.waypoints == null || ai.waypoints.Length == 0)
        {
            Debug.LogWarning("PatrolState: no waypoints");
            return;
        }

        SetDestination();
    }

    public void Tick()
    {
        // Transition Patrol -> Chase
        if (ai.CanDetectPlayer())
        {
            ai.UpdateLastSeen();
            ai.sm.ChangeState(new ChaseState(ai));
            return;
        }

        if (ai.waypoints == null || ai.waypoints.Length == 0)
            return;

        if (HasReachedDestination(ai.agent))
        {
            waypointIndex = (waypointIndex + 1) % ai.waypoints.Length;
            SetDestination();
        }
    }

    public void Exit()
    {
        // rien
    }

    void SetDestination()
    {
        Transform wp = ai.waypoints[waypointIndex];
        if (wp == null) return;
        ai.agent.SetDestination(wp.position);
    }

    bool HasReachedDestination(NavMeshAgent agent)
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance == Mathf.Infinity) return false;
        return agent.remainingDistance <= ai.waypointReachedDistance;
    }
}
