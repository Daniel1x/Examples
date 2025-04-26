using DL.Editor;
using UnityEditor;
using UnityEditor.UI;
using Mode = UnityEngine.UI.Navigation.Mode;

[CanEditMultipleObjects]
[CustomEditor(typeof(CustomButton), true)]
public class CustomButtonEditor : ButtonEditor
{
    private CustomButton button = null;

    private SerializedProperty invokeOnAnyItemSelected = null;
    private SerializedProperty allowNavigationToOther = null;
    private SerializedProperty allowNavigationToThis = null;
    private SerializedProperty forceSelectEventOnEnable = null;
    private SerializedProperty forceDeselectEventOnDisable = null;
    private SerializedProperty forceSelectOnPointerEnter = null;
    private SerializedProperty forceClickOnPointerDown = null;
    private SerializedProperty refreshColorTransitionAfterFrame = null;
    private SerializedProperty explicitNavigator = null;
    private SerializedProperty fadeStyle = null;
    private SerializedProperty graphicsToFade = null;
    private SerializedProperty selectionEventController = null;
    private SerializedProperty graphicsTransitionController = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        button = (CustomButton)target;

        invokeOnAnyItemSelected = serializedObject.FindProperty("invokeOnAnyItemSelected");
        allowNavigationToOther = serializedObject.FindProperty("allowNavigationToOtherParent");
        allowNavigationToThis = serializedObject.FindProperty("allowNavitationToThisObject");
        forceSelectEventOnEnable = serializedObject.FindProperty("forceSelectEventOnEnable");
        forceDeselectEventOnDisable = serializedObject.FindProperty("forceDeselectEventOnDisable");
        forceSelectOnPointerEnter = serializedObject.FindProperty("forceSelectOnPointerEnter");
        forceClickOnPointerDown = serializedObject.FindProperty("forceClickOnPointerDown");
        refreshColorTransitionAfterFrame = serializedObject.FindProperty("refreshColorTransitionAfterFrame");
        explicitNavigator = serializedObject.FindProperty("explicitNavigator");
        fadeStyle = serializedObject.FindProperty("fadeStyle");
        graphicsToFade = serializedObject.FindProperty("graphicsToFade");
        selectionEventController = serializedObject.FindProperty("selectionEventController");
        graphicsTransitionController = serializedObject.FindProperty("graphicsTransitionController");
    }

    public override void OnInspectorGUI()
    {
        EditorDrawing.DrawScriptProperty(serializedObject);

        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Button Extensions:", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(invokeOnAnyItemSelected);
        EditorGUILayout.PropertyField(allowNavigationToOther);
        EditorGUILayout.PropertyField(allowNavigationToThis);
        EditorGUILayout.PropertyField(forceSelectEventOnEnable);
        EditorGUILayout.PropertyField(forceDeselectEventOnDisable);
        EditorGUILayout.PropertyField(forceSelectOnPointerEnter);
        EditorGUILayout.PropertyField(forceClickOnPointerDown);
        EditorGUILayout.PropertyField(refreshColorTransitionAfterFrame);

        if (button.navigation.mode == Mode.Explicit)
        {
            EditorGUILayout.PropertyField(explicitNavigator);
        }

        drawFadeImageSettings();
        drawSelectionEventSettings();
        EditorGUILayout.PropertyField(graphicsTransitionController);
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }

    private void drawFadeImageSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Graphics Fading:", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(fadeStyle);

        if (fadeStyle.intValue != (int)CustomButton.GraphicsFadeStyles.None)
        {
            EditorGUILayout.PropertyField(graphicsToFade);
        }
    }

    private void drawSelectionEventSettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Selection Events:", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(selectionEventController);
    }
}
