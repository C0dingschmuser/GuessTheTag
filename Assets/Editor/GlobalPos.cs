#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public static class DebugMenu
{
    [MenuItem("Debug/Print Global Position")]
    public static void PrintGlobalPosition()
    {
        if (Selection.activeGameObject != null)
        {
            Debug.Log(Selection.activeGameObject.name + " is at " + posToString(Selection.activeGameObject));
        }
    }

    private static string posToString(GameObject obj)
    {
        string pos = "(" + obj.transform.position.x.ToString("F3") + ", " +
            obj.transform.position.y.ToString("F3") + ", " +
            obj.transform.position.z.ToString("F3") + ")";

        return pos;
    }
}

#endif