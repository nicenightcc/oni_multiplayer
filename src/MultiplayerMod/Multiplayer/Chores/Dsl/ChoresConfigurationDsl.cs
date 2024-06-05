using MultiplayerMod.Multiplayer.StateMachines.Configuration;
using MultiplayerMod.Multiplayer.StateMachines.Configuration.Configurers;

namespace MultiplayerMod.Multiplayer.Chores.Dsl;

public static class ChoresConfigurationDsl {

    public static ChoreConfiguration Synchronize<TChore>() => new(typeof(TChore), null);

    public static ChoreConfiguration Synchronize<TChore, TStateMachine, TStateMachineInstance>(
        StateMachineConfigurerDelegate<TStateMachine, TStateMachineInstance, TChore, object> scopedAction
    )
        where TStateMachine : GameStateMachine<TStateMachine, TStateMachineInstance, TChore, object>
        where TStateMachineInstance :
        GameStateMachine<TStateMachine, TStateMachineInstance, TChore, object>.GameInstance
        where TChore : Chore, IStateMachineTarget
    {
        return new ChoreConfiguration(
            typeof(TChore),
            new StateMachineConfigurerDsl<TStateMachine, TStateMachineInstance, TChore>(scopedAction)
        );
    }

    public static ChoreConfiguration Synchronize<TChore, TStateMachine, TStateMachineInstance, TConfigurer>()
        where TStateMachine : GameStateMachine<TStateMachine, TStateMachineInstance, TChore, object>
        where TStateMachineInstance :
        GameStateMachine<TStateMachine, TStateMachineInstance, TChore, object>.GameInstance
        where TChore : Chore, IStateMachineTarget
        where TConfigurer : StateMachineBoundedConfigurer<TStateMachine, TStateMachineInstance, TChore, object>
    {
        return new ChoreConfiguration(
            typeof(TChore),
            new StateMachineConfigurer<TStateMachine, TStateMachineInstance, TChore, object, TConfigurer>()
        );
    }

}
