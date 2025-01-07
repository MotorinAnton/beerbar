using System;
using Core.Authoring.Products;
using Core.Constants;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(UpConfig))]
    
    public sealed class UpConfig : ScriptableObject
    {
        public Up[] UpLine;
    }
    [Serializable]
    public struct Up
    {
        public int Rating;
        public UpType UpType;
        public UpVisualType UpVisualType;
        public Sprite Icon;
        public string Description;
        
        //[HideReferenceObjectPicker]  
        //public UpData UpData;
    }

    public enum UpType
    {
        BottleBeerContainer,
        FishSnackContainer,
        MiniSnackContainer,
        NutsContainer,
        SpillContainer,
        UpTable,
        AddTable,
        UpBarman,
        AddBarman
    }
    [Serializable]
    public class UpData
    {
        public UpType UpType;
        public int Level;
    }
    
    [Serializable]
    public class ContainerUp : UpData
    {
        public ProductType ProductType;
    }
    
    [Serializable]
    public enum UpVisualType
    {
        Small,
        Big
    }
}