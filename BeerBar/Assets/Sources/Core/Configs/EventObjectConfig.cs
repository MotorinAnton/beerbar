using Core.Authoring.EventObjects;
using Core.Authoring.TVs;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(EventObjectConfig))]
    
    public sealed class EventObjectConfig : ScriptableObject
    {
        public BreakBottleAuthoring BreakBottlePrefab;
        public LossWalletAuthoring LossWalletPrefab;
        public TubeAuthoring TubePrefab;
        public ElectricityAuthoring ElectricityPrefab;
        public TVAuthoring TVPrefab;
    }
    
    public class EventObjectConfigData : IComponentData
    {
        public EventObjectConfig Config;
    }
    
    public struct EventObjectConfigEntity : IComponentData { }
}