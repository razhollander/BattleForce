using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "BF/Network/Environment Config")]
    public class EnvironmentConfig : ScriptableObject
    {
        [SerializeField]
        SerializableDictionary<int, string> _wallsJson = new SerializableDictionary<int, string>();

        public WallConfig[] GetWalls(int index)
        {
            return _wallsJson[index].FromJson<WallConfig[]>();
        }
        
        public void SetWalls(WallConfig[] wallConfigs, int index)
        {
            _wallsJson[index] = wallConfigs.ToJson();
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }
    }
    
    public class WallConfig
    {
        public ushort Id;
        public Vector2[] Points;

        public WallConfig(ushort id, Vector2[] points)
        {
            Id = id;
            Points = points;
        }
    }
}