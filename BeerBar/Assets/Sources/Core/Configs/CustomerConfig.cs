using System;
using Core.Constants;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [InlineEditor]
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(CustomerConfig))]
    public sealed class CustomerConfig : ScriptableObject
    {
        public CustomerConfigData[] Customers;
        public int RandomEventChance;
        public BreakBottle BreakBottle;
        public LossWallet[] LossWallets;
        public int DrinkAtTheTableChance;
        public int StartCountCustomers;
        public float Ratio;
        public float MaxWaitTimeInPurchaseQueue;
        public float DrinkAtTheTableTime;
        public float RespawnTime;
        [InfoBox("Runtime variables")]
        [NonSerialized, ShowInInspector, ReadOnly]
        public int CurrentMaxCustomers;
    }
    
    [Serializable]
    public class CustomerConfigData
    {
        [HorizontalGroup("Rating")]
        public int RatingMin;
        [HorizontalGroup("Rating")]
        public int RatingMax;
        public VisualCustomerConfig Visual;
        public AudioCustomerConfig Audio;
        public PresetCustomerPhraseConfig Dialogs;
        public float Speed = 1.5f;
    }
    
    [Serializable]
    public struct BreakBottle
    {
        public int Rating;
    }
    
    [Serializable]
    public struct LossWallet 
    {
        public int Rating;
        public int MinCoins;
        public int MaxCoins;
    }
    
    public class CustomerConfigs : IComponentData
    {
        public CustomerConfig Config;
    }
    public struct CustomerConfigEntity : IComponentData { }
}