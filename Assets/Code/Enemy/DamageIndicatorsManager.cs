using UnityEngine;

public class DamageIndicatorsManager : MonoBehaviour
{
    public static DamageIndicatorsManager Instance { get; private set; } = null;

    [SerializeField] private DamageIndicatorPoolBehaviour damageIndicatorPool = null;
    [SerializeField] private Vector3 spawnOffset = Vector3.up * 3f;
    [SerializeField, Range(0f, 1f)] private float randomSphereRadius = 0.5f;

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

    public void ShowDamageIndicator(Vector3 _position, float _amount, DamageIndicator.DamageType _type)
    {
        if (damageIndicatorPool == null)
        {
            return;
        }

        int _roundedAmount = Mathf.RoundToInt(_amount);

        if (_roundedAmount == 0)
        {
            return;
        }

        DamageIndicator _indicator = damageIndicatorPool.GetObjectFromPool(false);

        if (_indicator == null)
        {
            return;
        }

        _indicator.transform.position = _position + spawnOffset + (Random.insideUnitSphere * randomSphereRadius);
        _indicator.Initialize(_roundedAmount, _type);
        _indicator.gameObject.SetActive(true);
    }
}
