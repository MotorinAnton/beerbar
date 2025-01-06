using Core.Authoring.Characters;
using Core.Constants;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(VisualCustomerConfig))]
    public sealed class VisualCustomerConfig : ScriptableObject
    {
        public CharacterAuthoring Prefab;
        public Sprite Avatar;
        public float WalkAnimatorSpeed = 1;
    }
}