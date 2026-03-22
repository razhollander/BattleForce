using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.SwapFields.Scripts.Mvc
{
    public class SwapFieldControllers : ISwapFieldControllers
    {
        private readonly SwapFieldPool _swapFieldPool;
        private readonly List<SwapFieldController> _swapFieldControllers = new ();
        private Transform _swapFieldsParent;
        
        public SwapFieldControllers(SwapFieldView swapFieldViewPrefab, DiContainer diContainer)
        {
            _swapFieldPool = new SwapFieldPool(swapFieldViewPrefab, diContainer);
        }

        public void InitEntryPoint()
        {
            _swapFieldsParent = (new GameObject("SwapFieldsParent")).transform;
            _swapFieldPool.InitPool();
        }

        public void CreateSwapField(ushort swapFieldId, float swapFieldRadius, Vector2 position)
        {
            var swapFieldController = new SwapFieldController(swapFieldId, _swapFieldPool, _swapFieldsParent);
            swapFieldController.CreateSwapFieldView(position, swapFieldRadius);
            _swapFieldControllers.Add(swapFieldController);
        }

        public void SetSwapFieldTransform(ushort swapFieldId, Vector2 position, float radius)
        {
            var swapFieldController = GetSwapField(swapFieldId);
            swapFieldController.SetPosition(position);
            swapFieldController.SetRadius(radius);
        }

        public void DestroySwapField(ushort swapFieldId)
        {
            var swapFieldController = GetSwapField(swapFieldId);
            swapFieldController.Destroy();
            _swapFieldControllers.Remove(swapFieldController);
        }

        private SwapFieldController GetSwapField(ushort swapFieldId)
        {
            return _swapFieldControllers.Find(x => x.SwapFieldId == swapFieldId);
        }

        public void DestroyAll()
        {
            foreach (var swapFieldController in _swapFieldControllers)
            {
                swapFieldController.Destroy();
            }
            _swapFieldControllers.Clear();
        }
    }
}