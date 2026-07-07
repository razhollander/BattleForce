using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.FrigidBlock;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    public class ShootFrigidBlockForPlayerCommand : BaseCommand, ICommandVoid
    {
        private IFrigidBlocksController _frigidBlocksController;

        private ushort _casterPlayerId;
        private Vector2 _position;
        private Vector2 _direction;
        private int _tick;
        private int _cooldownEndTick;

        public ShootFrigidBlockForPlayerCommand SetCasterPlayerId(ushort casterPlayerId)
        {
            _casterPlayerId = casterPlayerId;
            return this;
        }

        public ShootFrigidBlockForPlayerCommand SetPosition(Vector2 position)
        {
            _position = position;
            return this;
        }

        public ShootFrigidBlockForPlayerCommand SetDirection(Vector2 direction)
        {
            _direction = direction;
            return this;
        }

        public ShootFrigidBlockForPlayerCommand SetTick(int tick)
        {
            _tick = tick;
            return this;
        }

        public ShootFrigidBlockForPlayerCommand SetCooldownEndTick(int cooldownEndTick)
        {
            _cooldownEndTick = cooldownEndTick;
            return this;
        }

        public override void ResolveDependencies()
        {
            _frigidBlocksController = _diContainer.Resolve<IFrigidBlocksController>();
        }

        public void Execute()
        {
            _frigidBlocksController.ShootFrigidBlock(_casterPlayerId, _position, _direction, _tick, _cooldownEndTick);
        }
    }
}
