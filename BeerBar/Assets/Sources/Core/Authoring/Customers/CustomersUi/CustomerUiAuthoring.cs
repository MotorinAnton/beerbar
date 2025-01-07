using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.Customers.CustomersUi
{
    public sealed class CustomerUiAuthoring : EntityBehaviour
    {
        [SerializeField] private CustomerEmotionSprites _faceEmotions;
        public CustomerEmotionSprites FaceEmotionSprites => _faceEmotions;
        
        [SerializeField] private Image _dialogImage;
        public Image DialogImage => _dialogImage;
        
        [SerializeField] private Image _product1Image;
        public Image Product1Image => _product1Image;
        
        public CanvasGroup CanvasGroupProduct1 => _canvasGroupProduct1;

        [SerializeField]
        private CanvasGroup _canvasGroupProduct1;
        
        [SerializeField] private Image _product2Image;
        public Image Product2Image => _product2Image;
        
        public CanvasGroup CanvasGroupProduct2 => _canvasGroupProduct2;

        [SerializeField]
        private CanvasGroup _canvasGroupProduct2;

        
        [SerializeField] private Image _faceEmotionImage;
        public Image FaceEmotionImage => _faceEmotionImage;
        
        public CanvasGroup CanvasGroupFaceEmotion => _canvasGroupFaceEmotion;

        [SerializeField]
        private CanvasGroup _canvasGroupFaceEmotion;

        
        public void EnableFaceEmotion() => FaceEmotionImage.gameObject.SetActive(true);
        
        public void DisableFaceEmotion() => FaceEmotionImage.gameObject.SetActive(false);

        public void EnableDialog() => DialogImage.gameObject.SetActive(true);
        
        public void DisableDialog() => DialogImage.gameObject.SetActive(false);
        
        public void EnableProduct1() => Product1Image.gameObject.SetActive(true);
        
        public void DisableProduct1() => Product1Image.gameObject.SetActive(false);
        
        public void EnableProduct2() => Product2Image.gameObject.SetActive(true);
        
        public void DisableProduct2() => Product2Image.gameObject.SetActive(false);
    }

    public struct SwearEmotionCustomer : IComponentData { }
    
    public struct PleasedEmotionCustomer : IComponentData { }
    
    public struct ShowProductImage : IComponentData { }
    
    public struct SwearEmotionAnimation : IComponentData { }
    
    public struct PleasedEmotionAnimation : IComponentData { }

    [Serializable]
    public class CustomerEmotionSprites
    {
        public Sprite Displeased;
        public Sprite Pleased;
        public Sprite Thinks;
        public Sprite Swears;
        public Sprite Hourglass;
        public Sprite DrinkBeer;
    }

    public class CustomerUiView : IComponentData
    {
        public CustomerUiAuthoring Value;
        public Entity CustomerEntity;
        
        public void RemoveSwearCustomerAnimation()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            manager.RemoveComponent<SwearEmotionAnimation>(Value.Entity);
            manager.RemoveComponent<SwearEmotionCustomer>(Value.Entity);
        }
        
        public void RemovePleasedCustomerAnimation()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            manager.RemoveComponent<PleasedEmotionAnimation>(Value.Entity);
            manager.RemoveComponent<PleasedEmotionCustomer>(Value.Entity);
        }
    }
}