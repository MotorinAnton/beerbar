using Core.Authoring.Characters;
using Core.Constants;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Configs
{
    [InlineEditor]
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(VisualBarmanConfig))]
    public sealed class VisualBarmanConfig : ScriptableObject
    {
        public CharacterAuthoring Prefab;
        public float WalkAnimatorSpeed = 1;
    }
}