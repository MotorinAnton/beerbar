using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.Customers.CustomersUi
{
    public sealed class ImageCustomerUiAuthoring : EntityBehaviour
    {
        [SerializeField] private Image _image;
        
        public Image FaceEmotionImage => _image;
        
        public CanvasGroup CanvasGroupFaceEmotion => _canvasGroup;

        [SerializeField]
        private CanvasGroup _canvasGroup;
    }
}