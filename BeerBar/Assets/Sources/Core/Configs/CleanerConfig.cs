using Core.Authoring.Characters;
using Core.Constants;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [InlineEditor]
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(CleanerConfig))]
    public sealed class CleanerConfig : ScriptableObject
    {
        public CharacterAuthoring CleanerPrefab;
        public float WalkAnimatorSpeed = 1f;
        public float MoveSpeed = 1.5f;
    }
    public class CleanerConfigData : IComponentData
    {
        public CleanerConfig Config;
    }
    public struct CleanerConfigEntity : IComponentData { }
}