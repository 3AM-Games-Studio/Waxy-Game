using System;
using Player.Candle.Stats;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Candle
{
    public class CandleModel : MonoBehaviour
    {
        [Header("Events")] 
        // [SerializeField] private GameEventFloat onWaxPercent;
        // [SerializeField] private GameEventFloat flame;
        
        [Space, Header("Object Movement")] 
        [SerializeField] private Vector3 startPos;
        [SerializeField] private Vector3 endPos;

        [Space, Header("Collider Movement")] 
        [SerializeField] private float normalRadius;
        [SerializeField] private float increaseRadius;
        [SerializeField] private Vector3 normalCenter, increaseCenter;
        [SerializeField]private SphereCollider _flameCollider;

        private void Awake()
        {
            // if(!onWaxPercent || !flame) Debug.LogError("Missing Candle Model Events");
            if (TryGetComponent(out _flameCollider))
                _flameCollider.isTrigger = true;
            else Debug.LogError("SphereCollider not set");
        }

        public void SetUp(CandleStats stats)
        {
            _maxWax = stats.WaxAmount;
            _changeFlameMulti = stats.ChangeFlameMulti;
            _wax = stats.StartingWax;
            _normalConsume = stats.ConsumingWaxOnNormal;
            _increaseConsume = stats.ConsumingWaxOnIncrease;
            ForceFlameTo(stats.StartingFlame);
            CandleController.AddFlameEvents(FlameType.Off, delegate
            {
                FlameUpdate = () => ChangeFlame(FlameType.Off);
                OnConsumeWax = delegate { };
            });
            CandleController.AddFlameEvents(FlameType.Normal,delegate
            {
                FlameUpdate = () => ChangeFlame(FlameType.Normal);
                OnConsumeWax = ConsumeWax;
            });
            CandleController.AddFlameEvents(FlameType.Increase,delegate
            {
                FlameUpdate = () => ChangeFlame(FlameType.Increase);
                OnConsumeWax = ConsumeWax;
            });
        }

        
        #region Wax Handle

        // Wax
        private float _wax;
        private float _maxWax;
        public float WaxPercent => _wax / _maxWax;
        private bool _outOfWax;
        
        [Space,Header("Consuming Wax")] 
        [SerializeField] private bool forceStopConsuming = false;
        [SerializeField] private bool stopConsuming = false;
        public bool IsConsuming => !forceStopConsuming && !stopConsuming;
        public void StopConsuming(bool enable) => stopConsuming = enable;
        public void ForceStopConsuming(bool enable) => forceStopConsuming = enable;

        private void UpdateWax()
        {   
            transform.localPosition = Vector3.Lerp(startPos,endPos,1 - WaxPercent);
            // onWaxPercent.Invoke(WaxPercent);
        }
        
        public Action OnConsumeWax = delegate { };
        private void ConsumeWax()
        {
            if(!IsConsuming) return;
            var consumeMulti = _flameMulti > 1 ?  
                Mathf.Lerp(_normalConsume,_increaseConsume,_flameMulti -1) 
                : _flameMulti;
            RemoveWax( (consumeMulti * Time.deltaTime));
            
        }

        public void AddWax(float amount)
        {
             ClampWax(_wax + amount);
             UpdateWax();
             if (_wax > 0) _outOfWax = false;
        }

        public void RemoveWax(float amount)
        {
            ClampWax(_wax - amount);
            UpdateWax();
            if (_outOfWax || !(_wax <= 0)) return;
            _outOfWax = true;
            // EventManager.Trigger(Events.PlayerDie,PlayerDeath.OutOfWax,3f);
        }
        private void ClampWax(float newWaxAmount) => _wax = Mathf.Clamp(newWaxAmount, 0, _maxWax);
        public bool IsWaxFull() => _wax >= _maxWax - .5f;

        #endregion

        #region Flame Handle
        
        //Constants
        private const float FlameThreshold = 0.05F;
        private const float FlameLow = 0F;
        private const float FlameNormal = 1f;
        private const float FlameHigh = 2F;
        
        // flame
        private float _flameMulti;
        private float _changeFlameMulti;
        private FlameType _flameType;
        
        private float _normalConsume;
        private float _increaseConsume;
        public  Func<float> FlameUpdate;
        private float DefaultFlameUpdate() => _flameMulti;
        private float ChangeFlame(FlameType flameType)
        {
            switch (flameType)
            {
                case FlameType.Off:
                    if (_flameMulti <= FlameThreshold)
                    {
                        _flameMulti = FlameLow;
                        FlameUpdate = DefaultFlameUpdate;
                    }
                    else
                    {
                        _flameMulti -= _changeFlameMulti * Time.deltaTime;
                        _flameMulti = Mathf.Clamp(_flameMulti, FlameLow, FlameHigh);
                    }
                    break;

                case FlameType.Normal:
                    if (Mathf.Abs(_flameMulti - FlameNormal) <= FlameThreshold)
                    {
                        _flameMulti = FlameNormal;
                        FlameUpdate = DefaultFlameUpdate;
                    }
                    else
                    {
                        if (_flameMulti > FlameNormal)
                            _flameMulti = Mathf.Clamp(_flameMulti - _changeFlameMulti * Time.deltaTime, FlameNormal, FlameHigh);
                        else
                            _flameMulti = Mathf.Clamp01(_flameMulti + _changeFlameMulti * Time.deltaTime);
                    }
                    break;

                case FlameType.Increase:
                    if (_flameMulti >= FlameHigh - FlameThreshold)
                    {
                        _flameMulti = FlameHigh;
                        FlameUpdate = DefaultFlameUpdate;
                    }
                    else
                    {
                        _flameMulti += _changeFlameMulti * Time.deltaTime;
                        _flameMulti = Mathf.Clamp(_flameMulti, FlameLow, FlameHigh);
                    }
                    break;
            }
            _flameType = flameType;
            UpdateCollider();
            // flame.Invoke(_flameMulti);
            return _flameMulti;
        }

        private void UpdateCollider()
        {
            if (_flameMulti >= 1f)
            {
                var t = _flameMulti -1f;
                _flameCollider.radius = Mathf.Lerp(normalRadius, increaseRadius, t);
                _flameCollider.center = Vector3.Lerp(normalCenter,increaseCenter,t);
            }
            // else
            // {
            //     _flameCollider.radius = Mathf.Lerp(decreaseRadius, normalRadius, _flameMulti);
            //     _flameCollider.center = Vector3.Lerp(decreaseCenter,normalCenter,_flameMulti);
            // }
        }

        public void ForceFlameToNormal() => ForceFlameTo(FlameType.Normal);
        public void ForceFlameTo(FlameType type)
        {
            _flameType = type;
            _flameMulti = type switch
            {
                FlameType.Off => 0,
                FlameType.Normal => 1,
                FlameType.Increase => 2,
                _ => 0
            };
            if (!_flameCollider) return;
            _flameCollider.radius = type == FlameType.Increase ? increaseRadius : normalRadius;
            _flameCollider.center = type == FlameType.Increase ?  increaseCenter :  normalCenter;
            FlameUpdate  = DefaultFlameUpdate;
        }
        #endregion
        private void OnTriggerEnter(Collider other)
        {
            // if(other.gameObject.layer == (int) Layers.Flamable)
            // if (other.gameObject.layer != 7) return;
    
            if (!other.gameObject.TryGetComponent(out IFlameable flamable)) return;
            Debug.Log("Entra");
            flamable.Interact(_flameMulti, _flameType);
        }

        private void OnTriggerStay(Collider other)
        {
            // if(other.gameObject.layer == (int) Layers.Flamable)
            if (other.gameObject.layer != 7) return;
            
            if(other.gameObject.TryGetComponent(out IFlameable flamable))
            {
                flamable.Interact(_flameMulti, _flameType);
            }
        }

        
    }
}