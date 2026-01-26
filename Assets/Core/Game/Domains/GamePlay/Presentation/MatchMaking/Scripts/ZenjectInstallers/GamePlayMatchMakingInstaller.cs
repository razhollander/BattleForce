using Core.Game.Domains.GamePlay.Presentation.Features.Bullets.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Features.Environment.Walls.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts;
using Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Bullets;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Player.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.DataService;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Initiator;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.TickProcessor;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Network.PacketsHandlers;
using Core.Game.Domains.GamePlay.Presentation.Scripts.TickProcessors;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Scripts.ZenjectInstallers
{
    public class GamePlayMatchMakingInstaller : MonoInstaller
    {
        [SerializeField] private PlayerView _playerViewPrefab;
        [SerializeField] private BulletView _bulletViewPrefab;
        [SerializeField] private EnvironmentWallView _environmentWallViewPrefab;
        [SerializeField] private EnvironmentTeamFloorView _environmentTeamFloorViewPrefab;
        
        public override void InstallBindings()
        {
            BindServices();
            BindControllers();
        }

        private void BindServices()
        {
            Container.Bind<IGamePlayMatchMakingInitiator>().To<GamePlayMatchMakingInitiator>().AsSingle().NonLazy();
            Container.Bind<IMatchMakingDataService>().To<MatchMakingDataService>().AsSingle().NonLazy();
            Container.Bind<ITickProcessor>().To<ClientMatchMakingNetworkTickProcessor>().AsSingle().NonLazy();
            Container.Bind<IFullTickPacketsHandler>().To<MatchMakingFullTickPacketsHandler>().AsSingle().NonLazy();
            Container.Bind<IClientMatchMakingPresentationTickProcessor>().To<ClientMatchMakingPresentationTickProcessor>().AsSingle().NonLazy();
        }
        
        private void BindControllers()
        {
            Container.BindInterfacesTo<MatchMakingPlayerControllers>().AsSingle().WithArguments(_playerViewPrefab).NonLazy();
            Container.BindInterfacesTo<MatchMakingBulletControllers>().AsSingle().WithArguments(_bulletViewPrefab).NonLazy();
            Container.BindInterfacesTo<MatchMakingEnvironmentWallsControllers>().AsSingle().WithArguments(_environmentWallViewPrefab).NonLazy();
            Container.BindInterfacesTo<MatchMakingEnvironmentTeamFloorControllers>().AsSingle().WithArguments(_environmentTeamFloorViewPrefab).NonLazy();
        }
    }
}
