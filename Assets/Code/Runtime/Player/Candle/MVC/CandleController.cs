using System;
using System.Collections.Generic;
using Player.Candle.Stats;
using UnityEngine;
using UnityUtils;

namespace Player.Candle
{
    [RequireComponent(typeof(CandleModel))]
    [RequireComponent(typeof(CandleView))]
    public class CandleController : Singleton<CandleController>, IFire 
    {
        //Inspector References
        [Header("References")]
        [SerializeField] private CandleStats stats;
        [SerializeField] private InputReader input;
        private CandleModel _model;
        private CandleView _view;
        #region UnityMethods
        protected override void Awake()
        {
            base.Awake();
            if(!TryGetComponent(out _model)) Debug.LogError("No Model");
            if(!TryGetComponent(out _view)) Debug.LogError("No View");
            if(!stats || !input ) Debug.LogError("Missing Candle References");
        }
        private void Start()
        {
            _model.SetUp(stats);
            _view.SetUp(stats);
            StateEvents[FlameType.Off].Invoke();
        }
        private void OnEnable()
        {
            input.Run += Increase;
            // EventManager.Subscribe(Events.GameResume,GameResume);
            // EventManager.Subscribe(Events.GamePause,GamePause);
            // EventManager.Subscribe(Events.PlayerDie,OnPlayerDie);
            // EventManager.Subscribe(Events.LevelReset,LevelReset);
            //     _flameType = FlameType.Normal;
        //     _model.ForceFlameToNormal();
        //     _view.UpdateFlame(1, _flameType);
        //
        //     // _input.Reduce += Decrease;
        //     EventManager.Subscribe(Events.LevelReset,ResetCandle);
        }

#if UNITY_EDITOR
        public bool onDebugMode;
        public FlameType type;
        public float flamemultitest;
        private void OnValidate()
        {
            if(!onDebugMode)return;
            if(!TryGetComponent(out _model)) Debug.LogError("No Model");
            if(!TryGetComponent(out _view)) Debug.LogError("No View");
            _model.ForceFlameTo(type);
            _view.UpdateFlame(flamemultitest,type);
        }
#endif
        private void OnDisable()
        {
            input.Run -= Increase;
            // EventManager.UnSubscribe(Events.GameResume,GameResume);
            // EventManager.UnSubscribe(Events.GamePause,GamePause);
            // EventManager.UnSubscribe(Events.PlayerDie,OnPlayerDie);
            // EventManager.UnSubscribe(Events.LevelReset,LevelReset);
            //     // _input.Reduce -= Decrease;
        //     EventManager.UnSubscribe(Events.LevelReset,ResetCandle);
        }
        private void OnDestroy() => DisposeFlameEvents();
        
        private void Update()
        {
            if(_pause) return;
            UpdateFlame();
            UpdateWax();
        //     IsFireOn = IsFireOn;
        //     // UpdateSuffocation();
        }
        private void LevelReset(object[] parameter)
        {
            deathByWax();
        }

        private Action deathByWax;
        private void OnPlayerDie(object[] death)
        {
            // var type = (PlayerDeath)death[0];
            // if (type.Equals(PlayerDeath.OutOfWax))
            // {
            //     deathByWax = delegate
            //     {
            //         AddWax(stats.WaxAmount);
            //     };
            // }
            // else
            // {
            //     deathByWax = delegate { };
            // }
        }

        private void OnParticleCollision(GameObject other)
        {
            TurnFireOff();
        }

        #endregion
        
        #region Pause

        private bool _pause;
        private void GamePause(object[] parameter) => _pause = true;
        private void GameResume(object[] parameter) => _pause = false;

        #endregion

        #region Commented old code
            //if (isIncrease)
            //{
            //    State = FlameType.Increase;
            //    //ChangeFlame(FlameType.Normal, FlameType.Increase);
            //}
            //else
            //{
            //    State = FlameType.Normal;
            //    //ChangeFlame(FlameType.Increase, FlameType.Normal);
            //}
        // private void Update()
        // {
        //     if(_pause) return;
        //     IsFireOn = IsFireOn;
        //     // UpdateFlame();
        //     // UpdateSuffocation();
        //     // UpdateWax();
        //
        // }
        // #endregion
        //
        // #region Candle Update
        //
        // // private bool _suffocated;
        // // private bool _playerReset;
        // // private float _resetValue;
        // // private void UpdateSuffocation()
        // // {
        // //     if (_suffocated)
        // //     {
        // //         if (_playerReset)
        // //         {
        // //             _resetValue = Mathf.Clamp01(_resetValue - Time.deltaTime);
        // //             _view.UpdateSuffocation(_resetValue);
        // //             _suffocated = _resetValue > 0;
        // //         }
        // //     }
        // //     else
        // //     {
        // //         var suffocationPercent = _model.UpdateSuffocation();
        // //         _view.UpdateSuffocation(suffocationPercent);
        // //         if (suffocationPercent >= 1)
        // //         {
        // //         EventManager.Trigger(Events.PlayerDie,PlayerDeath.suffocation);
        // //         _suffocated = true;
        // //         _playerReset = false;
        // //         _resetValue = 1;
        // //         }
        // //     }
        // //     
        // // }
        //
        //

        //
        // private void UpdateWax()
        // {
        //     _model.ConsumeWax();
        //     var waxAmount = _model.UpdateWax();
        //     if (_model.WaxPercent <= waxpercentToApply && !canPlayFullWax) canPlayFullWax = true;
        //     _view.UpdateWax(waxAmount,_model.WaxPercent);
        // }
        //
        // #endregion
        //
        // #region Input Handler
        //
        // private FlameType _flameType = FlameType.Normal;
        // public void Increase(bool isIncrease)
        // {
        //     if (isIncrease)
        //     {
        //         ChangeFlame(FlameType.Normal,FlameType.Increace);
        //     }
        //     else
        //     {
        //         ChangeFlame(FlameType.Increace,FlameType.Normal);
        //     }
        // }
        //
        //
        // public void Decreace(bool isDecreace)
        // {
        //     if (isDecreace)
        //     {
        //         ChangeFlame(FlameType.Normal,FlameType.Decreace);
        //     }
        //     else
        //     {
        //         ChangeFlame(FlameType.Decreace,FlameType.Normal);
        //     }
        // }
        // private void ChangeFlame(FlameType from, FlameType goToType)
        // {
        //     if (_flameType == goToType) return;
        //     if (goToType == FlameType.Increace && _flameType == FlameType.Decreace) return;
        //     if (goToType == FlameType.Decreace && _flameType == FlameType.Increace) return;
        //     if (goToType == FlameType.Normal && _flameType != from) return;
        //     _flameType = goToType;
        // }
        // #endregion
        //
        // #region Candle Handler
        //
        // public void StopConsuming(bool enable) =>_model.StopConsuming(enable);
        // public void ForceStopConsuming(bool enable) =>  _model.ForceStopConsuming( enable);
        // [Range(0.75f,1f)]public float waxpercentToApply;
        //         private bool canPlayFullWax; esta 
        // public void AddWax(float amount) esta 
        // {
        //     _model.AddWax(amount);
        //     if(_model.WaxPercent >= 1 && canPlayFullWax)
        //     {
        //         _view.FullWax();
        //         canPlayFullWax = false;
        //     }
        // }
        // public void RemoveWax(float amount) => _model.RemoveWax(amount); esta
        //
        // public void ResetCandle(params object[] objs)   NO ESTA
        // {
        //     // AddWax(_stats.WaxAmount);
        //     _model.ResetLevel();
        //     _flameType = FlameType.Normal;
        //     // _playerReset = true;
        //     _model.ResetSuffocation();
        // }
        //
        // public float GetWaxPercent() esta
        // {
        //     return _model.WaxPercent;
        // }
        // public bool IsWaxFull()  esta
        // {
        //     return _model.IsWaxFull();
        // }
        //
        #endregion

        #region Flame State Observers

        private static readonly Dictionary<FlameType, Action> StateEvents = new()
        {
            {FlameType.Off,delegate{}},
            {FlameType.Normal,delegate{}},
            {FlameType.Increase,delegate{}}
        };
        public static event Action<bool> OnFlameTurnOn = delegate{};
        
        private FlameType _state = FlameType.Off;
        private FlameType State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;
                if(StateEvents.TryGetValue(_state, out var @event)) @event();
            }
        }

        public static void AddFlameEvents(FlameType state, Action actEvent)
        {
            if (!StateEvents.TryAdd(state, actEvent))
            {
                StateEvents[state] += actEvent;
            }
        }
        public static void RemoveFlameEvents(FlameType state, Action actEvent)
        {
            if (StateEvents.ContainsKey(state))
            {
                StateEvents[state] -= actEvent;
            }
        }
        private static void DisposeFlameEvents()
        {
            StateEvents[FlameType.Normal] = delegate { };
            StateEvents[FlameType.Increase] = delegate { };
            StateEvents[FlameType.Off] = delegate { };
        }
        

        #endregion
        
        #region Fire Handle

         private bool _fireOn;
         private bool _canChangeFire = true;
         
         /// <summary>
         /// Do not Change Directly outside Candle Controller
         /// Use Instead IFire Methods  ( TurnFireOn , TurnFireOff)
         /// </summary>
         public bool IsFireOn
         {
             get => _fireOn;
             private set
             {
                 if (!_canChangeFire || value == _fireOn) return;
                 OnFlameTurnOn.Invoke(value);
                 State = value switch
                 {
                     true when _state.Equals(FlameType.Off) => FlameType.Normal,
                     false when _state is FlameType.Normal or FlameType.Increase => FlameType.Off,
                     _ => State
                 };
                 _fireOn = value;
             }
         }
         public void TurnFireOn() => IsFireOn = true;
         public void TurnFireOff() => IsFireOn = false;
         private void Increase(bool isIncrease)
         {
             if (!_fireOn) return;
             State = isIncrease ? FlameType.Increase : FlameType.Normal;
         }
         private void UpdateFlame()
         {
             var flameMulti = _model.FlameUpdate();
             _view.UpdateFlame(flameMulti, State);
         }

         #endregion

        #region Wax Handle
        [Range(0.75f,1f)]public float waxpercentToApply;
        private bool _canPlayFullWax;
        private void UpdateWax()
        {
            _model.OnConsumeWax();
            if (_model.WaxPercent <= waxpercentToApply && !_canPlayFullWax) _canPlayFullWax = true;
            _view.UpdateWax(_model.WaxPercent);
        }
        public void AddWax(float amount)
        {
            _model.AddWax(amount);
            if (!(_model.WaxPercent >= 1) || !_canPlayFullWax) return;
            _view.FullWax();
            _canPlayFullWax = false;
        }
        public void RemoveWax(float amount) => _model.RemoveWax(amount);
        
        public float GetWaxPercent() => _model.WaxPercent;
        public bool IsWaxFull() => _model.IsWaxFull();
        public void StopConsuming(bool stop) => _model.StopConsuming(stop);
        
        #endregion

        #region Cheats

        // Cheats
        public void ForceStopConsuming(bool enable) =>  _model.ForceStopConsuming( enable);
        public void ForceFlame(bool on) => IsFireOn = on;
        public void ForceChangeFlame(bool canChangeFlame) => _canChangeFire = canChangeFlame;

        public void ForceFlameType(FlameType type)
        {
            switch (type)
            {
                case FlameType.Off:
                    if(_fireOn)TurnFireOff();
                    State = FlameType.Off;
                    break;
                case FlameType.Normal:
                    if(!_fireOn)TurnFireOn();
                    State = FlameType.Normal;
                    break;
                case FlameType.Increase:
                    if(!_fireOn)TurnFireOn();
                    State = FlameType.Increase;
                    break;
            }
        }
        

        #endregion
    } // class
}// namespace
public enum FlameType { Off,Normal,Increase}