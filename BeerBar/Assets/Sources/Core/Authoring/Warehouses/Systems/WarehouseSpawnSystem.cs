using Core.Authoring.NoteBookShops;
using Core.Constants;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Warehouses.Systems
{
    public partial class WarehouseSpawnSystem : SystemBase
    {
        private EntityQuery _warehouseQuery;

        protected override void OnCreate()
        {
            _warehouseQuery = WarehouseUtils.GetWarehouseQuery(this);
        }

        protected override void OnUpdate()
        {
            if (_warehouseQuery.HasSingleton<Warehouse>())
            {
                return;
            }

            Entities.WithAll<SpawnNoteBookShop>().ForEach((Entity entity, in SpawnNoteBookShop spawnNoteBookShop) =>
            {
                CreateWarehouseEntity(entity, spawnNoteBookShop);
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void CreateWarehouseEntity(Entity entity, SpawnNoteBookShop spawnNoteBookShop)
        {
            var warehouse = EntityManager.CreateSingleton<Warehouse>();
            EntityManager.SetName(warehouse, EntityConstants.WarehouseName);

            var noteBookShop = Object.Instantiate(spawnNoteBookShop.NoteBookShopPrefab);
            EntityManager.AddComponentObject(warehouse, new NoteBookShopView
            {
                Value = noteBookShop
            });
            noteBookShop.Initialize(EntityManager, warehouse);
        }
    }
}