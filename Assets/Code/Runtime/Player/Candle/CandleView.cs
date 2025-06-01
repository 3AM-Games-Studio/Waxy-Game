using System;
using Player.Candle.Stats;
using UnityEngine;

namespace Player.Candle
{
    public class CandleView : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _candleMesh;
        [SerializeField] private SkinnedMeshRenderer _topCandleMesh;
        [SerializeField] private Material _flameMat;
        [SerializeField] private ParticleSystem _flameTrail;
        [SerializeField] private ParticleSystem _fullWaxParticle;
        [SerializeField] private ParticleSystem _smokeParticles;
        [SerializeField] private ParticleSystem _extinguishParticles;
        [SerializeField] SkinnedMeshRenderer leftEyebrow;
        [SerializeField] SkinnedMeshRenderer rightEyebrow;
        [SerializeField, Range(0, 1f)] private float percentToHideEyebrow;
        private float eyebrowPercent;

        // [SerializeField] private Material _suffocationMat;
        private readonly int flameHash = Shader.PropertyToID("_LifeScalar");
        private readonly int waxHash = Shader.PropertyToID("_WaxPercent");
        private readonly int suffocationeHash = Shader.PropertyToID("_FillAmount");

        [Header("Sounds")]
        public AudioSource flameAudio;

        private void Awake()
        {
            if(!_candleMesh) Debug.LogError("Candle Mesh Reference Not Set");
            if (!_topCandleMesh) Debug.LogError("Top Candle Mesh Reference Not Set");
            if (!_flameMat) Debug.LogError("Candle flame Material Reference Not Set");
            if (!_flameTrail || !_fullWaxParticle || !_smokeParticles) Debug.LogError("Particles not assigned");
        }

        private void OnDestroy()
        {
            _flameMat.SetFloat(flameHash,1);
        }

        public void SetUp(CandleStats stats)
        {
            CandleController.AddFlameEvents(FlameType.Off, delegate
            {
                _flameTrail.Stop();
                flameAudio.Stop();
                _smokeParticles.Stop();
                _extinguishParticles.Play();
                // AudioSystem.PlayerAudio(ClipName.ExtinguishFire, transform.position, true);
            });
            CandleController.AddFlameEvents(FlameType.Normal,delegate
            {
                _flameTrail.Play();
                flameAudio.Play();
                _smokeParticles.Stop();
            });
            CandleController.AddFlameEvents(FlameType.Increase,_smokeParticles.Play);
            if (stats.StartingFlame.Equals(FlameType.Off))
            {
                _flameTrail.Stop();
                flameAudio.Stop();
            }
            else
            {
                _flameTrail.Play();
                flameAudio.Play();
            }
        }
        #region Candle Update

        public void UpdateFlame(float flameMulti, FlameType flameState)
        {
            _flameMat.SetFloat(flameHash,flameMulti);
            switch (flameMulti)
            {
                case >= 0.5f when !_flameTrail.isEmitting:
                    _flameTrail.Play();
                    break;
                case < 0.5f when _flameTrail.isEmitting:
                    _flameTrail.Stop();
                    break;
            }
            // if(flameState.Equals(FlameType.Off) && _flameTrail.isEmitting)
            // {
            //     _flameTrail.Stop();
            //     flameAudio.Stop();
            // }
            // else if(!flameState.Equals(FlameType.Off) && !_flameTrail.isEmitting)
            // {
            //     _flameTrail.Play();
            //     flameAudio.Play();              
            // }
            //
            // if (!flameState.Equals(FlameType.Off))
            // {
            //     //FlameOffMusic.Instance.LowPassSoundOff();
            // }
        }

        
        // public void UpdateSuffocation(float suffocationPercent)
        // {
        //     _suffocationMat.SetFloat(suffocationeHash,suffocationPercent);
        // }
        public void UpdateWax(float waxPercent)
        {
            var value = 100 - (100 * waxPercent);
            _candleMesh.SetBlendShapeWeight(0,value);
            _topCandleMesh.SetBlendShapeWeight(0,value);

            if (waxPercent < percentToHideEyebrow)
            {
                eyebrowPercent = Mathf.InverseLerp(percentToHideEyebrow, 0, waxPercent) * 100;
                UpdateEyebrows();
            }
            else if (eyebrowPercent > 0)
            {
                eyebrowPercent = 0;
                UpdateEyebrows();
            }

            if (waxPercent < 0.3f)
            {
                var t=Mathf.InverseLerp(-0.1f,0.3f,waxPercent);
                _flameMat.SetFloat(waxHash, t);
                if (waxPercent < 0.01f)
                    _flameMat.SetFloat(waxHash, 0);
            }
            else
                _flameMat.SetFloat(waxHash, 1);
            
        }

        private void UpdateEyebrows()
        {
            leftEyebrow.SetBlendShapeWeight(0, eyebrowPercent);
            rightEyebrow.SetBlendShapeWeight(0, eyebrowPercent);
        }

        public void FullWax()
        {
            if(!_fullWaxParticle.isEmitting)_fullWaxParticle.Play();
        }
        #endregion
    }
}
