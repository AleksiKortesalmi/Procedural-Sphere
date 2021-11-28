#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralSphere))]
public class ProceduralSphereEditor : Editor
{
    ProceduralSphere ps;

    public override void OnInspectorGUI()
    {
        ps = (ProceduralSphere)target;

        if (!ps.enableGPUInstancing)
        {
            DrawEditorWithoutTitle(this);

            // Draw editors for the scriptableObjects
            DrawSettingsEditor(ps.shapeSettings);
            DrawSettingsEditor(ps.colorSettings);

            // Generate sphere again if something was changed
            if (GUI.changed)
            {
                ps.OnSettingsUpdated();
            }
        }

        EditorGUILayout.LabelField("GPU Instancing", EditorStyles.boldLabel);

        ps.enableGPUInstancing = EditorGUILayout.Toggle(new GUIContent("Enable GPU Instancing"), ps.enableGPUInstancing);

        if (ps.enableGPUInstancing)
        {
            ps.copySphere = EditorGUILayout.ObjectField(new GUIContent("Copy Sphere", "GenerateSphere object to copy the mesh from."), ps.copySphere, typeof(ProceduralSphere), true) as ProceduralSphere;
        }
    }

    // Create an editor for a class with a label that has a " Settings" suffix
    static void DrawSettingsEditor(Object serializedObject)
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            EditorGUILayout.LabelField(serializedObject.name + " Settings", EditorStyles.boldLabel);

            Editor editor = CreateEditor(serializedObject);

            DrawEditorWithoutTitle(editor);
        }
    }

    static void DrawEditorWithoutTitle(Editor editor)
    {
        editor.serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        DrawPropertiesExcluding(editor.serializedObject, "m_Script");
        if (EditorGUI.EndChangeCheck())
            editor.serializedObject.ApplyModifiedProperties();
    }
}

#endif