using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.ContainersUI
{
    public class ContainerUiAuthoring : EntityBehaviour
    {
        [SerializeField]
        private Image _lowImage;
        public Image LowImage => _lowImage;
        
        [SerializeField]
        private Image _itsOverImage;
        public Image ItsOverImage => _itsOverImage;
    }
}