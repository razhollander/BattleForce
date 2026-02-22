using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.TeleportGate.Scripts.Mvcs.EnvironmentTeleportGate
{
    public class EnvironmentTeleportGateController
    {
        private readonly EnvironmentTeleportGateView _view;
        private readonly EnvironmentTeleportGateModel _model;

        public EnvironmentTeleportGateController(EnvironmentTeleportGateView view, EnvironmentTeleportGateModel model)
        {
            _view = view;
            _model = model;
        }

        public void Init(Vector2 size, Color color)
        {
            _view.transform.position = _model.Position.ToUnityVector2();
            _view.transform.rotation = Quaternion.Euler(0, 0, _model.Rotation);
            _view.SetSize(size);
            _view.SetColor(color);
        }

        public void PlayAnimation()
        {
            _view.PlayTeleportAnimation();
        }

        public ushort PairId => _model.PairId;
        public bool IsGateB => _model.IsGateB;
    }
}
