﻿using System.Collections.Generic;
using System.Linq;
using Core.Authoring.Products;
using Core.Authoring.StoreRatings;
using Core.Authoring.Tables;
using Core.Configs;
using Core.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Authoring.Customers.Systems
{
    [RequireMatchingQueriesForUpdate]
    public partial class RespawnCustomerSystem : SystemBase
    {
        private EntityQuery _customerQuery;
        private EntityQuery _spawnPointCustomerQuery;
        private EntityQuery _tablePointQuery;
        private EntityQuery _storeRatingQuery;
        private EntityQuery _upCompletedQuery;
        
        private float _time;
        private HashSet<CustomerConfigData> _usedConfigs = new ();
        
        protected override void OnCreate()
        {
            using var spawnPointCustomerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _spawnPointCustomerQuery = spawnPointCustomerBuilder.WithAll<SpawnPointCustomer>().Build(this);
            
            using var tablePointBuilder = new EntityQueryBuilder(Allocator.Temp);
            _tablePointQuery = tablePointBuilder.WithAll<AtTablePoint>().WithNone<PointDirtTable>().Build(this);

            using var storeRatingBuilder = new EntityQueryBuilder(Allocator.Temp);
            _storeRatingQuery = storeRatingBuilder.WithAll<StoreRating>().Build(this);

            using var customerBuilder = new EntityQueryBuilder(Allocator.Temp);
            _customerQuery = customerBuilder.WithAll<Customer>().Build(this);
            
            using var upCompletedBuilder = new EntityQueryBuilder(Allocator.Temp);
            _upCompletedQuery = upCompletedBuilder.WithAll<AvailableUp>().Build(this);
        }

        protected override void OnUpdate()
        {
            var customerCount = 0;

            if (!_customerQuery.IsEmpty)
            {
                customerCount = _customerQuery.ToEntityArray(Allocator.Temp).Length;
            }

            var customerConfig = EntityUtilities.GetCustomerConfig();
            var tablePoints = _tablePointQuery.ToEntityArray(Allocator.Temp).Length;
            var maxCustomers =
                Mathf.FloorToInt(customerConfig.StartCountCustomers + tablePoints * customerConfig.Ratio);
            customerConfig.CurrentMaxCustomers = maxCustomers;

            _time += World.Time.DeltaTime;

            if (!(_time >= 1f) || customerCount >= maxCustomers)
            {
                return;
            }

            CreateCustomer();
            _time = 0;
        }

        private void CreateCustomer()
        {
            var storeRating = _storeRatingQuery.GetSingleton<StoreRating>();
            var config = EntityUtilities.GetGameConfig();
            var spawnArray = _spawnPointCustomerQuery.ToEntityArray(Allocator.Temp);
            var spawnPointEntity = spawnArray[0];
            var spawnPoint = EntityManager.GetComponentData<SpawnPointCustomer>(spawnPointEntity);
            var customerConfigs = config.CustomerConfig.Customers;
            var result = SelectCustomers(customerConfigs, storeRating.CurrentValue);

            if (result.Count == 0)
            {
                // _usedConfigs.Clear();
                // result = SelectCustomers(customerConfigs, storeRating.CurrentValue);
                //
                // if (result.Count == 0)
                // {
                //     return;
                // }
                return;
            }
            
            var upCompletedEntity = _upCompletedQuery.ToEntityArray(Allocator.Temp)[0];
            var productToBay = EntityManager.GetComponentObject<ProductToBay>(upCompletedEntity).Products.ToArray();
            
            var random = Random.Range(0, result.Count);
            var customerData = result[random];
            
            var customerEntity = EntityManager.CreateEntity();

            var customerUi = config.UIConfig.CustomerUiPrefab;

            var randomCountProduct = Random.Range(1, 3);
            var productToBayCustomer = new List<ProductData>();

            while (productToBayCustomer.Count < randomCountProduct)
            {
                var randomProduct = Random.Range(0, productToBay.Length);
                var prod = productToBay[randomProduct];

                if (productToBayCustomer.Contains(prod))
                {
                    continue;
                }

                prod.Count = Random.Range(1, 3);
                productToBayCustomer.Add(prod);
            }

            EntityManager.AddComponentObject(customerEntity, new SpawnCustomer
            {
                CustomerPrefab = customerData.Visual.Prefab, 
                Point = spawnPoint,
                Level = Mathf.FloorToInt(customerData.RatingMin), 
                Avatar = customerData.Visual.Avatar, 
                Products = productToBayCustomer.ToArray(),
                Dialogs = customerData.Dialogs, 
                CustomerUiPrefab = customerUi,
                Audio = customerData.Audio, 
                CustomerData = customerData
            });
        }
        
        List<CustomerConfigData> SelectCustomers(CustomerConfigData[] customerConfigs, int rating)
        {
            var result = new List<CustomerConfigData>();
            
            foreach (var customerConfig in customerConfigs)
            {
                if (customerConfig.RatingMin <= rating 
                    && customerConfig.RatingMax >= rating
                    && !_usedConfigs.Contains(customerConfig))
                {
                    result.Add(customerConfig);
                }
            }

            return result;
        }
    }
}