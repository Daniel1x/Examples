using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage = null;
    [SerializeField] private Image backgroundImage = null;

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float fillAmount = 1f;
    [SerializeField] private Color fillColor = new Color(0.6f, 0f, 0f, 1f);
    [SerializeField] private Color backgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);

    public float FillAmount
    {
        get => fillAmount;
        set
        {
            fillAmount = value;
            UpdateFill();
        }
    }

    public Color FillColor
    {
        get => fillColor;
        set
        {
            fillColor = value;
            UpdateColor();
        }
    }

    public Color BackgroundColor
    {
        get => backgroundColor;
        set
        {
            backgroundColor = value;
            UpdateBackgroundColor();
        }
    }

    private void OnEnable()
    {
        UpdateHealthBar();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateHealthBar();
    }
#endif

    public void UpdateHealthBar()
    {
        UpdateFill();
        UpdateColor();
        UpdateBackgroundColor();
    }

    public void UpdateFill()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = fillAmount;
        }
    }

    public void UpdateColor()
    {
        if (fillImage != null)
        {
            fillImage.color = fillColor;
        }
    }

    public void UpdateBackgroundColor()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }
}
