using Core.Authoring.LoadingUi;
using Core.Save;
using Core.Services;
using Core.Utilities;
using DG.Tweening;
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

        [SerializeField]
        private Button _loadingScreenTest;

        private void Start()
        {
            _masterVolumeSlider.onValueChanged.AddListener(value => GameServicesUtilities
                .Get<SaveParametersService>().Get<MasterVolumeParameter>().Apply(value));

            _closeButton.onClick.AddListener(AddClosedClicked);
            _blocker.onClick.AddListener(AddClosedClicked);

            _loadingScreenTest.onClick.AddListener(TestLoadingScreen);
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

        public void TestLoadingScreen()
        {
            var showLoadingScreen = new ShowLoadingScreen
            {
                AutoHide = true
            };

            EntityManager.AddComponentObject(Entity, showLoadingScreen);

            DOVirtual.Float(0f, 1f, 2f, x => showLoadingScreen.ProgressAction?.Invoke(x));
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