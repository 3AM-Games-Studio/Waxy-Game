using UnityEditor;
using System.Collections.Generic;
using DebugSystem;

namespace DebugSystem
{
    
    static class DebugUserSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Preferences/Debug Logs", SettingsScope.User)
            {
                label = "Debug Logs",
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.LabelField("Active Debug Users", EditorStyles.boldLabel);

                    var current = new HashSet<DebugUserId>(Debugger.GetActiveUsers());
                    bool changed = false;

                    foreach (DebugUserId user in System.Enum.GetValues(typeof(DebugUserId)))
                    {
                        bool oldValue = current.Contains(user);
                        bool newValue = EditorGUILayout.ToggleLeft(user.ToString(), oldValue);

                        if (newValue && !oldValue)
                        {
                            current.Add(user);
                            changed = true;
                        }
                        else if (!newValue && oldValue)
                        {
                            current.Remove(user);
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        Debugger.SetActiveUsers(current);
                    }
                },

                keywords = new HashSet<string>(new[] { "debug", "log", "user", "filter" })
            };

            return provider;
        }
    }
}
