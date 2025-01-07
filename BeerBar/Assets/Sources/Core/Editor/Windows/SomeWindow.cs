using Core.Authoring.StoreRatings.Systems;
using Core.Configs;
using Core.Constants;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Editor.Windows
{
    public class PivnuskaConfigs : OdinEditorWindow
    {
        [FormerlySerializedAs("CustomerConfig")] [FormerlySerializedAs("CustomerConfigDatas")] [InlineEditor]
        public CustomerConfig Customers;
        public int MaximumRating;

        [PropertyRange(0, "$MaximumRating")]
        [OnValueChanged("UpdateCurrentRating")]
        public int CurrentRating;
        
        public BarmanConfig Barmans;
        
        public ProductKeeperConfig ProductKeeper;
        
        public RepairmanConfig Repairman;

        public CleanerConfig Cleaner;
        
        private StoreRatingSystem _storeRatingSystem;
        
        public RandomEventConfig RandomEventConfig;

        [UnityEditor.MenuItem("Pivnushka/Settings")]
        private static void OpenWindow()
        {
            var window = GetWindow<PivnuskaConfigs>();

            RefreshWindowData(window);

            window.Show();
        }
        
        

        private void OnFocus()
        {
            RefreshWindowData(GetWindow<PivnuskaConfigs>());
        }

        private static void RefreshWindowData(PivnuskaConfigs window)
        {
            window.Customers = Resources.Load<CustomerConfig>(ResourceConstants.CustomerConfig);
            window.Barmans = Resources.Load<BarmanConfig>(ResourceConstants.BarmanConfig);
            window.ProductKeeper = Resources.Load<ProductKeeperConfig>(ResourceConstants.ProductKeeperConfig);
            window.Repairman = Resources.Load<RepairmanConfig>(ResourceConstants.RepairmanConfig);
            window.Cleaner = Resources.Load<CleanerConfig>(ResourceConstants.CleanerConfig);
            window.RandomEventConfig = Resources.Load<RandomEventConfig>(ResourceConstants.RandomEventConfig);
            //window._storeRatingSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<StoreRatingSystem>();
            
        }

        // Invokes on CurrentRating value changing.
        // ReSharper disable once UnusedMember.Global
        // private void UpdateCurrentRating()
        // {
        //     _storeRatingSystem.SetRating(CurrentRating);
        // }
    }

    public class SomeType
    {
        [TableColumnWidth(50)] public bool Toggle;

        [AssetsOnly] public GameObject SomePrefab;

        public string Message;

        [TableColumnWidth(160)]
        [HorizontalGroup("Actions")]
        public void Test1()
        {
        }

        [HorizontalGroup("Actions")]
        public void Test2()
        {
        }
    }
}