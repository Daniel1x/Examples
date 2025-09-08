using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Grenade : ObjectSpawnedByPool, IProjectileBehaviour
{
    private const int MAX_COLLIDERS = 50;
    private const string EMISSIVE_COLOR_PROPERTY = "_Emissive";

    private static readonly int emissiveColorID = Shader.PropertyToID(EMISSIVE_COLOR_PROPERTY);

    [Header("Grenade Settings")]
    [SerializeField] private float flightDuration = 2f;
    [SerializeField] private float explosionDelay = 3f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float xAngleRotation = 0f;

    [Header("Damage Settings")]
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float damageAmount = 50f;
    [SerializeField] private AnimationCurve damageDecreasePerDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("Explosion Settings")]
    [SerializeField] private VFXManager.VFXType explosionVFX = VFXManager.VFXType.Explosion;
    [SerializeField] private AudioClip explosionSFX = null;
    [SerializeField] private float explosionSFXVolume = 1f;

    [Header("Cooldown Visualization Settings")]
    [SerializeField] private Renderer emissiveRenderers = null;
    [SerializeField, ColorUsage(false, true)] private Color emissiveColor = Color.red;
    [SerializeField, ReadOnlyProperty] private AnimationCurve emissiveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField, Range(1, 100)] private int oscilationCount = 5;
    [SerializeField, ReadOnlyProperty] private AnimationCurve oscilationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve amplitudeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool hasExploded = false;
    private float explosionTimer = 0f;
    private Rigidbody rb;
    private Collider[] affectedColliders = new Collider[MAX_COLLIDERS];
    private MaterialPropertyBlock propertyBlock = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void Update()
    {
        base.Update();

        if (hasExploded)
        {
            return;
        }

        explosionTimer += Time.deltaTime;

        if (explosionTimer >= explosionDelay)
        {
            explode();
        }
        else
        {
            updateEmissiveAnimation();
        }
    }

    public override void OnPulledFromPool()
    {
        base.OnPulledFromPool();
        resetProjectileState();
    }

    public override void ReturnToPool()
    {
        base.ReturnToPool();
        resetProjectileState();
    }

    private void resetProjectileState()
    {
        hasExploded = false;
        explosionTimer = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        updateEmissiveAnimation();
    }

    public void Initialize(Vector3 _start, Vector3 _target)
    {
        startPosition = _start;
        targetPosition = _target;
        transform.position = startPosition;
        transform.LookAt(targetPosition);

        launch();
    }

    private void launch()
    {
        if (rb == null)
        {
            return;
        }

        float _t = Mathf.Max(0.001f, flightDuration);
        Vector3 _displacement = targetPosition - startPosition;

        if (_displacement.sqrMagnitude < 0.001f)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 _velocity;

        if (rb.useGravity)
        {
            // v0 = (Δp - 0.5 g t^2) / t
            _velocity = (_displacement - 0.5f * Physics.gravity * _t * _t) / _t;
        }
        else
        {
            _velocity = _displacement / _t;
        }

        rb.linearVelocity = _velocity;

        if (_velocity.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(_velocity.normalized, Vector3.up);
        }

        if (Mathf.Abs(xAngleRotation) > 0.01f)
        {
            rb.angularVelocity = transform.right * xAngleRotation * Mathf.Deg2Rad;
        }
    }

    private void explode()
    {
        hasExploded = true;

        // Show explosion VFX
        if (VFXManager.Instance != null)
        {
            VFXManager.Instance.SpawnVFX(explosionVFX, transform.position, Quaternion.identity);
        }

        // Play explosion SFX
        if (explosionSFX != null)
        {
            AudioSource.PlayClipAtPoint(explosionSFX, transform.position, explosionSFXVolume);
        }

        // Detect affected colliders
        int _numColliders = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, affectedColliders, damageableLayers);

        if (_numColliders > 0)
        {
            for (int i = 0; i < _numColliders; i++)
            {
                Collider _collider = affectedColliders[i];

                if (_collider == null)
                {
                    continue;
                }

                IDamageable _damageable = _collider.GetComponent<IDamageable>();

                if (_damageable == null)
                {
                    continue;
                }

                Vector3 _contactPoint = _collider.ClosestPoint(transform.position);
                float _finalDamage = getAdjustedDamage(_contactPoint);
                _damageable.CanReceiveDamage(_finalDamage);
            }
        }

        // Return grenade to pool
        ReturnToPool();
    }

    private float getAdjustedDamage(Vector3 _targetPosition)
    {
        float _distance = Vector3.Distance(transform.position, _targetPosition);

        if (_distance > explosionRadius)
        {
            return 0f;
        }

        return damageAmount * damageDecreasePerDistance.Evaluate(_distance / explosionRadius);
    }

    private void updateEmissiveAnimation()
    {
        if (emissiveRenderers == null)
        {
            return;
        }

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        emissiveRenderers.GetPropertyBlock(propertyBlock);

        float _normalizedTime = Mathf.Clamp01(explosionTimer / explosionDelay);
        float _lerpProgress = emissiveCurve.Evaluate(_normalizedTime);
        Color _finalColor = Color.Lerp(Color.clear, emissiveColor, _lerpProgress);

        propertyBlock.SetColor(emissiveColorID, _finalColor);
        emissiveRenderers.SetPropertyBlock(propertyBlock);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    private void OnValidate()
    {
        oscilationCurve = CreateChirpOscilations(oscilationCount);

        //Recreate emissive curve based on oscilation curve and amplitude curve
        Keyframe[] _keys = oscilationCurve.keys;

        for (int i = 0; i < _keys.Length; i++)
        {
            float _amplitude = amplitudeCurve.Evaluate(_keys[i].time);
            _keys[i].value *= _amplitude;
        }

        emissiveCurve = new AnimationCurve(_keys);
    }

    public static AnimationCurve CreateChirpOscilations(int _numberOfOscillations, float _maxTime = 1f, float _maxValue = 1f)
    {
        _numberOfOscillations *= 2;

        AnimationCurve _curve = new AnimationCurve();
        _curve.AddKey(0f, 0f);

        for (int i = 1; i <= _numberOfOscillations; i++)
        {
            float _time = Mathf.Pow((float)i / _numberOfOscillations, 2f) * _maxTime;

            // Invert time to start from the end
            _time = 1f - _time;

            float _value = (i % 2 == 0 ? 0f : _maxValue);
            _curve.AddKey(_time, _value);
        }

        return _curve;
    }
}
