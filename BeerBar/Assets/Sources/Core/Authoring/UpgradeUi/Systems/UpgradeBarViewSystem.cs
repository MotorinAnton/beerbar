using Core.Authoring.StoreRatings;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.UpgradeUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class UpgradeBarViewSystem : SystemBase
    {
        private EntityQuery _storeRatingQuery;

        private EntityQuery _completedUpQuery;

        protected override void OnCreate()
        {
            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAll<StoreRating>().Build(this);

            using var completedUpBuilder = new EntityQueryBuilder(Allocator.Temp);
            _completedUpQuery = completedUpBuilder.WithAll<CompletedUp>().Build(this);
        }

        protected override void OnUpdate()
        {
            var currentStoreRating = _storeRatingQuery.GetSingleton<StoreRating>();
            var upCompletedEntity = _completedUpQuery.ToEntityArray(Allocator.Temp)[0];
            var completedUp = EntityManager.GetComponentObject<CompletedUp>(upCompletedEntity).CompleteUp;
            var availableUp = EntityManager.GetComponentObject<AvailableUp>(upCompletedEntity).AvailableUps;
            Entities.WithAll<UpgradeBarUiView>()
                .ForEach((Entity entity, in UpgradeBarUiView upgradeBarUiView) =>
                {
                    var upLine = EntityUtilities.GetGameConfig().UpConfig.UpLine;
                    var maxValue = upgradeBarUiView.UpgradeBarUiAuthoring.RatingSlider.maxValue;
                    var s = ((int)maxValue / upLine.Length) * (completedUp.Count  + availableUp.Count);
                    
                    upgradeBarUiView.SetRating(s);
                    
                }).WithoutBurst().Run();

            Entities
                .WithAll<UpgradeElementUiView>()
                .ForEach((Entity entity, in UpgradeElementUiView upgradeElementUiView) =>
                {
                    if (completedUp.Contains(upgradeElementUiView.Up))
                    {
                        upgradeElementUiView.UpgradeElementUiAuthoring.EnableCompletedIcon();
                    }
                }).WithoutBurst().Run();
        }
    }
}