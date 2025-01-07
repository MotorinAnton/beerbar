using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Unity.Entities;

namespace Core.Authoring.EventObjects
{
    public class LossWalletAuthoring : SelectAuthoring<RendererSelectAuthoring> { }
    
    public struct LossWalletEntity : IComponentData
    {
        public int Coins;
    }
    
    public class LossWalletView : IComponentData
    {
        public LossWalletAuthoring Value;
    }
}