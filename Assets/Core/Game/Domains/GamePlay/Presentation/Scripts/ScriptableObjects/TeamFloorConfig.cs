using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TeamFloorConfig", menuName = "BF/Presentation/Team Floor Config")]
    public class TeamFloorConfig : ScriptableObject
    {
        public SerializableDictionary<ushort, Material> TeamFloorMaterialPerTeamId;
    }
}