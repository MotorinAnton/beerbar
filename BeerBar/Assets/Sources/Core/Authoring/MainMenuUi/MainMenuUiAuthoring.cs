using Core.Utilities;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.MainMenu
{
    public class MainMenuUiAuthoring : EntityBehaviour
    {
        public int SortingOrder => -_sortingOrder;
        
        [SerializeField] private int _sortingOrder;
        
        [SerializeField] public Button _playButton;

        [SerializeField] public Button _settingButton;
        
        [SerializeField] public Image _backgroundImage;

        private void Start()
        {
            _playButton.onClick.AddListener(PlayClicked);
            _settingButton.onClick.AddListener(SettingsClicked);
        }

        private void PlayClicked()
        {
            EntityUtilities.AddOneFrameComponent<PlayClicked>(Entity);
        }
        
        private void SettingsClicked()
        {
            EntityUtilities.AddOneFrameComponent<SettingsClicked>(Entity);
        }
    }
    
    public struct MainMenuUi : IComponentData { }

    public class MainMenuUiView : IComponentData
    {
        public MainMenuUiAuthoring Value;
    }
    
    public class SpawnMainMenuUi : IComponentData
    {
        public MainMenuUiAuthoring MainMenuUiPrefab;
    }
    
    public struct PlayClicked : IComponentData { }
    
    public struct SettingsClicked : IComponentData { }
    
    public struct PauseGame : IComponentData { }
}