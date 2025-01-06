using Core.Authoring.StoreRatings;
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

            Entities
                .WithAll<UpgradeBarUiView>()
                .ForEach((Entity entity, in UpgradeBarUiView upgradeBarUiView) =>
                {
                    upgradeBarUiView.SetRating((int)currentStoreRating.CurrentValue);
                }).WithoutBurst().Run();

            // TODO: Нужна оптимизация?
            var upCompletedEntity = _completedUpQuery.ToEntityArray(Allocator.Temp)[0];
            var availableUp = EntityManager.GetComponentObject<AvailableUp>(upCompletedEntity).AvailableUps;
            var completedUp = EntityManager.GetComponentObject<CompletedUp>(upCompletedEntity).CompleteUp;

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