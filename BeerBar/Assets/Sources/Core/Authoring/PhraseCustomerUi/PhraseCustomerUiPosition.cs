using UnityEngine;
using Unity.Entities;

namespace Core.Authoring.PhraseCustomerUi
{
    public class PhraseCustomerUiPosition : EntityBehaviour
    {
        public RectTransform EventPanelPoint;
        public RectTransform[] PanelPonts;
    }
    
    public class SpawnPhraseCustomerUiManager : IComponentData
    {
        public PhraseCustomerUiPosition PhraseCustomerUiPrefab;
    }
    
    public class PhraseCustomerUiPositionView : IComponentData
    {
        public PhraseCustomerUiPosition Positions;
    }
}