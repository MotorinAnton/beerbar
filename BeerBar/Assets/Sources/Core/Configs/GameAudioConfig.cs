using Core.Constants;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(GameAudioConfig))]
    public sealed class GameAudioConfig : ScriptableObject
    {
        public AudioClip[] Coins;
        public AudioClip[] Music;
    }
}