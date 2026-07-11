using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Core.Scripts.Editor
{
    public static class DeepCopyUtils
    {
        private static readonly Regex GuidReferenceRegex =
            new Regex("guid:\\s*([0-9a-fA-F]{32})", RegexOptions.Compiled);

        [MenuItem("PracticAPI/Deep Copy", false)]
        public static void Copy()
        {
            var sourcePath = EditorUtility.OpenFolderPanel("Select source folder", GetSelectedFolder(), "");
            if (string.IsNullOrEmpty(sourcePath))
            {
                return;
            }

            var destinationPath = EditorUtility.OpenFolderPanel("Select destination folder", GetSelectedFolder(), "");
            if (string.IsNullOrEmpty(destinationPath))
            {
                return;
            }

            CopyDirectoryDeep(sourcePath, destinationPath);
            AssetDatabase.Refresh();
        }

        private static string GetSelectedFolder()
        {
            var obj = Selection.activeObject;
            return obj == null ? "Assets" : AssetDatabase.GetAssetPath(obj.GetInstanceID());;
        }

        private static void CopyDirectoryDeep(string sourcePath, string destinationPath)
        {
            CopyDirectoryRecursively(sourcePath, destinationPath);

            var guidTable = new Dictionary<string, string>();

            // 1) Give every asset copied from the source folder a fresh GUID.
            RemapMetaGuids(destinationPath, guidTable);

            // 2) Copy the external assets (materials / textures / meshes / ...) that the
            //    copied assets depend on but that live outside the source folder, so the
            //    deep copy is fully self-contained. Their new GUIDs join the same table.
            CopyExternalDependencies(destinationPath, guidTable);

            // 3) Rewrite every old GUID to its new one across all copied text assets.
            ReplaceGuidsInTextAssets(destinationPath, guidTable);
        }

        private static void RemapMetaGuids(string root, Dictionary<string, string> guidTable)
        {
            var metaFiles = GetFilesRecursively(root, f => f.EndsWith(".meta"));

            foreach (var metaFile in metaFiles)
            {
                var lines = File.ReadAllLines(metaFile);
                var originalGuid = ExtractGuidFromMeta(lines);
                if (string.IsNullOrEmpty(originalGuid) || guidTable.ContainsKey(originalGuid)) continue;

                var newGuid = GUID.Generate().ToString().Replace("-", "");
                guidTable[originalGuid] = newGuid;

                ReplaceGuidInMetaFile(metaFile, lines, newGuid);
            }
        }

        private static void ReplaceGuidsInTextAssets(string root, Dictionary<string, string> guidTable)
        {
            var allFiles = GetFilesRecursively(root, IsTextAsset);

            foreach (var fileToModify in allFiles)
            {
                var content = File.ReadAllText(fileToModify);

                foreach (var guidPair in guidTable)
                {
                    content = content.Replace(guidPair.Key, guidPair.Value);
                }

                File.WriteAllText(fileToModify, content);
            }
        }

        // Walks the GUID references of the copied assets and pulls in every referenced asset
        // that lives outside the copied folder (a particle system's materials, those
        // materials' textures, meshes, etc.), copying it into the destination with a new GUID.
        private static void CopyExternalDependencies(string destinationPath, Dictionary<string, string> guidTable)
        {
            var dataPathParent = Directory.GetParent(Application.dataPath);
            if (dataPathParent == null) return;

            var projectRoot = dataPathParent.FullName;
            var dependencyRoot = Path.Combine(destinationPath, "_ExternalDependencies");
            var handledGuids = new HashSet<string>();

            var filesToScan = new Queue<string>(GetFilesRecursively(destinationPath, IsTextAsset));

            while (filesToScan.Count > 0)
            {
                var file = filesToScan.Dequeue();

                string content;
                try
                {
                    content = File.ReadAllText(file);
                }
                catch
                {
                    continue;
                }

                foreach (Match match in GuidReferenceRegex.Matches(content))
                {
                    var guid = match.Groups[1].Value.ToLowerInvariant();

                    // Already part of the copied folder, or already resolved on a previous pass.
                    if (guidTable.ContainsKey(guid) || !handledGuids.Add(guid)) continue;

                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!IsCopyableDependency(assetPath)) continue;

                    var copiedFile = CopyExternalAsset(projectRoot, assetPath, dependencyRoot, guidTable);
                    if (copiedFile != null && IsTextAsset(copiedFile))
                    {
                        // Follow this dependency's own references (e.g. material -> texture).
                        filesToScan.Enqueue(copiedFile);
                    }
                }
            }
        }

        private static string CopyExternalAsset(string projectRoot, string assetPath, string dependencyRoot,
            Dictionary<string, string> guidTable)
        {
            var absoluteSource = Path.Combine(projectRoot, assetPath);
            if (!File.Exists(absoluteSource)) return null;

            var relativePath = assetPath.Substring("Assets/".Length);
            var absoluteDest = Path.Combine(dependencyRoot, relativePath);

            if (File.Exists(absoluteDest)) return absoluteDest;

            var destDir = Path.GetDirectoryName(absoluteDest);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(absoluteSource, absoluteDest, false);

            var sourceMeta = absoluteSource + ".meta";
            var destMeta = absoluteDest + ".meta";
            if (File.Exists(sourceMeta))
            {
                File.Copy(sourceMeta, destMeta, false);

                var lines = File.ReadAllLines(destMeta);
                var originalGuid = ExtractGuidFromMeta(lines);
                if (!string.IsNullOrEmpty(originalGuid) && !guidTable.ContainsKey(originalGuid))
                {
                    var newGuid = GUID.Generate().ToString().Replace("-", "");
                    guidTable[originalGuid] = newGuid;
                    ReplaceGuidInMetaFile(destMeta, lines, newGuid);
                }
            }

            return absoluteDest;
        }

        // True for assets we can and should duplicate. Excludes anything unresolved,
        // outside the Assets folder (Packages / built-in resources), and code / shader
        // assets which are shared by reference rather than duplicated.
        private static bool IsCopyableDependency(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return false;
            if (!assetPath.StartsWith("Assets/")) return false;

            switch (Path.GetExtension(assetPath).ToLowerInvariant())
            {
                case ".cs":
                case ".dll":
                case ".asmdef":
                case ".asmref":
                case ".shader":
                case ".shadergraph":
                case ".shadersubgraph":
                case ".hlsl":
                case ".cginc":
                case ".compute":
                    return false;
                default:
                    return true;
            }
        }

        private static bool IsTextAsset(string f)
        {
            return !f.EndsWith(".meta") &&
                   !f.EndsWith(".jpg") &&
                   !f.EndsWith(".png") &&
                   !f.EndsWith(".fbx") &&
                   !f.EndsWith(".dae") &&
                   !f.EndsWith(".obj") &&
                   !f.EndsWith(".blend") &&
                   !f.EndsWith(".3ds") &&
                   !f.EndsWith(".stl") &&
                   !f.EndsWith(".tga") &&
                   !f.EndsWith(".psd") &&
                   !f.EndsWith(".tiff") &&
                   !f.EndsWith(".wav") &&
                   !f.EndsWith(".mp3") &&
                   !f.EndsWith(".ogg") &&
                   !f.EndsWith(".cs") &&
                   !f.EndsWith(".mp4") &&
                   !f.EndsWith(".mov") &&
                   !f.EndsWith(".shadersubgraph") &&
                   !f.EndsWith(".shadergraph") &&
                   !f.EndsWith(".shader") &&
                   !f.EndsWith(".DS_Store") &&
                   !f.EndsWith(".hlsl") &&
                   !f.EndsWith(".ttf") &&
                   !f.EndsWith(".otf") &&
                   !f.EndsWith(".tif");
        }

        private static void CopyDirectoryRecursively(string sourceDirName, string destDirName)
        {
            var dir = new DirectoryInfo(sourceDirName);

            var dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destDirName, subdir.Name);
                CopyDirectoryRecursively(subdir.FullName, temppath);
            }
        }

        private static List<string> GetFilesRecursively(string path, Func<string, bool> criteria = null)
        {
            var files = new List<string>();

            foreach (var file in Directory.GetFiles(path))
            {
                if (criteria == null || criteria(file))
                {
                    files.Add(file);
                }
            }

            foreach (var directory in Directory.GetDirectories(path))
            {
                files.AddRange(GetFilesRecursively(directory, criteria));
            }

            return files;
        }

        private static string ExtractGuidFromMeta(string[] lines)
        {
            foreach (var line in lines)
            {
                if (line.StartsWith("guid:"))
                {
                    return line.Substring(6).Trim();
                }
            }

            return null;
        }

        private static void ReplaceGuidInMetaFile(string metaFilePath, string[] lines, string newGuid)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("guid:"))
                {
                    lines[i] = $"guid: {newGuid}";
                    break;
                }
            }

            File.WriteAllLines(metaFilePath, lines);
        }
    }
}
