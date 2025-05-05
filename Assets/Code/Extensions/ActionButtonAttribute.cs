using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ActionButtonAttribute : PropertyAttribute
{
    public ActionButtonAttribute()
    {
    }
}
