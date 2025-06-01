using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using Event_Bus;
using Object = UnityEngine.Object;

namespace UnityUtils
{
    public interface ISceneRequiredSingleton { }

    #if UNITY_EDITOR
    
    [InitializeOnLoad]
    public static class SceneSingletonVerifier
    {
        private const string ENABLED_PREF = "SingletonVerifier.Enabled";
        private const string TYPES_PREF = "SingletonVerifier.TypeNames";
        
        
#pragma warning disable UDR0001
        private static bool _enabled;
#pragma warning restore UDR0001
        private static readonly List<string> TypeNames = new();
        private static readonly Dictionary<string, MonoBehaviour> References = new();

        static SceneSingletonVerifier()
        {
            LoadPrefs();
#pragma warning disable UDR0001
            EditorSceneManager.sceneSaving += OnSceneSaving;
#pragma warning restore UDR0001
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            if (!_enabled) return;

            UpdateReferences();
            EnsureSingletons();
        }

        public static bool IsEnabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                EditorPrefs.SetBool(ENABLED_PREF, _enabled);
            }
        }

        public static IEnumerable<string> GetTypeNames() => TypeNames;

        public static MonoBehaviour GetReference(string typeName)
        {
            References.TryGetValue(typeName, out var refObj);
            return refObj;
        }

        public static void RecheckTypes()
        {
            if (!_enabled)
                return;

            var discoveredTypes = PredefinedAssemblyUtil
                .GetTypes(typeof(ISceneRequiredSingleton))
                .Where(IsPersistentSingleton)
                .Select(t => t.FullName)
                .ToList();

            bool anyNew = false;

            foreach (string newType in discoveredTypes)
            {
                if (!TypeNames.Contains(newType))
                {
                    TypeNames.Add(newType);
                    anyNew = true;
                }
            }

            if (anyNew)
            {
                SavePrefs();
                UpdateReferences();
            }
            else
            {
                OnSceneSaving(default, default);
            }
        }

        private static void EnsureSingletons()
        {
            foreach (var typeName in TypeNames)
            {
                Type type = Type.GetType(typeName);
                if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

                if (Object.FindFirstObjectByType(type) is MonoBehaviour existing)
                {
                    References[typeName] = existing;
                    continue;
                }

                GameObject go = new($"{type.Name} Auto-Generated");
                var component = go.AddComponent(type) as MonoBehaviour;
                References[typeName] = component;

                Debug.Log($"[SceneSingletonVerifier] Created singleton: {type.Name}");
            }
        }

        private static void UpdateReferences()
        {
            References.Clear();

            foreach (var typeName in TypeNames)
            {
                Type type = Type.GetType(typeName);
                if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

                var found = Object.FindFirstObjectByType(type) as MonoBehaviour;
                if (found != null)
                    References[typeName] = found;
            }
        }

        private static void LoadPrefs()
        {
            _enabled = EditorPrefs.GetBool(ENABLED_PREF, true);
            string joined = EditorPrefs.GetString(TYPES_PREF, string.Empty);
            TypeNames.Clear();

            if (!string.IsNullOrEmpty(joined))
                TypeNames.AddRange(joined.Split('|'));

            UpdateReferences();
        }

        private static void SavePrefs()
        {
            string joined = string.Join("|", TypeNames);
            EditorPrefs.SetString(TYPES_PREF, joined);
        }

        private static bool IsPersistentSingleton(Type type)
        {
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(PersistentSingleton<>))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
#endif
}
