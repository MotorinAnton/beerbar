using Core.Authoring.StoreRatings;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.CoinsUi.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class StoreRatingUiViewSystem : SystemBase
    {
        private EntityQuery _storeRatingQuery;

        protected override void OnCreate()
        {
            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAll<StoreRating>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<StoreRatingUiView>().ForEach((in StoreRatingUiView storeRatingUiView) =>
            {
                ChangeText(storeRatingUiView);
            }).WithoutBurst().Run();
        }

        private void ChangeText(in StoreRatingUiView storeRatingUiView)
        {
            var storeRating = _storeRatingQuery.GetSingleton<StoreRating>();
            storeRatingUiView.StoreRatingText.text = "Rating: " + storeRating.CurrentValue;
            storeRatingUiView.SuccessPointsText.text = "Success: " + storeRating.SuccessPoints;
        }
    }
}