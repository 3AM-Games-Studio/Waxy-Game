using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public static class SceneLoader
    {
        public static event Action<EGameScene> OnSceneLoadStarted;
        public static event Action<EGameScene> OnSceneLoadCompleted;

        public static void Load(EGameScene scene)
        {
            OnSceneLoadStarted?.Invoke(scene);
            SceneManager.LoadScene((int)scene);
            OnSceneLoadCompleted?.Invoke(scene);
        }

        public static async Task LoadAsync(EGameScene scene)
        {
            OnSceneLoadStarted?.Invoke(scene);

            AsyncOperation op = SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Single);
            while (!op.isDone)
                await Task.Yield();

            OnSceneLoadCompleted?.Invoke(scene);
        }
        public static async Task LoadAsyncAdditive(EGameScene scene)
        {
            OnSceneLoadStarted?.Invoke(scene);

            AsyncOperation op = SceneManager.LoadSceneAsync((int)scene, LoadSceneMode.Additive);
            while (!op.isDone)
                await Task.Yield();

            OnSceneLoadCompleted?.Invoke(scene);
        }

        public static async Task UnloadAsync(EGameScene scene)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync((int)scene);
            while (!op.isDone)
                await Task.Yield();
        }
    }
}
