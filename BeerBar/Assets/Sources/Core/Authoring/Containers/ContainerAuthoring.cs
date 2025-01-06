using System;
using System.Collections.Generic;
using Core.Authoring.Points;
using Core.Authoring.Products;
using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Containers
{
    public sealed class ContainerAuthoring : SelectAuthoring<RendererSelectAuthoring>
    {
        [SerializeField] private PivotsContainer _pivots;
        
        public PivotsContainer Pivots => _pivots;
        
        
        [SerializeField] private ParticleSystem _up;
                
        public ParticleSystem ParticleSystem => _up;
        
    }

    [Serializable]
    public class PivotsContainer
    {
        public List<Transform> Product1;
        public List<Transform> Product2;
        public Transform IndicatorQuantityProduct1;
        public Transform IndicatorQuantityProduct2;
    }
    
    public struct Container : IComponentData { }
    
    public struct Fridge: IComponentData { }
    
    public struct FishSnack: IComponentData { }
    
    public struct MiniSnack: IComponentData { }
    
    public struct Spill: IComponentData { }

    public struct SpillLevelContainer : IComponentData
    {
        public int Value;
    }
    
    public struct Nuts: IComponentData { }
    
    public struct AddNewProducts : IComponentData { }
    
    public class SpawnContainer : IComponentData
    {
        public ContainerAuthoring Prefab;
        public SpawnPoint Point;
        public NativeArray<CustomerContainerPoint> CustomerContainerPoints;
        public NativeArray<BarmanContainerPoint> BarmanContainerPoints;
        public int Level;
        public int Capacity;
        public ProductType Type;
    }
    
    public class ContainerView : IComponentData
    {
        public ContainerAuthoring Value;
    }
    
    public struct ContainerDescription : IComponentData
    {
        public ProductType Type;
        public int Level;
        public int Capacity;
    }
    
    public struct ContainerProduct : IBufferElementData
    {
        public ProductData Value;
    }
}