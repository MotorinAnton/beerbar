using Core.Authoring.Characters;
using Core.Constants;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [InlineEditor]
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(RepairmanConfig))]
    public sealed class RepairmanConfig : ScriptableObject
    {
        public CharacterAuthoring RepairmanPrefab;
        public float WalkAnimatorSpeed = 1;
        public float MoveSpeed = 1.5f;
    }
    public class RepairmanConfigData : IComponentData
    {
        public RepairmanConfig Config;
    }
    public struct RepairmanConfigEntity : IComponentData { }
}