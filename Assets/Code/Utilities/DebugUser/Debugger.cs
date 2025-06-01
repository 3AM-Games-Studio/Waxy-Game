using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DebugSystem
{
    public static class Debugger
    {
        private static readonly HashSet<DebugUserId> activeUsers = new HashSet<DebugUserId>();
        private const string PREF_KEY = "DebugUser.SelectedUsers";
#pragma warning disable UDR0001
        private static bool _initialized = false;
#pragma warning restore UDR0001

        private static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            activeUsers.Clear();

#if UNITY_EDITOR
            string raw = UnityEditor.EditorPrefs.GetString(PREF_KEY, DebugUserId.Luca.ToString()); // default: Luca
            var split = raw.Split(',');

            foreach (string val in split)
            {
                if (System.Enum.TryParse(val, out DebugUserId user))
                    activeUsers.Add(user);
            }
#else
            // En build: por defecto mostrar todo
            foreach (DebugUserId user in System.Enum.GetValues(typeof(DebugUserId)))
                activeUsers.Add(user);
#endif
        }

        public static void SetActiveUsers(IEnumerable<DebugUserId> users)
        {
#if UNITY_EDITOR
            activeUsers.Clear();
            foreach (var user in users)
                activeUsers.Add(user);

            string serialized = string.Join(",", activeUsers.Select(u => u.ToString()));
            UnityEditor.EditorPrefs.SetString(PREF_KEY, serialized);
#endif
        }

        public static bool IsUserActive(DebugUserId user)
        {
            Initialize();
            return activeUsers.Contains(user);
        }

        public static IEnumerable<DebugUserId> GetActiveUsers()
        {
            Initialize();
            return activeUsers.ToArray();
        }

        public static void Log(string message, DebugUserId user)
        {
            if (!IsUserActive(user)) return;
            Debug.Log($"<color=cyan>[{user}]</color> {message}");
        }

        public static void Warning(string message, DebugUserId user)
        {
            if (!IsUserActive(user)) return;
            Debug.LogWarning($"<color=yellow>[{user}]</color> {message}");
        }

        public static void Error(string message, DebugUserId user)
        {
            if (!IsUserActive(user)) return;
            Debug.LogError($"<color=red>[{user}]</color> {message}");
        }
    }
}
