using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ApplicationManager : MonoBehaviour
{
    public static event UnityAction<ScreenResolution> OnScreenResolutionChanged = null;

    private static ApplicationManager instance = null;

    [SerializeField] private InitializationBehaviour[] initializationBehaviours = new InitializationBehaviour[] { };
    [SerializeField] private InitializationObject[] initializationObjects = new InitializationObject[] { };

    private ScreenResolutionChecker screenResolutionChecker = new ScreenResolutionChecker();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        screenResolutionChecker.CheckForResolutionChange();
    }

    private IEnumerator Start()
    {
        yield return initializationBehaviours.HandleInitialization();
        yield return initializationObjects.HandleInitialization();
    }

    private void Update()
    {
        if (screenResolutionChecker.CheckForResolutionChange())
        {
            OnScreenResolutionChanged?.Invoke(screenResolutionChecker.Resolution);
        }
    }
}
