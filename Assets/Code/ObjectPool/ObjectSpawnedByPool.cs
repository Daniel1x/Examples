using UnityEngine;

public class ObjectSpawnedByPool : MonoBehaviour, IPoolable
{
    protected float lifetime = 0f;
    protected float timeAlive = 0f;

    protected IPoolProvider poolProvider = null;

    protected virtual void Update()
    {
        if (lifetime < 0f)
        {
            return; // infinite lifetime
        }

        timeAlive += Time.deltaTime;

        if (timeAlive >= lifetime)
        {
            ReturnToPool();
        }
    }

    public virtual void Initialize(IPoolProvider pool)
    {
        poolProvider = pool;
        lifetime = poolProvider.ObjectMaxLifetime;
        timeAlive = 0f;
    }

    public virtual void ReturnToPool()
    {
        // Restore parent if required
        if (poolProvider != null && poolProvider.ParentTransform != null)
        {
            transform.parent = poolProvider.ParentTransform;
        }

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public virtual void OnPulledFromPool()
    {
        timeAlive = 0f;
    }
}
