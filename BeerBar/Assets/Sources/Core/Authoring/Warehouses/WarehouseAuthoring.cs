using Core.Authoring.Products;
using Unity.Entities;

namespace Core.Authoring.Warehouses
{
    public struct Warehouse : IComponentData { }
    
    public struct WarehouseProduct : IComponentData
    {
        public ProductData ProductData;
    }

    public struct ProductOrder : IComponentData
    {
        public int Count;
        public bool AssignedUi;
    }

    public struct StartedOrder : IComponentData { }

    public struct ProcessingOrder : IComponentData
    {
        public float TotalTime;
        public float TimeLeft;
    }

    public struct OrderProcessed : IComponentData { }
}