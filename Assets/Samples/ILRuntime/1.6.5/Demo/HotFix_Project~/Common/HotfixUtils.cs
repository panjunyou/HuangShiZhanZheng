using ILitJson;
using Lockstep.Math;
using SRF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class HotfixUtils
{
	#region 定点数支持
	/// <summary>
	/// unity的float转 LFloat
	/// 用法：1.2f.ToLFloat()
	/// 也可以（LFloat）（1.2f）
	/// </summary>
	/// <param name="o"></param>
	/// <returns></returns>
	public static LFloat ToLFloat(this float o)
	{
		return new LFloat(true, (long)(o * LFloat.Precision));
	}
	/// <summary>
	/// LFloat转unity的float
	/// </summary>
	/// <param name="o"></param>
	/// <returns></returns>
	public static float ToFloat(this LFloat o)
	{
		return (float)(o);
	}
	/// <summary>
	/// Unity的Vector3转 LVector3 
	/// </summary>
	/// <param name="o"></param>
	/// <returns></returns>
	public static LVector3 ToLVevtor3_My(this Vector3 o)
	{
		return new LVector3(
			o.x.ToLFloat(),
			o.y.ToLFloat(),
			o.z.ToLFloat()
			);
	}
	/// <summary>
	///  LVector3转unity的Vector3
	/// </summary>
	/// <param name="o"></param>
	/// <returns></returns>
	public static Vector3 ToVector3(this LVector3 o)
	{
		return new Vector3(o.x.ToFloat(), o.y.ToFloat(), o.z.ToFloat());
	}
	/// <summary>
	/// LQuaternion转Unity的Quaternion
	/// </summary>
	/// <param name="o"></param>
	/// <returns></returns>
	public static Quaternion ToQuaternion(this LQuaternion o)
	{
		return new Quaternion(o.x.ToFloat(), o.y.ToFloat(), o.z.ToFloat(), o.w.ToFloat());
	}
	/// <summary>
	/// Unity的Quaternion转LQuaternion
	/// </summary>
	/// <param name="o"></param>
	/// <returns></returns>
	public static LQuaternion ToLQuaternion(this Quaternion o)
	{
		return new LQuaternion(o.x.ToLFloat(), o.y.ToLFloat(), o.z.ToLFloat(), o.w.ToLFloat());
	}

	#endregion

	public static void SendMessage(this object obj, string msgName, params object[] args)
	{
		var mi = obj.GetType().GetMethod(msgName);//取函数名
		if (mi == null)
		{
			return;
		}
		mi.Invoke(obj, args);//Invoke(调用者, 参数)
	}

	#region 通用对象字段输出支持
	/// <summary>
	/// 输出对象的每一个字段
	/// </summary>
	/// <param name="obj">要输出的对象</param>
	/// <param name="sep">对象字段间的分隔符</param>
	/// <returns></returns>
	public static string ToStringEx(this object obj, string sep = ", ")
	{
		//获取对象类型
		Type type = obj.GetType();

		if (typeof(IList).IsAssignableFrom(type))
		{
			StringBuilder sb2 = new StringBuilder();
			var list = obj.As<IList>();
			if (list.Count > 1)
			{
				sb2.AppendLine($"[{list.Count}]");
			}
			else
			{
				sb2.Append($"[{list.Count}]==>");
			}

			for (int i2 = 0; i2 < list.Count; i2++)
			{
				var elem = list[i2];
				sb2.Append(elem.ToStringEx(sep) + (i2 == list.Count - 1 ? "" : "\r\n"));
			}
			return sb2.ToString();
		}
		else
		{
			StringBuilder sb = new StringBuilder();
			var fis = type.GetFields();
			for (int i = 0; i < fis.Length; i++)
			{
				FieldInfo fi = fis[i];
				object v = fi.GetValue(obj);
				if (v.GetType().IsValueType == false)
				{
					v = v.ToStringEx(sep);
				}
				sb.AppendFormat("{0} = {1}{2}"
					, fi.Name
					, v
					, i == fis.Length - 1 ? "" : sep
					);
			}
			return sb.ToString();
		}
	}
	#endregion
	public static T As<T>(this object o) { return (T)o; }

	public static T DeepClone<T>(this T o)
	{
		//JsonMapper是引入定点库包含进来的，该类用于将对象序列化或反序列化到JSON字符串
		string js = JsonMapper.ToJson(o);//object=>Json String 
		return JsonMapper.ToObject<T>(js);//Json String =>object （转回 JsonMapper）
	}
	public static string InitialLower(this string s)
	{
		StringBuilder sb = new StringBuilder(s);
		sb[0] = char.ToLower(sb[0]);
		return sb.ToString();
	}
	public static string InitialUpper(this string s)
	{
		StringBuilder sb = new StringBuilder(s);
		sb[0] = char.ToUpper(sb[0]);
		return sb.ToString();
	}
	/// <summary>
	/// 获取节点完整路径
	/// </summary>
	/// <param name="tr"></param>
	/// <returns></returns>
	public static string FullPath(this Transform tr)
	{
		string s = tr.gameObject.name;
		do
		{
			tr = tr.parent;//逐级往父节点搜
			if (tr == null)
			{
				break;
			}
			s = tr.gameObject.name + "/" + s;
		} while (true);
		return s;
	}

	// find out if any input is currently active by using Selectable.all
	// (FindObjectsOfType<InputField>() is far too slow for huge scenes)
	public static bool AnyInputActive()
	{
		return Selectable.allSelectables.Any(
			sel => sel is InputField && ((InputField)sel).isFocused
		);
	}

	// check if the cursor is over a UI or OnGUI element right now
	// note: for UI, this only works if the UI's CanvasGroup blocks Raycasts
	// note: for OnGUI: hotControl is only set while clicking, not while zooming
	public static bool IsCursorOverUserInterface()
	{
		// IsPointerOverGameObject check for left mouse (default)
		if (EventSystem.current.IsPointerOverGameObject())
			return true;

		// IsPointerOverGameObject check for touches
		for (int i = 0; i < Input.touchCount; ++i)
			if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
				return true;

		// OnGUI check
		return GUIUtility.hotControl != 0;
	}

	///// <summary>
	///// 带有[Serializable]和[MessagePackObject]标志的类型
	///// </summary>
	///// <param name="t"></param>
	///// <returns></returns>
	//public static bool IsSerializableType(Type t)
	//{
	//	return t.IsSerializable && Attribute.IsDefined(t, typeof(MessagePackObjectAttribute));
	//}

	/// <summary>
	/// 取出所有无[NonSerialized]字段
	/// </summary>
	/// <param name="t"></param>
	/// <returns></returns>
	public static IEnumerable<FieldInfo> GetSerializableFields(Type t)
	{
		var serializableFields = t
			.GetFields(BindingFlags.Public /*| BindingFlags.NonPublic*/ | BindingFlags.Instance)
			.Where(fi => !Attribute.IsDefined(fi, typeof(NonSerializedAttribute)));
		return serializableFields;
	}

	#region Regex & Pwd
	private const string EMAIL_PATTERN = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
	private const string USERNAME_AND_DISCRIMINATOR_PATTERN = @"^[a-zA-Z0-9]{4,20}#[0-9]{4}$";
	private const string USERNAME_PATTERN = @"^[a-zA-Z0-9]{4,20}$";
	private const string RANDOM_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
	private static System.Random rnd = new System.Random();

	public static bool IsEmail(string email) { return email != null && Regex.IsMatch(email, EMAIL_PATTERN); }
	public static bool IsUsername(string username) { return username != null && Regex.IsMatch(username, USERNAME_PATTERN); }
	public static bool IsUsernameAndDiscriminator(string usernameAndDiscriminator) { return usernameAndDiscriminator != null && Regex.IsMatch(usernameAndDiscriminator, USERNAME_AND_DISCRIMINATOR_PATTERN); }
	public static string GenerateRandom(int length) { return new string(Enumerable.Repeat(RANDOM_CHARS, length).Select(s => s[rnd.Next(s.Length)]).ToArray()); }

	public static string SHA256(string password)
	{
		byte[] hashValue = null;
		using (var sha = new SHA256Managed())
		{
			hashValue = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
		}

		return BitConverter.ToString(hashValue);
	}
	#endregion

	// string.GetHashCode is not quaranteed to be the same on all machines, but
	// we need one that is the same on all machines. simple and stupid:
	public static int GetStableHashCode(this string text)
	{
		unchecked
		{
			int hash = 23;
			foreach (char c in text)
				hash = hash * 31 + c;
			return hash;
		}
	}


	// hard mouse scrolling that is consistent between all platforms
	//   Input.GetAxis("Mouse ScrollWheel") and
	//   Input.GetAxisRaw("Mouse ScrollWheel")
	//   both return values like 0.01 on standalone and 0.5 on WebGL, which
	//   causes too fast zooming on WebGL etc.
	// normally GetAxisRaw should return -1,0,1, but it doesn't for scrolling
	public static float GetAxisRawScrollUniversal()
	{
		float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
		if (scroll < 0) return -1;
		if (scroll > 0) return 1;
		return 0;
	}

	// two finger pinch detection
	// source: https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
	public static float GetPinch()
	{
		if (Input.touchCount == 2)
		{
			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

			// Find the difference in the distances between each frame.
			return touchDeltaMag - prevTouchDeltaMag;
		}
		return 0;
	}

	// universal zoom: mouse scroll if mouse, two finger pinching otherwise
	public static float GetZoomUniversal()
	{
		if (Input.mousePresent)
			return HotfixUtils.GetAxisRawScrollUniversal();
		else if (Input.touchSupported)
			return GetPinch();
		return 0;
	}

	public static bool IsSameBlock(this Vector3 v1, Vector3 v2)
	{
		const int BLOCK_SIZE = 4;
		return ((int)v1.x / BLOCK_SIZE) == ((int)v2.x / BLOCK_SIZE)
			&& ((int)v1.z / BLOCK_SIZE) == ((int)v2.z / BLOCK_SIZE);
	}

	public static void CopyDirectory(string srcPath, string destPath)
	{
		try
		{
			DirectoryInfo dir = new DirectoryInfo(srcPath);
			FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
			foreach (FileSystemInfo i in fileinfo)
			{
				if (i is DirectoryInfo)     //判断是否文件夹
				{
					if (!Directory.Exists(destPath + "\\" + i.Name))
					{
						Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
					}
					CopyDirectory(i.FullName, destPath + "\\" + i.Name);    //递归调用复制子文件夹
				}
				else
				{
					File.Copy(i.FullName, destPath + "\\" + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
				}
			}
		}
		catch (Exception e)
		{
			throw;
		}
	}

	public static string GetMd5(string fileName)
	{
		string result = "";
		using (FileStream fs = File.OpenRead(fileName))
		{
			result = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(fs));
			fs.Close();
		}
		return result;
	}
}
