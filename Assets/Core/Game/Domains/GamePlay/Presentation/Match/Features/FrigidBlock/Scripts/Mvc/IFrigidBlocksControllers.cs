using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.FrigidBlock.Scripts.Mvc
{
    public interface IFrigidBlocksControllers
    {
        void InitEntryPoint();
        void CreateFrigidBlock(ushort blockId, Vector2 position, Vector2 rotation);
        void InterpolateFrigidBlockTransform(ushort blockId, Vector2 position, Quaternion rotation);
        void DestroyFrigidBlock(ushort blockId);
        void DestroyAll();
    }
}
