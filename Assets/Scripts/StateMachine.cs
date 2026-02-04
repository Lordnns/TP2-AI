using UnityEngine;

public class StateMachine
{
    public IState CurrentState;

    public void ChangeState(IState nextState)
    {
        if (nextState == null)
        {
            Debug.LogWarning("ChangeState: nextState is null");
            return;
        }

        // Quitter l'état courant si existant
        if (CurrentState != null)
        {
            CurrentState.Exit();
        }

        // Passer au nouvel état
        CurrentState = nextState;

        // Entrer dans le nouvel état
        CurrentState.Enter();
    }

    public void Tick()
    {
        if (CurrentState != null)
        {
            CurrentState.Tick();
        }
    }
}
