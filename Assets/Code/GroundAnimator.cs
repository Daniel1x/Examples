using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GroundAnimator : MonoBehaviour
{
    public static event UnityAction<Vector3, Color> OnCircleStarted = null;

    [Header("Material Properties")]
    [SerializeField] private string distanceProperty = "_Distance";
    [SerializeField] private string thicknessProperty = "_Thickness";
    [SerializeField] private string startPointProperty = "_StartPoint";
    [SerializeField] private string circleColorProperty = "_LineColor";

    [Header("Settings")]
    [SerializeField] private Terrain terrainToModify = null;
    [SerializeField] private float thickness = 3f;
    [SerializeField, Min(1f)] private float maxDistance = 100f;
    [SerializeField, Min(0.01f)] private float movementSpeed = 1f;

    private float distance = 0f;
    private Vector3 startPoint = Vector3.zero;
    private Color circleColor = Color.white;

    private MaterialPropertyBlock terrainPropertyBlock = null;
    private Coroutine animationCoroutine = null;

    private void Awake()
    {
        if (terrainToModify == null)
        {
            terrainToModify = GetComponent<Terrain>();
        }

        terrainPropertyBlock = new();

        OnCircleStarted += resetAnimation;
    }

    private void OnDestroy()
    {
        OnCircleStarted -= resetAnimation;
    }

    private void OnEnable()
    {
        resetProperties();
    }

    private void updateTerrain()
    {
        if (terrainToModify == null)
        {
            return;
        }

        terrainToModify.GetSplatMaterialPropertyBlock(terrainPropertyBlock);
        applyPropertyBlock(terrainPropertyBlock);
        terrainToModify.SetSplatMaterialPropertyBlock(terrainPropertyBlock);
    }

    private void applyPropertyBlock(MaterialPropertyBlock _block)
    {
        _block.SetFloat(distanceProperty, distance);
        _block.SetFloat(thicknessProperty, thickness);
        _block.SetVector(startPointProperty, startPoint);
        _block.SetColor(circleColorProperty, circleColor);
    }

    private void resetProperties()
    {
        startPoint = Vector3.zero;
        distance = -thickness;

        updateTerrain();
    }

    private void resetAnimation(Vector3 _startPoint, Color _color)
    {
        stopAnimationCoroutine();

        circleColor = _color;
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
            updateTerrain();

            yield return null;
        }

        resetProperties();
    }

    public static void ShowCircle(Vector3 _startPoint, Color _color)
    {
        OnCircleStarted?.Invoke(_startPoint, _color);
    }
}
