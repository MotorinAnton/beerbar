using System.Collections.Generic;
using Core.Authoring.Characters;
using Core.Authoring.Cleaners;
using Unity.Collections;
using Unity.Entities;

namespace Core.Authoring.Tables.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class ClearTableSystem : SystemBase
    {
        private EntityQuery _cleanTableQuery;
        private EntityQuery _cleanerQuery;

        protected override void OnCreate()
        {
            using var tableBuilder = new EntityQueryBuilder(Allocator.Temp);
            _cleanTableQuery = tableBuilder.WithAll<Table, CleanTable>()
                .Build(this);
            
            using var allCleanerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _cleanerQuery = allCleanerBuilder.WithAll<Cleaner>()
                .Build(this);

        }

        protected override void OnUpdate()
        {
            var cleanTableArray = _cleanTableQuery.ToEntityArray(Allocator.Temp);

            foreach (var table in cleanTableArray)
            {
                CreateClearTableOrder(table);
            }
        }

        private void CreateClearTableOrder(Entity table)
        {
            EntityManager.RemoveComponent<CleanTable>(table);
            
            var cleanerArray = _cleanerQuery.ToEntityArray(Allocator.Temp);
            var cleanerEntity = cleanerArray[0];
            var tablesOrder = new HashSet<Entity>();

            if (EntityManager.HasBuffer<OrderCleanTable>(cleanerEntity))
            {
                 var buffer = EntityManager.GetBuffer<OrderCleanTable>(cleanerEntity);
                 
                 foreach (var orderTable in buffer)
                 {
                     tablesOrder.Add(orderTable.Table);
                 }
            }
           

            if (tablesOrder.Contains(table))
            {
                return;
            }
            
            
            var newBuffer = EntityManager.AddBuffer<OrderCleanTable>(cleanerEntity);
            newBuffer.Add(new OrderCleanTable { Table = table });
            
            if (!EntityManager.HasComponent<CleaningCompletedCleaner>(cleanerEntity))
            {
                return;
            }

            EntityManager.RemoveComponent<MoveCharacter>(cleanerEntity);
            EntityManager.RemoveComponent<CleaningCompletedCleaner>(cleanerEntity);
            EntityManager.RemoveComponent<MoveExitCleaner>(cleanerEntity);
            EntityManager.AddComponent<FreeCleaner>(cleanerEntity);
            EntityManager.RemoveComponent<CleanTable>(table);
        }
    }
}