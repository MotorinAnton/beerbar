using Core.Components.Destroyed;
using DG.Tweening;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.PhraseCustomerUi
{
    public sealed class PhraseCustomerUiAuthoring : EntityBehaviour
    {
        public Image Avatar => _avatarImage;
       

        [SerializeField]
        private Image _avatarImage;
        
        public Image Background => _backgroundImage;

        [SerializeField]
        private Image _backgroundImage;
        
        public Image AvatarBorder => _avatarBorderImage;

        [SerializeField]
        private Image _avatarBorderImage;
        
        public Image[] ProductCustomer => _productCustomer;

        [SerializeField] 
        private Image[] _productCustomer;

        public TMP_Text Text => _text;
            
        [SerializeField]
        private TMP_Text _text;
        
        public CanvasGroup CanvasGroup => _canvasGroup;

        [SerializeField]
        private CanvasGroup _canvasGroup;
        
        public RectTransform RectTransform => _rectTransform;

        [SerializeField]
        private RectTransform _rectTransform;
        
        public int _index;
        public Entity _customer;
        public Sequence _sequence;
        
        public void SetEventPanelUiComponent(string text, Sprite avatar)
        {
            Text.text = text;
            Avatar.sprite = avatar;

            foreach (var product in ProductCustomer)
            {
                product.gameObject.SetActive(false);
            }
        }
        
        public void SetPhraseComponent(string text, Sprite avatar, Sprite[] products, Entity customerEntity, int index, int indexPosition)
        {
            Text.text = text;
            Avatar.sprite = avatar;
            _index = index;
            _customer = customerEntity;
            
            if (products.Length > 1)
            {
                ProductCustomer[0].gameObject.SetActive(true);
                ProductCustomer[0].sprite = products[0];
                ProductCustomer[1].gameObject.SetActive(true);
                ProductCustomer[1].sprite = products[1];
                return;
            }
            
            ProductCustomer[0].gameObject.SetActive(true);
            ProductCustomer[1].gameObject.SetActive(false);
            ProductCustomer[0].sprite = products[0];
            
        }
        
        public void PanelFadeIn(Vector3 position)
        {
            gameObject.SetActive(true);
            var startPosition = RectTransform.position;
            startPosition.y -= 30f;
            CanvasGroup.alpha = 0;
            _rectTransform.position = startPosition;

            var newSequence = DOTween.Sequence();
            var tweenPosition = RectTransform.DOAnchorPos3D(position, 0.9f).SetEase(Ease.OutQuint);
            var tweenFade = CanvasGroup.DOFade(1, 0.5f);
            
            newSequence.Append(tweenPosition);
            newSequence.Join(tweenFade);
            _sequence = newSequence;
        }
        
        public void PanelFadeOut()
        {
            RefreshSequence();
            var newSequence = DOTween.Sequence();
            var targetPosition = RectTransform.localPosition;
            targetPosition.x += 20f;
            var tweenPosition = RectTransform.DOAnchorPos3D(targetPosition, 0.2f).SetEase(Ease.Linear);
            var tweenFade = CanvasGroup.DOFade(0, 0.1f);
            
            newSequence.Append(tweenPosition);
            newSequence.Join(tweenFade);
            newSequence.AppendCallback(DestroyPanelEntity);
            EntityManager.AddComponent<PanelFadeOut>(Entity);
        }
        
        public void EventPanelFadeIn( Vector3 eventPanelPosition)
        {
            //RefreshSequence();
            gameObject.SetActive(true);
            CanvasGroup.alpha = 0;
            var startPosition = eventPanelPosition;
            startPosition.x += 30f;
            RectTransform.localPosition = startPosition;
            var newSequence = DOTween.Sequence();
            var tweenPosition = RectTransform.DOAnchorPos3D(eventPanelPosition, 0.7f).SetEase(Ease.OutQuint);
            var tweenFade = CanvasGroup.DOFade(1, 0.4f);
            newSequence.Append(tweenPosition);
            newSequence.Join(tweenFade);
            newSequence.AppendInterval(1f);
            var tweenEndPosition = RectTransform.DOAnchorPos3D(startPosition, 0.2f).SetEase(Ease.OutQuint);
            var tweenEndFade = CanvasGroup.DOFade(0, 0.2f);

            newSequence.Append(tweenEndPosition);
            newSequence.Join(tweenEndFade);
            newSequence.AppendCallback(DestroyEventPanel);
            _sequence = newSequence;
        }
        
        private void RefreshSequence()
        {
            if (_sequence == null)
            {
                return;
            }
            
            _sequence.Kill();
            _sequence = null;
        }
        
        private void DestroyEventPanel()
        {
            EntityManager.RemoveComponent<EventPanelCustomerIsShow>(_customer);
            EntityManager.AddComponent<Destroyed>(Entity);
        }

        private void DestroyPanelEntity()
        {
            EntityManager.AddComponent<Destroyed>(Entity);
        }
    }
    
    public struct PhrasePanelCustomerUi : IComponentData { }
    
    public struct EventPhrasePanelCustomerUi : IComponentData { }
    
    public struct StartTweenPhraseCustomerUi : IComponentData { }
    
    public struct UpdatePhraseCustomerList : IComponentData { }
    
    public struct MovePhrasePanels : IComponentData { }

    public struct TweenProcessing : IComponentData { }
    
    public struct PanelFadeOut : IComponentData { }
    
    public struct PanelCustomerIsShow: IComponentData { }
    
    public struct EventPanelCustomerIsShow: IComponentData { }
    
    public class PhraseCustomerUiView : IComponentData
    {
        public PhraseCustomerUiAuthoring Value;
    }
    public class SpawnPhrasePanelCustomerUi : IComponentData
    {
        public Entity Customer;
        public int Index;
    }
 
    public class SpawnEventPhrasePanelCustomerUi : IComponentData
    {
        public Entity Customer;
        public EventPhraseType Type;
    }
    
    public enum EventPhraseType
    {
        Swear,
        DirtyTable,
        Displeased
    }
}