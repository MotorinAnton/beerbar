using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Bartenders;
using Core.Authoring.Characters;
using Core.Authoring.Containers;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.Points;
using Core.Authoring.Products;
using Core.Authoring.Warehouses;
using Core.Components;
using Core.Components.Wait;
using Core.Constants;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.ProductKeepers.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class ProductKeeperMovementSystem : SystemBase
    {
        private EntityQuery _freeProductKeeperQuery;
        private EntityQuery _moveContainerProductKeeperQuery;
        private EntityQuery _moveWarehouseProductKeeperQuery;
        private EntityQuery _uploadProductKeeperQuery;
        private EntityQuery _unloadingProductKeeperQuery;
        private EntityQuery _warehouseProductsQuery;
        private EntityQuery _barmanPointsContainerQuery;
        private EntityQuery _spawnPointProductKeeperQuery;
        private EntityQuery _uploadPointSpillContainerQuery;
        private const float MaxSpillYPosition = 0.45f;
        private const float DurationMoveSpillY = 5f;
        private const float DelayMoveSpillY = 0.2f;
        
        protected override void OnCreate()
        {
            using var productKeeperBuilder = new EntityQueryBuilder(Allocator.Temp);
            _freeProductKeeperQuery = productKeeperBuilder.WithAll<ProductKeeper, FreeProductKeeper>().Build(this);
            
            using var moveContainerProductKeeperBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveContainerProductKeeperQuery = moveContainerProductKeeperBuilder.WithAll<ProductKeeper, MoveContainerProductKeeper>().WithNone<MoveCharacter>().Build(this);
            
            using var moveWarehouseProductKeeperBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveWarehouseProductKeeperQuery = moveWarehouseProductKeeperBuilder.WithAll<ProductKeeper, MoveWarehouseProductKeeper>().WithNone<MoveCharacter>().Build(this);
            
            using var uploadProductKeeperBuilder = new EntityQueryBuilder(Allocator.Temp);
            _uploadProductKeeperQuery = uploadProductKeeperBuilder.WithAll<ProductKeeper, UploadProductKeeper>().WithNone<WaitTime>().Build(this);
            
            using var unloadingProductKeeperBuilder = new EntityQueryBuilder(Allocator.Temp);
            _unloadingProductKeeperQuery = unloadingProductKeeperBuilder.WithAll<ProductKeeper, UploadProductKeeper, WaitTime>().Build(this);
            
            using var warehouseBuilder = new EntityQueryBuilder(Allocator.Temp);
            _warehouseProductsQuery = warehouseBuilder.WithAll<WarehouseProduct>().Build(this);
            
            using var barmanContainerPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _barmanPointsContainerQuery = barmanContainerPointsBuilder.WithAll<BarmanPointContainer, Take>().Build(this);
            
            using var spawnPointProductKeeperBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointProductKeeperQuery = spawnPointProductKeeperBuilder.WithAll<SpawnPointProductKeeper>().Build(this);
            
            using var uploadPointSpillContainerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _uploadPointSpillContainerQuery = uploadPointSpillContainerBuilder.WithAll<UploadPointSpillContainer>().Build(this);
        }

        protected override void OnUpdate()
        {
            FreeProductKeeper();
            MoveContainerProductKeeper();
            UnloadingProductKeeper();
            UploadProductKeeper();
            MoveWarehouseProductKeeper();
        }


        private void FreeProductKeeper()
        {
            var freeProductKeeperArray = _freeProductKeeperQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var productKeeperEntity in freeProductKeeperArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(productKeeperEntity).Value;
                animator.SetBool(ProductKeeperAnimationConstants.Walk, false);
                
                if (!HasBuffer<OrderProductKeeper>(productKeeperEntity))
                {
                    continue;
                }
                
                var orders = EntityManager.GetBuffer<OrderProductKeeper>(productKeeperEntity);

                if (orders.Length == 0)
                {
                    continue;
                }
                
                if (!CheckProductStock(orders))
                {
                    continue;
                }
                
                EntityManager.AddComponent<MoveContainerProductKeeper>(productKeeperEntity);
                EntityManager.RemoveComponent<FreeProductKeeper>(productKeeperEntity);
            }
        }

        private void MoveContainerProductKeeper()
        {
            var moveContainerProductKeeperArray = _moveContainerProductKeeperQuery.ToEntityArray(Allocator.Temp);
            var barmanContainerPoints =
                _barmanPointsContainerQuery.ToComponentDataArray<BarmanPointContainer>(Allocator.Temp);
            var uploadPointSpillContainer =
                _uploadPointSpillContainerQuery.ToComponentDataArray<UploadPointSpillContainer>(Allocator.Temp)[0];
            
            foreach (var productKeeperEntity in moveContainerProductKeeperArray)
            {
                if (EntityManager.HasComponent<MoveCharacterCompleted>(productKeeperEntity))
                {
                    var animator = EntityManager.GetComponentObject<AnimatorView>(productKeeperEntity).Value;
                    EntityManager.AddComponent<UploadProductKeeper>(productKeeperEntity);
                    EntityManager.AddComponentData(productKeeperEntity, new WaitTime { Current = AnimationLength(animator, "Kladovshik_box_anim") });
                    EntityManager.RemoveComponent<MoveContainerProductKeeper>(productKeeperEntity);
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(productKeeperEntity);
                }
                else
                {
                    EntityManager.GetComponentObject<ProductKeeperView>(productKeeperEntity).Value.PivotHand[0].gameObject.SetActive(true);
                    
                    var orders = EntityManager.GetBuffer<OrderProductKeeper>(productKeeperEntity);
                    var orderPoint = barmanContainerPoints.FirstOrDefault(point => point.Container == orders[0].Container);
                    
                    if (EntityManager.HasComponent<Spill>(orders[0].Container))
                    {
                        orderPoint.Point.Position = uploadPointSpillContainer.Position;
                    }
                    
                    EntityManager.AddComponentData(productKeeperEntity, new MoveCharacter { TargetPoint = orderPoint.Point.Position });
                    EntityManager.AddComponentData(productKeeperEntity, new IndexOrderPoint { Value = orderPoint.Index });
                }
            }
        }

        private void MoveWarehouseProductKeeper()
        {
            var moveWarehouseProductKeeperArray = _moveWarehouseProductKeeperQuery.ToEntityArray(Allocator.Temp);
            
            foreach (var productKeeperEntity in moveWarehouseProductKeeperArray)
            {
                if (EntityManager.HasComponent<MoveCharacterCompleted>(productKeeperEntity))
                {
                    if (EntityManager.HasBuffer<OrderProductKeeper>(productKeeperEntity))
                    {
                        var buffer = EntityManager.GetBuffer<OrderProductKeeper>(productKeeperEntity);
                        var bufferNonClone = buffer.ToNativeArray(Allocator.Temp).ToHashSet();
                        buffer.Clear();
                        foreach (var newBuffer in bufferNonClone)
                        {
                            buffer.Add(newBuffer);
                        }
                    }

                    EntityManager.RemoveComponent<MoveCharacterCompleted>(productKeeperEntity);
                    EntityManager.RemoveComponent<MoveWarehouseProductKeeper>(productKeeperEntity);
                    EntityManager.AddComponent<FreeProductKeeper>(productKeeperEntity);
                }
                else
                {
                    var warehousePoint =
                        _spawnPointProductKeeperQuery.ToComponentDataArray<SpawnPointProductKeeper>(Allocator.Temp)[0];
                    EntityManager.AddComponentData(productKeeperEntity,
                        new MoveCharacter { TargetPoint = warehousePoint.Position });
                }
            }
        }

        private void UploadProductKeeper()
        {
            var uploadProductKeeperArray = _uploadProductKeeperQuery.ToEntityArray(Allocator.Temp);

            foreach (var productKeeperEntity in uploadProductKeeperArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(productKeeperEntity).Value;
                animator.SetBool(ProductKeeperAnimationConstants.BoxUpload, false);
                
                var box = EntityManager.GetComponentObject<ProductKeeperView>(productKeeperEntity).Value.PivotHand[0].gameObject;
                box.SetActive(false);
                
                var orders = EntityManager.GetBuffer<OrderProductKeeper>(productKeeperEntity);
                AddProduct(orders[0].Container, orders[0].CountAdditional);
                orders.RemoveAt(0);

                EntityManager.AddComponent<MoveWarehouseProductKeeper>(productKeeperEntity);
                EntityManager.RemoveComponent<IndexOrderPoint>(productKeeperEntity);
                EntityManager.RemoveComponent<UploadProductKeeper>(productKeeperEntity);

            }
        }

        private void UnloadingProductKeeper()
            {
                var unloadingProductKeeperArray = _unloadingProductKeeperQuery.ToEntityArray(Allocator.Temp);

                foreach (var productKeeperEntity in unloadingProductKeeperArray)
                {
                    var animator = EntityManager.GetComponentObject<AnimatorView>(productKeeperEntity).Value;
                    var orders = EntityManager.GetBuffer<OrderProductKeeper>(productKeeperEntity);
                    var containerProduct = EntityManager.GetBuffer<ContainerProduct>(orders[0].Container)[0];
                    var containerView = EntityManager.GetComponentObject<ContainerView>(orders[0].Container).Value;
                    
                    if (EntityManager.HasComponent<Spill>(orders[0].Container) &&
                        !EntityManager.HasComponent<TweenProcessing>(orders[0].Container))
                    {
                        var containerDescription =
                            EntityManager.GetComponentData<ContainerDescription>(orders[0].Container);
                        var productView = EntityManager.GetComponentObject<ProductView>(orders[0].Container);
                        var pos = productView.Products[0][0].transform.localPosition;
                        
                        pos.y = Mathf.Clamp(
                            -MaxSpillYPosition + MaxSpillYPosition / containerDescription.Capacity *
                            (containerProduct.Value.Count + orders[0].CountAdditional[0]), -MaxSpillYPosition,
                            0f);
                        
                        productView.Products[0][0].transform.DOLocalMoveY(pos.y, DurationMoveSpillY).SetDelay(DelayMoveSpillY)
                            .SetEase(Ease.Linear)
                            .OnComplete(containerView.TweenFinished);
                        EntityManager.AddComponent<TweenProcessing>(orders[0].Container);
                    }

                    var productKeeperView = EntityManager.GetComponentObject<ProductKeeperView>(productKeeperEntity);
                
                    productKeeperView.Value.TurningCharacterToPoint(containerView.transform.position);
                    animator.SetBool(ProductKeeperAnimationConstants.BoxUpload, true);
                }
            }
        
            private bool CheckProductStock(DynamicBuffer<OrderProductKeeper> orders)
            {
                var warehouseProducts = _warehouseProductsQuery.ToComponentDataArray<WarehouseProduct>(Allocator.Temp);
                var order = orders[0];
                var containerProducts = EntityManager.GetBuffer<ContainerProduct>(order.Container);
                var amountProductWarehouse = new List<int>();;
                var warehouseProduct = new List<WarehouseProduct>();
            
                for (var index = 0; index < containerProducts.Length; index++)
                {
                    var productContainer = containerProducts[index];
                
                    foreach (var product in warehouseProducts)
                    {
                        if (product.ProductData.ProductType != productContainer.Value.ProductType ||
                            product.ProductData.Level != productContainer.Value.Level)
                        {
                            continue;
                        }

                        amountProductWarehouse.Add(product.ProductData.Count);
                        warehouseProduct.Add(product);
                    }
                }

                var sum = amountProductWarehouse.Sum(); 
            
                if (sum == 0)
                {
                    orders.Clear();
                    return false;
                }
            
                for (var index = 0; index < order.CountAdditional.Length; index++)
                {
                    var additionalCount = order.CountAdditional[index];
                
                    if (additionalCount > amountProductWarehouse[index])
                    {
                        order.CountAdditional[index] = amountProductWarehouse[index];
                        orders[0] = order;
                    }
                
                    DeleteProductWarehouse(warehouseProduct[index], order.CountAdditional[index]);
                
                }
            
                return true;
            }
        
            private void AddProduct(Entity container , NativeArray<int> count)
            {
                var containerProduct = EntityManager.GetBuffer<ContainerProduct>(container);
            
                for (var index = 0; index < containerProduct.Length; index++)
                {
                    var product = containerProduct[index];
                    product.Value.Count += count[index];
                    containerProduct[index] = product;
                }
            }

            private void DeleteProductWarehouse(WarehouseProduct product, int count)
            {
                var warehouseProducts = _warehouseProductsQuery.ToEntityArray(Allocator.Temp);
                foreach (var warehouseProductEntity in warehouseProducts)
                {
                    var warehouseProduct = EntityManager.GetComponentData<WarehouseProduct>(warehouseProductEntity);
                    if (warehouseProduct.ProductData.ProductType != product.ProductData.ProductType ||
                        warehouseProduct.ProductData.Level != product.ProductData.Level ||
                        warehouseProduct.ProductData.Count != product.ProductData.Count)
                    {
                        continue;
                    }

                    warehouseProduct.ProductData.Count -= count;
                    EntityManager.SetComponentData(warehouseProductEntity, warehouseProduct);
                }
            }
        
            private float AnimationLength(Animator animator, string name)
            {
                var ac = animator.runtimeAnimatorController;

                foreach (var animation in ac.animationClips)
                {
                    if(animation.name == name)
                    {
                        return animation.length;
                    }
                }

                return default;
            }
    }
}