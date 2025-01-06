using Core.Authoring.Characters;
using Core.Configs;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.ProductKeepers
{
    public struct ProductKeeper : IComponentData { }
    
    public class ProductKeeperView : IComponentData
    {
        public CharacterAuthoring Value;
    }
    
    public class SpawnProductKeeper : IComponentData
    {
        public ProductKeeperConfig ProductKeeper;
        public SpawnPointProductKeeper Point;
        public int Level;
    }
    
    public class ProductKeeperDataComponent : IComponentData
    {
        public ProductKeeperConfig Value;
    }
    
    public struct MoveContainerProductKeeper : IComponentData { }
    
    public struct MoveWarehouseProductKeeper : IComponentData { }
    
    public struct UploadProductKeeper : IComponentData { }
    
    public struct FreeProductKeeper : IComponentData { }

    public struct IndexOrderPoint : IComponentData
    {
        public int Value;
    }

    public struct OrderProductKeeper : IBufferElementData
    {
        public Entity Container;
        public NativeArray<int> CountAdditional;
    }
}