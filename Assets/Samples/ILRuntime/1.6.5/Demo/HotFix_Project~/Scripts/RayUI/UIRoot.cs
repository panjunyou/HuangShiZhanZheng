using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Init The UI Root
/// 
/// UIRoot
/// -Canvas
/// --FixedRoot
/// --NormalRoot
/// --PopupRoot
/// -Camera
/// </summary>
public class UIRoot : MonoBehaviour
{
	private static UIRoot m_Instance = null;
	public static UIRoot Instance
	{
		get
		{
			if (m_Instance == null)
			{
				InitRoot();
				GameObject.DontDestroyOnLoad(m_Instance.gameObject);
			}
			return m_Instance;
		}
	}

	public static void SetInitParams(Vector2 resolution)
	{
		UIRoot.resolution = resolution;
	}

	// 默认分辨率
	private static Vector2 resolution = new Vector2(1080, 1920); // 改成跟项目一致的，便于调试

	public Transform root; // 注意：此root是指TTUIRoot所在的Transform，不是整个场景树的根！
	public Transform fixedRoot;
	public Transform normalRoot;
	public Transform popupRoot;
	public Camera uiCamera;

	static void InitRoot()
	{
		GameObject go = new GameObject("UIRoot");

		//GameObject goGameCtrl = GameObject.FindWithTag("GameController");
		//if (goGameCtrl == null)
		//{
		//	print("必须指定一个Tage=GameController的对象作为场景根节点，以便于BroadcastMessage");
		//	return;
		//}
		//go.transform.parent = goGameCtrl.transform;

		go.layer = LayerMask.NameToLayer("UI");
		m_Instance = go.AddComponent<UIRoot>();
		go.AddComponent<RectTransform>();

		Canvas can = go.AddComponent<Canvas>();
		can.renderMode = RenderMode.ScreenSpaceOverlay;
		can.pixelPerfect = true;

		go.AddComponent<GraphicRaycaster>();

		m_Instance.root = go.transform;

		GameObject camObj = new GameObject("UICamera");
		camObj.layer = LayerMask.NameToLayer("UI");
		camObj.transform.parent = go.transform;
		camObj.transform.localPosition = new Vector3(0, 0, -100f);
		Camera cam = camObj.AddComponent<Camera>();
		cam.clearFlags = CameraClearFlags.Depth;
		cam.orthographic = true;
		cam.farClipPlane = 200f;
		cam.cullingMask = 1 << 5;
		cam.nearClipPlane = -50f;
		cam.farClipPlane = 50f;
		cam.backgroundColor = Color.black;

		can.worldCamera = cam;

		m_Instance.uiCamera = cam;

		//add audio listener
		//camObj.AddComponent<AudioListener>();
		//camObj.AddComponent<GUILayer>();

		CanvasScaler cs = go.AddComponent<CanvasScaler>();
		cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		cs.referenceResolution = resolution;
		print($"参考分辨率为：{cs.referenceResolution}");
		cs.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

		////add auto scale camera fix size.
		//TTCameraScaler tcs = go.AddComponent<TTCameraScaler>();
		//tcs.scaler = cs;

		//set the raycaster
		//GraphicRaycaster gr = go.AddComponent<GraphicRaycaster>();

		GameObject subRoot;

		// 固定窗口默认在底层，以防止固定窗口遮挡非固定窗口
		subRoot = CreateSubCanvasForRoot(go.transform, 250);
		subRoot.name = "FixedRoot";
		m_Instance.fixedRoot = subRoot.transform;
		m_Instance.fixedRoot.transform.localScale = Vector3.one;

		subRoot = CreateSubCanvasForRoot(go.transform, 0);
		subRoot.name = "NormalRoot";
		m_Instance.normalRoot = subRoot.transform;
		m_Instance.normalRoot.transform.localScale = Vector3.one;

		subRoot = CreateSubCanvasForRoot(go.transform, 500);
		subRoot.name = "PopupRoot";
		m_Instance.popupRoot = subRoot.transform;
		m_Instance.popupRoot.transform.localScale = Vector3.one;

		//add Event System
		GameObject esObj = GameObject.Find("EventSystem");
		if (esObj != null)
		{
			GameObject.DestroyImmediate(esObj);
		}

		GameObject eventObj = new GameObject("EventSystem");
		eventObj.layer = LayerMask.NameToLayer("UI");
		eventObj.transform.SetParent(go.transform);
		eventObj.AddComponent<EventSystem>();
		eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

	}

	static GameObject CreateSubCanvasForRoot(Transform root, int sort)
	{
		GameObject go = new GameObject("canvas");
		go.transform.parent = root;
		go.layer = LayerMask.NameToLayer("UI");

		RectTransform rect = go.AddComponent<RectTransform>();
		rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
		rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;

		//  Canvas can = go.AddComponent<Canvas>();
		//  can.overrideSorting = true;
		//  can.sortingOrder = sort;
		//  go.AddComponent<GraphicRaycaster>();

		return go;
	}

	void OnDestroy()
	{
		m_Instance = null;
	}
}
