using System.Collections.Generic;
using Core.Authoring.MovementArrows;
using Core.Authoring.Points;
using Core.Authoring.SelectGameObjects;
using Core.Authoring.SelectGameObjects.Types;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Core.Authoring.Tables
{
    public sealed class TableAuthoring : SelectAuthoring<RendererSelectAuthoring>
    {
        [SerializeField] private NavMeshObstacle _agentObstacle;
        public NavMeshObstacle NavMeshObstacle  => _agentObstacle;

        [SerializeField] private ParticleSystem _up;
                
        public ParticleSystem ParticleSystem => _up;
    }

    public struct Table : IComponentData
    {
        public int Level;
        public int DirtValue;
    }
    
    public class SpawnTable : IComponentData
    {
        public TableAuthoring Prefab;
        public SpawnPoint SpawnPoint;
        public Point CleanTablePoint;
        public NativeArray<PointAtTheTable> AtTablePoints;
        public NativeArray<PointOnTheTable> OnTablePoints;
        public int Level;
        public int QuantityAtTablePoints;
        public int IndexLevelUpFx;
        public MovementArrowAuthoring ClearArrow;
        public MovementArrowAuthoring RepairArrow;
    }
    
    public class TableView : IComponentData
    {
        public TableAuthoring Value;
        public List<Entity> AtTablePointsEntity;
        public Point CleanTablePoint;
    }
    
    public struct CleanTable : IComponentData { }
    
    public struct OrderCleanTable : IBufferElementData
    {
        public Entity Table;
    }

    public class DirtTableViewEntities : IComponentData
    {
        public HashSet<Entity> DirtTableObjectEntities;
    }

    public class DirtTableView : IComponentData
    {
        public GameObject DirtObject;
    }
}