using Core.Authoring.PhraseCustomerUi;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.Authoring.SelectGameObjects
{
    public abstract class SelectAuthoring<T> : EntityBehaviour, IPointerClickHandler, IPointerEnterHandler,
        IPointerExitHandler where T : SelectObjectAuthoring 
    {
        [SerializeField] private T _select;
        
        public T Select => _select;


        public void OnPointerClick(PointerEventData eventData)
        {
            EntityManager.AddComponent<Clicked>(Entity);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            EntityManager.AddComponent<SelectObject>(Entity);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            EntityManager.AddComponent<DeselectObject>(Entity);
        }

        public void TweenFinished()
        {
            EntityManager.RemoveComponent<TweenProcessing>(Entity);
        }
    }
    public struct Clicked : IComponentData { }
    
    public struct DeselectObject : IComponentData { }
    
    public struct SelectObject : IComponentData { }

    public class SelectMaterial : IComponentData
    {
        public Material RendererObject;
        public Material[] ParticleBreakBottleRendererObject;
        public Material[] ParticleSprayRendererObject;
    }
}