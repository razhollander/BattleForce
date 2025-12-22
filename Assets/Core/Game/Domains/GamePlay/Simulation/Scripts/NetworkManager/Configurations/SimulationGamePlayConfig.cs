using ASoliman.Utils.EditableRefs;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "SimulationGamePlayConfig", menuName = "BF/Network/GamePlay Config")]
    public class SimulationGamePlayConfig : ScriptableObject
    {
        [EditableRef] public PlayerSpaceshipConfig PlayerSpaceship;
        [EditableRef] public PlayerBulletConfig PlayerBullet;
        [EditableRef] public EnvironmentConfig Environment;
    }
}