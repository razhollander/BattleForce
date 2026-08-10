using System.Collections.Generic;
using System.IO;
using Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Newtonsoft.Json;
using TMPro;
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
        private const string MULTIPLIER_FONT_PATH = "Assets/Core/Plugins/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private const float ARENA_HALF_X = 20f;
        private const float ARENA_HALF_Y = 11f;
        private const float WALL_THICKNESS = 1f;

        // A half circular pit bulges out of the middle of each side wall, exactly like the ones in layout 0. Its radius
        // is also the half height of the gap it leaves in the wall, so the pit's mouth is the gap itself.
        private const float PIT_RADIUS = 4f;
        private const int PIT_ARC_SEGMENTS = 12;
        private const ushort LEFT_PIT_FIRST_ARC_WALL_ID = 10;
        private const ushort RIGHT_PIT_FIRST_ARC_WALL_ID = 30;

        // The gate trap wall is a portcullis hidden inside the lower half of the side wall; it slides up to plug the
        // pit's mouth the moment somebody flies in.
        private const ushort LEFT_GATE_TRAP_ID = 1;
        private const ushort RIGHT_GATE_TRAP_ID = 2;
        private const ushort LEFT_GATE_TRAP_WALL_ID = 50;
        private const ushort RIGHT_GATE_TRAP_WALL_ID = 51;
        private const float GATE_TRAP_MOVEMENT_SPEED = 16f;
        private const float GATE_TRAP_SECONDS_STAY_CLOSED = 3f;
        private const float GATE_TRAP_SECONDS_STAY_OPEN = 2f;
        private const int GATE_TRAP_AREA_POINTS = 8;

        [MenuItem("BF/GatePass/1 - Setup Configs And Layout")]
        public static void SetupConfigsAndLayout()
        {
            SetupSharedConfig();
            SetupSimulationConfig();
            SetupEnvironmentLayout();
            AssetDatabase.SaveAssets();
            Debug.Log("[GatePassSetup] Configs + layout done. GatePass layout authored at index " + GATEPASS_LAYOUT_INDEX + ".");
        }

        // Re-authors only the layout (arena walls, the two pits, the boundaries and the gate traps), leaving the shared
        // and simulation tuning values untouched - use it after changing the layout geometry above.
        [MenuItem("BF/GatePass/3 - Rebuild Layout Only")]
        public static void RebuildLayoutOnly()
        {
            SetupEnvironmentLayout();
            AssetDatabase.SaveAssets();
            Debug.Log("[GatePassSetup] Layout " + GATEPASS_LAYOUT_INDEX + " rebuilt: walls, pits, boundaries and gate traps.");
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
            inner.GatePass.ScoreGateMass = 20f;
            inner.GatePass.ScoreGateDensity = 4f;
            inner.GatePass.ScoreGateRestitution = 0.2f;
            inner.GatePass.ScoreGateLinearDamping = 1.5f;
            inner.GatePass.ScoreGateAngularDamping = 1.5f;
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
            layout.SetWallsJson(JsonConvert.SerializeObject(BuildArenaWalls()));
            layout.SetLavaWallsJson(JsonConvert.SerializeObject(new WallConfig[0]));
            // The pit arcs are deliberately left out of the boundaries: a player sitting in a pit must not read as
            // outside the stage and get snapped back out of it.
            layout.SetStageBoundariesJson(JsonConvert.SerializeObject(BuildBorderWalls()));
            layout.SetFieldBarriersJson(JsonConvert.SerializeObject(BuildTeamBarriers()));
            layout.SetGateTrapsJson(JsonConvert.SerializeObject(BuildGateTraps()));

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

        // Top and bottom run the full width; each side wall is split in two so the pit's mouth stays open between them.
        private static WallConfig[] BuildBorderWalls()
        {
            var halfEdgeLength = ARENA_HALF_X + WALL_THICKNESS;
            var halfThickness = WALL_THICKNESS * 0.5f;
            var sideHalfLength = (ARENA_HALF_Y + WALL_THICKNESS - PIT_RADIUS) * 0.5f;
            var sideCenterY = PIT_RADIUS + sideHalfLength;
            var sideCenterX = ARENA_HALF_X + halfThickness;

            return new[]
            {
                MakeRectWall(1, 0f, ARENA_HALF_Y + halfThickness, halfEdgeLength, halfThickness),   // top
                MakeRectWall(2, 0f, -(ARENA_HALF_Y + halfThickness), halfEdgeLength, halfThickness), // bottom
                MakeRectWall(3, -sideCenterX, -sideCenterY, halfThickness, sideHalfLength),          // left, below the pit
                MakeRectWall(4, -sideCenterX, sideCenterY, halfThickness, sideHalfLength),           // left, above the pit
                MakeRectWall(5, sideCenterX, -sideCenterY, halfThickness, sideHalfLength),           // right, below the pit
                MakeRectWall(6, sideCenterX, sideCenterY, halfThickness, sideHalfLength),            // right, above the pit
            };
        }

        private static WallConfig[] BuildArenaWalls()
        {
            var walls = new List<WallConfig>(BuildBorderWalls());
            walls.AddRange(BuildPitArcWalls(GetPitCenterX(isLeftPit: true), LEFT_PIT_FIRST_ARC_WALL_ID, isLeftPit: true));
            walls.AddRange(BuildPitArcWalls(GetPitCenterX(isLeftPit: false), RIGHT_PIT_FIRST_ARC_WALL_ID, isLeftPit: false));
            return walls.ToArray();
        }

        // The pit hangs off the outer face of the side wall, so its mouth is flush with the gap left between the two
        // side wall pieces.
        private static float GetPitCenterX(bool isLeftPit)
        {
            var pitCenterX = ARENA_HALF_X + WALL_THICKNESS;
            return isLeftPit ? -pitCenterX : pitCenterX;
        }

        // Each arc piece is a quad between the inner and the outer radius, so the ring of them reads as one wall of the
        // same thickness as the arena border.
        private static WallConfig[] BuildPitArcWalls(float pitCenterX, ushort firstWallId, bool isLeftPit)
        {
            var arcWalls = new WallConfig[PIT_ARC_SEGMENTS];
            var outerRadius = PIT_RADIUS + WALL_THICKNESS;
            var degreesPerSegment = 180f / PIT_ARC_SEGMENTS;
            // Both arcs sweep the half of the circle that faces away from the arena: 90 to 270 on the left, -90 to 90 on the right.
            var firstSegmentDegrees = GetPitFirstArcDegrees(isLeftPit);

            for (int i = 0; i < PIT_ARC_SEGMENTS; i++)
            {
                var startDegrees = firstSegmentDegrees + degreesPerSegment * i;
                var endDegrees = startDegrees + degreesPerSegment;

                var points = new[]
                {
                    GetPointOnPitCircle(pitCenterX, PIT_RADIUS, startDegrees),
                    GetPointOnPitCircle(pitCenterX, outerRadius, startDegrees),
                    GetPointOnPitCircle(pitCenterX, outerRadius, endDegrees),
                    GetPointOnPitCircle(pitCenterX, PIT_RADIUS, endDegrees),
                };

                arcWalls[i] = new WallConfig((ushort)(firstWallId + i), points);
            }

            return arcWalls;
        }

        private static NumVec GetPointOnPitCircle(float pitCenterX, float radius, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return new NumVec(pitCenterX + radius * Mathf.Cos(radians), radius * Mathf.Sin(radians));
        }

        // Both traps are identical mirrors: a bar that lives inside the lower side wall and slides straight up into the
        // pit's mouth, sealing whoever flew in.
        private static EnvironmentGateTrapConfig[] BuildGateTraps()
        {
            return new[]
            {
                BuildGateTrap(LEFT_GATE_TRAP_ID, LEFT_GATE_TRAP_WALL_ID, isLeftPit: true),
                BuildGateTrap(RIGHT_GATE_TRAP_ID, RIGHT_GATE_TRAP_WALL_ID, isLeftPit: false),
            };
        }

        private static EnvironmentGateTrapConfig BuildGateTrap(ushort id, ushort wallId, bool isLeftPit)
        {
            var mouthHeight = PIT_RADIUS * 2f;
            var halfThickness = WALL_THICKNESS * 0.5f;
            var wallCenterX = isLeftPit ? -(ARENA_HALF_X + halfThickness) : ARENA_HALF_X + halfThickness;

            return new EnvironmentGateTrapConfig
            {
                Id = id,
                WallId = wallId,
                // Authored along +X from its origin, so at 90 degrees it stands upright and fills the mouth upwards.
                WallPoints = new[]
                {
                    new NumVec(0f, -halfThickness),
                    new NumVec(mouthHeight, -halfThickness),
                    new NumVec(mouthHeight, halfThickness),
                    new NumVec(0f, halfThickness),
                },
                AreaPolygons = new[] { BuildGateTrapAreaPolygon(GetPitCenterX(isLeftPit), isLeftPit) },
                OpenPosition = new NumVec(wallCenterX, -(ARENA_HALF_Y + WALL_THICKNESS)),
                ClosedPosition = new NumVec(wallCenterX, -PIT_RADIUS),
                OpenRotationDegrees = 90f,
                ClosedRotationDegrees = 90f,
                LocalRotationPivot = NumVec.Zero,
                MovementSpeed = GATE_TRAP_MOVEMENT_SPEED,
                SecondsStayClosed = GATE_TRAP_SECONDS_STAY_CLOSED,
                SecondsStayOpen = GATE_TRAP_SECONDS_STAY_OPEN,
                IsAttachedToRotationWheel = false,
                AttachToRotationWheelId = 0,
            };
        }

        // The sensing area is the pit's inside, closed off by the chord across its mouth - a player only counts as
        // caught once they are past the wall line the bar rises through.
        private static GateTrapAreaPolygonConfig BuildGateTrapAreaPolygon(float pitCenterX, bool isLeftPit)
        {
            var points = new NumVec[GATE_TRAP_AREA_POINTS];
            var degreesPerStep = 180f / (GATE_TRAP_AREA_POINTS - 1);
            var firstPointDegrees = GetPitFirstArcDegrees(isLeftPit);

            for (int i = 0; i < GATE_TRAP_AREA_POINTS; i++)
            {
                points[i] = GetPointOnPitCircle(pitCenterX, PIT_RADIUS, firstPointDegrees + degreesPerStep * i);
            }

            return new GateTrapAreaPolygonConfig { Points = points };
        }

        private static float GetPitFirstArcDegrees(bool isLeftPit)
        {
            return isLeftPit ? 90f : -90f;
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
            var multiplierText = CreateMultiplierText("MultiplierText", root.transform);

            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("_leftPost").objectReferenceValue = leftPost.transform;
            serializedView.FindProperty("_rightPost").objectReferenceValue = rightPost.transform;
            var tintables = serializedView.FindProperty("_tintableRenderers");
            tintables.arraySize = 2;
            tintables.GetArrayElementAtIndex(0).objectReferenceValue = leftPost.GetComponent<SpriteRenderer>();
            tintables.GetArrayElementAtIndex(1).objectReferenceValue = rightPost.GetComponent<SpriteRenderer>();
            serializedView.FindProperty("_multiplierText").objectReferenceValue = multiplierText;
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

        // World-space label centered in the gap that shows the x2/x3/x4 next-pass multiplier. Starts blank (x1 = no bonus).
        private static TextMeshPro CreateMultiplierText(string name, Transform parent)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = new Vector3(0f, 0f, -1f); // in front of the posts

            var text = textObject.AddComponent<TextMeshPro>();
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MULTIPLIER_FONT_PATH);
            text.text = string.Empty;
            text.fontSize = 6;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta = new Vector2(6f, 3f);
            text.GetComponent<MeshRenderer>().sortingOrder = 10; // above the gate posts

            return text;
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
