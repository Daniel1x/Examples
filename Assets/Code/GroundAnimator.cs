using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GroundAnimator : MonoBehaviour
{
    public static event UnityAction<Vector3> OnCircleStarted = null;
    public static event UnityAction<Vector3> OnGridPointUpdate = null;

    [Header("Material Properties")]
    [SerializeField] private string distanceProperty = "_Distance";
    [SerializeField] private string thicknessProperty = "_Thickness";
    [SerializeField] private string startPointProperty = "_StartPoint";
    [SerializeField] private string gridPointProperty = "_GridStartPoint";

    [Header("Settings")]
    [SerializeField] private Renderer rendererToModify = null;
    [SerializeField] private Terrain terrainToModify = null;
    [SerializeField] private float thickness = 3f;
    [SerializeField, Min(1f)] private float maxDistance = 100f;
    [SerializeField, Min(0.01f)] private float movementSpeed = 1f;

    private float distance = 0f;
    private Vector3 startPoint = Vector3.zero;
    private Vector3 gridPoint = Vector3.zero;

    private MaterialPropertyBlock materialPropertyBlock = null;
    private MaterialPropertyBlock terrainPropertyBlock = null;
    private Coroutine animationCoroutine = null;

    private void Awake()
    {
        if (rendererToModify == null)
        {
            rendererToModify = GetComponent<Renderer>();
        }

        if (terrainToModify == null)
        {
            terrainToModify = GetComponent<Terrain>();
        }

        materialPropertyBlock = new();
        terrainPropertyBlock = new();

        OnCircleStarted += resetAnimation;
        OnGridPointUpdate += onGridPointUpdate;
    }

    private void OnDestroy()
    {
        OnCircleStarted -= resetAnimation;
        OnGridPointUpdate -= onGridPointUpdate;
    }

    private void onGridPointUpdate(Vector3 _point)
    {
        gridPoint = _point;
        updateProperties();
    }

    private void updateProperties()
    {
        updateTerrain();
        updateRenderer();
    }

    private void updateTerrain()
    {
        if (terrainToModify == null)
        {
            return;
        }

        terrainToModify.GetSplatMaterialPropertyBlock(terrainPropertyBlock);

        terrainPropertyBlock.SetFloat(distanceProperty, distance);
        terrainPropertyBlock.SetFloat(thicknessProperty, thickness);
        terrainPropertyBlock.SetVector(startPointProperty, startPoint);
        terrainPropertyBlock.SetVector(gridPointProperty, gridPoint);

        terrainToModify.SetSplatMaterialPropertyBlock(terrainPropertyBlock);
    }

    private void updateRenderer()
    {
        if (rendererToModify == null)
        {
            return;
        }

        rendererToModify.GetPropertyBlock(materialPropertyBlock);

        materialPropertyBlock.SetFloat(distanceProperty, distance);
        materialPropertyBlock.SetFloat(thicknessProperty, thickness);
        materialPropertyBlock.SetVector(startPointProperty, startPoint);
        materialPropertyBlock.SetVector(gridPointProperty, gridPoint);

        rendererToModify.SetPropertyBlock(materialPropertyBlock);
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

    public static void UpdateGridStartPoint(Vector3 _gridPoint)
    {
        OnGridPointUpdate?.Invoke(_gridPoint);
    }
}
