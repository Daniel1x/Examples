using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshRenderer))]
public class GroundAnimator : MonoBehaviour
{
    public static event UnityAction<Vector3> OnCircleStarted = null;

    [Header("Material Properties")]
    [SerializeField] private string distanceProperty = "_Distance";
    [SerializeField] private string thicknessProperty = "_Thickness";
    [SerializeField] private string startPointProperty = "_StartPoint";

    [Header("Settings")]
    [SerializeField] private float thickness = 3f;
    [SerializeField, Min(1f)] private float maxDistance = 100f;
    [SerializeField, Min(0.01f)] private float movementSpeed = 1f;

    private float distance = 0f;
    private Vector3 startPoint = Vector3.zero;

    private MeshRenderer groundRenderer = null;
    private MaterialPropertyBlock materialPropertyBlock = null;
    private Coroutine animationCoroutine = null;

    private void Awake()
    {
        groundRenderer = GetComponent<MeshRenderer>();
        materialPropertyBlock = new();

        OnCircleStarted += resetAnimation;
    }

    private void OnDestroy()
    {
        OnCircleStarted -= resetAnimation;
    }

    private void updateProperties()
    {
        if (groundRenderer == null)
        {
            return;
        }

        MyLog.Log($"UpdateProperties :: D:{distance} :: T:{thickness} :: S:{startPoint}");

        groundRenderer.GetPropertyBlock(materialPropertyBlock);

        materialPropertyBlock.SetFloat(distanceProperty, distance);
        materialPropertyBlock.SetFloat(thicknessProperty, thickness);
        materialPropertyBlock.SetVector(startPointProperty, startPoint);

        groundRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void resetProperties()
    {
        startPoint = Vector3.zero;
        distance = -thickness;

        updateProperties();
    }

    private void resetAnimation(Vector3 _startPoint)
    {
        stopAnimationCoroutine();

        animationCoroutine = StartCoroutine(animateMovement(_startPoint));
    }

    private void stopAnimationCoroutine()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private IEnumerator animateMovement(Vector3 _startPoint)
    {
        resetProperties();
        startPoint = _startPoint;

        while (distance < maxDistance)
        {
            distance += Time.deltaTime * movementSpeed;
            updateProperties();

            yield return null;
        }

        resetProperties();
    }

    public static void ShowCircle(Vector3 _startPoint)
    {
        OnCircleStarted?.Invoke(_startPoint);
    }
}
