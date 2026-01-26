using System.Linq;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs
{
    public class MatchMakingEnvironmentTeamFloorController
    {
        private EnvironmentTeamFloorView _teamFloorView;
        private readonly ushort _teamId;
        private readonly WallConfig[] _walls;
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        private Transform _teamFloorsParent;
        
        public MatchMakingEnvironmentTeamFloorController(ushort teamId, WallConfig[] walls, SharedGamePlayConfig sharedGamePlayConfig)
        {
            _teamId = teamId;
            _walls = walls;
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }
        
        public void CreateTeamFloors(EnvironmentTeamFloorView teamFloorView, Transform parent)
        {
            _teamFloorsParent = new GameObject("TeamFloors_" + _teamId).transform;
            _teamFloorsParent.SetParent(parent);
            
            foreach (var wall in _walls)
            {
                _teamFloorView = Object.Instantiate(teamFloorView, _teamFloorsParent);
                _teamFloorView.name = "EnvironmentTeamFloor_" + wall.Id;
                var pointsUnityVector2 = wall.Points.Select(x => x.ToUnityVector2()).ToArray();
                var mesh = MeshUtils.BuildMesh(pointsUnityVector2);
                _teamFloorView.Setup(mesh, _sharedGamePlayConfig.ColorPerTeamId[_teamId]);
            }
        }

        public void AnimateBounce()
        {
            _teamFloorView.AnimateBounce();
        }
    }
}