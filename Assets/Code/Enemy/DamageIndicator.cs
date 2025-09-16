using TMPro;
using UnityEngine;

public class DamageIndicator : ObjectSpawnedByPool
{
    [System.Serializable]
    public struct FontSettings
    {
        public Color Color;
        public float FontSize;
    }

    public enum DamageType
    {
        Normal = 0,
        Critical = 1,
        Heal = 2,
    }

    [SerializeField] private TMP_Text damageText = null;
    [SerializeField] private FontSettings normalDamage = new FontSettings { Color = Color.white, FontSize = 36f };
    [SerializeField] private FontSettings criticalDamage = new FontSettings { Color = Color.red, FontSize = 54f };
    [SerializeField] private FontSettings healAmount = new FontSettings { Color = Color.green, FontSize = 36f };
    [SerializeField, Min(0f)] private float floatUpSpeed = 1f;
    [SerializeField] private AnimationCurve alphaCurveForLifetime = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    public void Initialize(int _amount, DamageType _type)
    {
        if (damageText == null)
        {
            return;
        }

        FontSettings _settings = getSettings(_type);
        damageText.text = _amount.ToStringWithSign();
        damageText.color = _settings.Color;
        damageText.fontSize = _settings.FontSize;
    }

    protected override void Update()
    {
        transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;

        if (CameraManager.Instance != null
            && CameraManager.Instance.VirtualCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - CameraManager.Instance.VirtualCamera.transform.position);
        }

        if (lifetime > 0f && damageText != null)
        {
            float _lifePercent = timeAlive / lifetime;
            damageText.color = damageText.color.WithAlpha(alphaCurveForLifetime.Evaluate(_lifePercent));
        }

        base.Update();
    }

    private FontSettings getSettings(DamageType _type)
    {
        return _type switch
        {
            DamageType.Normal => normalDamage,
            DamageType.Critical => criticalDamage,
            DamageType.Heal => healAmount,
            _ => normalDamage,
        };
    }
}
