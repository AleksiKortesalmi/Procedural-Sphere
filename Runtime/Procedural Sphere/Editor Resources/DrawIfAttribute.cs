// Written with help from: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/

#if UNITY_EDITOR

using UnityEngine;
using System;


/// <summary>
/// Draws the field/property ONLY if the condition is met.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfAttribute : PropertyAttribute
{
    #region Properties

    public string PropertyName { get; private set; }
    public object ComparedValue { get; private set; }
    public DisablingType DisablingTypeValue { get; private set; }

    #endregion

    public enum DisablingType
    {
        ReadOnly = 2,
        DontDraw = 3
    }


    /// <summary>
    /// Draw property if Enum property field is equal to enumIndex.
    /// </summary>
    public DrawIfAttribute(string enumPropertyName, int enumIndex, DisablingType disablingTypeValue = DisablingType.DontDraw)
    {
        this.PropertyName = enumPropertyName;
        this.ComparedValue = enumIndex;
        DisablingTypeValue = disablingTypeValue;
    }

    /// <summary>
    /// Draw property if boolean is true.
    /// </summary>
    public DrawIfAttribute(string booleanPropertyName, DisablingType disablingTypeValue = DisablingType.DontDraw)
    {
        this.PropertyName = booleanPropertyName;
        DisablingTypeValue = disablingTypeValue;
    }
}

#endif