using Core.Authoring.SelectGameObjects;
using Core.Configs;
using Core.Utilities;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.ButtonsUi.AddButton
{
    public class AddButtonUiAuthoring : EntityBehaviour
    {
        [SerializeField]
        private Button _addButton;
        public Button AddButton => _addButton;
        
        
        private void Start()
        {
            _addButton.onClick.AddListener(AddButtonClicked);
        }

        private void AddButtonClicked()
        {
            EntityUtilities.AddOneFrameComponent<Clicked>(Entity);
        }
    }
    
    public class SpawnAddButtonUi : IComponentData
    {
        public AddButtonUiAuthoring AddButtonUiPrefab;
        public Entity SpawnPointEntity;
        public Up UpData;
        public int IndexLevelUpFX;
    }
    
    public class AddButtonUiView : IComponentData
    {
        public AddButtonUiAuthoring AddButtonUiAuthoring;
        public Entity SpawnPointEntity;
        public Up UpData;
        public int IndexLevelUpFX;
    }
}