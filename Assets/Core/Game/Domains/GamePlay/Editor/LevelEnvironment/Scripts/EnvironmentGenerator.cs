using System.Collections.Generic;
using System.Linq;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Scripts.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Editor.LevelEnvironment.Scripts
{
    public class EnvironmentGenerator : MonoBehaviour
    {
        private const int MIN_BOX2D_ID = 1;
        [SerializeField] private EnvironmentConfig _environmentConfig;
        [SerializeField] private List<PolygonPath2D> _walls;
        [SerializeField] private List<LavaWall> _lavaWalls;

        [Button]
        public void RefreshConfig(int index)
        {
            _walls = GetWalls();
            var wallsConfigs = new WallConfig[_walls.Count];

            for (int i = 0; i < _walls.Count; i++)
            {
                var wallGenerator = _walls[i];
                var wallConfig = new WallConfig((ushort) (i + MIN_BOX2D_ID), wallGenerator.GetPointsRelativeToObject().Select(x=>x.ToNumericsVector2()).ToArray());
                wallsConfigs[i] = wallConfig;
            }

            _environmentConfig.SetWalls(wallsConfigs, index);

            _lavaWalls = GetLavaWalls();
            var lavaConfigs = new WallConfig[_lavaWalls.Count];
            // ID numbering should probably continue after walls or be independent?
            // Existing physics implementation uses ID to identify bodies. If Wall and Lava share ID space?
            // Box2D bodies have unique IDs or pointers. The UserData has ID.
            // If we use same ID for a wall and a lava, it might be confusing if we look up by ID.
            // But they are different types.
            // Let's offset ID to avoid collision just in case, or continue numbering.

            int lavaStartId = MIN_BOX2D_ID + _walls.Count;

            for (int i = 0; i < _lavaWalls.Count; i++)
            {
                var lavaWall = _lavaWalls[i];
                var lavaConfig = new WallConfig((ushort)(i + lavaStartId), lavaWall.GetPoints().Select(x => x.ToNumericsVector2()).ToArray());
                lavaConfigs[i] = lavaConfig;
            }

            _environmentConfig.SetLavaWalls(lavaConfigs, index);
        }

        private List<PolygonPath2D> GetWalls()
        {
            return GetComponentsInChildren<PolygonPath2D>().Where(p => p.GetComponent<LavaWall>() == null).ToList();
        }

        private List<LavaWall> GetLavaWalls()
        {
            return new List<LavaWall>(GetComponentsInChildren<LavaWall>());
        }
    }
}
