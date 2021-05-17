using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public abstract class MyMonoSingleton<T> : MonoBehaviour where T: MyMonoSingleton<T>
{
    private static T _instance;

    public static T instance
    {
        [DebuggerStepThrough]
        get
        {
            if (_instance==null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance==null)
                {
                    var go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }
                DontDestroyOnLoad(_instance.gameObject);
                
            }
            return _instance;
        }
        
    }
   
}
