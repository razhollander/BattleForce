using System.Numerics;
using Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel;
using Core.Game.Domains.GamePlay.Simulation.Scripts.Physics;
using CoreDomain.Scripts.Services.CommandFactory;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.Commands
{
    // The single place any talent shoves the score gate. Impulse and spin are given per unit mass / inertia, so the
    // tuning numbers stay stable when the gate's density is changed. Callers must set every field they care about;
    // set the impulse to zero for a spin-only hit (chicken egg) and the spin to zero for a pure push.
    public class PushScoreGateCommand : BaseCommand, ICommandVoid
    {
        private IMatchDataService _matchDataService;
        private IPhysicsSimulator _physicsSimulator;

        private ushort _scoreGateId;
        private Vector2 _impulsePerUnitMass;
        private Vector2 _worldContactPoint;
        private float _extraSpinImpulsePerUnitInertia;

        public PushScoreGateCommand SetScoreGateId(ushort scoreGateId)
        {
            _scoreGateId = scoreGateId;
            return this;
        }

        public PushScoreGateCommand SetImpulse(Vector2 impulsePerUnitMass)
        {
            _impulsePerUnitMass = impulsePerUnitMass;
            return this;
        }

        public PushScoreGateCommand SetWorldContactPoint(Vector2 worldContactPoint)
        {
            _worldContactPoint = worldContactPoint;
            return this;
        }

        public PushScoreGateCommand SetExtraSpinImpulse(float extraSpinImpulsePerUnitInertia)
        {
            _extraSpinImpulsePerUnitInertia = extraSpinImpulsePerUnitInertia;
            return this;
        }

        public override void ResolveDependencies()
        {
            _matchDataService = _diContainer.Resolve<IMatchDataService>();
            _physicsSimulator = _diContainer.Resolve<IPhysicsSimulator>();
        }

        public void Execute()
        {
            if (!_matchDataService.SimulationState.TryGetScoreGateIndexById(_scoreGateId, out _))
            {
                return;
            }

            var body = _physicsSimulator.GetScoreGate(_scoreGateId);
            body.ApplyLinearImpulse(_impulsePerUnitMass * body.GetMass(), _worldContactPoint); // off-centre point already induces spin

            if (_extraSpinImpulsePerUnitInertia != 0f)
            {
                body.ApplyAngularImpulse(_extraSpinImpulsePerUnitInertia * body.GetInertia());
            }
        }
    }
}
