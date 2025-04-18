using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using Event_Bus;

namespace UnityUtils
{
    public interface ISceneRequiredSingleton { }

    [InitializeOnLoad]
    public static class SceneSingletonVerifier
    {
        private const string ENABLED_PREF = "SingletonVerifier.Enabled";
        private const string TYPES_PREF = "SingletonVerifier.TypeNames";

        private static bool _enabled;
        private static readonly List<string> _typeNames = new();
        private static readonly Dictionary<string, MonoBehaviour> _references = new();

        static SceneSingletonVerifier()
        {
            LoadPrefs();
            EditorSceneManager.sceneSaving += OnSceneSaving;
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

        public static IEnumerable<string> GetTypeNames() => _typeNames;

        public static MonoBehaviour GetReference(string typeName)
        {
            _references.TryGetValue(typeName, out var refObj);
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
                if (!_typeNames.Contains(newType))
                {
                    _typeNames.Add(newType);
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
            foreach (var typeName in _typeNames)
            {
                Type type = Type.GetType(typeName);
                if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

                if (GameObject.FindObjectOfType(type) is MonoBehaviour existing)
                {
                    _references[typeName] = existing;
                    continue;
                }

                GameObject go = new($"{type.Name} Auto-Generated");
                var component = go.AddComponent(type) as MonoBehaviour;
                _references[typeName] = component;

                Debug.Log($"[SceneSingletonVerifier] Created singleton: {type.Name}");
            }
        }

        private static void UpdateReferences()
        {
            _references.Clear();

            foreach (var typeName in _typeNames)
            {
                Type type = Type.GetType(typeName);
                if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

                var found = GameObject.FindObjectOfType(type) as MonoBehaviour;
                if (found != null)
                    _references[typeName] = found;
            }
        }

        private static void LoadPrefs()
        {
            _enabled = EditorPrefs.GetBool(ENABLED_PREF, true);
            string joined = EditorPrefs.GetString(TYPES_PREF, string.Empty);
            _typeNames.Clear();

            if (!string.IsNullOrEmpty(joined))
                _typeNames.AddRange(joined.Split('|'));

            UpdateReferences();
        }

        private static void SavePrefs()
        {
            string joined = string.Join("|", _typeNames);
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
}
