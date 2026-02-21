using UnityEngine;

public class FallBackState : IState
{
    GuardAI ai;
    float cooldownEndTime;
    
    public FallBackState(GuardAI ai)
    {
        this.ai = ai;
    }
    public void Enter()
    {
        ai.FallBack();
        cooldownEndTime = Time.time + ai.attackCooldown;
        Debug.Log("Fallback");
    }

    public void Tick()
    {
        if (Time.time < cooldownEndTime) return;

        // Cooldown is over, decide next state
        if (ai.CanDetectPlayer())
        {
            // If still close, loop back to Attack; otherwise, Chase
            if (ai.InAttackRange())
                ai.sm.ChangeState(new AttackState(ai));
            else
                ai.sm.ChangeState(new ChaseState(ai));
        }
        else
        {
            // Player was lost during attack or fallback; go to Investigate last seen position
            ai.sm.ChangeState(new InvestigateState(ai));
        }
    }

    public void Exit()
    {
        ai.agent.isStopped = false;
        Debug.Log("Fallback over");
    }
}
