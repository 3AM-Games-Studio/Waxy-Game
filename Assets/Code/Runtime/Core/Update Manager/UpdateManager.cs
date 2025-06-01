    using System;
    using Event_Bus;
    using UnityUtils;

    namespace Core
{
    public class UpdateManager : PersistentSingleton<UpdateManager> , ISceneRequiredSingleton
    {
        
        private static event Action _update;
        private static event Action _updateUnpaused;
        private static event Action _fixedUpdate;
        private static event Action _fixedUpdateUnpaused;
        private static event Action _lateUpdate;
        private static event Action _lateUpdateUnpaused;

        public bool IsPaused { get; private set;}
        
        
        private EventBinding<GameStateEvent> _binding;
        private void OnGameStateChanged(GameStateEvent e) => IsPaused = e.NewState == EGameState.Paused;

        protected override void Awake()
        {
            UnregisterAll();
            base.Awake();
            IsPaused = false;
        }

        private void OnDestroy()
        {
            UnregisterAll();
        }

        void OnEnable()
        {
            _binding = new EventBinding<GameStateEvent>(OnGameStateChanged);
            EventBus<GameStateEvent>.Register(_binding);
        }

        void OnDisable()
        {
            EventBus<GameStateEvent>.Deregister(_binding);
        }

        private void Update()
        {
            if (!IsPaused) _update?.Invoke();
            _updateUnpaused?.Invoke();
        }

        private void FixedUpdate()
        {
            if (!IsPaused) _fixedUpdate?.Invoke();
            _fixedUpdateUnpaused?.Invoke();
        }

        private void LateUpdate()
        {
            if (!IsPaused) _lateUpdate?.Invoke();
            _lateUpdateUnpaused?.Invoke();
        }

        // ---- Métodos de registro separados ----
        public static void RegisterToUpdate(IUpdateReceiver receiver, bool ignorePause = false)
        {
#pragma warning disable UDR0004
            if (ignorePause) _updateUnpaused += receiver.OnUpdate;
            else _update += receiver.OnUpdate;
#pragma warning restore UDR0004
        }

        public static void UnregisterFromUpdate(IUpdateReceiver receiver)
        {
            _update -= receiver.OnUpdate;
            _updateUnpaused -= receiver.OnUpdate;
        }

        public static void RegisterToFixedUpdate(IUpdateReceiver receiver, bool ignorePause = false)
        {
#pragma warning disable UDR0004
            if (ignorePause) _fixedUpdateUnpaused += receiver.OnFixedUpdate;
            else _fixedUpdate += receiver.OnFixedUpdate;
#pragma warning restore UDR0004
        }

        public static void UnregisterFromFixedUpdate(IUpdateReceiver receiver)
        {
            _fixedUpdate -= receiver.OnFixedUpdate;
            _fixedUpdateUnpaused -= receiver.OnFixedUpdate;
        }

        public static void RegisterToLateUpdate(IUpdateReceiver receiver, bool ignorePause = false)
        {
#pragma warning disable UDR0004
            if (ignorePause) _lateUpdateUnpaused += receiver.OnLateUpdate;
            else _lateUpdate += receiver.OnLateUpdate;
#pragma warning restore UDR0004
        }

        public static void UnregisterFromLateUpdate(IUpdateReceiver receiver)
        {
            _lateUpdate -= receiver.OnLateUpdate;
            _lateUpdateUnpaused -= receiver.OnLateUpdate;
        }

        private static void UnregisterAll()
        {
            UnregisterAllUpdates();
            UnregisterAllFixedUpdates();
            UnregisterAllLateUpdates();
        }
        private static void UnregisterAllUpdates()
        {
            _update = delegate { };
            _updateUnpaused = delegate { };
        }

        private static void UnregisterAllFixedUpdates()
        {
            _fixedUpdate = delegate { };
            _fixedUpdateUnpaused = delegate { };
        }

        private static void UnregisterAllLateUpdates()
        {
            _lateUpdate = delegate { };
            _lateUpdateUnpaused = delegate { };
        }
    }

}
