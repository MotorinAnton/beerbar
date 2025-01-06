using System;
using Core.Authoring.Products;
using Core.Constants;
using Sirenix.OdinInspector;
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
        //public 
        //public ContainerUp ContainerUp;
        //public TableUp TableUp;

    }
    [Serializable]
    public class ContainerUp : UpData
    {
        public ProductType ProductType;
    }
    [Serializable]
    public class TableUp : UpData
    {
        public int Count;
    }

    public enum UpVisualType
    {
        Small,
        Big
    }
}