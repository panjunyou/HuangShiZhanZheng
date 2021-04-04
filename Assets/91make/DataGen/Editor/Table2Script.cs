using Excel;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class Table2Script : MonoBehaviour
{
	[MenuItem("Tools/Clear Scripts")]
	public static void _DelScript()
	{
		string outPath = Application.dataPath + "/91make/DataGen/ConfigScript";
		foreach (string fname in Directory.EnumerateFiles(outPath, "*.cs"))
		{
			print(fname);
			File.Delete(fname);
		}
		AssetDatabase.Refresh();
		print("代码删除完毕");
	}

	[MenuItem("Tools/CSV To Script")]
	public static void _Csv2Script()
	{
		string inPath = Application.dataPath + "/91make/DataGen/ConfigTable";
		string outPath = Application.dataPath + "/91make/DataGen/ConfigScript";
		foreach (string fname in Directory.EnumerateFiles(inPath, "*.csv")) // 读取每一个文件
		{
			string[][] data = LoadCSV(fname); // 读取该CSV文件中的数据，以字符串数组的形式保存下来

			if (data.Length == 0)
				continue;

			GenScript(data, outPath); // 从这个字符串数组解析出对应的数据（如果是一个id，就把它解析成整数，如果是一个name，就把它解析成字符串……）
		}
		AssetDatabase.Refresh(); 
		print("代码生成完毕");
	}

	[MenuItem("Tools/Excel To Script")]
	public static void _Xls2Script()
	{
		string inPath = Application.dataPath + "/91make/DataGen/ConfigTable";
		string outPath = Application.dataPath + "/91make/DataGen/ConfigScript";
		foreach (string fname in Directory.EnumerateFiles(inPath, "*.xls"))
		{
			string[][] data = LoadXls(fname);
			GenScript(data, outPath);
		}
		AssetDatabase.Refresh();
		print("代码生成完毕");
	}

	static string[][] LoadXls(string filePath)
	{
		FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		IExcelDataReader excelReader = null;
		var fi = new FileInfo(filePath);
		if (fi.Extension == ".xls")
		{
			excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
		}
		else if (fi.Extension == ".xlsx")
		{
			excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
		}
		else
		{
			Debug.Log("无法读取非Excel文件");
			return null;
		}

		DataSet result = excelReader.AsDataSet();
		//Tables[0] 下标0表示excel文件中第一张表的数据
		int columnNum = result.Tables[0].Columns.Count;
		int rowNum = result.Tables[0].Rows.Count;
		DataRowCollection coll = result.Tables[0].Rows;

		string[][] data = new string[rowNum][]; // 支持锯齿数组
		for (int row = 0; row < rowNum; row++)
		{
			int colNum = coll[row].ItemArray.Length;
			string[] cols = new string[colNum];
			//Debug.Log(line);
			for(int col = 0; col < colNum; col++)
			{
				cols[col] = coll[row].ItemArray[col].ToString();
			}
			data[row] = cols;
		}

		return data;
	}

	public enum TypeHint
	{
		Enum,
		Array,
	}

	public const int META_NAME = 0;
	public const int META_VALUE = 1;
	public const int COMMNET = 3;
	public const int FIELD_NAME = 4;
	public const int FIELD_TYPE = 5;
	public const int NAMESPACES = 2;
	public const int FIELD_HINT = 6;
	public const int DATA_ROW_START = 7;
	public const string CLASS_DEF = @"
using System;
using System.Collections.Generic;
using UnityEngine;

{NAMESPACE_LIST}

[Serializable]
public partial class {ROW_NAME}
{
{FIELD_LIST}
}

[Serializable]
public partial class {CLASS_NAME}
{
	public List<{ROW_NAME}> list = new List<{ROW_NAME}>();

	public {CLASS_NAME}()
	{
{ROW_LIST}
	}

	public static {CLASS_NAME} instance = new {CLASS_NAME}();
}
";

	public static void GenScript(string[][] data, string path)
	{
		string className = data[META_VALUE][1];
		string rowName = data[META_VALUE][2];

		StringBuilder rowList = new StringBuilder();
		for (int row = DATA_ROW_START; row < data.Length; row++)
		{
			StringBuilder fieldValues = new StringBuilder();
			for (int col = 1; col < data[row].Length; col++)
			{
				string fieldHint = data[FIELD_HINT][col];
				string fieldName = data[FIELD_NAME][col];
				string fieldType = data[FIELD_TYPE][col];
				string fieldValue = data[row][col];

				fieldValue = ProcessFieldValueByHint(fieldValue, fieldType, fieldHint);

				fieldValues.AppendLine($"\t\t\t{fieldName} = {fieldValue},");
			}
			rowList.AppendLine($"\t\tlist.Add(new {rowName}(){{\r\n{fieldValues}\t\t}});\r\n");
		}

		StringBuilder fieldDefs = new StringBuilder();
		for (int col = 1; col < data[FIELD_NAME].Length; col++)
		{
			string fieldHint = data[FIELD_HINT][col];
			fieldDefs.AppendLine($"\t\tpublic {data[FIELD_TYPE][col]}{(fieldHint.Contains("Array")?"[]":"")} {data[FIELD_NAME][col]};\r\n");
		}

		StringBuilder namespaceList = new StringBuilder();
		for (int col = 1; col < data[NAMESPACES].Length; col++)
		{
			if (string.IsNullOrEmpty(data[NAMESPACES][col]))
				continue;
			namespaceList.AppendLine($"using {data[NAMESPACES][col]};\r\n");
		}

		StringBuilder classImpl = new StringBuilder(CLASS_DEF);
		classImpl.Replace("{CLASS_NAME}", className);
		classImpl.Replace("{FIELD_LIST}", fieldDefs.ToString());
		classImpl.Replace("{ROW_NAME}", rowName);
		classImpl.Replace("{ROW_LIST}", rowList.ToString());
		classImpl.Replace("{NAMESPACE_LIST}", namespaceList.ToString());

		string script = classImpl.ToString();
		print(script);

		string fname = path + $"/{className}.cs";
		File.Delete(fname);
		File.WriteAllText(fname, script);
	}

	private static string ProcessFieldValueByType(string fieldValue, string fieldType)
	{
		switch (fieldType) // Type是小粒度的字段提示
		{
			case "string":
				return $"\"{fieldValue}\"";
			case "float":
				return $"{fieldValue}f";
			case "Vector3": // 1234, 1234.56
				{
					return Regex.Replace(fieldValue, @"([+|-]?\d+\.?\d*),\s*([+|-]?\d+\.?\d*),\s*([+|-]?\d+\.?\d*)", @"$1f, $2f, $3f");
				}
		}

		return fieldValue;
	}

	private static string ProcessFieldValueByHint(string fieldValue, string fieldType, string fieldHint)
	{
		switch (fieldHint) // Hint是大粒度的字段类型
		{
			case "Enum":
				return $"{fieldType}.{fieldValue}";
			case "ValArray":
				{
					string[] elems = fieldValue.Split('|');

					string sVal = "";
					for (int i = 0; i < elems.Length; i++)
					{
						if (i != 0)
							sVal += ", ";
						sVal += ProcessFieldValueByType(elems[i], fieldType);
					}
					return $"new []{{{sVal}}}"; // NOTE: 类型必须提供与元素匹配的构造器
				}
			case "RefArray":
				{
					string[] elems = fieldValue.Split('|');

					string sVal = "";
					for (int i = 0; i < elems.Length; i++)
					{
						if (i != 0)
							sVal += ", ";
						sVal += $"new {fieldType}({ProcessFieldValueByType(elems[i], fieldType)})";
					}
					return $"new []{{{sVal}}}";
				}
			default:
				return ProcessFieldValueByType(fieldValue, fieldType);
		}

	}

	private static string[] SplitCSV(string s)
	{
		Regex regex = new Regex("\".*?\"");
		var a = regex.Matches(s).Cast<Match>().Select(m => m.Value).ToList(); // a存放所有带双引号的字符串
		var b = regex.Replace(s, "%_%"); // b是从s抽取的双引号之外的字符串
		var c = b.Split(','); // 把b用逗号分割好
		// 处理双引号字符串
		for (int i = 0, j = 0; i < c.Length && j < a.Count; i++)
		{
			if (c[i] == "%_%") // 遇到双引号切割符就知道一个双引号字符串结束了，于是加入c
			{
				c[i] = a[j++].Replace("\"",""); // 加入前去掉双引号
			}
		}
		return c;
	}

	public static string[][] LoadCSV(string pathName)
	{
		string text = File.ReadAllText(pathName);

		string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		string[][] data = new string[lines.Length][]; // 支持锯齿数组
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			//Debug.Log(line);

			// Method 1:
			//string[] cols = line.Split(new[] { ',' }, StringSplitOptions.None);
			//data[i] = cols;

			// Method 2: https://www.cnblogs.com/nonkicat/p/3557808.html
			//var cols = new List<string>();
			//var filter = @"([^\""\,]*[^\""\,])|[\""]([^\""]*)[\""]";
			//Match match = Regex.Match(line, filter, RegexOptions.IgnoreCase);

			//while (match.Success)
			//{
			//	if (!string.IsNullOrEmpty(match.Groups[2].Value))
			//	{
			//		cols.Add(match.Groups[2].Value);
			//	}
			//	else
			//	{
			//		cols.Add(match.Value);
			//	}
			//	match = match.NextMatch();
			//}
			//data[i] = cols.ToArray();

			// Method 3: https://bbs.csdn.net/topics/392003882
			data[i] = SplitCSV(line);
		}

		return data;
	}
}
