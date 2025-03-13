using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light))]
public class SkyboxAnimator : MonoBehaviour
{
    [SerializeField] private string rotationPropertyName = "_Rotation";
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Vector2 cookieAnimationSpeed = Vector2.zero;

    protected UniversalAdditionalLightData lightData = null;

    private float startX = 0f;
    private float startY = 0f;

    private void Awake()
    {
        lightData = GetComponent<UniversalAdditionalLightData>();

        Vector3 _rotationAngles = transform.eulerAngles;

        startX = _rotationAngles.x;
        startY = _rotationAngles.y;
    }

    private void OnDestroy()
    {
        setGlobalSkyboxRotation(0f);
    }

    private void Update()
    {
        float _rotation = Time.unscaledTime * rotationSpeed;

        setGlobalSkyboxRotation(-_rotation);
        transform.rotation = Quaternion.Euler(startX, startY - _rotation, 0f);

        if (lightData != null)
        {
            lightData.lightCookieOffset += cookieAnimationSpeed * Time.unscaledDeltaTime;
        }
    }

    private void setGlobalSkyboxRotation(float _rotation)
    {
        if (RenderSettings.skybox is Material _globalSkyboxMaterial
            && _globalSkyboxMaterial.HasProperty(rotationPropertyName))
        {
            _globalSkyboxMaterial.SetFloat(rotationPropertyName, Time.unscaledTime * rotationSpeed);
        }
    }
}
