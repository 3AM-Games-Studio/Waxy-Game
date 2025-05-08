using DebugSystem;
using Event_Bus;
using UnityEngine;
using UnityUtils;

namespace Core
{
    
    public class GameManager : PersistentSingleton<GameManager> , ISceneRequiredSingleton
    {
        private const EGameState InitialState = EGameState.Booting;

        private EGameState CurrentState { get; set; }
        protected override void Awake()
        {
            base.Awake();
            SetState(InitialState);
        }

       
        public static void SetState(EGameState newState)
        {
            if (IsState(newState)) return;

            var previous = Instance.CurrentState;
            Instance.CurrentState = newState;

            Debugger.Log($"[GameManager] State changed: {previous} → {newState}", DebugUserId.Luca);

            EventBus<GameStateEvent>.Raise(new GameStateEvent(previous, newState));
        }

        public static bool IsState(EGameState state) => instance.CurrentState == state;

        private static bool CanPause()
        {
            return IsState(EGameState.Playing);
        }

        public void TryPause()
        {
            if (CanPause())
                SetState(EGameState.Paused);
        }

        public void TryResume()
        {
            if (IsState(EGameState.Paused))
                SetState(EGameState.Playing);
        }
        
    }
}
