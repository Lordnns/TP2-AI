using UnityEngine;

public class FallBackState : IState
{
    GuardAI ai;
    private bool fallback = false;
    
    public FallBackState(GuardAI ai)
    {
        this.ai = ai;
    }
    public void Enter()
    {
        ai.SetAttack(false);
        ai.agent.isStopped = false;
        fallback = true;
        Debug.Log("Fallback");
    }

    public void Tick()
    {
        if(fallback)
            ai.FallBack();
    }

    public void Exit()
    {
        fallback = false;
        Debug.Log("Fallback over");
    }
}
