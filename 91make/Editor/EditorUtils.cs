using UnityEngine;
using UnityEditor;

public static class EditorUtils 
{
    [MenuItem("GameObject/RayGame/复制对象路径", priority = 1)]
    public static void CopyObjectPath()
    {
        var tr = Selection.activeTransform;
        string s = string.Empty;
        while (tr!=null)
        {
            if (s==string.Empty)
            {
                s = tr.gameObject.name;

            }
            else
            {
                s = tr.gameObject.name + "/" + s;
                tr = tr.parent;
            }
            Debug.Log(s);

            GUIUtility.systemCopyBuffer = s;
        }
    }
}