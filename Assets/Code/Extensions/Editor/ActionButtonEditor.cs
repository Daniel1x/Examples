using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ActionButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MonoBehaviour _targetObject = (MonoBehaviour)target;

        if (_targetObject == null)
        {
            return;
        }

        MethodInfo[] _methods = _targetObject.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (MethodInfo _method in _methods)
        {
            object[] attributes = _method.GetCustomAttributes(typeof(ActionButtonAttribute), true);

            if (attributes.Length == 0)
            {
                continue;
            }

            if (GUILayout.Button(_method.Name))
            {
                _method.Invoke(_targetObject, null);
            }
        }
    }
}
