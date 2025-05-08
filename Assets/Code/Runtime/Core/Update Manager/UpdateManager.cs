    using System;
    using Event_Bus;
    using UnityUtils;

    namespace Core
{
    public class UpdateManager : PersistentSingleton<UpdateManager> , ISceneRequiredSingleton
    {
        
        private event Action _update;
        private event Action _updateUnpaused;

        private event Action _fixedUpdate;
        private event Action _fixedUpdateUnpaused;

        private event Action _lateUpdate;
        private event Action _lateUpdateUnpaused;

        public bool IsPaused { get; private set;}
        
        
        private EventBinding<GameStateEvent> _binding;
        private void OnGameStateChanged(GameStateEvent e) => IsPaused = e.NewState == EGameState.Paused;

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
        public void RegisterToUpdate(Action method, bool ignorePause = false)
        {
            if (ignorePause) _updateUnpaused += method;
            else _update += method;
        }

        public void UnregisterFromUpdate(Action method)
        {
            _update -= method;
            _updateUnpaused -= method;
        }

        public void RegisterToFixedUpdate(Action method, bool ignorePause = false)
        {
            if (ignorePause) _fixedUpdateUnpaused += method;
            else _fixedUpdate += method;
        }

        public void UnregisterFromFixedUpdate(Action method)
        {
            _fixedUpdate -= method;
            _fixedUpdateUnpaused -= method;
        }

        public void RegisterToLateUpdate(Action method, bool ignorePause = false)
        {
            if (ignorePause) _lateUpdateUnpaused += method;
            else _lateUpdate += method;
        }

        public void UnregisterFromLateUpdate(Action method)
        {
            _lateUpdate -= method;
            _lateUpdateUnpaused -= method;
        }
    }

}
