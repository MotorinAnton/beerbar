using System;
using Core.Authoring.Points;
using Unity.Entities;
using UnityEngine;

namespace Core.Authoring.Bartenders
{
    public class SpawnPointBarmanAuthoring : MonoBehaviour
    {
        [SerializeField] private BarmanSpawnPoints _barmanSpawnPoints;
        public class SpawnPointCameraBaker : Baker<SpawnPointBarmanAuthoring>
        {
            public override void Bake(SpawnPointBarmanAuthoring authoring)
            {
                for (var i = 0; i < authoring._barmanSpawnPoints.BarmanSpawnPoint.Length; i++)
                {
                    var entity = CreateAdditionalEntity(TransformUsageFlags.None);
                    var spawnPoint = authoring._barmanSpawnPoints.BarmanSpawnPoint[i];
                    AddComponent(entity, new SpawnPoint { Position = spawnPoint.position, Rotation = spawnPoint.rotation } );
                    AddComponent<BarmanSpawnPoint>(entity);
                }
            }
        }
    }

    [Serializable]
    public class BarmanSpawnPoints
    {
        public Transform[] BarmanSpawnPoint;
    }
    
    public struct BarmanSpawnPoint : IComponentData { }
}