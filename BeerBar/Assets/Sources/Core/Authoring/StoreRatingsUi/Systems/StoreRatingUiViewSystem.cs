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
            Entities.WithAll<StoreRatingUiView>().ForEach((Entity entity, in StoreRatingUiView storeRatingUiView) =>
            {
                ChangeText(entity, storeRatingUiView);
                
            }).WithoutBurst().Run();
        }

        private void ChangeText(Entity entity, in StoreRatingUiView storeRatingUiView)
        {
            var storeRating = _storeRatingQuery.GetSingleton<StoreRating>();
            storeRatingUiView.StoreRatingText.text = storeRating.CurrentValue.ToString();
            storeRatingUiView.SuccessPointsText.text = storeRating.SuccessPoints.ToString();
        }
    }
}