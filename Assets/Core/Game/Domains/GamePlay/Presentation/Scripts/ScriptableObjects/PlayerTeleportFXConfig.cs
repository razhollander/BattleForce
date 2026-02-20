using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerTeleportFXConfig", menuName = "BF/Presentation/Player Teleport FX Config")]
    public class PlayerTeleportFXConfig : ScriptableObject
    {
        public MVC.PlayerTeleportFX.PlayerTeleportFXView Prefab;
    }
}
