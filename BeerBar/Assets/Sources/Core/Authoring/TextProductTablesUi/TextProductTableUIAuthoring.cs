using TMPro;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.TextProductTablesUI
{
    public sealed class TextProductTableUIAuthoring : EntityBehaviour
    {
        [SerializeField]
        private TMP_Text[] _productTexts;
        
        public TMP_Text[] ProductTexts => _productTexts;
        
    }
    
    public class SpawnTextProductTableUI : IComponentData
    {
        public TextProductTableUIAuthoring TextProductTablesUIPrefab;
    }
    public struct TextProductTableUI : IComponentData { }
    
    public class TextProductTableUIView : IComponentData
    {
        public TextProductTableUIAuthoring Value;
    }
    
}