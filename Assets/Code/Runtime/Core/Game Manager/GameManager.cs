using DebugSystem;
using Event_Bus;
using UnityUtils;

namespace Core
{
    
    public class GameManager : PersistentSingleton<GameManager> , ISceneRequiredSingleton
    {
        private const EGameState _initialState = EGameState.Booting;

        private EGameState CurrentState { get; set; }
        protected override void Awake()
        {
            base.Awake();
            SetState(_initialState);
            
            Debugger.Log("debug",DebugUserId.Luca);
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
    
    public enum EGameState 
    {
        Booting,
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Cinematic
    }
    public class GameStateEvent : IEvent
    {
        public EGameState NewState;
        public EGameState PreviousState;

        public GameStateEvent(EGameState from, EGameState to)
        {
            PreviousState = from;
            NewState = to;
        }
    }
}
