using UnityEngine;

/// <summary>
/// Pooled object variant that automatically returns itself to the pool after a configured lifetime.
/// Lifetime is provided by the owning <see cref="ObjectPoolBehaviour"/> during <see cref="Initialize"/>.
/// </summary>
/// <remarks>
/// Behavior:
/// - If lifetime &lt; 0 => infinite (never auto-destruct / auto-return).
/// - A per-instance timer (<see cref="timeAlive"/>) is incremented every frame (Update).
/// - When <see cref="timeAlive"/> >= lifetime the object is returned to the pool (deactivated).
/// - On each pull from the pool (<see cref="OnPulledFromPool"/>) the timer is reset to zero.
/// Override <see cref="Update"/> if you need more complex lifetime policies (e.g. conditional pausing).
/// </remarks>
public class ObjectSpawnedByPoolWithLifetime : ObjectSpawnedByPool
{
    /// <summary>Configured lifetime in seconds (negative => infinite).</summary>
    protected float lifetime = 0f;

    /// <summary>Accumulated alive time since last activation (seconds).</summary>
    protected float timeAlive = 0f;

    /// <summary>
    /// Unity update loop; increments internal timer and returns the object to the pool
    /// when the configured lifetime elapses (unless lifetime is infinite).
    /// </summary>
    protected virtual void Update()
    {
        if (lifetime < 0f)
        {
            return; // Infinite lifetime - no timing logic required.
        }

        timeAlive += Time.deltaTime;

        if (timeAlive >= lifetime)
        {
            ReturnToPool(); // Safe idempotent deactivation.
        }
    }

    /// <summary>
    /// Initializes this pooled object with lifetime pulled from the pool settings
    /// and resets internal timer.
    /// </summary>
    public override void Initialize(ObjectPoolBehaviour _poolBehaviour)
    {
        base.Initialize(_poolBehaviour);

        lifetime = _poolBehaviour.ObjectLifetime;
        timeAlive = 0f;
    }

    /// <summary>
    /// Called when object is pulled (reused) from the pool: resets the alive timer.
    /// </summary>
    public override void OnPulledFromPool()
    {
        timeAlive = 0f;
    }
}
