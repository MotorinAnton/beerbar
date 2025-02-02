using Core.Authoring.ButtonsUi.AddButton;
using Core.Authoring.CoinsUi;
using Core.Authoring.MainMenu;
using Core.Authoring.StoreRatings;
using Core.Authoring.UpgradeUi;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.MainMenuUi.Systems
{
    public partial class PauseMenuUiViewSystem : SystemBase
    {
        private EntityQuery _pauseMenuUiQuery;
        private EntityQuery _storeRatingQuery;
        private EntityQuery _upgradeBarUiQuery;
        private EntityQuery _coinsUiQuery;
        private EntityQuery _addButtonUiQuery;
        
        protected override void OnCreate()
        {
            using var pauseMenuUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _pauseMenuUiQuery = pauseMenuUiBuilder.WithAll<PauseMenuUi, PauseMenuUiView>().Build(this);
            
            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAll<StoreRatingUi, StoreRatingUiView>().Build(this);
            
            using var upgradeBarUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _upgradeBarUiQuery = upgradeBarUiBuilder.WithAll<UpgradeBarUi, UpgradeBarUiView>().Build(this);
            
            using var coinsUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _coinsUiQuery = coinsUiBuilder.WithAll<CoinsUi.CoinsUi, CoinsUiView>().Build(this);
            
            using var addButtonUiBuilder = new EntityQueryBuilder(Allocator.Temp);
            _addButtonUiQuery = addButtonUiBuilder.WithAll<AddButtonUiView>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<PauseMenuUiView, PauseGame, PlayClicked>().ForEach(
                (Entity entity, in PauseMenuUiView mainMenuUiView) =>
                {
                    Resume(entity, mainMenuUiView);
                }).WithoutBurst().WithStructuralChanges().Run();
        }
        
        private void Resume (Entity entity, in PauseMenuUiView pauseMenuUiView)
        {
            var storeRatingUi = _storeRatingQuery.GetSingleton<StoreRatingUiView>();
            storeRatingUi.StoreRatingText.gameObject.SetActive(true);
            storeRatingUi.SuccessPointsText.gameObject.SetActive(true);
            
            var upgradeBarUi = _upgradeBarUiQuery.GetSingleton<UpgradeBarUiView>().UpgradeBarUiAuthoring;
            upgradeBarUi.gameObject.SetActive(true);

            var coinsUi = _coinsUiQuery.GetSingleton<CoinsUiView>();
            coinsUi.Text.gameObject.SetActive(true);

            var addButtonArray = _addButtonUiQuery.ToEntityArray(Allocator.Temp);

            foreach (var addButtonEntity in addButtonArray)
            {
                var addButtonUiView = EntityManager.GetComponentObject<AddButtonUiView>(addButtonEntity).AddButtonUiAuthoring;
                addButtonUiView.gameObject.SetActive(true);
            }
            
            
            EntityManager.RemoveComponent<PauseGame>(entity);
            pauseMenuUiView.Value.gameObject.SetActive(false);
            UnityEngine.Time.timeScale = 1f;
        }
    }
}