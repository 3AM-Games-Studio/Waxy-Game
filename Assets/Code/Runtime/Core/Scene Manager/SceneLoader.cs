using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Core
{
    public static class SceneLoader
    {
        public static event Action<EGameScene> OnSceneLoadStarted;
        public static event Action<EGameScene> OnSceneLoadCompleted;

        private static readonly Dictionary<EGameScene, bool> Scenes = new();
        public static bool IsSceneLoaded(EGameScene scene) => Scenes.TryGetValue(scene, out var active) && active;
        public static IEnumerable<EGameScene> GetActiveScenes() => from pair in Scenes where pair.Value select pair.Key;

        static SceneLoader()
        {
            Initialize();
        }

        private static void Initialize()
        {
            foreach (EGameScene scene in Enum.GetValues(typeof(EGameScene)))
                Scenes[scene] = false;

            Scenes[EGameScene.Boot] = true;
        }

        public static void Load(EGameScene scene)
        {
            if (Scenes.TryGetValue(scene, out var isActive) && isActive) return;

            OnSceneLoadStarted?.Invoke(scene);
            SceneManager.LoadScene((int)scene);
            var keys = new List<EGameScene>(Scenes.Keys);
            foreach (var key in keys)
            {
                Scenes[key] = key == scene;
            }
            OnSceneLoadCompleted?.Invoke(scene);
        }

        public static void LoadAsync(EGameScene scene, bool useLoadingScreen = false)
        {
            if (Scenes.TryGetValue(scene, out var isActive) && isActive) return;

            ShaderManager.Instance.FadeIn(1f, FinishFade);
            return;

            async void FinishFade()
            {
                OnSceneLoadStarted?.Invoke(scene);
                
                var op = SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Single);
                while (!op.isDone)
                    await Task.Yield();

                var keys = new List<EGameScene>(Scenes.Keys);
                foreach (var key in keys)
                {
                    Scenes[key] = key == scene;
                }
                ShaderManager.Instance.FadeOut(1f);
                OnSceneLoadCompleted?.Invoke(scene);
            }
        }

        public static async Task LoadAsyncAdditive(EGameScene scene)
        {
            if (Scenes.TryGetValue(scene, out var isActive) && isActive) return;

            OnSceneLoadStarted?.Invoke(scene);

            var op = SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive);
            while (!op!.isDone)
                await Task.Yield();

            Scenes[scene] = true;

            OnSceneLoadCompleted?.Invoke(scene);
        }

        public static async Task UnloadAsync(EGameScene scene)
        {
            if (!Scenes.TryGetValue(scene, out var isActive) || !isActive) return;

            var op = SceneManager.UnloadSceneAsync((int)scene);
            while (!op!.isDone)
                await Task.Yield();

            Scenes[scene] = false;
        }
    }
}
