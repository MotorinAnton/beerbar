using Core.Constants;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(AudioCustomerConfig))]
    public sealed class AudioCustomerConfig : ScriptableObject
    {
        public AudioClip[] Purchase;
        public AudioClip[] Swears;
        public AudioClip[] Success;
        public AudioClip[] Neutral;
        
    }
}