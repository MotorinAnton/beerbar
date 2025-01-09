using Core.Authoring.Products;
using Core.Authoring.SelectGameObjects;
using Core.Utilities;
using Unity.Entities;
using UnityEngine.EventSystems;

namespace Core.Authoring.Warehouses
{
    public sealed class WarehouseAuthoring : EntityBehaviour, IPointerClickHandler
    {
        private void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = entityManager.CreateEntity();

            entityManager.AddComponentObject(entity, new BindWarehouse
            {
                WarehouseAuthoring = this
            });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            EntityUtilities.AddOneFrameComponent<Clicked>(Entity);
        }
    }

    public struct Warehouse : IComponentData { }

    public class BindWarehouse : IComponentData
    {
        public WarehouseAuthoring WarehouseAuthoring;
    }
    
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