using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Authoring.ProfitUi
{
    public sealed class ProfitUiAuthoring : EntityBehaviour
    {
        [SerializeField] private TMP_Text _text;
        public TMP_Text Text => _text;
        
        [SerializeField] private Image _coinImage;
        public Image CoinImage => _coinImage;
        
        [SerializeField] private Image _displeasedImage;
        
        public Image DispleasedImage => _displeasedImage;
        
        [SerializeField] private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup => _canvasGroup;

        public void DestroyProfitObject() => Destroy(gameObject);
    }

    public class SpawnProfitUi : IComponentData
    {
        public string Text;
        public Vector3 Point;
        public bool Profit;
    }
    
    public enum ProfitUiType
    {
        Profit,
        Displase
    }
}