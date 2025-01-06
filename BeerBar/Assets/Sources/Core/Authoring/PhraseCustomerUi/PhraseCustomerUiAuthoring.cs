using System.Collections.Generic;
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

        public bool IsShow;
        public int Index;
        public Entity Customer;
        public Sequence _showSequence;


        public void SetPhrasePanelUiComponent(string text, Sprite avatar)
        {
            Text.text = text;
            Avatar.sprite = avatar;
            IsShow = true;

            foreach (var product in ProductCustomer)
            {
                product.gameObject.SetActive(false);
            }
        }
        
        public void SetPhraseComponent(string text, Sprite avatar, Sprite[] products, Entity customerEntity, int index)
        {
            Text.text = text;
            Avatar.sprite = avatar;
            Index = index;
            //IsShow = true;
            //gameObject.SetActive(true);
            Customer = customerEntity;
            
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
        
        public Sequence PanelFadeIn(Vector3 position)
        {
            RefreshTween();
            EnablePanel();
            
            var startPosition = new Vector3();

            CanvasGroup.alpha = 0;
            startPosition = RectTransform.position;
            startPosition.y -= 30f;

            _rectTransform.position = startPosition;

            var newSequence = DOTween.Sequence();
            var tweenPosition = RectTransform.DOAnchorPos3D(position, 0.9f).SetEase(Ease.OutQuint);
            var tweenFade = CanvasGroup.DOFade(1, 0.5f);
            
            newSequence.Append(tweenPosition);
            newSequence.Join(tweenFade);
            //newSequence.AppendCallback(TweenFinished);
            newSequence.Pause();
            _showSequence = newSequence;
            return newSequence;
        }
        
        public Sequence PanelFadeOut()
        {
            RefreshTween();
            var targetPosition = new Vector3();
            targetPosition = RectTransform.localPosition;
            targetPosition.x += 20f;

            var newSequence = DOTween.Sequence();
            var tweenPosition = RectTransform.DOAnchorPos3D(targetPosition, 0.2f).SetEase(Ease.Linear);
            var tweenFade = CanvasGroup.DOFade(0, 0.1f);

            newSequence.Append(tweenPosition);
            newSequence.Join(tweenFade);
            //newSequence.AppendCallback(TweenFinished);
            newSequence.AppendCallback(DisablePanel);
            newSequence.Pause();
            _showSequence = newSequence;
            return newSequence;
        }
        
        
        
        
        public Sequence PanelMoveUp(Vector3 position , int index)
        {
            RefreshTween();
            var tween = RectTransform.DOAnchorPos3D(position, 0.2f).SetEase(Ease.OutQuint);
            var tweenFade = CanvasGroup.DOFade(1, 0.1f).SetEase(Ease.Flash);
            var newSequence = DOTween.Sequence();
            newSequence.Append(tween);
            newSequence.Join(tweenFade);
            newSequence.AppendCallback(() => SetIndex(index));
            newSequence.Pause();
            _showSequence = newSequence;
            return newSequence;
        }
        private void TweenFinished()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            manager.RemoveComponent<TweenProcessing>(Entity);
        }
        
        private void SetIndex(int index)
        {
            Index = index;
        }
        
        
        private void RefreshTween()
        {
            if (_showSequence != null)
            {
                _showSequence.Kill();
                _showSequence = null;
            }
        }
        
        public void DisablePanel()
        {
            RefreshTween();
            IsShow = false;
            Customer = default;
            gameObject.SetActive(false);
        }
        
        public void DeactivatedPanel()
        {
            IsShow = false;
        }
        
        public void EnablePanel()
        {
            IsShow = true;
            gameObject.SetActive(true);
        }

    }
    
    public struct PhraseCustomerUi : IComponentData { }
    
    public struct StartTweenPhraseCustomerUi : IComponentData { }
    
    public struct UpdatePhraseCustomerList : IComponentData { }
    
    public struct MovePhrasePanels : IComponentData { }

    public struct TweenProcessing : IComponentData { }

    public class SpawnPhraseCustomerUiManager : IComponentData
    {
        public PhraseCustomerUiManager PhraseCustomerUiPrefab;
    }
    public class PhraseCustomerUiManagerView : IComponentData
    {
        public PhraseCustomerUiManager PhraseCustomerUiManager;
        public List<Entity> CustomerList;


        public void EnablePhrasePanelsUi() => PhraseCustomerUiManager.gameObject.SetActive(true);

        public void DisablePhrasePanelsUi() => PhraseCustomerUiManager.gameObject.SetActive(false);

        
    }
    
}