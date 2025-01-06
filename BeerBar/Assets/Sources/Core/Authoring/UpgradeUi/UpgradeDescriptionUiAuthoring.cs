using Core.Configs;
using DG.Tweening;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.UpgradeUi
{
    public class UpgradeDescriptionUiAuthoring : EntityBehaviour
    {
        public int SortingOrder => _sortingOrder;

        public Image UpgradeIcon => _upgradeIcon;

        public TMP_Text RatingText => _ratingText;

        public TMP_Text PriceText => _priceText;

        public TMP_Text NameText => _nameText;

        public TMP_Text DescriptionText => _descriptionText;

        [SerializeField]
        private int _sortingOrder;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private Image _upgradeIcon;

        [SerializeField]
        private TMP_Text _nameText;

        [SerializeField]
        private TMP_Text _descriptionText;

        [SerializeField]
        private TMP_Text _ratingText;

        [SerializeField]
        private TMP_Text _priceText;

        private Tween _showTween;

        public void Show(float duration)
        {
            RefreshTween();

            gameObject.SetActive(true);
            _showTween = _canvasGroup.DOFade(1f, duration);
        }

        public void Hide(float duration)
        {
            RefreshTween();

            _showTween = _canvasGroup.DOFade(0f, duration)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void RefreshTween()
        {
            if (_showTween != null)
            {
                _showTween.Kill();
                _showTween = null;
            }
        }
    }

    public struct UpgradeDescriptionUi : IComponentData { }

    public class UpgradeDescriptionUiView : IComponentData
    {
        public UpgradeDescriptionUiAuthoring UpgradeDescriptionUiAuthoring;

        public void SetData(Up up, Sprite icon)
        {
            UpgradeDescriptionUiAuthoring.UpgradeIcon.sprite = icon;

            // TODO: Add name instead of type
            UpgradeDescriptionUiAuthoring.NameText.text = $"{up.UpType}";

            UpgradeDescriptionUiAuthoring.DescriptionText.text = $"{up.Description}";

            UpgradeDescriptionUiAuthoring.RatingText.text = $"{up.Rating}";

            // TODO: Add price 
            UpgradeDescriptionUiAuthoring.PriceText.text = $"{up.Rating}";
        }

        public void Show(float duration) => UpgradeDescriptionUiAuthoring.Show(duration);

        public void Hide(float duration) => UpgradeDescriptionUiAuthoring.Hide(duration);
    }
}