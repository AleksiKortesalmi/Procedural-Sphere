#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public static class ProceduralSphereCreator
{
    [MenuItem("GameObject/Create Other/Procedural Sphere")]
    static void CreateProceduralSphere()
    {
        GameObject prefab = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath("Packages/com.aleksikortesalmi.proceduralsphere/Runtime/Procedural Sphere/Prefab/Procedural Sphere.prefab", typeof(Object))) as GameObject;
        
        // Create folder to store settings for each prefabs settings
        if(!AssetDatabase.IsValidFolder("Assets/Procedural Sphere Settings"))
            AssetDatabase.CreateFolder("Assets", "Procedural Sphere Settings");

        // Find previously created prefab settings and add an index as a suffix to the asset name
        string suffix = AssetDatabase.FindAssets("Prefab Color").Length.ToString();
        string shapePath = "Assets/Procedural Sphere Settings/Prefab Shape " + suffix + ".asset";
        string colorPath = "Assets/Procedural Sphere Settings/Prefab Color " + suffix + ".asset";

        AssetDatabase.CopyAsset("Packages/com.aleksikortesalmi.proceduralsphere/Runtime/Procedural Sphere/Prefab/Prefab Shape.asset", shapePath);
        AssetDatabase.CopyAsset("Packages/com.aleksikortesalmi.proceduralsphere/Runtime/Procedural Sphere/Prefab/Prefab Color.asset", colorPath);

        // Assign the copied settings to the instantiated prefab
        ProceduralSphere ps = prefab.GetComponent<ProceduralSphere>();
        ps.shapeSettings = AssetDatabase.LoadAssetAtPath(shapePath, typeof(Object)) as ShapeSettings;
        ps.colorSettings = AssetDatabase.LoadAssetAtPath(colorPath, typeof(Object)) as ColorSettings;

        ps.OnSettingsUpdated();
    }
}

#endif