using UnityEngine;

public class AttackState : IState
{
    GuardAI ai;
    float stateEndTime;
    float animationDuration = 0.16f;

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
        Vector3 lookTarget;
        
        if (ai.CanDetectPlayer())
        {
            ai.UpdateLastSeen();
            lookTarget = ai.player.position;
        }
        else
        {
            lookTarget = ai.lastSeenPosition;
        }
        
        RotateTowards(lookTarget);

        // 5. Wait for the animation to end
        if (Time.time >= stateEndTime)
        {
            // Transition to FallBack/Recover as required by the TP [cite: 37, 55, 60]
            ai.sm.ChangeState(new FallBackState(ai));
        }
    }
    
    private void RotateTowards(Vector3 target)
    {
        Vector3 direction = (target - ai.transform.position).normalized;
        direction.y = 0; // Keep the guard upright

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // High rotation speed to ensure they face the target during the short attack
            ai.transform.rotation = Quaternion.RotateTowards(
                ai.transform.rotation, 
                targetRotation, 
                720f * Time.deltaTime
            );
        }
    }

    public void Exit()
    {
        ai.SetAttack(false);
    }
}
