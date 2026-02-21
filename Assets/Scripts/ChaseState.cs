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
        //ai.SetAttack(false);
        ai.agent.isStopped = false;

        nextUpdateTime = 0f;

        if (ai.player != null)
        {
            ai.UpdateLastSeen();
        }
    }

    public void Tick()
    {
        if (ai.InAttackRange())
        {
            ai.sm.ChangeState(new AttackState(ai));
            return;
        }

        if (!ai.CanDetectPlayer())
        {
            ai.sm.ChangeState(new InvestigateState(ai));
            return;
        }
        
        ai.UpdateLastSeen();
    }

    public void Exit()
    {
        // rien
    }
}
