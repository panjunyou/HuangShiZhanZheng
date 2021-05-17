using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
