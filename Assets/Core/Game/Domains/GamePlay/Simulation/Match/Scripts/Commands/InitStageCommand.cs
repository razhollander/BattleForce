using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersInLavaTracker;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Services.TeleportGate;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Stage;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations;
using Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using Core.Game.Domains.GamePlay.Simulation.Scripts.RNG;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Services.CommandFactory;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class InitStageCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;
        private SimulationGamePlayConfig _gamePlayConfig;
        private IStageDataService _stageDataService;
        private IPlayersInLavaTrackerService _playersInLavaTrackerService;
        private ITeleportGateService _teleportGateService;
        private SharedGamePlayConfig _sharedGamePlayConfig;

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
            _gamePlayConfig = _diContainer.Resolve<SimulationGamePlayConfig>();
            _stageDataService = _diContainer.Resolve<IStageDataService>();
            _playersInLavaTrackerService = _diContainer.Resolve<IPlayersInLavaTrackerService>();
            _teleportGateService = _diContainer.Resolve<ITeleportGateService>();
            _sharedGamePlayConfig = _diContainer.Resolve<SharedGamePlayConfig>();
        }

        public void Execute()
        {
            _physicsSimulator.ClearAllData();
            _playersInLavaTrackerService.ClearAllData();
            _teleportGateService.ClearData();
            
            _matchDataService.SimulationState.Bullets.Clear();
            _matchDataService.SimulationState.PowerUpBalls.Clear();
            _matchDataService.SimulationState.TalentCards.Clear();

            CreateWalls();
            CreateLavaWalls();
            CreateTalentCards();
            CreateEnvironmentSprings();
            CreateTeleportGates();
            ResetPlayers();

            _stageDataService.IsStageEnded = false;
            _stageDataService.StageRestartTimer = -1;
            _stageDataService.ClearData();
        }

        private void ResetPlayers()
        {
            var halfSize = _matchDataService.Environment.EnvironmentHalfSize;
            var players = _matchDataService.SimulationState.Players;

            for (int i = 0; i < players.Count; i++)
            {
                var player = players.GetByIndex(i);

                var health = _gamePlayConfig.PlayerSpaceship.StartHealth;
                var shootCooldown = _gamePlayConfig.PlayerSpaceship.ShootCooldown;
                var radius = _gamePlayConfig.PlayerSpaceship.DefaultPlayerRadius;

                player.Spaceship.Health.CurrentHealth = health;
                player.Spaceship.Health.MaxHealth = health;
                player.Spaceship.Shoot.CooldownSecondsLeft = shootCooldown;
                player.Spaceship.Shoot.MaxCooldown = shootCooldown;

                var position = GetRandomFreePosition(radius, halfSize);
                var direction = RNG.NextFloat(0, 360).AngleToVector();
                var velocity = direction * _gamePlayConfig.PlayerSpaceship.TargetMovementSpeed;

                player.Spaceship.Transform.Position = position;
                player.Spaceship.Transform.Direction = direction;
                player.Spaceship.Transform.Velocity = velocity;
                player.Spaceship.Transform.Radius = radius;
                player.Spaceship.IsEngineOn = true;
                player.Spaceship.IsAlive = true;
                
                _physicsSimulator.AddPlayer(player.Id, player.TeamId, position, velocity, radius);
            }
        }

        private Vector2 GetRandomFreePosition(float radius, Vector2 halfSize) // todo: in each environemnt we should have spawn points, and just choose one of them
        {
             for (int i = 0; i < 100; i++)
             {
                 var x = RNG.NextFloat(-halfSize.X + radius, halfSize.X - radius);
                 var y = RNG.NextFloat(-halfSize.Y + radius, halfSize.Y - radius);
                 var pos = new Vector2(x, y);

                 if (!_physicsSimulator.IsSquareHitAnyBodyTypes(pos, radius, PhysicsBodyType.Wall, PhysicsBodyType.Lava, PhysicsBodyType.StartMatchWall))
                 {
                     return pos;
                 }
             }
             LogService.LogError("No free position found!");
             return Vector2.Zero;
        }

        private void CreateWalls()
        {
            var wallConfigs = _matchDataService.Environment.WallConfigs;

            foreach (var wallConfig in wallConfigs)
            {
                var wallId = wallConfig.Id;
                var wallPoints = wallConfig.Points;
                _physicsSimulator.AddWall(wallId, wallPoints);
            }
        }

        private void CreateLavaWalls()
        {
            var lavaWallConfigs = _matchDataService.Environment.LavaWallConfigs;
            if (lavaWallConfigs.IsNullOrEmpty())
            {
                return;
            }

            foreach (var lavaWallConfig in lavaWallConfigs)
            {
                var lavaWallId = lavaWallConfig.Id;
                var lavaWallPoints = lavaWallConfig.Points;
                _physicsSimulator.AddLavaWall(lavaWallId, lavaWallPoints);
            }
        }

        private void CreateTalentCards()
        {
            var talentCards = _matchDataService.Environment.TalentCards;
            if (talentCards.IsNullOrEmpty())
            {
                return;
            }

            foreach (var talentCard in talentCards)
            {
                var talentCardPosition = talentCard.Position;
                var talentCardId = talentCard.Id;
                _matchDataService.AddTalentCard(talentCardId, talentCardPosition, talentCard.TalentType, _gamePlayConfig.Talents.TalentCardHealth);
                _physicsSimulator.AddTalentCard(talentCardId, talentCardPosition, _gamePlayConfig.Talents.TalentCardWidth, _gamePlayConfig.Talents.TalentCardHeight);
            }
        }

        private void CreateEnvironmentSprings()
        {
            var environmentSprings = _matchDataService.Environment.EnvironmentSprings;
            if (environmentSprings.IsNullOrEmpty())
            {
                return;
            }

            foreach (var environmentSpring in environmentSprings)
            {
                var springId = environmentSpring.Id;
                var springRotationAngle = environmentSpring.DirectionAngle + 90;
                var springSize = _gamePlayConfig.EnvironmentSpring.Size.ToNumericsVector2();
                var springPosition = environmentSpring.Position;
                _physicsSimulator.AddEnvironmentSpring(springId, springPosition, springRotationAngle, springSize);
            }
        }

        private void CreateTeleportGates()
        {
            var teleportGates = _matchDataService.Environment.TeleportGates;
            if (teleportGates.IsNullOrEmpty())
            {
                return;
            }

            foreach (var pair in teleportGates)
            {
                var gateSize = _sharedGamePlayConfig.EnvironmentTeleport.Size.ToNumericsVector2();
                _physicsSimulator.AddTeleportGate(pair.GateAId, pair.GateA.Position, pair.GateA.NormalRotation, gateSize);
                _physicsSimulator.AddTeleportGate(pair.GateBId, pair.GateB.Position, pair.GateB.NormalRotation, gateSize);
            }
        }
    }
}
