using Core.Utilities;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.MainMenu
{
    public class PauseMenuUiAuthoring : EntityBehaviour
    {
        public int SortingOrder => -_sortingOrder;
        
        [SerializeField]
         private int _sortingOrder;
        
        [SerializeField]
        public Button _playButton;
        
        [SerializeField]
        public Button _settingButton;

        [SerializeField]
        public Button _blocker;
        
        [SerializeField]
        public ButtonHoverEffect _textPlayButton;
        
        [SerializeField]
        public ButtonHoverEffect _textSettingButton;
        
        
        private void Start()
        {
            _playButton.onClick.AddListener(PlayClicked);
            _settingButton.onClick.AddListener(SettingsClicked);
        }

        private void PlayClicked()
        {
            EntityUtilities.AddOneFrameComponent<PlayClicked>(Entity);
            _textPlayButton.buttonText.color = _textPlayButton.originalColor;
        }
        
        private void SettingsClicked()
        {
            EntityUtilities.AddOneFrameComponent<SettingsClicked>(Entity);
            _textSettingButton.buttonText.color = _textSettingButton.originalColor;
        }
    }
    
    public struct PauseMenuUi : IComponentData { }

    public class PauseMenuUiView : IComponentData
    {
        public PauseMenuUiAuthoring Value;
    }
    
    public class SpawnPauseMenuUi : IComponentData
    {
        public PauseMenuUiAuthoring PauseMenuUiPrefab;
    }
    
    public struct ResumeClicked : IComponentData { }
    
    // public struct SettingsClicked : IComponentData { }
    //
    // public struct PauseGame : IComponentData { }
}