using UnityEngine;

namespace Core.Authoring.SelectGameObjects
{
    public abstract class SelectObjectAuthoring : MonoBehaviour
    {
        public abstract SelectObjectType SelectType { get; }
    }
    
    public enum SelectObjectType
    {
        Renderer, 
        Skinned,
        Particle,
        RendererAndSkinned,
        RendererAndParticle
    }
}