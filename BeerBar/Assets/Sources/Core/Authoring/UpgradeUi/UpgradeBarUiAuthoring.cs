using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.UpgradeUi
{
    public class UpgradeBarUiAuthoring : EntityBehaviour
    {
        public int SortingOrder => _sortingOrder;

        public RectTransform IconsParentRectTransform => _iconsParentRectTransform;

        public RectTransform ContentRectTransform => _contentRectTransform;

        public ScrollRect ScrollRect => _scrollRect;

        public Slider RatingSlider => _ratingSlider;

        public Button ArrowLeft => _arrowLeft;

        public Button ArrowRight => _arrowRight;

        [SerializeField]
        private int _sortingOrder;

        [SerializeField]
        private RectTransform _iconsParentRectTransform;

        [SerializeField]
        private RectTransform _contentRectTransform;

        [SerializeField]
        private ScrollRect _scrollRect;

        [SerializeField]
        private Slider _ratingSlider;

        [SerializeField]
        private Button _arrowLeft;

        [SerializeField]
        private Button _arrowRight;

        public void SetMaximumRating(int value)
        {
            _ratingSlider.maxValue = value;
        }

        public void SetRating(int value)
        {
            _ratingSlider.value = value;
        }

        public void UpdateContentWidth()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_iconsParentRectTransform);

            _contentRectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal, _iconsParentRectTransform.rect.width);

            _scrollRect.horizontalNormalizedPosition = 1f - _ratingSlider.value / _ratingSlider.maxValue;
        }
    }

    public struct UpgradeBarUi : IComponentData { }

    public class SpawnUpgradeUi : IComponentData
    {
        public UpgradeBarUiAuthoring UpgradeBarUiPrefab;
        public UpgradeElementUiAuthoring UpgradeElementUiSmallPrefab;
        public UpgradeElementUiAuthoring UpgradeElementUiBigPrefab;
        public UpgradeDescriptionUiAuthoring UpgradeDescriptionUiPrefab;
    }

    public class UpgradeBarUiView : IComponentData
    {
        public UpgradeBarUiAuthoring UpgradeBarUiAuthoring;

        public void SetRating(int value) => UpgradeBarUiAuthoring.SetRating(value);
    }
}