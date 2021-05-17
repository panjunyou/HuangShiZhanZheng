using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MyDllLoader : MonoBehaviour
{

    TextAsset dll;
    TextAsset pdb;
   
    private  void Start()
    {
        ////TextAsset可以用于承载文本数据和二进制数据
        //dll = await Addressables.LoadAssetAsync<TextAsset>("HelloDll.dll").Task;
        //pdb = await Addressables.LoadAssetAsync<TextAsset>("HelloDll.pdb").Task;

        StartCoroutine("MyLoad");
        ////载入到mono虚拟机来
        //var ass = Assembly.Load(dll.bytes,pdb.bytes);

        ////foreach (var t in ass.GetTypes())
        ////{
        ////    print(t);
        ////}

        ////执行SayHello方法
        //Type t = ass.GetType("HelloDll");
        //t.GetMethod("SayHello").Invoke(null,null);

        //Addressables.Release<TextAsset>(dll);
        //Addressables.Release<TextAsset>(pdb);
    }

    //public void MyLoad(AsyncOperation<TextAsset> s)
    //{

    //}
    IEnumerator MyLoad()
    {
        AsyncOperationHandle<TextAsset> goHandle = Addressables.LoadAssetAsync<TextAsset>("HelloDll.dll");        
        yield return goHandle;
        if (goHandle.Status == AsyncOperationStatus.Succeeded)
        {
            dll = goHandle.Result;
            //etc...
        }

        AsyncOperationHandle<TextAsset> goHandle1 = Addressables.LoadAssetAsync<TextAsset>("HelloDll.pdb");
        yield return goHandle1;
        if (goHandle1.Status == AsyncOperationStatus.Succeeded)
        {
            pdb = goHandle1.Result;
            //etc...
        }

        //载入到mono虚拟机来
        var ass = Assembly.Load(dll.bytes, pdb.bytes);

        //foreach (var t in ass.GetTypes())
        //{
        //    print(t);
        //}

        //执行SayHello方法
        Type t = ass.GetType("HelloDll");
        t.GetMethod("SayHello").Invoke(null, null);

        Addressables.Release<TextAsset>(dll);
        Addressables.Release<TextAsset>(pdb);
    }


}
