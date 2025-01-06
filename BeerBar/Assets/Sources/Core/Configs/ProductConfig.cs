using Core.Authoring.Products;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(ProductConfig))]
    public sealed class ProductConfig : ScriptableObject
    {
        public Product[] Products;
    }

    public class ProductConfigData : IComponentData
    {
        public ProductConfig Config;
    }

    [System.Serializable]
    public struct Product
    {
        public ProductType ProductType;
        public GameObject[] Prefabs;
        public Sprite Visual;
        public int Level;
        public int PurchaseCost;
        public int SellPrice;
    }

    public struct ProductConfigEntity : IComponentData { }
}