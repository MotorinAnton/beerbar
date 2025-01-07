using Core.Authoring.Characters;
using Core.Constants;
using Sirenix.OdinInspector;
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
}