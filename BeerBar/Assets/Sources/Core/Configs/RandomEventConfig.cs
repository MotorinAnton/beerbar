using System;
using Core.Constants;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Configs
{
    [InlineEditor]
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(RandomEventConfig))]
    
    public sealed class RandomEventConfig : ScriptableObject
    {
        [HorizontalGroup("TimeRange")]
        public int MinTime;
        
        [HorizontalGroup("TimeRange")]
        public int MaxTime;
        
        public RandomEvent[] Events;
    }
    
    [Serializable]
    public struct RandomEvent
    {
        public int Rating;
        public int Chance;
        public BreakdownObject Object;

    }
    
    [Serializable]
    public enum BreakdownObject
    {
        Tube,
        TV,
        Electricity,
        Table,
    }
    
    // public class RandomEventObjectConfigData : IComponentData
    // {
    //     public RandomEventObjectConfig Config;
    // }
    //
    // public struct RandomEventObjectConfigEntity : IComponentData { }
}