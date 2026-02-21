using UnityEngine;

public class AttackState : IState
{
    GuardAI ai;
    float stateEndTime;
    float animationDuration = 3.0f;

    public AttackState(GuardAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.agent.isStopped = true;
        ai.SetAttack(true); 
        ai.PerformAttack(); 
        stateEndTime = Time.time + animationDuration;
    }

    public void Tick()
    {
        if (ai.CanDetectPlayer())
        {
            ai.UpdateLastSeen();
        }

        // 5. Wait for the animation to end
        if (Time.time >= stateEndTime)
        {
            // Transition to FallBack/Recover as required by the TP [cite: 37, 55, 60]
            ai.sm.ChangeState(new FallBackState(ai));
        }
    }

    public void Exit()
    {
        ai.SetAttack(false);
    }
}
