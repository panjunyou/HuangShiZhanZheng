using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

/// <summary>
/// 每个页面表示一个UI面板
/// 页面显示的三个步骤:
/// 实例化UI > 根据数据刷新UI > 显示
/// </summary>

#region define
/// <summary>
/// UI类型：Normal | Fixed | PopUp | Any<para/>
/// Normal是大类，大类下又分DoNothing | HideOther | NeedBack | NoNeedBack四小类<para/>
/// </summary>
public enum UIType
{
	Normal = 1,
	Fixed = 1 << 1,
	PopUp = 1 << 2,
	Any = Normal | Fixed | PopUp,      //独立的窗口
}

/// <summary>
/// DoNothing | HideOther | NeedBack<para/>
/// DoNothing：不关闭其他界面<para/>
/// HideOther：显示时，关闭其他界面。关闭时，将其他页面处理为同时关闭，就达到了禁止返回上一页面的目的（例如：主面板-》子面板-》点击子面板确定按钮-》不需要返回主面板，主子面板全关）<para/>
/// NeedBack：显示时，要前台显示；关闭时，显示前台后面的页面（只要其他页面不处理，就自动退回前一页面了）<para/>
/// 注意：NeedBack或HideOther的页面只能关闭或返回同类型页面
/// </summary>
public enum UIMode
{
	DoNothing,      // 不关闭其他界面
	HideOther,      // 关闭其他界面
	NeedBack,       // 点击返回按钮关闭当前,不关闭其他界面(需要调整好层级关系)
					//NoNeedBack,		// 关闭TopBar,关闭其他界面,不加入backSequence队列
}

/// <summary>
/// None | Normal | WithBg
/// </summary>
public enum UICollider
{
	None,      // 显示该界面不包含碰撞背景
	Normal,    // 碰撞透明背景
	WithBg,    // 碰撞非透明背景
}
#endregion

// 看懂这个类，你就看懂整个框架了
public abstract class UIPage : MyComponent
{
	private string name = string.Empty;

	//this page's id
	protected int id = -1;

	//this page's type
	protected UIType type = UIType.Normal;

	//how to show this page.
	protected UIMode mode = UIMode.DoNothing;

	//the background collider mode（没实现）
	protected UICollider collider = UICollider.None;

	// 指定用于动态加载UI界面的路径，必须在Resources下
	// TODO: 可以通过JSON文件去配置
	protected abstract string uiPath
	{
		get;
	}


	//all pages with the union type
	private static Dictionary<string, UIPage> m_allPages;
	//public static Dictionary<string, UIPage> allPages { get { return m_allPages; } }

	/// <summary>
	/// 已经打开的页面，可以有多个这样的已打开页面（背包、装备、任务……）
	/// 控制每个页面关闭后返回前一页面（1>2>3>4>5）
	/// </summary>
	private static List<UIPage> m_currentPageNodes;
	//public static List<UIPage> currentPageNodes
	//{ get { return m_currentPageNodes; } }

	//record this ui load mode.async or sync.
	private bool isAsyncUI = false;

	//this page active flag
	protected bool isActived = false;

	//refresh page 's data.
	private object m_data = null;
	protected object data { get { return m_data; } }

	//delegate load ui function.
	public static Func<string, GameObject> delegateSyncLoadUI = null;
	public static Action<string, Action<GameObject>> delegateAsyncLoadUI = null;

	#region virtual api

	///When Instance UI Ony Once.
	protected virtual void OnAwake() { }

	///Show UI Refresh Eachtime.
	protected virtual void OnRefresh() { }

	protected virtual void OnActive() { }

	protected virtual void OnHide() { }

	/// <summary>
	/// 当数据改变时，刷新UI组件显示
	/// </summary>
	public void Refresh()
	{
		OnRefresh();
	}

	/// <summary>
	/// 激活自身页面，做UI动画
	/// </summary>
	protected void Active()
	{
		this.gameObject.SetActive(true);
		isActived = true;

		OnActive();
	}

	/// <summary>
	/// 失活自身页面，但不清理数据
	/// </summary>
	protected void Hide()
	{
		OnHide();

		this.gameObject.SetActive(false);
		isActived = false;
		//set this page's data null when hide.
		this.m_data = null;

		Debug.Log($"页面[{this.name}]已关闭");
	}

	#endregion

	#region internal api

	private UIPage() { }
	public UIPage(UIType type, UIMode mod, UICollider col)
	{
		this.type = type;
		this.mode = mod;
		this.collider = col;
		this.name = this.GetType().ToString();

		//when create one page.
		//bind special delegate .
		UIBind.Bind();
		//Debug.LogWarning("[UI] create page:" + ToString());
	}

	/// <summary>
	/// Sync Show UI Logic
	/// </summary>
	protected void _Show()
	{
		Debug.Log($"准备显示页面[{this.name}]");

		//1:instance UI
		if (this.gameObject == null && string.IsNullOrEmpty(uiPath) == false)
		{
			GameObject go = null;
			if (delegateSyncLoadUI != null)
			{
				Object o = delegateSyncLoadUI(uiPath);
				go = o != null ? GameObject.Instantiate(o) as GameObject : null;
			}
			else
			{
				go = GameObject.Instantiate(Resources.Load(uiPath)) as GameObject;
			}

			//protected.
			if (go == null)
			{
				Debug.LogError($"[UI] 同步加载UI预制体{uiPath}失败.");
				return;
			}

			AnchorUIGameObject(go);

			//after instance should awake init.
			OnAwake();

			//mark this ui sync ui
			isAsyncUI = false;
		}

		// 显示UIPage，并做UI动画
		Active(); // => SetActive

		// 当数据改变时，刷新UI组件显示
		Refresh();

		//:popup this node to top if need back.
		PushTop(this);

		Debug.Log($"页面[{this.name}]已打开");
	}

	/// <summary>
	/// Async Show UI Logic
	/// </summary>
	protected void _ShowAsync(Action callback)
	{
		Debug.Log($"准备显示页面[{this.name}]");

		//1:Instance UI
		//FIX:support this is manager multi gameObject,instance by your self.
		if (this.gameObject == null && string.IsNullOrEmpty(uiPath) == false)
		{
			GameObject go = null;
			//bool _loading = true;

			#region Addressables
			float _t0 = Time.realtimeSinceStartup;
			Addressables.InstantiateAsync(uiPath).Completed += (obj) =>
			{
				if (obj.IsDone)
				{
					go = obj.Result;

					AnchorUIGameObject(go);
					OnAwake();
					isAsyncUI = true;

					//:animation active.
					Active();

					//:refresh ui component.
					Refresh();

					//:popup this node to top if need back.
					PushTop(this);

					if (callback != null) callback();
				}
				else
				{
					if (Time.realtimeSinceStartup - _t0 >= 10.0f)
					{
						Debug.LogError("[UI] WTF async load your ui prefab timeout!");
						return;
					}
				}
			};
			#endregion

			#region Resources.LoadAsync
			//delegateAsyncLoadUI(uiPath, (o) =>
			//{
			//	go = o != null ? GameObject.Instantiate(o) as GameObject : null;
			//	AnchorUIGameObject(go);
			//	OnAwake();
			//	isAsyncUI = true;
			//	_loading = false;

			//	//:animation active.
			//	Active();

			//	//:refresh ui component.
			//	Refresh();

			//	//:popup this node to top if need back.
			//	PopNode(this);

			//	if (callback != null) callback();
			//});

			//float _t0 = Time.realtimeSinceStartup;
			//while (_loading)
			//{
			//	if (Time.realtimeSinceStartup - _t0 >= 10.0f)
			//	{
			//		Debug.LogError("[UI] WTF async load your ui prefab timeout!");
			//		yield break;
			//	}
			//	yield return null;
			//}
			#endregion
		}
		else
		{
			//:animation active.
			Active();

			//:refresh ui component.
			Refresh();

			//:popup this node to top if need back.
			PushTop(this);

			if (callback != null) callback();
		}
	}

	internal bool BackOrHide()
	{
		if (type == UIType.Normal && (mode == UIMode.HideOther || mode == UIMode.NeedBack))
			return true;

		return false;
	}

	protected void AnchorUIGameObject(GameObject ui)
	{
		if (UIRoot.Instance == null || ui == null) return;

		this.gameObject = ui;
		this.transform = ui.transform;

		//check if this is ugui or (ngui)?
		Vector3 anchorPos = Vector3.zero;
		Vector2 sizeDel = Vector2.zero;
		Vector3 scale = Vector3.one;
		if (ui.GetComponent<RectTransform>() != null)
		{
			anchorPos = ui.GetComponent<RectTransform>().anchoredPosition;
			sizeDel = ui.GetComponent<RectTransform>().sizeDelta;
			scale = ui.GetComponent<RectTransform>().localScale;
		}
		else
		{
			anchorPos = ui.transform.localPosition;
			scale = ui.transform.localScale;
		}

		//Debug.Log("anchorPos:" + anchorPos + "|sizeDel:" + sizeDel);

		if (type == UIType.Fixed)
		{
			ui.transform.SetParent(UIRoot.Instance.fixedRoot);
		}
		else if (type == UIType.Normal)
		{
			ui.transform.SetParent(UIRoot.Instance.normalRoot);
		}
		else if (type == UIType.PopUp)
		{
			ui.transform.SetParent(UIRoot.Instance.popupRoot);
		}


		if (ui.GetComponent<RectTransform>() != null)
		{
			ui.GetComponent<RectTransform>().anchoredPosition = anchorPos;
			ui.GetComponent<RectTransform>().sizeDelta = sizeDel;
			ui.GetComponent<RectTransform>().localScale = scale;
		}
		else
		{
			ui.transform.localPosition = anchorPos;
			ui.transform.localScale = scale;
		}
	}

	public override string ToString()
	{
		return ">Name:" + name + ",ID:" + id + ",Type:" + type.ToString() + ",ShowMode:" + mode.ToString() + ",Collider:" + collider.ToString();
	}

	public bool IsActive()
	{
		//fix,if this page is not only one gameObject
		//so,should check isActived too.
		bool ret = gameObject != null && gameObject.activeSelf;
		return ret || isActived;
	}

	#endregion

	#region static api

	//private static bool CheckIfNeedBack(UIPage page)
	//{
	//	return page != null && page.CheckIfNeedBack();
	//}

	/// <summary>
	/// 若传入页面NeedBack则将其在页面栈中置顶，
	/// 若传入页面HideOther，则进一步将其他页面隐藏
	/// </summary>
	private static void PushTop(UIPage page)
	{
		if (m_currentPageNodes == null)
		{
			m_currentPageNodes = new List<UIPage>();
		}

		if (page == null)
		{
			Debug.LogError($"UI页面[{page.name}]为空.");
			return;
		}

		// 子页面不用返回父页面（只有同级别页面需要）：
		// 如果传入的页面是固定、弹出或独立窗口类型的页面，
		// 或者页面模式为不加入UI栈模式的页面（如顶层窗口）或者DoNothing模式的页面（如通知页面），
		// 则不处理返回
		// 只处理：type=UIType.Normal && (mode==UIMode.HideOther || mode==UIMode.NeedBack)的页面

		// return type == UIType.Normal && (mode == UIMode.HideOther || mode == UIMode.NeedBack)
		if (page.BackOrHide() == false)
		{
			return;
		}

		// 如果UI页面在UI栈中任何一个位置，将其调出（从原位置移除，再放到栈顶——List的最后）
		bool _isFound = false;
		for (int i = 0; i < m_currentPageNodes.Count; i++)
		{
			if (m_currentPageNodes[i].Equals(page))
			{
				m_currentPageNodes.RemoveAt(i);
				m_currentPageNodes.Add(page);
				_isFound = true;
				break;
			}
		}

		//如果UI栈没有该元素，说明是一个新的UIPage，将其入栈
		if (!_isFound)
		{
			m_currentPageNodes.Add(page);
		}

		// 如果栈顶元素的mode == UIMode.HideOther则显示该栈顶元素的同时，将栈中其他UIPage全部隐藏
		HideOldNodes();
	}

	/// <summary>
	/// 如果栈顶元素的mode == UIMode.HideOther则显示该栈顶元素的同时，将栈中其他UIPage全部隐藏
	/// 注意：该方法的前置步骤是把要显示的页面移动到页面栈顶
	/// </summary>
	private static void HideOldNodes()
	{
		if (m_currentPageNodes.Count < 0) return;
		UIPage topPage = m_currentPageNodes[m_currentPageNodes.Count - 1];
		if (topPage.mode == UIMode.HideOther)
		{
			//form bottm to top.
			for (int i = m_currentPageNodes.Count - 2; i >= 0; i--)
			{
				if (m_currentPageNodes[i].IsActive())
					m_currentPageNodes[i].Hide();
			}
		}
	}

	private static void ClearNodes()
	{
		m_currentPageNodes.Clear();
	}

	private static void _ShowPage<T>(Action callback, object pageData, bool isAsync) where T : UIPage, new()
	{
		string pageName = typeof(T).Name;

		if (m_allPages != null && m_allPages.ContainsKey(pageName))
		{
			_ShowPage(pageName, m_allPages[pageName], callback, pageData, isAsync);
		}
		else
		{
			T instance = new T();
			_ShowPage(pageName, instance, callback, pageData, isAsync);
		}
	}

	private static void _ShowPage(string pageName, UIPage pageInstance, Action callback, object pageData, bool isAsync)
	{
		if (string.IsNullOrEmpty(pageName) || pageInstance == null)
		{
			Debug.LogError("[UI] show page error with :" + pageName + " maybe null instance.");
			return;
		}

		if (m_allPages == null)
		{
			m_allPages = new Dictionary<string, UIPage>();
		}

		UIPage page = null;
		if (m_allPages.ContainsKey(pageName))
		{
			page = m_allPages[pageName];
		}
		else
		{
			m_allPages.Add(pageName, pageInstance);
			page = pageInstance;
		}

		//if active before,wont active again.
		//if (page.isActive() == false)
		{
			//before show should set this data if need. maybe.!!
			page.m_data = pageData;

			if (isAsync)
				page._ShowAsync(callback);
			else
				page._Show();
		}
	}

	/// <summary>
	/// Sync Show Page
	/// </summary>
	[Obsolete("建议采用异步版本，并将UI资源设为Addressable")]
	public static void ShowPage<T>() where T : UIPage, new()
	{
		_ShowPage<T>(null, null, false);
	}

	/// <summary>
	/// Sync Show Page With Page Data Input.
	/// </summary>
	[Obsolete("建议采用异步版本，并将UI资源设为Addressable")]
	public static void ShowPage<T>(object pageData) where T : UIPage, new()
	{
		_ShowPage<T>(null, pageData, false);
	}

	[Obsolete("建议采用异步版本，并将UI资源设为Addressable")]
	public static void ShowPage(string pageName, UIPage pageInstance)
	{
		_ShowPage(pageName, pageInstance, null, null, false);
	}

	[Obsolete("建议采用异步版本，并将UI资源设为Addressable")]
	public static void ShowPage(string pageName, UIPage pageInstance, object pageData)
	{
		_ShowPage(pageName, pageInstance, null, pageData, false);
	}

	/// <summary>
	/// Async Show Page with Async loader bind in 'UIBind.Bind()'
	/// </summary>
	public static void ShowPageAsync<T>(Action callback = null) where T : UIPage, new()
	{
		_ShowPage<T>(callback, null, true);
	}

	public static void ShowPageAsync<T>(object pageData, Action callback = null) where T : UIPage, new()
	{
		_ShowPage<T>(callback, pageData, true);
	}

	/// <summary>
	/// Async Show Page with Async loader bind in 'UIBind.Bind()'
	/// </summary>
	public static void ShowPageAsync(string pageName, UIPage pageInstance, Action callback = null)
	{
		_ShowPage(pageName, pageInstance, callback, null, true);
	}

	public static void ShowPageAsync(string pageName, UIPage pageInstance, object pageData, Action callback = null)
	{
		_ShowPage(pageName, pageInstance, callback, pageData, true);
	}

	/// <summary>
	/// 关闭页面栈的顶层页面（可能是当前类对象所代表的页面，也可能不是）<para/>
	/// 注意：关闭页面时虽然从页面栈移除了页面对象，但其仍位于页面字典（m_allPages）中，在用于调用ShowPage方法时可以立刻从缓存取出
	/// </summary>
	public static void ClosePage()
	{
		//Debug.Log("Back&Close PageNodes Count:" + m_currentPageNodes.Count);

		if (m_currentPageNodes == null || m_currentPageNodes.Count < 1) return;

		UIPage closePage = m_currentPageNodes[m_currentPageNodes.Count - 1];
		m_currentPageNodes.RemoveAt(m_currentPageNodes.Count - 1);
		closePage.Hide();

		//show older page.
		//TODO:Sub pages.belong to root node.
		if (m_currentPageNodes.Count > 0)
		{
			UIPage page = m_currentPageNodes[m_currentPageNodes.Count - 1];
			ShowPage(page.name, page, page.isAsyncUI);
		}
	}

	/// <summary>
	/// Close target page
	/// </summary>
	public static void ClosePage(UIPage target, bool showPrevPage = true)
	{
		if (target == null) return;

		// 如果页面已经通过Hide()隐藏了，将其从页面栈移除即可返回
		if (target.IsActive() == false)
		{
			if (m_currentPageNodes != null)
			{
				for (int i = 0; i < m_currentPageNodes.Count; i++)
				{
					if (m_currentPageNodes[i] == target)
					{
						m_currentPageNodes.RemoveAt(i);
						break;
					}
				}
				return;
			}
		}

		// 如果当前页面是正在显示的栈顶页面，则将其从页面栈移除，并显示移除后的栈顶页面
		if (m_currentPageNodes != null && m_currentPageNodes.Count >= 1 && m_currentPageNodes[m_currentPageNodes.Count - 1] == target)
		{
			m_currentPageNodes.RemoveAt(m_currentPageNodes.Count - 1);

			//show older page.
			//TODO:Sub pages.belong to root node.
			if (m_currentPageNodes.Count > 0)
			{
				UIPage page = m_currentPageNodes[m_currentPageNodes.Count - 1];
				if (page.isAsyncUI)
				{
					if (showPrevPage)
					{
						ShowPageAsync(page.name, page, () =>
						{
							target.Hide();
						});
					}
					else
					{
						target.Hide();
					}
				}
				else
				{
					if (showPrevPage)
					{
						ShowPage(page.name, page);
					}

					target.Hide();
				}

				return;
			}
		}
		// 如果目标页面不是栈顶页面，且目标页面设置了返回或独占显示，则将其从页面栈移除并隐藏显示
		else if (target.BackOrHide())
		{
			for (int i = 0; i < m_currentPageNodes.Count; i++)
			{
				if (m_currentPageNodes[i] == target)
				{
					m_currentPageNodes.RemoveAt(i);
					break;
				}
			}
		}

		target.Hide();
	}

	public static void ClosePage<T>() where T : UIPage
	{
		string pageName = typeof(T).Name;

		ClosePage(pageName);
	}

	public static void ClosePage(string pageName)
	{
		if (m_allPages != null && m_allPages.ContainsKey(pageName))
		{
			ClosePage(m_allPages[pageName]);
		}
		else
		{
			Debug.LogError($"页面[{pageName}]尚未显示，无法关闭！");
		}
	}

	/// <summary>
	/// 关闭页面栈中所有页面
	/// </summary>
	public static void CloseAllPages(UIType uiType = UIType.Any)
	{
		for (int i = m_allPages.Count - 1; i >= 0; i--)
		{
			var page = m_allPages.ElementAt(i);
			if (uiType == UIType.Any
				|| uiType == page.Value.type)
			{
				UIPage.ClosePage(page.Value, false);
			}
		}
	}

	public static TPage GetPage<TPage>()
		where TPage : UIPage
	{
		m_allPages.TryGetValue(typeof(TPage).Name, out UIPage page);
		return page as TPage;
	}
	#endregion

}//TTUIPage
