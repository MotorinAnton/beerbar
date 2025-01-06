using Core.Authoring.SelectGameObjects;
using Core.Utilities;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.ButtonsUi.SpeedXButton
{
    public class SpeedXButtonUiAuthoring : EntityBehaviour
    {
        [SerializeField]
        
        private Button _speedXButton;
        public Button SpeedXButton => _speedXButton;
        
        [SerializeField]
        
        private TMP_Text _text;
        public TMP_Text Text => _text;
        
        private void Start()
        {
            _speedXButton.onClick.AddListener(SpeedXButtonButtonClicked);
        }

        private void SpeedXButtonButtonClicked()
        {
            EntityUtilities.AddOneFrameComponent<Clicked>(Entity);
        }
    }
    public struct SpeedXButtonUi : IComponentData { }
    
    public class SpawnSpeedXButtonUi : IComponentData
    {
        public SpeedXButtonUiAuthoring SpeedXButtonUiPrefab;
    }
    
    public class SpeedXButtonUiView : IComponentData
    {
        public SpeedXButtonUiAuthoring SpeedXButtonUiAuthoring;
        public int SpeedX;
        
    }
    
}