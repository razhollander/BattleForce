using System.Collections.Generic;
using Core.Scripts.Services.AudioService;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CoolBGMusicConfig", menuName = "BF/Presentation/Cool BG Music Config")]
    public class CoolBGMusicConfig : ScriptableObject
    {
        public List<AudioData> BackgroundMusics;
    }
}
