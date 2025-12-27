using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Core.Scripts.Editor.Utils
{
    public static class EditorUtils
    {
        public static bool IsSymbolEnabled(string defineSymbol)
        {
            foreach (var targetGroup in GetAllBuildTargetGroups())
            {
                var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget).Split(';').ToHashSet();
                if (!defines.Contains(defineSymbol))
                {
                    return false;
                }
            }

            return true;
        }
        
        public static void AddCompileDefine(string define)
        {
            foreach (var targetGroup in GetAllBuildTargetGroups())
            {
                var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget).Split(';').ToHashSet();
                defines.Add(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defines));
            }
        }

        public static void UpdateCompileDefines(List<string> addDefines, List<string> removeDefines)
        {
            foreach (var targetGroup in GetAllBuildTargetGroups())
            {
                var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget).Split(';').ToHashSet();
        
                foreach (var define in addDefines)
                {
                    defines.Add(define);
                }

                foreach (var define in removeDefines)
                {
                    defines.Remove(define);
                }
        
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defines));
            }
        }
        
        public static void RemoveCompileDefine(string define)
        {
            foreach (var targetGroup in GetAllBuildTargetGroups())
            {
                var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                var defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget).Split(';').ToHashSet();
                defines.Remove(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defines));
            }
        }
        
        private static IEnumerable<BuildTargetGroup> GetAllBuildTargetGroups()
        {
            var enumType = typeof(BuildTargetGroup);
            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType);

            for (var i = 0; i < names.Length; i++)
            {
                var name = names[i];
                var value = (BuildTargetGroup)values.GetValue(i);

                if (value == BuildTargetGroup.Unknown) continue;

                var member = enumType.GetMember(name);
                var entry = member.FirstOrDefault(p => p.DeclaringType == enumType);

                if (entry == null)
                {
                    Debug.LogError($"Unhandled build target: {name}. State may not be applied correctly to this platform.");
                    continue;
                }

                if (entry.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length != 0)
                {
                    // obsolete, ignore.
                    continue;
                }

                yield return value;
            }
        }
        
        public static void SaveScriptableObject(ScriptableObject so)
        {
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
        }
    }
}