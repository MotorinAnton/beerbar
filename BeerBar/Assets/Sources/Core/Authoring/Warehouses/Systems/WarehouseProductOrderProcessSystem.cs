using Core.Utilities;
using Unity.Entities;

namespace Core.Authoring.Warehouses.Systems
{
    // Какая система накидывает ProcessingOrder и OrderProcess, та и должна убирать
    public partial class WarehouseProductOrderProcessSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<Warehouse, ProcessingOrder>().ForEach(
                (Entity entity, in ProcessingOrder processingOrder) =>
                {
                    var processingOrderData = processingOrder;
                    processingOrderData.TimeLeft -= UnityEngine.Time.deltaTime;

                    if (processingOrderData.TimeLeft > 0)
                    {
                        EntityManager.SetComponentData(entity, processingOrderData);
                        return;
                    }

                    EntityManager.RemoveComponent<ProcessingOrder>(entity);

                    EntityUtilities.AddOneFrameComponent<OrderProcessed>(entity);
                }).WithStructuralChanges().Run();
        }
    }
}