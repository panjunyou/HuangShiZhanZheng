using System;
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

public static class Utils
{
	public static T As<T>(this object o) { return (T)o; }
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

	public static string FullPath(this Transform tr)
	{
		string s = "";
		do
		{
			s = s + "/" + tr.name;
			tr = tr.parent;
		} while (tr != null);
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
			return Utils.GetAxisRawScrollUniversal();
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
