using Core.Authoring.Containers;
using Core.Authoring.Products;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(ContainerConfig))]
    public sealed class ContainerConfig : ScriptableObject
    {
        public Container[] ContainersData;
    }
    public class ContainerConfigData : IComponentData
    {
        public ContainerConfig Config;
    }
    [System.Serializable]
    public struct Container
    {
        public ContainerAuthoring Prefab;
        public ProductType Type;
        public int Level;
        public int Capacity;
        public int Price;
    }
    public struct ContainerConfigEntity : IComponentData { }
}