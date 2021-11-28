// Written with help from: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(DrawIfAttribute))]
public class DrawIfPropertyDrawer : PropertyDrawer
{
    #region Fields

    DrawIfAttribute drawIf;

    SerializedProperty comparedField;

    #endregion


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        drawIf = attribute as DrawIfAttribute;

        // Get property's path with the name given in the attribute
        string propertyPath = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawIf.PropertyName) : drawIf.PropertyName;
        comparedField = property.serializedObject.FindProperty(propertyPath);

        if (comparedField == null)
        {
            Debug.LogError("Field '" + label.text + "' DrawIf attribute error, cannot find property with name '" + propertyPath + "'");

            return;
        }

        // If the condition is met, simply draw the field.
        if (CheckCondition(comparedField))
        {
            EditorGUI.PropertyField(position, property, label, true);
        } //...check if the disabling type is read only. If it is, draw it disabled
        else if (drawIf.DisablingTypeValue == DrawIfAttribute.DisablingType.ReadOnly)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property);
            GUI.enabled = true;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        drawIf = attribute as DrawIfAttribute;

        // Get property's path with the name given in the attribute
        string propertyPath = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawIf.PropertyName) : drawIf.PropertyName;
        comparedField = property.serializedObject.FindProperty(propertyPath);

        if (comparedField == null)
        {
            Debug.LogError("Field '" + label.text + "' DrawIf attribute error, cannot find property with name '" + propertyPath + "'");

            return -EditorGUIUtility.standardVerticalSpacing; ;
        }

        // If the condition is met, simply draw the field.
        if (CheckCondition(comparedField) || drawIf.DisablingTypeValue == DrawIfAttribute.DisablingType.ReadOnly)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        // Undo the spacing added before and after the property
        return -EditorGUIUtility.standardVerticalSpacing;
    }

    private bool CheckCondition(SerializedProperty comparedField)
    {
        switch (comparedField.type)
        {
            case "Enum":
                return comparedField.enumValueIndex.Equals((int)drawIf.ComparedValue);
            case "bool":
                return comparedField.boolValue;
            default:
                Debug.LogError("Error: " + comparedField.type + " is not supported");
                return false;
        }
    }
}

#endif