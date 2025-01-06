using Core.Components;
using Unity.Entities;
using UnityEngine;

public class EntityBehaviour : MonoBehaviour
{
    [SerializeField] private EntityComponentBehaviour[] _componentBehaviours;

    public Transform Transform { get; private set; }
    
    public EntityManager EntityManager { get; private set; }
    
    public Entity Entity { get; private set; }
    
    private void OnValidate()
    {
        _componentBehaviours = GetComponents<EntityComponentBehaviour>();
    }
    
    public void Initialize(EntityManager manager, Entity entity)
    {
        EntityManager = manager;
        Entity = entity;
        foreach (var entityComponentBehaviour in _componentBehaviours)
        {
            entityComponentBehaviour.Initialize(manager, entity);
        }

        manager.AddComponentObject(entity, new EntityBehaviourView { Value = this });

        Transform = transform;
    }

    public void EntityDestroyed()
    {
        foreach (var entityComponentBehaviour in _componentBehaviours)
        {
            entityComponentBehaviour.EntityDestroyed();
        }
        Destroy(gameObject);
    }
}