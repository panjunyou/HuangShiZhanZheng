using UnityEngine;
using System.Collections;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using System.IO;
using System.Text;
using ILitJson;
using Lockstep.Math;
using System.Text.RegularExpressions;

public class MyEditor : Editor
{
#if UNITY_EDITOR
    //将所有游戏场景导出为JSON格式
    [MenuItem("91make.top/Export SaeneInfo to <SceneMame>.JSON")]
    static void ExportJSON()
    {
        string sceneName = EditorSceneManager.GetActiveScene().name;
        string scenePath = EditorSceneManager.GetActiveScene().path;

        string filepath = Application.dataPath + @"/_VIP/Resources_moved/MySceneInfo/" + sceneName + ".json";

        if (!File.Exists(filepath))
        {
            File.Delete(filepath);
        }

        //准备号字符串和JSON数据写入器
        StringBuilder sb = new StringBuilder();
        JsonWriter writer = new JsonWriter(sb);
        writer.PrettyPrint = true;
        writer.WriteObjectStart();
        writer.WritePropertyName(sceneName);
        writer.WriteObjectStart();
        GameObject[] objs = Object.FindObjectsOfType<GameObject>();//获取场景所有obj

        foreach  (GameObject obj in objs)
        {
            writer.WritePropertyName(obj.transform.FullPath());
            writer.WriteObjectStart();
            {
                {
                    writer.WritePropertyName("position");
                    writer.WriteObjectStart();
                    LVector3 lpos = obj.transform.position.ToLVevtor3_My();
                    writer.WritePropertyName("x");
                    writer.Write(lpos._x);
                    writer.WritePropertyName("y");
                    writer.Write(lpos._y);
                    writer.WritePropertyName("z");
                    writer.Write(lpos._z);
                    writer.WriteObjectEnd();

                    LQuaternion lrot = obj.transform.rotation.ToLQuaternion();
                    writer.WritePropertyName("rotation");
                    writer.WriteObjectStart();
                    writer.WritePropertyName("x");
                    writer.Write(lrot.x._val);
                    writer.WritePropertyName("y");
                    writer.Write(lrot.y._val);
                    writer.WritePropertyName("z");
                    writer.Write(lrot.z._val);
                    writer.WritePropertyName("w");
                    writer.Write(lrot.w._val);
                    writer.WriteObjectEnd();
                }
            }
            writer.WriteObjectEnd();
        }
        writer.WriteObjectEnd();
        writer.WriteObjectEnd();

        File.WriteAllText(filepath, Regex.Unescape(sb.ToString()), Encoding.UTF8);

        Debug.Log($"MySceneExporter:Write scene [{sceneName}] finished");

        AssetDatabase.Refresh();
    }
#endif
}
