using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Banks;
using Core.Authoring.Characters;
using Core.Authoring.Customers.CustomersUi;
using Core.Authoring.EventObjects;
using Core.Authoring.MovementArrows;
using Core.Authoring.PhraseCustomerUi;
using Core.Authoring.Points;
using Core.Authoring.ProfitUi;
using Core.Authoring.StoreRatings;
using Core.Authoring.Tables;
using Core.Components;
using Core.Components.Destroyed;
using Core.Components.Wait;
using Core.Configs;
using Core.Constants;
using Core.Utilities;
using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using MoveCharacterCompleted = Core.Authoring.Characters.MoveCharacterCompleted;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Core.Authoring.Customers.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class MoveExitCustomerSystem : SystemBase
    {
        private EntityQuery _moveExitCustomerQuery;
        private EntityQuery _waitingMoveExitCustomerQuery;
        private EntityQuery _movePointUpdateIndexQueueCustomersQuery;
        private EntityQuery _randomEventCustomerQuery;
        private EntityQuery _randomEventStartCustomerQuery;
        private EntityQuery _randomEventEndedCustomerQuery;
        private EntityQuery _moveDinkAtTheTableCustomerQuery;
        private EntityQuery _dinkAtTheTableCustomerQuery;
        private EntityQuery _toastingCustomerQuery;
        private EntityQuery _afterToastingCustomerQuery;
        private EntityQuery _afterDinkAtTheTableCustomerQuery;
        private EntityQuery _exitPointQuery;
        private EntityQuery _tablePointQuery;
        private EntityQuery _allTablePointQuery;
        private EntityQuery _updatePositionPointQuery;
        private EntityQuery _purchasePointsQuery;
        private EntityQuery _purchaseQueueAllCustomersQuery;
        private EntityQuery _storeRatingQuery;
        private EntityQuery _phraseUiManager;
        private EntityQuery _bankQuery;


        protected override void OnCreate()
        {
            using var moveExitCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveExitCustomerQuery = moveExitCustomerBuilder.WithAll<Customer, MoveExit>()
                .WithNone<MoveCharacter, WaitTime, WaitingCustomer>().Build(this);

            using var waitingMoveExitCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _waitingMoveExitCustomerQuery = waitingMoveExitCustomerBuilder
                .WithAll<Customer, MoveExit, WaitingCustomer, WaitTime>().Build(this);

            using var waitingFinishedMoveExitCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _movePointUpdateIndexQueueCustomersQuery = waitingFinishedMoveExitCustomerBuilder
                .WithAll<Customer, MoveExit, WaitingCustomer,UpdatePurchaseQueuePosition>().WithNone<WaitTime>()
                .Build(this);

            using var randomEventCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _randomEventCustomerQuery = randomEventCustomerBuilder
                .WithAll<Customer, RandomEvent>()
                .WithNone<UpdatePurchaseQueuePosition, RandomEventStart, RandomEventEnded>()
                .Build(this); 

            using var randomEventStartCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _randomEventStartCustomerQuery = randomEventStartCustomerBuilder
                .WithAll<Customer, RandomEvent, RandomEventStart>().Build(this);

            using var randomEventEndedCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _randomEventEndedCustomerQuery = randomEventEndedCustomerBuilder
                .WithAll<Customer, RandomEvent, RandomEventEnded>().WithNone<RandomEventStart, WaitTime>()
                .Build(this);

            using var moveDrinkAtTheTableCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _moveDinkAtTheTableCustomerQuery = moveDrinkAtTheTableCustomerBuilder
                .WithAll<Customer, DrinkAtTheTableCustomer, IndexMovePoint>()
                .WithNone<WaitTime, UpdatePurchaseQueuePosition>()
                .Build(this);

            using var drinkAtTheTableCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _dinkAtTheTableCustomerQuery = drinkAtTheTableCustomerBuilder
                .WithAll<Customer, DrinkAtTheTableCustomer, IndexMovePoint, WaitTime, StartWaitTime>()
                .WithNone<ToastCustomer,UpdatePurchaseQueuePosition>()
                .Build(this);

            using var toastingCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _toastingCustomerQuery = toastingCustomerBuilder
                .WithAll<Customer, DrinkAtTheTableCustomer, IndexMovePoint, ToastCustomer, WaitTime>()
                .Build(this);

            using var afterToastingCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _afterToastingCustomerQuery = afterToastingCustomerBuilder
                .WithAll<Customer, DrinkAtTheTableCustomer, IndexMovePoint, ToastCustomer>().WithNone<WaitTime,UpdatePurchaseQueuePosition>()
                .Build(this);

            using var afterDrinkAtTheTableCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _afterDinkAtTheTableCustomerQuery = afterDrinkAtTheTableCustomerBuilder
                .WithAll<Customer, DrinkAtTheTableCustomer, IndexMovePoint, WaitingCustomer>()
                .WithNone<WaitTime, ToastCustomer, UpdatePurchaseQueuePosition>().Build(this);
            
            using var purchaseQueueAllCustomersBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchaseQueueAllCustomersQuery = purchaseQueueAllCustomersBuilder
                .WithAll<Customer, PurchaseQueueCustomer>().Build(this);

            using var exitPointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _exitPointQuery = exitPointBuilder.WithAll<ExitPoint, MoveCustomerPoint>().Build(this);

            using var tablePointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _tablePointQuery = tablePointBuilder.WithAll<AtTablePoint>().WithNone<PointNotAvailable>().Build(this);

            using var allTablePointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _allTablePointQuery = allTablePointBuilder.WithAll<AtTablePoint>().Build(this);

            using var updatePositionPointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _updatePositionPointQuery = updatePositionPointBuilder
                .WithAll<UpdateQueuePositionPoint, MoveCustomerPoint>().Build(this);

            using var purchasePointsBuilder = new EntityQueryBuilder(Allocator.Temp);
            _purchasePointsQuery = purchasePointsBuilder.WithAll<PurchasePoint, MoveCustomerPoint>()
                .WithNone<PointNotAvailable>().Build(this);

            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAllRW<StoreRating>().Build(this);

            using var phraseUiManagerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _phraseUiManager = phraseUiManagerBuilder.WithAll<PhraseCustomerUiManagerView>().Build(this);

            using var bankBuilder = new EntityQueryBuilder(Allocator.Temp);
            _bankQuery = bankBuilder.WithAllRW<Bank>().Build(this);
        }

        protected override void OnUpdate()
        {
            MovePointUpdateIndexQueueCustomers();
            WaitingCustomers();
            MoveDrinkAtTheTableCustomers();
            DrinkAtTheTableCustomers();
            AfterDrinkAtTheTableCustomers();
            EventCustomers();
            RandomEventStartCustomers();
            RandomEventEndedCustomers();
            ToastAtTheTableCustomers();
            AfterToastAtTheTableCustomers();
            MoveExitCustomers();
        }

        private void MoveExitCustomers()
        {
            var customerEntityArray = _moveExitCustomerQuery.ToEntityArray(Allocator.Temp);
            var exitPoint = _exitPointQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp)[0].Point.Position;

            foreach (var customerEntity in customerEntityArray)
            {
                if (!EntityManager.HasComponent<MoveCharacterCompleted>(customerEntity) &&
                    !EntityManager.HasComponent<UpdatePurchaseQueuePosition>(customerEntity))
                {
                    EntityManager.AddComponentData(customerEntity, new MoveCharacter { TargetPoint = exitPoint });
                }

                if (!EntityManager.HasComponent<MoveCharacterCompleted>(customerEntity) &&
                    EntityManager.HasComponent<UpdatePurchaseQueuePosition>(customerEntity))
                {
                    var customerPos = EntityManager.GetComponentObject<TransformView>(customerEntity).Value.transform
                        .position;

                    var vector = customerPos;
                    vector.x -= 0.4f;
                    vector.z -= 0.4f;
                    vector.y = 0;
                    
                    EntityManager.AddComponentData(customerEntity, new MoveCharacter { TargetPoint = vector });
                    
                    /*if (EntityManager.HasComponent<DissatisfiedCustomer>(customerEntity))
                    {
                        continue;
                    }

                    CreateRandomEventCustomer(customerEntity, exitPoint);
                    
                    var customerConfig = EntityUtilities.GetCustomerConfig();
                    
                    if (Random.Range(0, 100) >= customerConfig.DrinkAtTheTableChance)
                    {
                        continue;
                    }

                    if (!CheckFreeTablePoint(customerEntity, out var freeIndex))
                    {
                        continue;
                    }

                    var tablePoints =
                        _tablePointQuery.ToComponentDataArray<AtTablePoint>(Allocator.Temp);
                    var tablePoint =
                        tablePoints.FirstOrDefault(tablePoint =>
                            tablePoint.IndexPoint == freeIndex);
                    var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                    customerIndex.Value = freeIndex;
                    
                    var positionCustomer = EntityManager.GetComponentObject<TransformView>(customerEntity).Value.position;
                    var newUpdateQueuePoint = CalculateUpdatePoint(positionCustomer, tablePoint.Point.Position, 0.4f);
                    
                    EntityManager.AddComponentData(customerEntity, new MoveCharacter { TargetPoint =  newUpdateQueuePoint});
                                
                    CreateRandomEventCustomer(customerEntity, tablePoint.Point.Position );
                    EntityManager.SetComponentData(customerEntity, customerIndex);
                    EntityManager.AddComponent<DrinkAtTheTableCustomer>(customerEntity);
                    EntityManager.RemoveComponent<MoveExit>(customerEntity);*/
                }
                
                if (!EntityManager.HasComponent<UpdatePurchaseQueuePosition>(customerEntity) &&
                    EntityManager.HasComponent<MoveCharacterCompleted>(customerEntity))
                {
                    var customerUiEntity = EntityManager.GetComponentObject<CustomerView>(customerEntity).UiEntity;
                    var customerUIView = EntityManager.GetComponentObject<CustomerUiView>(customerUiEntity).Value;
                    DOTween.KillAll(customerUIView.gameObject);
                    EntityManager.AddComponent<Destroyed>(customerUiEntity);
                    EntityManager.AddComponent<Destroyed>(customerEntity);
                }

                if (!EntityManager.HasComponent<MoveCharacterCompleted>(customerEntity) ||
                    !EntityManager.HasComponent<UpdatePurchaseQueuePosition>(customerEntity))
                {
                    continue;
                }

                
                var updateRow = EntityManager.GetComponentData<UpdatePurchaseQueuePosition>(customerEntity)
                    .UpdateRow;
                
                
                var entityCustomerUi = EntityManager.GetComponentData<CustomerUIEntity>(customerEntity).UiEntity;
                var customerUiView = EntityManager.GetComponentObject<CustomerUiView>(entityCustomerUi);
                    
                customerUiView.Value.CanvasGroupFaceEmotion.DOFade(0, 0.8f).SetDelay(1f);
                customerUiView.Value.CanvasGroupProduct1.DOFade(0, 0.8f).SetDelay(1f);
                customerUiView.Value.CanvasGroupProduct2.DOFade(0, 0.8f).SetDelay(1f);

                EntityManager.RemoveComponent<PurchaseQueueCustomer>(customerEntity);
                
                UpdatePurchaseQueueCustomersPosition(updateRow);
                
                EntityManager.RemoveComponent<UpdatePurchaseQueuePosition>(customerEntity);
                EntityManager.RemoveComponent<MoveCharacterCompleted>(customerEntity);
                
                if (EntityManager.HasComponent<DissatisfiedCustomer>(customerEntity))
                {
                    continue;
                }

                CreateRandomEventCustomer(customerEntity, exitPoint);

                if (EntityManager.HasComponent<RandomEvent>(customerEntity))
                {
                    EntityManager.RemoveComponent<MoveCharacterCompleted>(customerEntity);
                }

                var customerConfig = EntityUtilities.GetCustomerConfig();
                    
                if (Random.Range(0, 100) >= customerConfig.DrinkAtTheTableChance)
                {
                    continue;
                }

                if (!CheckFreeTablePoint(customerEntity, out var freeIndex))
                {
                    continue;
                }

                var tablePoints =
                    _tablePointQuery.ToComponentDataArray<AtTablePoint>(Allocator.Temp);
                var tablePoint =
                    tablePoints.FirstOrDefault(tablePoint =>
                        tablePoint.IndexPoint == freeIndex);
                var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                customerIndex.Value = freeIndex;
                                
                CreateRandomEventCustomer(customerEntity, tablePoint.Point.Position );
                EntityManager.SetComponentData(customerEntity, customerIndex);
                EntityManager.AddComponent<DrinkAtTheTableCustomer>(customerEntity);
                EntityManager.RemoveComponent<MoveCharacterCompleted>(customerEntity);
                EntityManager.RemoveComponent<MoveExit>(customerEntity);

                
            }
        }

        private Vector3 CalculateUpdatePoint(Vector3 a, Vector3 b, float distance)
        {
            var direction = (b - a).normalized;
            var point = a + direction * distance;

            return point;
        }
        private void WaitingCustomers()
        {
            var waitingCustomerEntity = _waitingMoveExitCustomerQuery.ToEntityArray(Allocator.Temp);

            foreach (var customerEntity in waitingCustomerEntity)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;
                animator.SetBool(CustomerAnimationConstants.Walk, false);
            }
        }

        private void MovePointUpdateIndexQueueCustomers()
        {
            var movePointUpdateIndexQueueCustomerArray = _movePointUpdateIndexQueueCustomersQuery.ToEntityArray(Allocator.Temp);
            var updatePositionPoint =
                _updatePositionPointQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);

            foreach (var customerEntity in movePointUpdateIndexQueueCustomerArray)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;

                var indexPoint = EntityManager.GetComponentData<UpdatePurchaseQueuePosition>(customerEntity)
                    .UpdateRow;
                EntityManager.AddComponentData(customerEntity,
                    new MoveCharacter { TargetPoint = updatePositionPoint[indexPoint].Point.Position });
                EntityManager.RemoveComponent<WaitingCustomer>(customerEntity);
                EntityManager.RemoveComponent<PhraseSayCustomer>(customerEntity);

                animator.SetBool(CustomerAnimationConstants.CashIdle, false);
                animator.SetBool(CustomerAnimationConstants.TakeBottle, false);
            }
        }

        private void EventCustomers()
        {
            var eventCustomerArray = _randomEventCustomerQuery.ToEntityArray(Allocator.Temp);

            foreach (var customerEntity in eventCustomerArray)
            {
                var transform = EntityManager.GetComponentObject<TransformView>(customerEntity).Value;
                var distanceToExit = EntityManager.GetComponentData<RandomEvent>(customerEntity).DistanceToExit;

                if (!EntityManager.HasComponent<MoveCharacter>(customerEntity))
                {
                    continue;
                }

                var point = EntityManager.GetComponentData<MoveCharacter>(customerEntity).TargetPoint;
                var distanceToPosition = Vector3.Distance(transform.position, point);
                
                if (distanceToPosition <= distanceToExit)
                {
                    EntityManager.AddComponent<RandomEventStart>(customerEntity);
                }
            }
        }

        private void RandomEventStartCustomers()
        {
            var randomEventStartCustomerEntity = _randomEventStartCustomerQuery.ToEntityArray(Allocator.Temp);

            foreach (var customerEntity in randomEventStartCustomerEntity)
            {
                switch (Random.Range(0, 2))
                {
                    case 0:

                        EntityManager.AddComponent<BreakBottleCustomer>(customerEntity);
                        break;

                    case 1:

                        EntityManager.AddComponent<LossWalletCustomer>(customerEntity);
                        
                        break;
                }
                
                EntityManager.RemoveComponent<RandomEventStart>(customerEntity);
                EntityManager.AddComponent<RandomEventEnded>(customerEntity);
            }
        }

        private void RandomEventEndedCustomers()
        {
            var randomEventEndedCustomerEntity = _randomEventEndedCustomerQuery.ToEntityArray(Allocator.Temp);
            var config = EntityUtilities.GetGameConfig();

            foreach (var customerEntity in randomEventEndedCustomerEntity)
            {
                var transform = EntityManager.GetComponentObject<TransformView>(customerEntity).Value;

                if (EntityManager.HasComponent<BreakBottleCustomer>(customerEntity))
                {
                    var customerView = EntityManager.GetComponentObject<CustomerView>(customerEntity);
                    var customerUiEntity = EntityManager.GetComponentObject<CustomerView>(customerEntity).UiEntity;
                    var customerUiView = EntityManager.GetComponentObject<CustomerUiView>(customerUiEntity).Value;
                    var audioSource = EntityManager.GetComponentObject<AudioSourceView>(customerEntity).Value;
                    customerUiView.FaceEmotionImage.overrideSprite = customerUiView.FaceEmotionSprites.Swears;
                    var breakBottleView = Object.Instantiate(config.EventObjectConfig.BreakBottlePrefab,
                        transform.position, transform.rotation);
                    
                    var breakBottleObjectEntity = EntityManager.CreateEntity();
                    
                    AddClearMovementArrow(breakBottleView.transform, breakBottleObjectEntity);

                    customerView.Value.PivotHand[0].gameObject.SetActive(false);
                    EntityManager.SetName(breakBottleObjectEntity, EntityConstants.BreakBottleName);
                    EntityManager.AddComponentData(breakBottleObjectEntity, new WaitTime { Current = 5f });
                    EntityManager.AddComponent<BreakBottleEntity>(breakBottleObjectEntity);
                    EntityManager.AddComponentObject(breakBottleObjectEntity,
                        new BreakBottleView { Value = breakBottleView });
                    breakBottleView.Initialize(EntityManager, breakBottleObjectEntity);
                    EntityManager.RemoveComponent<BreakBottleCustomer>(customerEntity);
                    
                    if (!_phraseUiManager.IsEmpty)
                    {
                        var randomPhrase = Random.Range(0, customerView.Dialogs.EventPurchase.Length);
                        var phraseUiManagerEntity = _phraseUiManager.ToEntityArray(Allocator.Temp)[0];
                        var phraseUiManager =
                            EntityManager.GetComponentObject<PhraseCustomerUiManagerView>(phraseUiManagerEntity)
                                .PhraseCustomerUiManager;

                        phraseUiManager.EventPanel.SetPhrasePanelUiComponent(
                            customerView.Dialogs.EventPurchase[randomPhrase], customerView.Avatar);

                        phraseUiManager.StartEventPanelTween();
                    } 
                }

                if (EntityManager.HasComponent<LossWalletCustomer>(customerEntity))
                {
                    var lossWalletsArray =
                        config.CustomerConfig.LossWallets;

                    var storeRatingScore = _storeRatingQuery.GetSingleton<StoreRating>().CurrentValue;
                    
                    var availableLossWallets = new HashSet<LossWallet>();

                    foreach (var lossWallet in lossWalletsArray)
                    {
                        if (lossWallet.Rating <= storeRatingScore)
                        {
                            availableLossWallets.Add(lossWallet);
                        }
                    }

                    var maxLossWallet = MaxWallet(availableLossWallets);

                    var lossWalletView = Object.Instantiate(config.EventObjectConfig.LossWalletPrefab,
                        transform.position, transform.rotation);
                    
                    var startPosition = transform.position;
                    startPosition.y += 0.6f;

                    lossWalletView.transform.position = startPosition;
                    
                    
                    var position = transform.position;
                    position.z += Random.Range(-0.5f, 0.5f);
                    position.x += Random.Range(-0.5f, 0.5f);
                    
                    lossWalletView.transform.DOMoveY(startPosition.y + 0.6f, 0.5f).SetEase(Ease.OutSine);
                    lossWalletView.transform.DOMoveY(0.1f, 0.99f).SetEase(Ease.OutSine).SetDelay(0.5f);

                    var lossWalletEntity = EntityManager.CreateEntity();

                    EntityManager.AddComponentData(lossWalletEntity,
                        new LossWalletEntity { Coins = Random.Range(maxLossWallet.MinCoins, maxLossWallet.MaxCoins) });
                    EntityManager.SetName(lossWalletEntity, EntityConstants.LossWalletName);
                    EntityManager.AddComponentObject(lossWalletEntity,
                        new LossWalletView { Value = lossWalletView });
                    lossWalletView.Initialize(EntityManager, lossWalletEntity);
                    EntityManager.RemoveComponent<LossWalletCustomer>(customerEntity);
                }
                
                EntityManager.RemoveComponent<RandomEvent>(customerEntity);
                EntityManager.RemoveComponent<RandomEventEnded>(customerEntity);
            }
        }

        private LossWallet MaxWallet(HashSet<LossWallet> wallets)
        {
            var maxWallet = 0;
            var foundWallet = new LossWallet();

            foreach (var wallet in wallets)
            {
                if (wallet.MaxCoins > maxWallet)
                {
                    maxWallet = wallet.MaxCoins;
                    foundWallet = wallet;
                }
            }

            return foundWallet;
        }

        private void MoveDrinkAtTheTableCustomers()
        {
            var moveDrinkAtTheTableCustomerEntity = _moveDinkAtTheTableCustomerQuery.ToEntityArray(Allocator.Temp);
            var tablePoints =
                _tablePointQuery.ToComponentDataArray<AtTablePoint>(Allocator.Temp);

            foreach (var customerEntity in moveDrinkAtTheTableCustomerEntity)
            {
                var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                var customerUiEntity = EntityManager.GetComponentObject<CustomerView>(customerEntity).UiEntity;
                var customerUiView = EntityManager.GetComponentObject<CustomerUiView>(customerUiEntity).Value;

                if (EntityManager.HasComponent<MoveCharacterCompleted>(customerEntity))
                {
                    var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;
                    var tablePointEntity = _tablePointQuery.ToEntityArray(Allocator.Temp);
                    var tablePoint = tablePointEntity.FirstOrDefault(tablePoint =>
                        EntityManager.GetComponentData<AtTablePoint>(tablePoint).IndexPoint == customerIndex.Value);

                    if (EntityManager.HasComponent<PointDirtTable>(tablePoint))
                    {
                        var exitPoint = _exitPointQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp)[0].Point
                            .Position;
                        var storeRating = _storeRatingQuery.GetSingleton<StoreRating>();

                        StarDirtTableEmotionEventPanel(customerEntity);
                        SpawnDisplaseEmotion(customerEntity);

                        if (!customerUiView.FaceEmotionImage.IsActive())
                        {
                            customerUiView.FaceEmotionImage.overrideSprite =
                                                     customerUiView.FaceEmotionSprites.Swears;
                            customerUiView.EnableDialog();
                            customerUiView.EnableFaceEmotion();
                        }
                        
                        
                        storeRating.SuccessPoints -= 1;
                        _storeRatingQuery.SetSingleton(storeRating);
                        EntityManager.AddComponent<MoveExit>(customerEntity);
                        EntityManager.AddComponentData(customerEntity, new MoveCharacter { TargetPoint = exitPoint });
                        EntityManager.RemoveComponent<DrinkAtTheTableCustomer>(customerEntity);
                    }
                    else
                    {
                        customerUiView.FaceEmotionImage.overrideSprite =
                            customerUiView.FaceEmotionSprites.DrinkBeer;
                        customerUiView.DisableDialog();
                        customerUiView.DisableFaceEmotion();

                        var randomDrinkAnimation = Random.Range(0, 3);
                        var drinkAtTheTableTime = EntityUtilities.GetCustomerConfig().DrinkAtTheTableTime;
                        animator.SetFloat(CustomerAnimationConstants.RandomDrink, randomDrinkAnimation);
                        EntityManager.AddComponentData(customerEntity, new WaitTime { Current = drinkAtTheTableTime });
                        var nextTransitionTime = drinkAtTheTableTime - 5f;
                        EntityManager.AddComponentData(customerEntity,
                            new RandomAnimation
                            {
                                NumberAnimation = randomDrinkAnimation, 
                                TransitionNextAnimation = false,
                                NextTimeTransition = nextTransitionTime
                            });
                    }

                    EntityManager.RemoveComponent<MoveCharacterCompleted>(customerEntity);
                }
                else
                {
                    var tablePoint =
                        tablePoints.FirstOrDefault(tablePoint =>
                            tablePoint.IndexPoint == customerIndex.Value); // непонятно почему тут мб дефаулт
                    if (tablePoint.Table == default)
                    {
                        if (CheckFreeTablePoint(customerEntity, out var freeIndexTablePoint))
                        {
                            customerIndex.Value = freeIndexTablePoint;
                            EntityManager.SetComponentData(customerEntity, customerIndex);
                            tablePoint = tablePoints.FirstOrDefault(tablePoint =>
                                tablePoint.IndexPoint == customerIndex.Value);
                        }
                    }
                    
                    EntityManager.AddComponentData(customerEntity,
                        new MoveCharacter { TargetPoint = tablePoint.Point.Position });
                }
            }
        }

        private void StarDirtTableEmotionEventPanel(Entity customerEntity)
        {
            if (_phraseUiManager.IsEmpty)
            {
                return;
            }
            
            var customerView = EntityManager.GetComponentObject<CustomerView>(customerEntity);
            var randomPhrase = Random.Range(0, customerView.Dialogs.DirtyTable.Length);
            var phraseUiManagerEntity = _phraseUiManager.ToEntityArray(Allocator.Temp)[0];
            var phraseUiManager =
                EntityManager.GetComponentObject<PhraseCustomerUiManagerView>(phraseUiManagerEntity)
                    .PhraseCustomerUiManager;

            phraseUiManager.EventPanel.SetPhrasePanelUiComponent(
                customerView.Dialogs.DirtyTable[randomPhrase], customerView.Avatar);
            phraseUiManager.StartEventPanelTween();
        }
        
        private void SpawnDisplaseEmotion(Entity customerEntity)
        {
            var customerTransform = EntityManager.GetComponentObject<TransformView>(customerEntity);
            var profitUiPosition = customerTransform.Value.position;

            profitUiPosition.y += 2f;
            profitUiPosition.x -= 1f;

            var spawnProfitUiEntity = EntityManager.CreateEntity();
            EntityManager.AddComponentObject(spawnProfitUiEntity,
                new SpawnProfitUi
                    { Type = ProfitUiType.Displase, Point = profitUiPosition, Text = "-" + 1 });
            
        }
        

        private void DrinkAtTheTableCustomers()
        {
            var drinkAtTheTableCustomerEntity = _dinkAtTheTableCustomerQuery.ToEntityArray(Allocator.Temp);

            var tablePoints =
                _allTablePointQuery.ToComponentDataArray<AtTablePoint>(Allocator.Temp);

            foreach (var customerEntity in drinkAtTheTableCustomerEntity)
            {
                EntityManager.GetComponentObject<CustomerView>(customerEntity).Value.PivotHand[0].gameObject
                    .SetActive(true);
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity);
                var randomAnimation = EntityManager.GetComponentData<RandomAnimation>(customerEntity);
                var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                var tableEntity = tablePoints.FirstOrDefault(tablePoint => tablePoint.IndexPoint == customerIndex.Value)
                    .Table;
                var tablePosition =
                    EntityManager.GetComponentObject<TableView>(tableEntity).Value.transform.position;
                
                var customerView =
                    EntityManager.GetComponentObject<CustomerView>(customerEntity).Value;
                
                customerView.TurningCharacterToPoint(tablePosition);
  
                animator.Value.SetBool(CustomerAnimationConstants.DrinkAtTheTable, true);

                var animationNumber = animator.Value.GetFloat(CustomerAnimationConstants.RandomDrink);
                var waitTime = EntityManager.GetComponentData<WaitTime>(customerEntity).Current;
                var up = true;
                
                if (waitTime <= randomAnimation.NextTimeTransition)
                {
                    randomAnimation.TransitionNextAnimation = true;
                    randomAnimation.NextTimeTransition -= 5;
                    randomAnimation.NumberAnimation = NextNumberAnimation((int)animationNumber);
                  
                    var customerTransform = EntityManager.GetComponentObject<TransformView>(customerEntity);
                    var profitUiPosition = customerTransform.Value.position;
                    var tableLevel = EntityManager.GetComponentData<Table>(tableEntity).Level;
                    
                    profitUiPosition.y += 2f;
                    
                    var spawnProfitUiEntity = EntityManager.CreateEntity();
                    EntityManager.AddComponentObject(spawnProfitUiEntity,
                        new SpawnProfitUi
                            { Type = ProfitUiType.Profit, Point = profitUiPosition, Text = "+" + 1 * tableLevel });
                 
                    var bank = _bankQuery.GetSingleton<Bank>();
                    bank.Coins += 1 * tableLevel ;
                    _bankQuery.SetSingleton(bank);
                }

                if (randomAnimation.TransitionNextAnimation)
                {
                    if (randomAnimation.NumberAnimation < animationNumber)
                    {
                        up = false;
                    }

                    if (up)
                    {
                        animationNumber = Mathf.Lerp(animationNumber, randomAnimation.NumberAnimation + 0.02f,
                            0.2f * World.Time.DeltaTime);

                        if (animationNumber >= randomAnimation.NumberAnimation)
                        {
                            randomAnimation.TransitionNextAnimation = false;
                            randomAnimation.NextTimeTransition = waitTime - 5;
                            animationNumber = randomAnimation.NumberAnimation;
                        }
                    }
                    else
                    {
                        animationNumber = Mathf.Lerp(animationNumber, randomAnimation.NumberAnimation - 0.02f,
                            0.2f * World.Time.DeltaTime);

                        if (animationNumber <= randomAnimation.NumberAnimation)
                        {
                            randomAnimation.TransitionNextAnimation = false;
                            randomAnimation.NextTimeTransition = waitTime - 5;
                            animationNumber = 0;
                        }
                    }
                }

                animator.Value.SetFloat(CustomerAnimationConstants.RandomDrink, animationNumber);
                EntityManager.SetComponentData(customerEntity, randomAnimation);
                EntityManager.AddComponent<WaitingCustomer>(customerEntity);
            }
        }

        private void ToastAtTheTableCustomers()
        {
            var toastAtTheTableCustomerEntity = _toastingCustomerQuery.ToEntityArray(Allocator.Temp);

            var tablePoints =
                _allTablePointQuery.ToComponentDataArray<AtTablePoint>(Allocator.Temp);

            foreach (var customerEntity in toastAtTheTableCustomerEntity)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;
                var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                var transform = EntityManager.GetComponentObject<TransformView>(customerEntity).Value;
                var tableEntity = tablePoints.FirstOrDefault(tablePoint => tablePoint.IndexPoint == customerIndex.Value)
                    .Table;
                var tableViewPosition =
                    EntityManager.GetComponentObject<TableView>(tableEntity).Value.transform.position;
                var direction = tableViewPosition - transform.position;
                var rotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 5f * World.Time.DeltaTime);
                animator.SetBool(CustomerAnimationConstants.Idle, false);
                animator.SetBool(CustomerAnimationConstants.Toast, true);
            }
        }

        private void AfterToastAtTheTableCustomers()
        {
            var afterToastAtTheTableCustomerEntity = _afterToastingCustomerQuery.ToEntityArray(Allocator.Temp);
            foreach (var customerEntity in afterToastAtTheTableCustomerEntity)
            {
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;

                animator.SetBool(CustomerAnimationConstants.Toast, false);

                var remainingTimeAtTheTable = EntityManager.GetComponentData<ToastCustomer>(customerEntity)
                    .RemainingTimeAtTheTable;
                EntityManager.AddComponentData(customerEntity, new WaitTime { Current = remainingTimeAtTheTable });
                EntityManager.RemoveComponent<ToastCustomer>(customerEntity);
            }
        }

        private void AfterDrinkAtTheTableCustomers()
        {
            var afterDrinkAtTheTableCustomerEntity = _afterDinkAtTheTableCustomerQuery.ToEntityArray(Allocator.Temp);
            var exitPoint = _exitPointQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp)[0].Point.Position;
            var tablePoints =
                _tablePointQuery.ToComponentDataArray<AtTablePoint>(Allocator.Temp);

            foreach (var customerEntity in afterDrinkAtTheTableCustomerEntity)
            {
                var customerUiEntity = EntityManager.GetComponentObject<CustomerView>(customerEntity).UiEntity;
                var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
                var animator = EntityManager.GetComponentObject<AnimatorView>(customerEntity).Value;
                var tableEntity = tablePoints.First(tablePoint => tablePoint.IndexPoint == customerIndex.Value).Table;
                var dirtValueTable = EntityManager.GetComponentData<Table>(tableEntity);
                
                animator.SetBool(CustomerAnimationConstants.DrinkAtTheTable, false);
                dirtValueTable.DirtValue += 1;
                
                EntityManager.AddComponent<PleasedEmotionCustomer>(customerUiEntity);
                
                CreateRandomEventCustomer(customerEntity, exitPoint);

                EntityManager.GetComponentObject<CustomerView>(customerEntity).Value.PivotHand[0].gameObject
                    .SetActive(false);
                EntityManager.SetComponentData(tableEntity, dirtValueTable);
                EntityManager.AddComponent<MoveExit>(customerEntity);
                EntityManager.AddComponentData(customerEntity, new MoveCharacter { TargetPoint = exitPoint });
                EntityManager.RemoveComponent<WaitingCustomer>(customerEntity);
                EntityManager.RemoveComponent<DrinkAtTheTableCustomer>(customerEntity);
            }
        }

        private void UpdatePurchaseQueueCustomersPosition(int updateRow)
        {
            var purchaseQueueCustomers = _purchaseQueueAllCustomersQuery.ToEntityArray(Allocator.Temp);
            var purchaseQueuePoints = _purchasePointsQuery.ToComponentDataArray<MoveCustomerPoint>(Allocator.Temp);
            var rowCount = purchaseQueuePoints.Select(point => point.Row).ToHashSet().Count;
            //var ocupPiont = _purchaseQueueAllCustomersQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            //var hash = ocupPiont.Select(s => s.Value);

            foreach (var customer in purchaseQueueCustomers)
            {
                var indexCustomer = EntityManager.GetComponentData<IndexMovePoint>(customer);
                var rowCustomer = indexCustomer.Value % rowCount;
                
                if (rowCustomer == updateRow && indexCustomer.Value >= rowCount )
                {
                    indexCustomer.Value -= rowCount;
                    EntityManager.SetComponentData(customer, indexCustomer);
                    EntityManager.RemoveComponent<WaitingCustomer>(customer);
                    if (EntityManager.HasComponent<WaitTime>(customer))
                    {
                        EntityManager.RemoveComponent<WaitTime>(customer);
                        EntityManager.RemoveComponent<StartWaitTime>(customer);
                    }

                    EntityManager.AddComponentData(customer, new WaitTime { Current = 0.2f * indexCustomer.Value });
                }
                
                //EntityManager.AddComponentData(customer, new WaitTime { Current = 0.2f * indexCustomer.Value });
            }
        }

        private bool CheckFreeTablePoint(Entity customerEntity, out int result)
        {
            var customerIndex = EntityManager.GetComponentData<IndexMovePoint>(customerEntity);
            var dinkAtTheTableCustomerIndexes =
                _dinkAtTheTableCustomerQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            var toastingCustomerIndexes = _toastingCustomerQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            var moveDrinkAtTheTableCustomer =
                _moveDinkAtTheTableCustomerQuery.ToComponentDataArray<IndexMovePoint>(Allocator.Temp);
            var tablePoints = _tablePointQuery.ToComponentDataArray<AtTablePoint>(Allocator.Temp);
            var customerIndexes = dinkAtTheTableCustomerIndexes
                .Select(customerDinkAtTheTableIndex => customerDinkAtTheTableIndex.Value).ToHashSet();
            var moveCustomerIndexes =
                moveDrinkAtTheTableCustomer.Select(indexMovePoint => indexMovePoint.Value).ToHashSet();
            var toastingCustomer = toastingCustomerIndexes.Select(indexMovePoint => indexMovePoint.Value).ToHashSet();
            var freeTablePoints = tablePoints.Select(point => point.IndexPoint).ToHashSet();

            freeTablePoints.ExceptWith(customerIndexes);
            freeTablePoints.ExceptWith(moveCustomerIndexes);
            freeTablePoints.ExceptWith(toastingCustomer);

            if (freeTablePoints.Count > 0)
            {
                result = freeTablePoints.ElementAt(new System.Random().Next(freeTablePoints.Count));
                return true;

            }

            result = result = customerIndex.Value;
            return false;
        }

        private void CreateRandomEventCustomer(Entity customerEntity , Vector3 movementPoint)
        {
            var customerConfig = EntityUtilities.GetCustomerConfig();

            if (Random.Range(0, 100) >= customerConfig.RandomEventChance)
            {
                return;
            }

            var transform = EntityManager.GetComponentObject<TransformView>(customerEntity).Value;
            var distance = Vector3.Distance(transform.position, movementPoint);
            var randomDistance = Random.Range(3f, distance - 3f);
            EntityManager.AddComponentData(customerEntity,
                new RandomEvent { DistanceToExit = randomDistance });
        }

        private int NextNumberAnimation(int currentAnimation)
        {
            var randomAnimationNumber = Random.Range(0, 3);

            if (randomAnimationNumber == currentAnimation)
            {
                randomAnimationNumber = NextNumberAnimation(currentAnimation);
            }

            return randomAnimationNumber;
        }

        private void AddClearMovementArrow(Transform parentTransform, Entity entity)
        {
            var config = EntityUtilities.GetGameConfig();
            var arrowPoint = parentTransform.position;
            arrowPoint.y += 0.3f;

            var clearArrow = Object.Instantiate(config.ClearArrow, arrowPoint, parentTransform.rotation);
            clearArrow.transform.SetParent(parentTransform);
            clearArrow.gameObject.SetActive(false);
            EntityManager.AddComponentObject(entity, new ClearMovementArrowView { Arrow = clearArrow});
        }
    }
}