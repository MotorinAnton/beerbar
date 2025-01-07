using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Characters;
using Core.Authoring.Containers;
using Core.Authoring.Customers;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.Points;
using Core.Authoring.ProductKeepers;
using Core.Authoring.Products;
using Core.Authoring.Tables;
using Core.Components;
using Core.Components.Purchase;
using Core.Components.Wait;
using Core.Constants;
using Core.Utilities;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Bartenders.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class BarmanMovementSystem : SystemBase
    {
        private EntityQuery _freeBarmanQuery;
        private EntityQuery _moveContainerBarmanQuery;
        private EntityQuery _moveCashPointQuery;
        private EntityQuery _takingProductBarmanQuery;
        private EntityQuery _afterTakeProductBarmanQuery;
        private EntityQuery _givingProductBarmanQuery;
        private EntityQuery _afterGiveProductBarmanQuery;
        private EntityQuery _startDraftBarmanQuery;
        private EntityQuery _draftBarmanQuery;
        private EntityQuery _afterDraftBarmanQuery;
        private EntityQuery _cleaningBreakBottleBarmanQuery;
        private EntityQuery _productKeeperMoveIndexPointQuery;
        private EntityQuery _indexPointsBarmanQuery;
        private EntityQuery _barmanPointsContainerQuery;
        private EntityQuery _purchasePointsQuery;
        private EntityQuery _spawnPointBarmanQuery;
        private EntityQuery _containerProductQuery;

        protected override void OnCreate()
        {
            using var freeBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _freeBarmanQuery = freeBarmanBuilder.WithAll<Barman, FreeBarman>().Build(this);

            using var moveContainerPointBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveContainerBarmanQuery = moveContainerPointBarmanBuilder
                .WithAll<Barman, MoveContainerPointBarman, IndexOrderPoint>().WithNone<WaitTime, MoveCharacter>()
                .Build(this);

            using var moveCashPointBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveCashPointQuery = moveCashPointBarmanBuilder.WithAll<Barman, MoveCashPointBarman>()
                .WithNone<MoveCharacter>().Build(this);

            using var takingProductBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _takingProductBarmanQuery = takingProductBarmanBuilder
                .WithAll<Barman, TakeProductBarman, OrderBarman, WaitTime>().Build(this);

            using var afterTakeProductBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _afterTakeProductBarmanQuery = afterTakeProductBarmanBuilder
                .WithAll<Barman, TakeProductBarman, OrderBarman>().WithNone<WaitTime>().Build(this);

            using var givingProductBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _givingProductBarmanQuery =
                givingProductBarmanBuilder.WithAll<Barman, GiveProductBarman, WaitTime>().Build(this);

            using var giveProductBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _afterGiveProductBarmanQuery = giveProductBarmanBuilder.WithAll<Barman, GiveProductBarman>()
                .WithNone<WaitTime>().Build(this);

            using var draftBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _draftBarmanQuery = draftBarmanBuilder.WithAll<Barman, DraftBarman, OrderBarman, WaitTime>().Build(this);

            using var afterDraftBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _afterDraftBarmanQuery = afterDraftBarmanBuilder.WithAll<Barman, DraftBarman, OrderBarman>()
                .WithNone<WaitTime>().Build(this);

            using var productKeeperMoveIndexPointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _productKeeperMoveIndexPointQuery =
                productKeeperMoveIndexPointBuilder.WithAll<IndexOrderPoint>().Build(this);

            using var indexPointsBarmanBuilder = new EntityQueryBuilder(Allocator.Temp);
            _indexPointsBarmanQuery = indexPointsBarmanBuilder.WithAll<Barman, IndexOrderPoint>().Build(this);

            using var barmanContainerPointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _barmanPointsContainerQuery =
                barmanContainerPointsBuilder.WithAll<BarmanPointContainer, Take>().Build(this);

            using var purchasePointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchasePointsQuery = purchasePointsBuilder.WithAll<PurchasePoint, MoveCustomerPoint>()
                .WithNone<PointNotAvailable>().Build(this);

            using var containerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _containerProductQuery = containerBuilder.WithAll<Container, ContainerProduct>().Build(this);

            using var spawnPointProductKeeperBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointBarmanQuery = spawnPointProductKeeperBuilder.WithAll<BarmanSpawnPoint, SpawnPoint>().Build(this);
        }

        protected override void OnUpdate()
        {
            FreeBarman();
            MoveContainerBarman();
            DraftBarman();
            AfterDraftBarman();
            TakingProductBarman();
            AfterTakeProductBarman();
            MoveCashPointBarman();
            GivingProductBarman();
            AfterGiveProductBarman();
        }

        private void FreeBarman()
        {
            var freeBarmanArray = _freeBarmanQuery.ToEntityArray(Allocator.Temp);
            var purchasePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);

            foreach (var barmanEntity in freeBarmanArray)
            {
                var barmanView = EntityManager.GetComponentObject<BarmanView>(barmanEntity);
                var indexBarman = EntityManager.GetComponentData<BarmanIndex>(barmanEntity).Value;
                var targetRotationPoint =
                    purchasePoints.FirstOrDefault(point => point.Row == indexBarman && point.Column == 0).Point
                        .Position;

                var vectorTargetRotationPoint =
                    new Vector3(targetRotationPoint.x, targetRotationPoint.y, targetRotationPoint.z);
                barmanView.Value.TurningCharacterToPoint(vectorTargetRotationPoint);


                if (!EntityManager.HasComponent<OrderBarman>(barmanEntity))
                {
                    continue;
                }

                var order = EntityManager.GetComponentObject<OrderBarman>(barmanEntity);

                if (EntityManager.HasComponent<DissatisfiedCustomer>(order.CustomerEntity))
                {
                    EntityManager.RemoveComponent<OrderBarman>(barmanEntity);
                    continue;
                }

                if (!CheckProductStock(order.Products, out var orderContainers))
                {
                    continue;
                }

                if (!CheckFreeContainerPoint(orderContainers, out var freeIndexPoint))
                {
                    continue;
                }

                var customerUiEntity = EntityManager
                    .GetComponentData<CustomerUIEntity>(order.CustomerEntity).UiEntity;

                if (EntityManager.HasComponent<WaitTime>(customerUiEntity))
                {
                    EntityManager.RemoveComponent<WaitTime>(customerUiEntity);
                    EntityManager.RemoveComponent<WaitTimer>(customerUiEntity);
                    EntityManager.RemoveComponent<StartWaitTime>(customerUiEntity);
                }

                EntityManager.AddComponentData(barmanEntity,
                    new IndexOrderPoint { Value = freeIndexPoint });
                EntityManager.AddComponent<MoveContainerPointBarman>(barmanEntity);
                EntityManager.RemoveComponent<FreeBarman>(barmanEntity);
            }
        }

        private void MoveContainerBarman()
        {
            var moveContainerBarmanArray = _moveContainerBarmanQuery.ToEntityArray(Allocator.Temp);
            var barmanContainerPoints =
                _barmanPointsContainerQuery.ToComponentDataArray<BarmanPointContainer>(Allocator.Temp);

            foreach (var barmanEntity in moveContainerBarmanArray)
            {
                if (!EntityManager.HasComponent<MoveCharacterCompleted>(barmanEntity))
                {
                    var indexOrderPoint = EntityManager.GetComponentData<IndexOrderPoint>(barmanEntity);
                    var orderContainerPoint = barmanContainerPoints.First(point => point.Index == indexOrderPoint.Value)
                        .Point.Position;
                    EntityManager.AddComponentData(barmanEntity,
                        new MoveCharacter { TargetPoint = orderContainerPoint });
                }

                if (!EntityManager.HasComponent<MoveCharacterCompleted>(barmanEntity))
                {
                    continue;
                }

                var animator = EntityManager.GetComponentObject<AnimatorView>(barmanEntity).Value;
                var barmanIndexOrderPoint = EntityManager.GetComponentData<IndexOrderPoint>(barmanEntity);
                var containerEntity = barmanContainerPoints.First(point => point.Index == barmanIndexOrderPoint.Value)
                    .Container;
                var containerDescription = EntityManager.GetComponentData<ContainerDescription>(containerEntity);

                if (containerDescription.Type == ProductType.Spill)
                {
                    var sumAnimationTime =
                        AnimationUtilities.AnimationLength(animator, BarmanAnimationConstants.BarmanIdleToDraft) +
                        AnimationUtilities.AnimationLength(animator, BarmanAnimationConstants.BarmanDraftProcess) +
                        AnimationUtilities.AnimationLength(animator, BarmanAnimationConstants.BarmanDraftEnd);

                    EntityManager.AddComponent<DraftBarman>(barmanEntity);
                    EntityManager.AddComponent<ApplyRootMotion>(barmanEntity);
                    EntityManager.AddComponentData(barmanEntity,
                        new WaitTime { Current = sumAnimationTime });
                    EntityManager.RemoveComponent<MoveContainerPointBarman>(barmanEntity);
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(barmanEntity);
                    continue;
                }

                EntityManager.AddComponent<TakeProductBarman>(barmanEntity);
                EntityManager.AddComponentData(barmanEntity,
                    new WaitTime
                    {
                        Current = AnimationUtilities.AnimationLength(animator,
                            BarmanAnimationConstants.BarmanPickProduct)
                    });
                EntityManager.RemoveComponent<MoveContainerPointBarman>(barmanEntity);
                EntityManager.RemoveComponent<MoveCharacterCompleted>(barmanEntity);
            }
        }

        private void TakingProductBarman()
        {
            var takingProductBarmanArray = _takingProductBarmanQuery.ToEntityArray(Allocator.Temp);
            var barmanContainerPoints =
                _barmanPointsContainerQuery.ToComponentDataArray<BarmanPointContainer>(Allocator.Temp);

            foreach (var barmanEntity in takingProductBarmanArray)
            {
                var barmanView = EntityManager.GetComponentObject<BarmanView>(barmanEntity);
                var indexOrderPoint = EntityManager.GetComponentData<IndexOrderPoint>(barmanEntity);
                var targetEntity = barmanContainerPoints.First(point => point.Index == indexOrderPoint.Value).Container;
                var containerDescription = EntityManager.GetComponentData<ContainerDescription>(targetEntity);
                var targetRotationPoint = EntityManager.GetComponentObject<ContainerView>(targetEntity).Value.transform
                    .position;
                var vectorTargetRotationPoint =
                    new Vector3(targetRotationPoint.x, targetRotationPoint.y, targetRotationPoint.z);

                barmanView.Value.TurningCharacterToPoint(vectorTargetRotationPoint);

                var animator = EntityManager.GetComponentObject<AnimatorView>(barmanEntity).Value;

                if (containerDescription.Type is ProductType.BottleBeer or ProductType.Spill)
                {
                    animator.SetBool(BarmanAnimationConstants.TakeBottle, true);
                    continue;
                }

                animator.SetBool(BarmanAnimationConstants.TakeSnack, true);
            }
        }

        private void AfterTakeProductBarman()
        {
            var takeBarmanArray = _afterTakeProductBarmanQuery.ToEntityArray(Allocator.Temp);

            foreach (var barmanEntity in takeBarmanArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(barmanEntity).Value;
                var barmanView = EntityManager.GetComponentObject<BarmanView>(barmanEntity);
                var order = EntityManager.GetComponentObject<OrderBarman>(barmanEntity);
                var indexOrderPoint = EntityManager.GetComponentData<IndexOrderPoint>(barmanEntity);
                var containerPoint =
                    _barmanPointsContainerQuery.ToComponentDataArray<BarmanPointContainer>(Allocator.Temp)
                        .FirstOrDefault(point => point.Index == indexOrderPoint.Value);
                var containerProduct = EntityManager.GetBuffer<ContainerProduct>(containerPoint.Container);

                if (containerPoint.Type is ProductType.BottleBeer or ProductType.Spill)
                {
                    barmanView.EnableBottle();
                    animator.SetBool(BarmanAnimationConstants.TakeBottle, false);
                }

                if (animator.GetBool(BarmanAnimationConstants.TakeSnack))
                {
                    animator.SetBool(BarmanAnimationConstants.TakeSnack, false);
                }

                for (var productIdx = 0; productIdx < order.Products.Length; productIdx++)
                {
                    var productOrder = order.Products[productIdx];

                    for (int containerProductIdx = 0;
                         containerProductIdx < containerProduct.Length;
                         containerProductIdx++)
                    {
                        var productContainer = containerProduct[containerProductIdx];

                        if (productOrder.ProductType == productContainer.Value.ProductType &&
                            productContainer.Value.Count >= productOrder.Count &&
                            productOrder.Level == productContainer.Value.Level)
                        {
                            productContainer.Value.Count -= productOrder.Count;
                            containerProduct[containerProductIdx] = productContainer;
                            var productListOrder = order.Products.ToList();
                            var productCompletedList = order.CompletedProduct.ToList();
                            productCompletedList.Add(productOrder);
                            productListOrder.Remove(productOrder);
                            order.CompletedProduct = productCompletedList.ToArray();
                            order.Products = productListOrder.ToArray();
                        }
                        else if (productContainer.Value.Count > 0 &&
                                 productContainer.Value.Count < productOrder.Count &&
                                 productContainer.Value.ProductType == productOrder.ProductType)
                        {
                            productOrder.Count = productContainer.Value.Count;
                            productContainer.Value.Count = 0;
                            containerProduct[containerProductIdx] = productContainer;
                            var productListOrder = order.Products.ToList();
                            var productCompletedList = order.CompletedProduct.ToList();
                            productCompletedList.Add(productOrder);
                            productListOrder.Remove(productOrder);
                            order.CompletedProduct = productCompletedList.ToArray();
                            order.Products = productListOrder.ToArray();
                        }
                    }
                }

                if (order.Products.Length == 0)
                {
                    EntityManager.AddComponent<MoveCashPointBarman>(barmanEntity);
                    EntityManager.RemoveComponent<IndexOrderPoint>(barmanEntity);
                    EntityManager.RemoveComponent<TakeProductBarman>(barmanEntity);
                    continue;
                }

                if (!CheckProductStock(order.Products, out var orderContainers))
                {
                    var productListOrder = order.Products.ToList();
                    productListOrder.RemoveAt(0);
                    order.Products = productListOrder.ToArray();
                    EntityManager.RemoveComponent<IndexOrderPoint>(barmanEntity);
                    EntityManager.AddComponent<MoveCashPointBarman>(barmanEntity);
                    EntityManager.RemoveComponent<TakeProductBarman>(barmanEntity);
                    continue;
                }

                if (!CheckFreeContainerPoint(orderContainers, out var freePoint))
                {
                    continue;
                }

                EntityManager.SetComponentData(barmanEntity, new IndexOrderPoint { Value = freePoint });
                EntityManager.AddComponent<MoveContainerPointBarman>(barmanEntity);
                EntityManager.RemoveComponent<TakeProductBarman>(barmanEntity);
            }
        }

        private void DraftBarman()
        {
            var draftBarmanArray = _draftBarmanQuery.ToEntityArray(Allocator.Temp);
            var barmanContainerPoints =
                _barmanPointsContainerQuery.ToComponentDataArray<BarmanPointContainer>(Allocator.Temp);

            foreach (var barmanEntity in draftBarmanArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(barmanEntity).Value;
                var barmanView = EntityManager.GetComponentObject<BarmanView>(barmanEntity);
                var indexOrderPoint = EntityManager.GetComponentData<IndexOrderPoint>(barmanEntity);
                var targetEntity = barmanContainerPoints.First(point => point.Index == indexOrderPoint.Value).Container;
                var targetRotationPoint = EntityManager.GetComponentObject<ContainerView>(targetEntity).Value.transform
                    .position;

                var product = EntityManager.GetBuffer<ContainerProduct>(targetEntity)[0];
                var containerDescription = EntityManager.GetComponentData<ContainerDescription>(targetEntity);

                barmanView.Value.TurningCharacterToPoint(targetRotationPoint);
                animator.SetBool(BarmanAnimationConstants.Draft, true);

                if (EntityManager.HasComponent<TweenProcessing>(targetEntity))
                {
                    continue;
                }

                var containerView = EntityManager.GetComponentObject<ContainerView>(targetEntity).Value;
                var orders = EntityManager.GetComponentObject<OrderBarman>(barmanEntity).Products;
                var productView = EntityManager.GetComponentObject<ProductView>(targetEntity);
                var order = orders.FirstOrDefault(productData =>
                    productData.ProductType == containerDescription.Type &&
                    productData.Level == containerDescription.Level);
                var pos = productView.Products[0][0].transform.localPosition;

                pos.y = Mathf.Clamp(
                    -BarmanAnimationConstants.MaxPositionYSpillProduct +
                    BarmanAnimationConstants.MaxPositionYSpillProduct / containerDescription.Capacity *
                    (product.Value.Count - order.Count), -BarmanAnimationConstants.MaxPositionYSpillProduct,
                    0f);
                productView.Products[0][0].transform.DOLocalMoveY(pos.y, BarmanAnimationConstants.SpillMoveDuration)
                    .SetDelay(BarmanAnimationConstants.SpillMoveDelay).SetEase(Ease.Linear)
                    .OnComplete(containerView.TweenFinished);
                EntityManager.AddComponent<TweenProcessing>(targetEntity);

            }
        }

        private void AfterDraftBarman()
        {
            var draftBarmanArray = _afterDraftBarmanQuery.ToEntityArray(Allocator.Temp);

            foreach (var barmanEntity in draftBarmanArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(barmanEntity).Value;

                animator.SetBool(BarmanAnimationConstants.Draft, false);

                EntityManager.RemoveComponent<DraftBarman>(barmanEntity);
                EntityManager.RemoveComponent<ApplyRootMotion>(barmanEntity);
                EntityManager.AddComponent<TakeProductBarman>(barmanEntity);
            }
        }


        private void MoveCashPointBarman()
        {
            var moveCashPointBarmanArray = _moveCashPointQuery.ToEntityArray(Allocator.Temp);
            var cashPoints =
                _spawnPointBarmanQuery.ToComponentDataArray<SpawnPoint>(Allocator.Temp);
            var purchasePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);

            foreach (var barmanEntity in moveCashPointBarmanArray)
            {
                var indexBarman = EntityManager.GetComponentData<BarmanIndex>(barmanEntity).Value;

                if (!EntityManager.HasComponent<MoveCharacterCompleted>(barmanEntity))
                {
                    var cashPoint = cashPoints[indexBarman].Position;

                    EntityManager.AddComponentData(barmanEntity, new MoveCharacter { TargetPoint = cashPoint });
                    continue;
                }

                var animator = EntityManager.GetComponentObject<AnimatorView>(barmanEntity).Value;

                EntityManager.AddComponent<GiveProductBarman>(barmanEntity);
                EntityManager.AddComponentData(barmanEntity,
                    new WaitTime
                    {
                        Current = AnimationUtilities.AnimationLength(animator,
                            BarmanAnimationConstants.BarmanPutOnTableX2SpeedPlease.ToString()) / 2
                    });
                EntityManager.RemoveComponent<MoveCashPointBarman>(barmanEntity);
                EntityManager.RemoveComponent<MoveCharacterCompleted>(barmanEntity);

                var barmanView = EntityManager.GetComponentObject<BarmanView>(barmanEntity);
                var targetRotationPoint =
                    purchasePoints.FirstOrDefault(point => point.Row == indexBarman && point.Column == 0).Point
                        .Position;
                barmanView.Value.TurningCharacterToPoint(targetRotationPoint);
            }
        }

        private void GivingProductBarman()
        {
            var givingProductBarmanArray = _givingProductBarmanQuery.ToEntityArray(Allocator.Temp);
            var purchasePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);

            foreach (var barmanEntity in givingProductBarmanArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(barmanEntity).Value;
                var barmanView = EntityManager.GetComponentObject<BarmanView>(barmanEntity);
                var indexBarman = EntityManager.GetComponentData<BarmanIndex>(barmanEntity).Value;
                var orderBarman = EntityManager.GetComponentObject<OrderBarman>(barmanEntity);

                var targetRotationPoint =
                    purchasePoints.FirstOrDefault(point => point.Row == indexBarman && point.Column == 0).Point
                        .Position;
                var vectorTargetRotationPoint =
                    new Vector3(targetRotationPoint.x, targetRotationPoint.y, targetRotationPoint.z);

                barmanView.Value.TurningCharacterToPoint(vectorTargetRotationPoint);

                animator.SetBool(BarmanAnimationConstants.Give, true);

                var customerAnimator = EntityManager.GetComponentObject<AnimatorView>(orderBarman.CustomerEntity).Value;
                var customerView = EntityManager.GetComponentObject<CustomerView>(orderBarman.CustomerEntity);

                customerAnimator.SetBool(CustomerAnimationConstants.TakeBottle, true);
                customerView.Value.TurningCharacterToPoint(barmanView.Value.transform.position);
            }
        }

        private void AfterGiveProductBarman()
        {
            var afterGiveProductBarmanArray = _afterGiveProductBarmanQuery.ToEntityArray(Allocator.Temp);

            foreach (var barmanEntity in afterGiveProductBarmanArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(barmanEntity).Value;
                var barmanView = EntityManager.GetComponentObject<BarmanView>(barmanEntity);
                var orderBarman = EntityManager.GetComponentObject<OrderBarman>(barmanEntity);

                if (barmanView.Value.PivotHand[0].gameObject.activeInHierarchy)
                {
                    barmanView.DisableBottle();
                }

                animator.SetBool(BarmanAnimationConstants.Give, false);

                var customerAnimator = EntityManager.GetComponentObject<AnimatorView>(orderBarman.CustomerEntity).Value;
                var customerView = EntityManager.GetComponentObject<CustomerView>(orderBarman.CustomerEntity);

                customerView.Value.PivotHand[0].gameObject.SetActive(true);
                customerAnimator.SetBool(CustomerAnimationConstants.TakeBottle, false);
                customerAnimator.SetBool(CustomerAnimationConstants.CashIdle, false);

                EntityManager.AddComponent<Purchase>(barmanEntity);
                EntityManager.RemoveComponent<GiveProductBarman>(barmanEntity);
            }
        }

        private bool CheckProductStock(ProductData[] orders, out HashSet<Entity> containers)
        {
            var containerEntities = new HashSet<Entity>();
            var containerArray = _containerProductQuery.ToEntityArray(Allocator.Temp);

            foreach (var orderProduct in orders)
            {
                foreach (var containerEntity in containerArray)
                {
                    var containerProducts = EntityManager.GetBuffer<ContainerProduct>(containerEntity);
                    foreach (var containerProduct in containerProducts)
                    {
                        if (containerProduct.Value.ProductType == orderProduct.ProductType &&
                            containerProduct.Value.Level == orderProduct.Level && containerProduct.Value.Count > 0)
                        {
                            containerEntities.Add(containerEntity);
                        }
                    }
                }
            }

            if (containerEntities.Count > 0)
            {
                containers = containerEntities;
                return true;
            }

            containers = default;
            return false;

        }

        private bool CheckFreeContainerPoint(HashSet<Entity> containers, out int pointIndex)
        {
            var containerPoints = IndexPointContainers(containers);
            var afterTakingBarman = _afterTakeProductBarmanQuery.ToEntityArray(Allocator.Temp);
            var barmanIndexes = _indexPointsBarmanQuery.ToComponentDataArray<IndexOrderPoint>(Allocator.Temp)
                .Select(barmanIndex => barmanIndex.Value).ToHashSet();
            var productKeeperMoveIndexPoints = _productKeeperMoveIndexPointQuery
                .ToComponentDataArray<IndexOrderPoint>(Allocator.Temp).Select(pointIndex => pointIndex.Value)
                .ToHashSet();

            containerPoints.ExceptWith(barmanIndexes);
            containerPoints.ExceptWith(productKeeperMoveIndexPoints);

            if (containerPoints.Count > 0)
            {
                pointIndex = containerPoints.Min();
                return true;
            }

            if (containerPoints.Count == 0)
            {
                var containerPoint = IndexPointContainers(containers);
                foreach (var barmanEntity in afterTakingBarman)
                {
                    var indexOrderPoint = EntityManager.GetComponentData<IndexOrderPoint>(barmanEntity).Value;
                    if (containerPoint.Contains(indexOrderPoint))
                    {
                        pointIndex = indexOrderPoint;
                        return true;
                    }
                }
            }

            pointIndex = default;
            return false;
        }

        private HashSet<int> IndexPointContainers(HashSet<Entity> containers)
        {
            var barPoints = _barmanPointsContainerQuery.ToComponentDataArray<BarmanPointContainer>(Allocator.Temp);

            var containerPoints = new HashSet<int>();

            foreach (var containerEntity in containers)
            {
                foreach (var barPoint in barPoints)
                {
                    if (barPoint.Container == containerEntity)
                    {
                        containerPoints.Add(barPoint.Index);
                    }
                }
            }

            return containerPoints;
        }
    }
}