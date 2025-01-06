using TMPro;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.StoreRatings
{
    public sealed class StoreRatingUiAuthoring : EntityBehaviour
    {
        public int SortingOrder => _sortingOrder;

        public TMP_Text StoreRatingText => _storeRatingText;

        [SerializeField]
        private int _sortingOrder;

        [SerializeField]
        private TMP_Text _storeRatingText;

        public TMP_Text SuccessPointsText => _successPointsText;

        [SerializeField]
        private TMP_Text _successPointsText;
    }

    public struct StoreRatingUi : IComponentData { }

    public class SpawnStoreRatingUi : IComponentData
    {
        public StoreRatingUiAuthoring StoreRatingUiPrefab;
    }

    public class StoreRatingUiView : IComponentData
    {
        public TMP_Text StoreRatingText;
        public TMP_Text SuccessPointsText;
    }
}