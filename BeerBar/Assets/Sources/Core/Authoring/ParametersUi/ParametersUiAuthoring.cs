using Core.Save;
using Core.Services;
using Core.Utilities;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.ParametersUi
{
    public class ParametersUiAuthoring : EntityBehaviour
    {
        public int SortingOrder => _sortingOrder;

        [SerializeField]
        private int _sortingOrder;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private Button _blocker;

        [SerializeField]
        private Slider _masterVolumeSlider;
        
        private void Start()
        {
            _masterVolumeSlider.onValueChanged.AddListener(value => GameServicesUtilities
                .Get<SaveParametersService>().Get<MasterVolumeParameter>().Apply(value));

            _closeButton.onClick.AddListener(AddClosedClicked);
            _blocker.onClick.AddListener(AddClosedClicked);
            
        }

        public void OpenParametersWindow()
        {
            _masterVolumeSlider.value = GameServicesUtilities
                .Get<SaveParametersService>().Get<MasterVolumeParameter>().Get();
            gameObject.SetActive(true);
        }

        public void CloseParametersWindow()
        {
            gameObject.SetActive(false);
        }

        private void AddClosedClicked()
        {
            EntityUtilities.AddOneFrameComponent<CloseClicked>(Entity);
        }
    }

    public struct ParametersUi : IComponentData { }

    public class SpawnParametersUi : IComponentData
    {
        public ParametersUiAuthoring ParametersUiPrefab;
    }

    public class ParametersUiView : IComponentData
    {
        public ParametersUiAuthoring ParametersUiAuthoring;
    }
    
    public struct CloseClicked : IComponentData { }
}