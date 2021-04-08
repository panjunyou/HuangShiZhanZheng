using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// 此脚本负责UI页面的导出，我们先学习如何使用该脚本，暂不涉及其实现
/// 用法：首先我们需要提供一个代码生成模版
/// </summary>
public static class GenUIPageScript
{
	public static string SCRIPT_GEN_PATH = "Assets/_VIP/Scripts/UIPages";
	public static string SCRIPT_TEMPLATE_PATH = "Assets/Plugins/RayUI/Template";
	//public static string UI_ROOT_PATH = "UI"; // NB: Addressable不需要根路径，把UI资源做一下名称简化保持【资源名==根对象名】即可

	public static string UIPAGE_CLASS_DEF = File.ReadAllText(SCRIPT_TEMPLATE_PATH + "/UIPageTemplate.cs.txt", Encoding.UTF8);

	public static string VIEW_CLASS_DEF = File.ReadAllText(SCRIPT_TEMPLATE_PATH + "/UIViewTemplate.cs.txt", Encoding.UTF8);

	public class UIFieldInfo
	{
		#region declaration
		public string fieldType;
		public string fieldName;
		#endregion

		#region initialization
		public string fieldPath;
		#endregion

		public override string ToString()
		{
			return $"{fieldType} {fieldName} => {fieldPath}";
		}
	}

	private static string GetFieldType(WidgetID w)
	{
		MonoBehaviour mono;
		if ((mono = w.GetComponents<WidgetID>().FirstOrDefault(x => x != w)) != null)
		{
			return mono.GetType().Name;
		}
		else if ((mono = w.GetComponent<Selectable>()) != null)
		{
			return mono.GetType().Name;
		}
		else if ((mono = w.GetComponent<Graphic>()) != null)
		{
			return mono.GetType().Name;
		}
		else
		{
			return "Transform";
		}
	}

	public static void _Gen(Transform tr, string path, List<UIFieldInfo> list)
	{
		if (tr == null)
			return;

		foreach (Transform c in tr)
		{
			string cPath = path == string.Empty ? c.name : path + "/" + c.name;
			var widgets = c.GetComponents<WidgetID>();
			foreach (var w in widgets)
			{
				if (w && w.GetType() == typeof(WidgetID) && w.ignore == false)
				{
					//Debug.Log("+" + cPath);
					string fname = c.name.InitialLower();
					var fieldInfo = list.Find(x => x.fieldName == fname);
					if (fieldInfo != null)
						fname = "_" + c.FullPath();
					list.Add(new UIFieldInfo()
					{
						fieldName = fname,
						fieldPath = cPath,
						fieldType = GetFieldType(w),
					});
					break;
				}
			}
			_Gen(c, cPath, list);
		}
	}

	[MenuItem("GameObject/RayGame/生成UIPage脚本", priority = 0)]
	public static void Gen()
	{
		if (Selection.activeTransform.parent.gameObject.name != "Canvas (Environment)")
		{
			Debug.LogError("生成UIPage脚本失败，检查以下条件是否满足：1、UI必须是预制体；2、命令执行在预制体根节点上——父节点必须是Canvas (Environment)");
			return;
		}

		var fieldList = new List<UIFieldInfo>();
		_Gen(Selection.activeTransform, string.Empty, fieldList);

		StringBuilder sbFieldDef = new StringBuilder();
		StringBuilder sbFieldInit = new StringBuilder();
		foreach (var field in fieldList)
		{
			sbFieldDef.AppendLine($"\tpublic {field.fieldType} {field.fieldName};");
			sbFieldInit.AppendLine($"\t\t{field.fieldName} = transform.Find(\"{field.fieldPath}\").GetComponent<{field.fieldType}>();");
		}
		string sPage = UIPAGE_CLASS_DEF
			.Replace("{ROOT_UI_NAME}", Selection.activeGameObject.name)
			.Replace("{UI_WIDGET_FIELD_LIST}", sbFieldDef.ToString())
			.Replace("{FIELD_INITIALIZATION_LIST}", sbFieldInit.ToString())
			.Replace("{UI_PATH}", /*UI_ROOT_PATH + "/" +*/ Selection.activeGameObject.name)
			;
		string sView = VIEW_CLASS_DEF
			.Replace("{ROOT_UI_NAME}", Selection.activeGameObject.name);

		string scriptPath = SCRIPT_GEN_PATH + "/" + Selection.activeGameObject.name + ".cs";
		if (File.Exists(scriptPath))
			File.Delete(scriptPath);
		File.WriteAllText(scriptPath, sPage, Encoding.UTF8);

		string viewPath = SCRIPT_GEN_PATH + "/" + Selection.activeGameObject.name.Replace("Page", "View") + ".cs";
		// NB: 视图文件不能自动删除并重建，因为可能已经写了很多代码了
		if (File.Exists(viewPath) == false
			|| (EditorUtility.DisplayDialog("文件已存在，是否覆盖？", $"File Name: {viewPath}", "是", "否")
			&& EditorUtility.DisplayDialog("文件已存在，是否覆盖？", $"File Name: {viewPath}", "是", "否"))
			)
		{
			Debug.Log(sView);
			File.WriteAllText(viewPath, sView, Encoding.UTF8);
		}

		AssetDatabase.Refresh();
	}
}
