using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(EntityBehaviour))]
public abstract class EntityComponentBehaviour : MonoBehaviour
{
    protected EntityManager Manager;
    protected Entity SelfEntity;
    
    public void Initialize(EntityManager manager, Entity entity)
    {
        Manager = manager;
        SelfEntity = entity;
    }

    public virtual void EntityDestroyed() { }
}