using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc
{
    public class SwapFieldController
    {
        private SwapFieldView _swapFieldView;
        private readonly SwapFieldPool _swapFieldPool;

        public readonly ushort SwapFieldId;
        private readonly Transform _swapFieldsParent;

        public SwapFieldController(ushort swapFieldId, SwapFieldPool swapFieldPool, Transform swapFieldsParent)
        {
            _swapFieldPool = swapFieldPool;
            SwapFieldId = swapFieldId;
            _swapFieldsParent = swapFieldsParent;
        }

        public void CreateSwapFieldView(Vector2 position, float radius)
        {
            _swapFieldView = _swapFieldPool.Spawn();
            _swapFieldView.name = "SwapField_" + SwapFieldId;
            _swapFieldView.transform.SetParent(_swapFieldsParent);
            _swapFieldView.SetPosition(position);
            _swapFieldView.SetRadius(radius);
        }

        public void SetRadius(float radius)
        {
            _swapFieldView.SetRadius(radius);
        }
        
        public void SetPosition(Vector2 position)
        {
            _swapFieldView.SetPosition(position);
        }

        public void Destroy()
        {
            _swapFieldView.Despawn();
        }
    }
}