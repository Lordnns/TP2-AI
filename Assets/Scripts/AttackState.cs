using UnityEngine;

public class AttackState : IState
{
    GuardAI ai;
    float nextAttackTime;

    public AttackState(GuardAI ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.agent.isStopped = true;
        ai.SetAttack(true);

        nextAttackTime = Time.time;
    }

    public void Tick()
    {
        // Update mémoire si proche
        if (ai.CanDetectPlayer())
        {
            ai.UpdateLastSeen();
        }

        // Sortie stable Attack -> Chase (hystérésis)
        float d = ai.DistanceToPlayer();
        if (d > ai.attackRange + ai.hysteresis)
        {
            ai.sm.ChangeState(new ChaseState(ai));
            return;
        }

        // Cooldown d'attaque
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + ai.attackCooldown;
            ai.PerformAttack();
        }
    }

    public void Exit()
    {
        ai.SetAttack(false);
        ai.agent.isStopped = false;
    }
}
