using System.Collections.Generic;
using System.IO;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NumVec = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Editor
{
    // One-shot Editor setup for the GatePass stage. Run the two menu items in order. Everything here is data/asset
    // authoring that cannot be expressed in runtime code (config .asset values, an authored layout, and a prefab).
    public static class GatePassEditorSetup
    {
        private const int GATEPASS_LAYOUT_INDEX = 23; // 21 & 22 are already taken by WhacAMole

        private const string SHARED_CONFIG_PATH = "Assets/Core/Game/Domains/GamePlay/Shared/Assets/SharedGamePlayConfig.asset";
        private const string SIM_CONFIG_PATH = "Assets/Core/Game/Domains/GamePlay/Simulation/Assets/Configs/SimulationGamePlayConfig.asset";
        private const string ENV_CONFIG_PATH = "Assets/Core/Game/Domains/GamePlay/Shared/Assets/EnvironmentConfig2.asset";
        private const string SCENE_PATH = "Assets/Core/Game/Domains/GamePlay/Presentation/Match/Assets/Scenes/GamePlayMatchScene.unity";
        private const string SCORE_GATE_ASSETS_DIR = "Assets/Core/Game/Domains/GamePlay/Presentation/Match/Features/ScoreGate/Assets";
        private const string PREFAB_PATH = SCORE_GATE_ASSETS_DIR + "/ScoreGate.prefab";
        private const string POST_SPRITE_PATH = SCORE_GATE_ASSETS_DIR + "/ScoreGatePostSquare.png";

        private const float ARENA_HALF_X = 20f;
        private const float ARENA_HALF_Y = 11f;
        private const float WALL_THICKNESS = 1f;

        [MenuItem("BF/GatePass/1 - Setup Configs And Layout")]
        public static void SetupConfigsAndLayout()
        {
            SetupSharedConfig();
            SetupSimulationConfig();
            SetupEnvironmentLayout();
            AssetDatabase.SaveAssets();
            Debug.Log("[GatePassSetup] Configs + layout done. GatePass layout authored at index " + GATEPASS_LAYOUT_INDEX + ".");
        }

        [MenuItem("BF/GatePass/2 - Create Gate Prefab And Bind")]
        public static void CreatePrefabAndBind()
        {
            var sprite = GetOrCreateWhitePostSprite();
            CreateScoreGatePrefab(sprite);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BindPrefabToInstaller();
            Debug.Log("[GatePassSetup] Gate prefab created at " + PREFAB_PATH + " and bound to the installer in the scene.");
        }

        private static void SetupSharedConfig()
        {
            var shared = AssetDatabase.LoadAssetAtPath<SharedGamePlayConfig>(SHARED_CONFIG_PATH);
            shared.ScoreGatePostSize = new Vector2(1.5f, 1.5f);
            shared.ScoreGateGapWidth = 4f;
            shared.ScoreGateDensity = 4f;
            shared.ScoreGateRestitution = 0.2f;
            shared.ScoreGateLinearDamping = 1.5f;
            shared.ScoreGateAngularDamping = 1.5f;
            EditorUtility.SetDirty(shared);
        }

        private static void SetupSimulationConfig()
        {
            var sim = AssetDatabase.LoadAssetAtPath<SimulationGamePlayConfig>(SIM_CONFIG_PATH);
            var inner = sim.InnerConfig;
            inner.AreBonusStagesEnabled = true;
            inner.EnabledBonusStageTypes = new List<StageType> { StageType.WhacAMole, StageType.GatePass };
            inner.DefaultGatePassEnvironmentId = GATEPASS_LAYOUT_INDEX;
            inner.GatePass ??= new GatePassConfig();
            EditorUtility.SetDirty(sim);
        }

        private static void SetupEnvironmentLayout()
        {
            var env = AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(ENV_CONFIG_PATH);

            // EnvironmentConfig.SetWalls/SetStageBoundaries/SetFieldBarriers serialize with Odin (ToJson), but the Get*
            // side reads with Newtonsoft (FromJson); those two do NOT round-trip. So the wall-like layers are written as
            // Newtonsoft JSON straight onto the layout. Camera + score gates already use Newtonsoft in their setters.
            var layout = GetOrCreateLayout(env, GATEPASS_LAYOUT_INDEX);
            layout.SetEnvironmentHalfSizeJson(JsonConvert.SerializeObject(new NumVec(ARENA_HALF_X, ARENA_HALF_Y)));
            layout.SetWallsJson(JsonConvert.SerializeObject(BuildBorderWalls()));
            layout.SetLavaWallsJson(JsonConvert.SerializeObject(new WallConfig[0]));
            layout.SetStageBoundariesJson(JsonConvert.SerializeObject(BuildBorderWalls())); // same rectangle keeps players inside
            layout.SetFieldBarriersJson(JsonConvert.SerializeObject(BuildTeamBarriers()));

            env.SetCameraBoundaries(new CameraBoundariesConfig(new NumVec(-ARENA_HALF_X, ARENA_HALF_Y), new NumVec(ARENA_HALF_X, -ARENA_HALF_Y)), GATEPASS_LAYOUT_INDEX);
            env.SetScoreGates(new[] { new ScoreGateConfig(1, NumVec.Zero, 0f) }, GATEPASS_LAYOUT_INDEX);

            if (env.GatePassLayoutIndexes == null)
            {
                env.GatePassLayoutIndexes = new List<int>();
            }
            if (!env.GatePassLayoutIndexes.Contains(GATEPASS_LAYOUT_INDEX))
            {
                env.GatePassLayoutIndexes.Add(GATEPASS_LAYOUT_INDEX);
            }
            EditorUtility.SetDirty(env);
        }

        private static EnvironmentLayoutConfig GetOrCreateLayout(EnvironmentConfig env, int index)
        {
            if (!env.Configs.TryGetValue(index, out var layout))
            {
                layout = new EnvironmentLayoutConfig("", "");
                env.Configs[index] = layout;
            }
            return layout;
        }

        private static WallConfig[] BuildBorderWalls()
        {
            var halfEdgeLength = ARENA_HALF_X + WALL_THICKNESS;
            var halfSideLength = ARENA_HALF_Y + WALL_THICKNESS;
            var halfThickness = WALL_THICKNESS * 0.5f;

            return new[]
            {
                MakeRectWall(1, 0f, ARENA_HALF_Y + halfThickness, halfEdgeLength, halfThickness),   // top
                MakeRectWall(2, 0f, -(ARENA_HALF_Y + halfThickness), halfEdgeLength, halfThickness), // bottom
                MakeRectWall(3, -(ARENA_HALF_X + halfThickness), 0f, halfThickness, halfSideLength),  // left
                MakeRectWall(4, ARENA_HALF_X + halfThickness, 0f, halfThickness, halfSideLength),     // right
            };
        }

        private static WallConfig MakeRectWall(ushort id, float centerX, float centerY, float halfWidth, float halfHeight)
        {
            var points = new[]
            {
                new NumVec(centerX - halfWidth, centerY - halfHeight),
                new NumVec(centerX + halfWidth, centerY - halfHeight),
                new NumVec(centerX + halfWidth, centerY + halfHeight),
                new NumVec(centerX - halfWidth, centerY + halfHeight),
            };
            return new WallConfig(id, points);
        }

        private static EnvironmentFieldBarrierConfig[] BuildTeamBarriers()
        {
            var barrierSize = new NumVec(3f, 3f);
            var offsetX = ARENA_HALF_X * 0.6f;
            var offsetY = ARENA_HALF_Y * 0.6f;

            return new[]
            {
                new EnvironmentFieldBarrierConfig(new NumVec(-offsetX, offsetY), barrierSize, FieldBarrierShape.Circle),
                new EnvironmentFieldBarrierConfig(new NumVec(offsetX, offsetY), barrierSize, FieldBarrierShape.Circle),
                new EnvironmentFieldBarrierConfig(new NumVec(-offsetX, -offsetY), barrierSize, FieldBarrierShape.Circle),
                new EnvironmentFieldBarrierConfig(new NumVec(offsetX, -offsetY), barrierSize, FieldBarrierShape.Circle),
            };
        }

        private static Sprite GetOrCreateWhitePostSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(POST_SPRITE_PATH);
            if (existing != null)
            {
                return existing;
            }

            EnsureDirectoryExists(SCORE_GATE_ASSETS_DIR);

            const int size = 8;
            var texture = new Texture2D(size, size);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            var fullPath = ProjectRelativeToFull(POST_SPRITE_PATH);
            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(POST_SPRITE_PATH, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(POST_SPRITE_PATH);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = size; // 8px sprite => 1 world unit, so localScale == world size
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(POST_SPRITE_PATH);
        }

        private static void CreateScoreGatePrefab(Sprite postSprite)
        {
            EnsureDirectoryExists(SCORE_GATE_ASSETS_DIR);

            var root = new GameObject("ScoreGate");
            var view = root.AddComponent<ScoreGateView>();

            var leftPost = CreatePost("PostLeft", root.transform, postSprite);
            var rightPost = CreatePost("PostRight", root.transform, postSprite);

            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("_leftPost").objectReferenceValue = leftPost.transform;
            serializedView.FindProperty("_rightPost").objectReferenceValue = rightPost.transform;
            var tintables = serializedView.FindProperty("_tintableRenderers");
            tintables.arraySize = 2;
            tintables.GetArrayElementAtIndex(0).objectReferenceValue = leftPost.GetComponent<SpriteRenderer>();
            tintables.GetArrayElementAtIndex(1).objectReferenceValue = rightPost.GetComponent<SpriteRenderer>();
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);
        }

        private static GameObject CreatePost(string name, Transform parent, Sprite sprite)
        {
            var post = new GameObject(name);
            post.transform.SetParent(parent, false);
            var spriteRenderer = post.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            return post;
        }

        private static void BindPrefabToInstaller()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

            // Load the component off the prefab GameObject (the canonical persistent reference Unity will accept). A
            // component loaded via LoadAssetAtPath<Component> directly can be rejected by objectReferenceValue.
            var prefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            var prefabView = prefabGameObject != null ? prefabGameObject.GetComponent<ScoreGateView>() : null;
            if (prefabView == null)
            {
                Debug.LogError("[GatePassSetup] Could not load the ScoreGateView from the prefab at " + PREFAB_PATH);
                return;
            }

            // The installer derives from Zenject's MonoInstaller, which this editor assembly does not reference, so it
            // is found by component type name and edited through SerializedObject rather than a typed reference.
            var installer = FindInstallerInScene(scene);
            if (installer == null)
            {
                Debug.LogError("[GatePassSetup] GamePlayMatchInstaller not found in the scene - could not bind the prefab.");
                return;
            }

            var serializedInstaller = new SerializedObject(installer);
            var prefabProperty = serializedInstaller.FindProperty("_scoreGateViewPrefab");
            if (prefabProperty == null)
            {
                Debug.LogError("[GatePassSetup] '_scoreGateViewPrefab' field not found on the installer.");
                return;
            }

            prefabProperty.objectReferenceValue = prefabView;
            serializedInstaller.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[GatePassSetup] Installer bind assigned = " + (prefabProperty.objectReferenceValue != null));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static MonoBehaviour FindInstallerInScene(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var monoBehaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (monoBehaviour != null && monoBehaviour.GetType().Name == "GamePlayMatchInstaller")
                    {
                        return monoBehaviour;
                    }
                }
            }
            return null;
        }

        private static void EnsureDirectoryExists(string projectRelativeDir)
        {
            var fullDir = ProjectRelativeToFull(projectRelativeDir);
            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
                AssetDatabase.Refresh();
            }
        }

        private static string ProjectRelativeToFull(string projectRelativePath)
        {
            // Application.dataPath ends with "/Assets"; project-relative paths start with "Assets/".
            return Path.Combine(Application.dataPath, projectRelativePath.Substring("Assets/".Length));
        }
    }
}
