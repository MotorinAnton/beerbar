using Core.Configs;
using Core.Utilities;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Authoring.UpgradeUi
{
    public class UpgradeElementUiAuthoring : EntityBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Sprite UpgradeIcon => _icon.sprite;

        [SerializeField]
        private RectTransform _rectTransform;

        [SerializeField]
        private Image _icon;

        [SerializeField]
        private Image _upgrade;

        [SerializeField]
        private Image _completed;

        [SerializeField]
        private TMP_Text _ratingText;

        // TODO: Добавить провреку - пк или смартфон. На пк действия внутри OnPointerDown и OnPointerUp отключать
        public void OnPointerDown(PointerEventData eventData)
        {
            ShowDescription();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            HideDescription();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowDescription();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideDescription();
        }

        public void SetIcon(Sprite upgradeIcon)
        {
            _icon.sprite = upgradeIcon;
        }

        public void SetRating(int value)
        {
            _ratingText.text = value.ToString();
        }

        private void ShowDescription() => EntityUtilities.AddOneFrameComponent<ShowDescription>(Entity);

        private void HideDescription() => EntityUtilities.AddOneFrameComponent<HideDescription>(Entity);

        public void EnableCompletedIcon()
        {
            _completed.gameObject.SetActive(true);
        }
        
        // TODO включать upgrade Icon
        
        public void EnableUpgradeIcon()
        {
            _upgrade.gameObject.SetActive(true);
        }
    }

    public class UpgradeElementUiView : IComponentData
    {
        public UpgradeElementUiAuthoring UpgradeElementUiAuthoring;
        public Up Up;
    }

    public struct ShowDescription : IComponentData { }

    public struct HideDescription : IComponentData { }
}