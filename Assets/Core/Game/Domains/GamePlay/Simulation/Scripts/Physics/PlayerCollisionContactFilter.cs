using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Services.GamePlayConfig;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    // When CanPlayersCollideWithEachOther is enabled in the gameplay config, this forces any two player
    // spaceship fixtures to physically collide - both enemies AND teammates. Teammates normally share a
    // negative teamId groupIndex, which Box2D's default filter treats as "never collide"; returning true
    // here overrides that for ship-vs-ship pairs only.
    //
    // Every other fixture pair falls through to the default category/mask/groupIndex logic, so bullet and
    // heart friendly-fire immunity (which relies on that same shared negative groupIndex) is left intact.
    public class PlayerCollisionContactFilter : ContactFilter
    {
        private readonly ISimulationGamePlayConfigService _gamePlayConfigService;

        public PlayerCollisionContactFilter(ISimulationGamePlayConfigService gamePlayConfigService)
        {
            _gamePlayConfigService = gamePlayConfigService;
        }

        public override bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            if (_gamePlayConfigService.GamePlayConfig.CanPlayersCollideWithEachOther
                && fixtureA.Body.UserData is PhysicsBodyData dataA
                && fixtureB.Body.UserData is PhysicsBodyData dataB
                && dataA.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship
                && dataB.PhysicsBodyType == PhysicsBodyType.PlayerSpaceship)
            {
                return true;
            }

            return base.ShouldCollide(fixtureA, fixtureB);
        }
    }
}
