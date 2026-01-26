using System.Linq;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs
{
    public class MatchMakingEnvironmentTeamFloorController
    {
        private EnvironmentTeamFloorsView _teamFloorsView;
        private readonly ushort _teamId;
        private readonly WallConfig[] _walls;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        
        public MatchMakingEnvironmentTeamFloorController(ushort teamId, WallConfig[] walls, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _teamId = teamId;
            _walls = walls;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }
        
        public void CreateTeamFloors(EnvironmentTeamFloorsView teamFloorView, Transform parent)
        {
            _teamFloorsView = Object.Instantiate(teamFloorView, parent);
            _teamFloorsView.name = "TeamFloors_" + _teamId;
            
            foreach (var wall in _walls)
            {
                var pointsUnityVector2 = wall.Points.Select(x => x.ToUnityVector2()).ToArray();
                var mesh = MeshUtils.BuildMesh(pointsUnityVector2, 2);
                _teamFloorsView.CreateFloor(mesh, wall.Id, _sharedGamePlayConfig.ColorPerTeamId[_teamId]);
            }
        }
        
        public void AnimateBounce()
        {
            _teamFloorsView.AnimateBounce();

        }
    }
}