// using UnityEditor;
// using UnityEditor.Build;
//
// namespace Core.Scripts.Editor.Utils
// {
//     public static class DefineSymbolsUtils
//     {
//         public static bool IsSymbolEnabled(string defineSymbol)
//         {
//             var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
//             return defines.Contains(defineSymbol);
//         }
//
//         public static void UpdateDefine(bool enabled)
//         {
//             string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
//
//             var defineList = new System.Collections.Generic.List<string>(
//                 defines.Split(';')
//             );
//
//             if (enabled)
//             {
//                 if (!defineList.Contains(DefineSymbol))
//                     defineList.Add(DefineSymbol);
//             }
//             else
//             {
//                 defineList.Remove(DefineSymbol);
//             }
//
//             PlayerSettings.SetScriptingDefineSymbolsForGroup(
//                 EditorUserBuildSettings.selectedBuildTargetGroup,
//                 string.Join(";", defineList)
//             );
//         }
//     }
// }
