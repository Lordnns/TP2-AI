using UnityEngine;

public class ChaseState : IState
{
    GuardAI ai;
    float nextUpdateTime;

    public ChaseState(GuardAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.SetAttack(false);
        ai.agent.isStopped = false;

        nextUpdateTime = 0f;

        if (ai.player != null)
        {
            ai.UpdateLastSeen();
        }
    }

    public void Tick()
    {
        // Transition Chase -> Attack
        if (ai.InAttackRange())
        {
            ai.sm.ChangeState(new AttackState(ai));
            return;
        }

        // Si détecté : refresh last seen
        if (ai.CanDetectPlayer())
        {
            ai.UpdateLastSeen();
        }

        // Aller vers last seen (pas à chaque frame)
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + ai.updateDestinationRate;
            ai.agent.SetDestination(ai.lastSeenPosition);
        }

        // Transition Chase -> Patrol si perdu trop longtemps
        if (Time.time - ai.lastSeenTime > ai.lostTime)
        {
            ai.sm.ChangeState(new PatrolState(ai));
            return;
        }
    }

    public void Exit()
    {
        // rien
    }
}
