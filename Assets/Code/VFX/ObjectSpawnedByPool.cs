using UnityEngine;

/// <summary>
/// Base component attached to pooled objects.
/// Provides a minimal contract for initialization, returning to the pool,
/// and a hook invoked when the object is pulled (reused).
/// </summary>
/// <remarks>
/// Default behavior:
/// - When the GameObject is disabled (OnDisable), it calls <see cref="ReturnToPool"/>.
/// - <see cref="ReturnToPool"/> simply deactivates the GameObject (idempotent).
/// Override <see cref="OnPulledFromPool"/> for per-use reset logic (e.g. resetting timers, effects).
/// </remarks>
public class ObjectSpawnedByPool : MonoBehaviour
{
    /// <summary>
    /// Reference to the pool that owns this instance.
    /// Assigned via <see cref="Initialize(ObjectPoolBehaviour)"/>.
    /// </summary>
    protected ObjectPoolBehaviour poolBehaviour = null;

    /// <summary>
    /// Unity callback invoked when the object becomes disabled.
    /// By default it returns the object to the pool (idempotent).
    /// </summary>
    /// <remarks>
    /// Deactivation flow: external code sets active = false -> OnDisable -> ReturnToPool() -> sets active (already false) -> no recursion.
    /// </remarks>
    protected virtual void OnDisable()
    {
        ReturnToPool();
    }

    /// <summary>
    /// Initializes this pooled object with a reference back to its pool.
    /// Must be called by the pool right after instantiation.
    /// </summary>
    /// <param name="_poolBehaviour">Owning pool.</param>
    public virtual void Initialize(ObjectPoolBehaviour _poolBehaviour)
    {
        poolBehaviour = _poolBehaviour;
    }

    /// <summary>
    /// Returns the object to its associated pool, resetting its state as necessary.
    /// </summary>
    /// <remarks>This method deactivates the object and, if a pool behavior is defined, reassigns the object's
    /// transform to the parent transform specified by the pool behavior. This ensures the object is  properly managed
    /// within the pooling system.</remarks>
    public virtual void ReturnToPool()
    {
        gameObject.SetActive(false);

        if (poolBehaviour != null && poolBehaviour.ParentTransform != null)
        {
            poolBehaviour.transform.SetParent(poolBehaviour.ParentTransform);
        }
    }

    /// <summary>
    /// Called by the pool right after the object is fetched for reuse.
    /// Override to reset internal state (animations, particle systems, timers, etc.).
    /// </summary>
    public virtual void OnPulledFromPool()
    {
        // Override in derived classes if needed.
    }
}
