using System.Linq;
using Core.Authoring.Customers.CustomersUi;
using Core.Authoring.Points;
using Core.Components;
using Core.Constants;
using Unity.Collections;
using Unity.Entities;
using Object = UnityEngine.Object;

namespace Core.Authoring.Customers.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class CustomerSpawnSystem : SystemBase
    {
        private EntityQuery _entryCustomerQuery;
        private EntityQuery _entryPointsQuery;
        
        protected override void OnCreate()
        {
            using var customerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _entryCustomerQuery = customerBuilder.WithAll<Customer, EntryCustomer, IndexMovePoint>().Build(this);
            
            using var entryPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _entryPointsQuery = entryPointsBuilder.WithAll<EntryPoint, MoveCustomerPoint>().Build(this);
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<SpawnCustomer>().ForEach((Entity entity, in SpawnCustomer spawnCustomer) =>
            {
                CreateCustomer(entity, spawnCustomer);
                
            }).WithoutBurst().WithStructuralChanges().Run();
        }

        private void CreateCustomer(Entity entity, in SpawnCustomer spawnCustomer)
        {
            if (!FreeIndexPoint(out var freeIndex))
            {
                EntityManager.DestroyEntity(entity);
                return;
            }

            var customerView = Object.Instantiate(spawnCustomer.CustomerPrefab,
                spawnCustomer.Point.Position, spawnCustomer.Point.Rotation);
            var animator = customerView.Animator;
            
            var agent = customerView.NavMeshAgent;
            var agentObstacle = customerView.NavMeshObstacle;
            agentObstacle.enabled = false;
            
            var customerUiViewEntity = EntityManager.CreateEntity();
            EntityManager.SetName(customerUiViewEntity, EntityConstants.CustomerUiName);
            
            var positionUi = customerView.gameObject.transform.position;
            positionUi.y += CustomerAnimationConstants.UiOffsetY;
            positionUi.x += CustomerAnimationConstants.UiOffsetX;
            
            var customerUiView =
                Object.Instantiate(spawnCustomer.CustomerUiPrefab, positionUi, customerView.transform.rotation,
                    customerView.gameObject.transform);
            
            customerUiView.DisableDialog();
            customerUiView.DisableFaceEmotion();
            customerUiView.DisableProduct1();
            customerUiView.DisableProduct2();
            
            EntityManager.AddComponentObject(customerUiViewEntity,
                new CustomerUiView 
                {
                    Value = customerUiView, 
                    CustomerEntity = entity 
                });
            var randomProductList = spawnCustomer.Products;

            EntityManager.AddComponentData(entity, new IndexMovePoint { Value = freeIndex });
            EntityManager.AddComponentObject(entity, new CustomerProduct { Products = randomProductList });
            EntityManager.AddComponent<Customer>(entity);
            EntityManager.AddComponent<EntryCustomer>(entity);
            EntityManager.AddComponentData(entity, new CustomerUIEntity { UiEntity = customerUiViewEntity });
            EntityManager.AddComponentObject(entity, new CustomerConfigDataComponent { Value = spawnCustomer.CustomerData });
            EntityManager.AddComponentObject(entity,
                new CustomerView
                {
                    Value = customerView, 
                    Avatar = spawnCustomer.Avatar, 
                    Dialogs = spawnCustomer.Dialogs,
                    UiEntity = customerUiViewEntity, 
                    Audio = spawnCustomer.Audio, 
                    Rating = spawnCustomer.Level
                });
            EntityManager.AddComponentObject(entity, new AnimatorView { Value = animator });
            EntityManager.AddComponentData(entity,
                new RandomAnimation 
                { 
                    NumberAnimation = 0, 
                    TransitionNextAnimation = false 
                });
            EntityManager.AddComponentObject(entity, new NavMeshAgentView { Agent = agent, Obstacle = agentObstacle });
            EntityManager.AddComponentObject(entity, new AudioSourceView { Value = customerView.AudioSource });
            EntityManager.AddComponentObject(entity, new TransformView { Value = customerView.transform });
            EntityManager.SetName(entity, EntityConstants.CustomerName + spawnCustomer.Level.ToString());
            
            customerView.Initialize(EntityManager, entity);
            customerUiView.Initialize(EntityManager, customerUiViewEntity);
            
            EntityManager.RemoveComponent<SpawnCustomer>(entity);
        }
        private bool FreeIndexPoint(out int freeIndex)
        {
            var indexesEntryCustomer = _entryCustomerQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            if (indexesEntryCustomer.Length == 0)
            {
                freeIndex = 0;
                return true;
            }
            
            var customerPointIndexes = indexesEntryCustomer.Select(index => index.Value).ToHashSet();
            var lastCustomerPoint = customerPointIndexes.Max();
            var entryPoints = _entryPointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);
            var freeEntryPoints = entryPoints.Select(point => point.IndexPoint).ToHashSet();
            freeEntryPoints.ExceptWith(customerPointIndexes);
            
            if (freeEntryPoints.Count == 0)
            {
                freeIndex = default;
                return false;
            }
            
            var rowCount = entryPoints.Select(point => point.Row).Max();
            var pointMinIndex = freeEntryPoints.Min();
            if (freeEntryPoints.Contains(pointMinIndex + rowCount + 1) )
            {
                freeIndex = pointMinIndex;
                return true;
            }

            if (lastCustomerPoint+1 < entryPoints.Length)
            {
                freeIndex = lastCustomerPoint + 1;
                return true;
            }
            
            freeIndex = default;
            return false;
        }
    }
}