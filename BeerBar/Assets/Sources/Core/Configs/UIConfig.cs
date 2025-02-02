using Core.Authoring.ButtonsUi.AddButton;
using Core.Authoring.ButtonsUi.SpeedXButton;
using Core.Authoring.CoinsUi;
using Core.Authoring.ContainersUI;
using Core.Authoring.Customers.CustomersUi;
using Core.Authoring.LoadingUi;
using Core.Authoring.MainMenu;
using Core.Authoring.NoteBookShops;
using Core.Authoring.ParametersUi;
using Core.Authoring.PauseButtonUi;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.ProfitUi;
using Core.Authoring.RootCanvas;
using Core.Authoring.StoreRatings;
using Core.Authoring.TextProductTablesUI;
using Core.Authoring.UpgradeAndEventButtonsUi;
using Core.Authoring.UpgradeUi;
using Core.Authoring.Warehouses;
using Core.Authoring.WarehouseUi;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Configs
{
    [CreateAssetMenu(menuName = AssetConstants.ConfigMenuPath + nameof(UIConfig))]
    public class UIConfig : ScriptableObject
    {
        public RootCanvasAuthoring RootCanvasPrefab;
        public CoinsUiAuthoring CoinsUiPrefab;
        public StoreRatingUiAuthoring StoreRatingUiPrefab;
        public WarehouseProductUiAuthoring WarehouseProductUiPrefab;
        public WarehouseProductUiAuthoring WarehouseOrderElementUiPrefab;
        public WarehouseUiAuthoring WarehouseUiPrefab;
        public PhraseCustomerUiPosition PhraseCustomerUiManagerPrefab;
        public PhraseCustomerUiAuthoring PhraseCustomerUiPrefab;
        public AddButtonUiAuthoring AddButtonUiPrefab;
        public UpgradeAndEvenButtonUiAuthoring UpgradeAndEventButtonUiPrefab;
        public CustomerUiAuthoring CustomerUiPrefab;
        public PauseButtonUiAuthoring PauseButtonUiPrefab;
        public ParametersUiAuthoring ParametersUiPrefab;
        public LoadingScreenUiAuthoring LoadingScreenUiPrefab;
        public MainMenuUiAuthoring MainMenuUiPrefab;
        public PauseMenuUiAuthoring PauseMenuUiPrefab;
        public UpgradeBarUiAuthoring UpgradeBarUiPrefab;
        public UpgradeElementUiAuthoring UpgradeElementUiSmallPrefab;
        public UpgradeElementUiAuthoring UpgradeElementUiBigPrefab;
        public UpgradeDescriptionUiAuthoring UpgradeDescriptionUiPrefab;
        public TextProductTableUIAuthoring TextProductTableUIPrefab;
        public SpeedXButtonUiAuthoring SpeedXButtonUiPrefab;
        public ContainerUiAuthoring ContainerUiPrefab;
        public ProfitUiAuthoring ProfitCoinPrefab;
    }
    public class UIConfigData : IComponentData
    {
        public UIConfig Config;
    }
    public struct UIConfigEntity : IComponentData { }
}