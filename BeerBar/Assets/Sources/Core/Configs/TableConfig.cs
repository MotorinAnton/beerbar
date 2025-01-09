using Core.Authoring.Tables;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(TableConfig))]
    
    public sealed class TableConfig : ScriptableObject
    {
        public TableDates[] TablesData;
        public Sprite CleanSprite;
        public Sprite RepairSprite;
    }
    
    public class TableConfigData : IComponentData
    {
        public TableConfig Config;
    }
    
    [System.Serializable]
    public struct TableDates
    {
        public TableAuthoring Prefab;
        public int Level;
        public int Price;
        public int QuantityAtTablePoints;
    }
    public struct TableConfigEntity : IComponentData { }
}