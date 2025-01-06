using Core.Utilities;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.WarehouseUi
{
    public class WarehouseUiAuthoring : EntityBehaviour
    {
        public int SortingOrder => _sortingOrder;

        public Transform WarehouseLayoutParent => _warehouseLayoutParent;

        public Transform OrderLayoutParent => _orderLayoutParent;

        public GameObject RaycastBlocker => _raycastBlocker;

        public Image ProgressBar => _progressBar;

        public TMP_Text CurrentCoinsText => _currentCoinsText;

        public TMP_Text OrderCostText => _orderCostText;

        [SerializeField]
        private int _sortingOrder;

        [SerializeField]
        private Transform _warehouseLayoutParent;

        [SerializeField]
        private Transform _orderLayoutParent;

        [SerializeField]
        private Button _goButton;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private Button _blocker;

        [SerializeField]
        private GameObject _raycastBlocker;

        [SerializeField]
        private Image _progressBar;

        [SerializeField]
        private TMP_Text _currentCoinsText;

        [SerializeField]
        private TMP_Text _orderCostText;

        private void Start()
        {
            _goButton.onClick.AddListener(AddGoClicked);
            _closeButton.onClick.AddListener(AddCloseClicked);
            _blocker.onClick.AddListener(AddCloseClicked);
        }

        private void AddGoClicked()
        {
            EntityUtilities.AddOneFrameComponent<GoClicked>(Entity);
        }

        private void AddCloseClicked()
        {
            EntityUtilities.AddOneFrameComponent<CloseClicked>(Entity);
        }
    }

    public struct WarehouseUi : IComponentData { }

    public class SpawnWarehouseUi : IComponentData
    {
        public WarehouseUiAuthoring WarehouseUiPrefab;
    }

    public class WarehouseUiView : IComponentData
    {
        public WarehouseUiAuthoring WarehouseUiAuthoring;

        public void EnableWarehouseWindow() => WarehouseUiAuthoring.gameObject.SetActive(true);

        public void DisableWarehouseWindow() => WarehouseUiAuthoring.gameObject.SetActive(false);

        public void EnableOrderingOption() => SetOrderingOptionStatus(true);

        public void DisableOrderingOption() => SetOrderingOptionStatus(false);

        public void UpdateOrderProgressBar(float totalTime, float timeLeft)
        {
            WarehouseUiAuthoring.ProgressBar.fillAmount = Mathf.Clamp01(1f - timeLeft / totalTime);
        }

        private void SetOrderingOptionStatus(bool status)
        {
            WarehouseUiAuthoring.RaycastBlocker.SetActive(!status);
            WarehouseUiAuthoring.ProgressBar.fillAmount = 0f;
            WarehouseUiAuthoring.ProgressBar.gameObject.SetActive(!status);
        }
    }

    public struct GoClicked : IComponentData { }

    public struct CloseClicked : IComponentData { }
}