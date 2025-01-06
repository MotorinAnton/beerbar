using Core.Authoring.Characters;
using Core.Constants;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [InlineEditor]
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(ProductKeeperConfig))]
    public sealed class ProductKeeperConfig : ScriptableObject
    {
        public CharacterAuthoring ProductKeeperPrefab;
        public float WalkAnimatorSpeed = 1;
        public float MoveSpeed = 1.5f;
    }
    public class ProductKeeperConfigData : IComponentData
    {
        public ProductKeeperConfig Config;
    }
    public struct ProductKeeperConfigEntity : IComponentData { }
}