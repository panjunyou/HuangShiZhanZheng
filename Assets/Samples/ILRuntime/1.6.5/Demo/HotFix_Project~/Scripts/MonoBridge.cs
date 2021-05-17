using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//继承MyMonoSingleton<MonoBridge>就会创建一个跨创建的物体并且添加上<MonoBridge>脚本
public class MonoBridge : MyMonoSingleton<MonoBridge>
{
    
    #region MoonBridge (用来更新所有没有在MonoBehaviour继承的物体)
    public Action OnUpdate;
    public Action OnIMGUI;

    private void Update()
    {
        OnUpdate?.Invoke();
    }

    private void OnGUI()
    {
        OnIMGUI?.Invoke();
    }
    #endregion
}
