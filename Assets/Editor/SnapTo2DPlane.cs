using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class SnapTo2DPlane
{
    static SnapTo2DPlane()
    {
    EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
    EditorApplication.delayCall += SnapSelectedObject;
    }

    private static void SnapSelectedObject()
    {
    GameObject obj = Selection.activeGameObject;

    if (obj == null)
    {
    return;
    }

    Vector3 position = obj.transform.position;
    position.z = 0f;

    obj.transform.position = position;

    EditorUtility.SetDirty(obj);
    }
}