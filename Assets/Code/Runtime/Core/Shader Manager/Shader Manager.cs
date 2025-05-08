using System;
using System.Threading.Tasks;
using DebugSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityUtils;

namespace Core
{
    public class ShaderManager : PersistentSingleton<ShaderManager> , ISceneRequiredSingleton
    {
        protected override void Awake()
        {
            base.Awake();
            CheckReferences();
        }

        private void CheckReferences()
        {
           if(!blackScreenShader) Debugger.Error("Black screen Material not assigned", DebugUserId.Important);
        }

        #region Black Screen

        [SerializeField] private Material blackScreenShader;
        private static readonly int BlackscreenHash = Shader.PropertyToID("_FillAmount");
        public int BlackScreenFillAmount => blackScreenShader.GetInt(BlackscreenHash);
        public async void FadeIn(float duration, Action onFadeEnd = null)
        {
            while (duration > 0)
            {
                duration -= Time.deltaTime;
                float alpha = Mathf.Clamp01(duration / duration);
                blackScreenShader.SetFloat(BlackscreenHash, alpha);
                await Task.Yield();
            }
            onFadeEnd?.Invoke();
        }
        public async void FadeOut(float duration, Action onFadeEnd = null)
        {
            while (duration > 0)
            {
                duration -= Time.deltaTime;
                var alpha = 1f - Mathf.Clamp01(duration / duration);
                blackScreenShader.SetFloat(BlackscreenHash, alpha);
                await Task.Yield();
            }
            onFadeEnd?.Invoke();
        }
        #endregion
    }
}
