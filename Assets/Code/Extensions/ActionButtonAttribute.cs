using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ActionButtonAttribute : PropertyAttribute
{
    public string NameOverride { get; private set; } = string.Empty;

    public ActionButtonAttribute(string _nameOverride = null)
    {
        NameOverride = _nameOverride;
    }
}
