using Core.Authoring.Characters;
using Core.Authoring.Points;
using Core.Authoring.Products;
using Core.Configs;
using Unity.Entities;

namespace Core.Authoring.Bartenders
{
    public struct Barman : IComponentData { }
    
    public class SpawnBarman : IComponentData
    {
        public BarmanConfig BarmanData;
        public SpawnPoint Point;
        public int IndexBarman;
    }
    
    public class BarmanView : IComponentData
    {
        public CharacterAuthoring Value;

        public void EnableBottle() => Value.PivotHand[0].gameObject.SetActive(true);

        public void DisableBottle() => Value.PivotHand[0].gameObject.SetActive(false);
        
    }
    
    public class BarmanDataComponent : IComponentData
    {
        public VisualBarmanConfig Value;
    }
    
    public struct MoveContainerPointBarman : IComponentData { }
    
    public struct MoveCashPointBarman : IComponentData { }
    
    public struct TakeProductBarman : IComponentData { }
    
    public struct DraftBarman : IComponentData { }
    
    public struct StartDraftBarman : IComponentData { }
    
    public struct EndDraftBarman : IComponentData { }
    
    public struct GiveProductBarman : IComponentData { }
    
    public struct FreeBarman : IComponentData { }

    public struct BarmanIndex : IComponentData
    {
        public int Value;
    }
    
    public class OrderBarman : IComponentData
    {
        public ProductData[] Products;
        public ProductData[] CompletedProduct;
        public Entity CustomerEntity;
    }
    
    public struct BarmanPointContainer : IComponentData
    {
        public Entity Container;
        public ProductType Type;
        public Point Point;
        public int Index;
    }
}