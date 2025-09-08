using UnityEngine;

public class ProjectilesManager : MonoBehaviour
{
    public static ProjectilesManager Instance { get; private set; } = null;

    [Header("Pools (assign in inspector)")]
    [SerializeField] private GrenadePoolBehaviour grenadePool = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public GameObject SpawnGrenade(Vector3 _startPosition, Vector3 _targetPosition)
    {
        if (grenadePool == null)
        {
            Debug.LogWarning("[ProjectilesManager] Grenade pool is not assigned.");
            return null;
        }

        ObjectSpawnedByPool _instance = grenadePool.GetObjectFromPool(false);

        if (_instance == null)
        {
            Debug.LogWarning("[ProjectilesManager] No available grenades in the pool.");
            return null;
        }

        _instance.transform.position = _startPosition;
        _instance.transform.rotation = Quaternion.LookRotation((_targetPosition - _startPosition).normalized);
        _instance.transform.SetParent(null);
        _instance.gameObject.SetActive(true);

        if (_instance is IProjectileBehaviour _projectile)
        {
            _projectile.Initialize(_startPosition, _targetPosition);
        }

        return _instance.gameObject;
    }
}
