using Core.Authoring.NoteBookShops;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(WarehouseConfig))]
    public class WarehouseConfig : ScriptableObject
    {
        public NoteBookShopAuthoring NoteBookShopPrefab;
        public float OrderTime;
    }

    public class WarehouseConfigData : IComponentData
    {
        public WarehouseConfig Config;
    }

    public struct WarehouseConfigEntity : IComponentData { }
}