using ASoliman.Utils.EditableRefs;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "GamePlayConfig", menuName = "BF/Network/GamePlay Config")]
    public class GamePlayConfig : ScriptableObject
    {
        [EditableRef] public PlayerSpaceshipConfig PlayerSpaceship;
        [EditableRef] public PlayerBulletConfig PlayerBullet;
    }
}