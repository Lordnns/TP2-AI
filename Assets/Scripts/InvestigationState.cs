using UnityEngine;

public class InvestigateState : IState
{
    GuardAI ai;
    float timer;

    public InvestigateState(GuardAI ai) => this.ai = ai;

    public void Enter()
    {
        ai.agent.isStopped = false;
        ai.agent.SetDestination(ai.lastSeenPosition); // Move to last memory [cite: 39]
        timer = 0;
    }

    public void Tick()
    {
        if (ai.CanDetectPlayer()) { ai.sm.ChangeState(new ChaseState(ai)); return; }

        // If reached spot, wait before going back to Patrol
        if (!ai.agent.pathPending && ai.agent.remainingDistance <= ai.waypointReachedDistance)
        {
            timer += Time.deltaTime;
            if (timer >= ai.lostTime) ai.sm.ChangeState(new PatrolState(ai));
        }
    }

    public void Exit() { }
}