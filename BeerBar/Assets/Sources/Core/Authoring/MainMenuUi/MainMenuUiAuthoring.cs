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
        
        [SerializeField]
         private int _sortingOrder;
        
        [SerializeField]
        public Button _playButton;
        
        [SerializeField]
        public TMP_Text _textButton;
        
        [SerializeField]
        public Button _settingButton;
        
        [SerializeField]
        public Image _backgroundImage;
        
        [SerializeField]
        public Button _blocker;
        
        
        private void Start()
        {
            _playButton.onClick.AddListener(PlayClicked);
            _settingButton.onClick.AddListener(SettingsClicked);
            //_blocker.onClick.AddListener(PlayClicked);
        }

        private void PlayClicked()
        {
            EntityUtilities.AddOneFrameComponent<PlayClicked>(Entity);
        }
        
        private void SettingsClicked()
        {
            EntityUtilities.AddOneFrameComponent<SettingsClicked>(Entity);
        }
        
        public void SetResumeMenu()
        {
            _textButton.text = "Resume";
            _backgroundImage.gameObject.SetActive(false);
            _blocker.gameObject.SetActive(true);
        }

        public void SetMainMenu()
        {
            _textButton.text = "Play";
            _backgroundImage.gameObject.SetActive(true);
            _blocker.gameObject.SetActive(false);
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