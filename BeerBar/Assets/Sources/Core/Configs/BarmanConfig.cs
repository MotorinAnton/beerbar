using Core.Authoring.Bartenders.AddBarmanFX;
using Core.Constants;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [InlineEditor]
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(BarmanConfig))]
    public sealed class BarmanConfig : ScriptableObject
    {
        public VisualBarmanConfig[] Visual;
        public AddBarmanFXAuthoring AddBarmanFX;
        public int UpgradePrice;
        public int Price;
        public float MoveSpeed = 1.5f;
    }
    public class BarmanConfigData : IComponentData
    {
        public BarmanConfig Config;
    }
    public struct BarmanConfigEntity : IComponentData { }
}