using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MyDllLoader : MonoBehaviour
{
    private async void Start()
    {
        //TextAsset可以用于承载文本数据和二进制数据
        TextAsset dll = await Addressables.LoadAssetAsync<TextAsset>("HelloDll.dll").Task;
        TextAsset pdb = await Addressables.LoadAssetAsync<TextAsset>("HelloDll.pdb").Task;

        //载入到mono虚拟机来
        var ass = Assembly.Load(dll.bytes,pdb.bytes);

        //foreach (var t in ass.GetTypes())
        //{
        //    print(t);
        //}

        //执行SayHello方法
        Type t = ass.GetType("HelloDll");
        t.GetMethod("SayHello").Invoke(null,null);
    }
}
