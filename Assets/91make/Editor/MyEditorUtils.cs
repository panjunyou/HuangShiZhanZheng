using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

public class MyEditorUtils : MonoBehaviour
{
    [MenuItem("91make.top/CPU/Enabled",false,200)]
    public static void CpuEnabled()
    {
        PlayerPrefs.SetInt("CpuEnabled", 1);
        Debug.Log("CPU出兵");
    }

    [MenuItem("91make.top/CPU/Disabled", false, 201)]
    public static void CpuDisabled()
    {
        PlayerPrefs.SetInt("CpuEnabled", 0);
        Debug.Log("CPU不出兵");
    }
}
