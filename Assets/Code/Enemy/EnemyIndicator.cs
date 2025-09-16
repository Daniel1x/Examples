using UnityEngine;
using UnityEngine.UI;

public class EnemyIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitStats unitStats = null;
    [SerializeField] private Image healthBar = null;
    [SerializeField] private Image staminaBar = null;
    [SerializeField] private Image manaBar = null;
    [SerializeField] private bool lookAtCamera = true;

    private void Awake()
    {
        if (unitStats == null)
        {
            unitStats = this.GetComponentHereOrInParent<UnitStats>();
        }
    }

    private void OnEnable()
    {
        updateStats();

        if (unitStats != null)
        {
            unitStats.OnDamaged += updateHP;
            unitStats.OnHealed += updateHP;
            unitStats.OnStatsChanged += updateStats;
        }
    }

    private void OnDisable()
    {
        if (unitStats != null)
        {
            unitStats.OnDamaged -= updateHP;
            unitStats.OnHealed -= updateHP;
            unitStats.OnStatsChanged -= updateStats;
        }
    }

    private void LateUpdate()
    {
        if (CameraManager.Instance == null
            || CameraManager.Instance.VirtualCamera == null)
        {
            return;
        }

        Vector3 _camForward = CameraManager.Instance.VirtualCamera.transform.forward;

        if (lookAtCamera)
        {
            _camForward.y = 0f;
            transform.rotation = Quaternion.LookRotation(_camForward);
        }
        else
        {
            _camForward.y = 0f;
            transform.rotation = Quaternion.LookRotation(Vector3.down, _camForward);
        }
    }

    private void updateStats()
    {
        updateHP();
        updateStamina();
        updateMana();
    }

    private void updateHP()
    {
        if (unitStats != null && healthBar != null)
        {
            healthBar.fillAmount = unitStats.Health.Percent01;
        }
    }

    private void updateStamina()
    {
        if (unitStats != null && staminaBar != null)
        {
            staminaBar.fillAmount = unitStats.Stamina.Percent01;
        }
    }

    private void updateMana()
    {
        if (unitStats != null && manaBar != null)
        {
            manaBar.fillAmount = unitStats.Mana.Percent01;
        }
    }
}
