using UnityEngine;
using UnityEditor;
using System.IO;

public static class CopyDlls 
{
    public static string src = "Library/ScriptAssemblies";//拷贝源
    public static string dest = "Assets/_VIP/Resources_moved/MyDlls";//拷贝目标

    public static string[] files = new[] {"HelloDll.dll","HelloDll.pdb" };//要拷贝的文件

    [MenuItem("Tools/Copy Dlls")]
    public static void DoCopyDlls()
    {
        //创建一个目标目录（不存在就创建）
        Directory.CreateDirectory(dest);

        foreach (var f in files)
        {
            //源文件逐个拷贝到目标位置，并给名加上。bytes后缀（只有。bytes后缀文件会被认为时二进制数据，dll无法被打包）
            Debug.Log($"{Path.Combine(src,f)}=>{Path.Combine(dest,f+".bytes")}");
            File.Copy(Path.Combine(src, f), Path.Combine(dest, f + ".bytes"),true);
        }
        //拷贝资源后自动刷新
        AssetDatabase.Refresh();
    }
}
