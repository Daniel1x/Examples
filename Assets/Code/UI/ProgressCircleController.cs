using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ProgressCircleController : MonoBehaviour
{
    [SerializeField] private Image image = null;
    [SerializeField] private Gradient colorGradient = new Gradient();
    [SerializeField, Min(4)] private int segments = 20;
    [SerializeField, Range(0f, 1f)] private float currentFill = 1f;

    private void Reset()
    {
        image = GetComponent<Image>();
    }

    private void OnValidate()
    {
        UpdateFill();
    }

    public float Fill
    {
        get => currentFill;
        set
        {
            currentFill = Mathf.Clamp01(value);
            UpdateFill();
        }
    }

    public void UpdateFill()
    {
        if (image == null)
        {
            return;
        }

        if (currentFill <= 0f)
        {
            image.fillAmount = 0f;
            UpdateGradient();
            return;
        }

        if (currentFill >= 1f)
        {
            image.fillAmount = 1f;
            UpdateGradient();
            return;
        }

        float _segmentFill = 1f / segments;
        int _numberOfFullSegments = Mathf.FloorToInt(currentFill / _segmentFill);

        image.fillAmount = _numberOfFullSegments * _segmentFill;
        UpdateGradient();
    }

    public void UpdateGradient()
    {
        if (image != null)
        {
            image.color = colorGradient.Evaluate(currentFill);
        }
    }
}