using Lockstep.Math;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class MyEntity 
{
    public MyView viewBase = null;
    protected MyEntity() { }//禁止直接new对象

    private static uint id_gen = 0;//id生成器，每生成一次 +1

    private static uint tmp_id_gen = 0x80000000;//临时id生成器，用于与长时间存在的对象做个区分，列如：预览对象
    /// <summary>
    /// 实体id号，用于区分和查找特定实体
    /// </summary>
    public uint eid
    {
        get; private set;
    }

    public string ename
    {
        get; private set;
    }
    /// <summary>
    /// 实例化以对象。需要提供实体类型和主视图类型
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TMainView">主要视图类型</typeparam>
    /// <param name="entPrefab">实体预制体，会从该预制体克隆一个实列，防止直接修改实体的模型成数据</param>
    /// <param name="viewPrefabName">视图预制体名称，会根据预制体资源名实列一个对象视图，他是一个GameObject，会挂上我们指定的TMianView类型的组件，从而跟Unity表现层组件交互</param>
    /// <param name="viewParent">对象视图的父节点</param>
    /// <param name="OnViewCreated">视图创建完毕的回调</param>
    /// <param name="tmp">是否临时</param>
    /// <returns></returns>
    public static async Task<TEntity> Instanciate<TEntity,TMainView>(
        TEntity entPrefab=null,//数据
        string viewPrefabName=null,//预制体名
        Transform viewParent=null,
        Action<TEntity> OnViewCreated=null,
        bool tmp =false,
        bool DontDestroyOnLoad = false
        )
        where TEntity :MyEntity, new()
        where TMainView : MyView,new()
    {
        //实例化对象视图
        GameObject unit = null;
        if (viewPrefabName!=null)
        {
            unit = await Addressables.InstantiateAsync(viewPrefabName, viewParent, false).Task;
        }
        else
        {
            //如果对象视图预制体为空，创建一个空对象
            unit = new GameObject();
        }

        //挂上主视图组件
        TMainView view = new TMainView();

        if (DontDestroyOnLoad)
        {
            GameObject.DontDestroyOnLoad(unit);
        }
       
        //if (view==null)
        //{
        //    view = unit.AddComponent<TMainView>();
        //}

        #region 双向关联
        MyEntity ent = entPrefab != null ? entPrefab.DeepClone() : new TEntity();//深拷贝创建实体副本（）
        view.dataBase = ent;//关联
        ent.viewBase = view;//关联
        ent.eid = tmp ? tmp_id_gen++ : id_gen++;//为实体生成id号，临时id为临时，普通id为普通，
        view.transform = unit.transform;//（因为实体不从MonoBehaviour继承,但是他要使用物体上的组件所以在这里赋值）
        view.gameObject.name = ent.ename = $"[{view.dataBase.eid}]{viewPrefabName ?? typeof(TEntity).Name}";//给对象实体和实体起名，便于调试观看
        #endregion

        #region 设置数据
        OnViewCreated?.Invoke(view.dataBase.As<TEntity>());//回调创建成功函数
        view.dataBase.UpdateWorldPos();//更新位置
        view.dataBase.UpdateRotation();//跟新旋转
        #endregion

        await ent.OnAwake();//模拟Unity中GameObject的Awake

        Debug.Log($"#FDATA# eid={ent.eid} Instamciate {ent.ename} @ {ent.worldPos}");

        return ent.As<TEntity>();//返回克隆出来的实体
    }

    /// <summary>
    /// 销毁一个对象，对象实体和主视图都回被销毁
    /// </summary>
    /// <param name="e">实体</param>
    /// <param name="delay">延时销毁时间</param>
    /// <returns></returns>
    public static async Task Destroy(MyEntity e,LFloat delay)
    {
        //设定对象
        //
        if (delay>LFloat.zero)
        {
            await new WaitForSeconds(delay.ToFloat());
        }

        if (!Addressables.ReleaseInstance(e.viewBase.gameObject))
        {
            Debug.LogError("销毁失败");
        }       
        e.OnDestroy();
    }
    /// <summary>
    /// 销毁一个对象,无延迟
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static async void Destroy(MyEntity e)
    {
        await Destroy(e, LFloat.zero);
    }

    public virtual Task OnAwake()
    {
        return Task.CompletedTask;
    }

    public abstract void OnDestroy();
    
    public virtual void OnRestart()
    {

    }


    #region 位置，旋转和插值
    /// <summary>
    /// 上次逻辑更新的时间(每次更新WorldPos都会 lastWorldPos=_WorldPos 而lastWorldPos被更新就会 lastUpdateTime = Avatar.Player.clntAccFrameTime)
    /// </summary>
    private LFloat lastUpdateTime;//上次更新lastWorldPos到现在的时间
    /// <summary>
    /// 插值系数=（客户端从启动到现在的时间-上次逻辑更新的时间）/每帧的间隔时间，就可以计算出插值进度
    /// </summary>
    public LFloat lerpT => (Avatar.Player.clntAccFrameTime - lastUpdateTime) / Avatar.Player.fixedDeltaTime;

    private LVector3 _worldPos = LVector3.zero;//当前位置
    private LVector3 _lastWorldPos = LVector3.zero;//上一逻辑位置

    public LVector3 worldPos
    {
        get { return _worldPos; }
        set
        {
            //把当前位置更新为上一逻辑帧位置，同时把新位置更新为当前位置
            //插值时，回根据lastWorldPos/_worldPos/ lerpT进行插值
            lastWorldPos = _worldPos;
            _worldPos = value;
        }
    }

    public LVector3 lastWorldPos
    {
        get { return _lastWorldPos; }
        set
        {
            _lastWorldPos = value;
            //将上一逻辑帧更新时间设置为客户端从启动到现在的时间
            //子啊计算插值系数时回用到
            lastUpdateTime = Avatar.Player.clntAccFrameTime;
        }
    }
    /// <summary>
    /// 如果希望直接把对象“传送”到某个位置，则之间调用此方法
    /// 原理时：当把 lastWorldPos与_worldPos等同时，对象视图就不会插值
    /// </summary>
    public void UpdateWorldPos()
    {
        if (Avatar.Player == null)
        {
            return;
        }

        lastWorldPos = _worldPos;
        viewBase?.SendMessage("OnSetWorldPos", _worldPos);
    }

    private LQuaternion _rot = LQuaternion.identity;
    private LQuaternion _lastRot = LQuaternion.identity;

    public LQuaternion rot
    {
        get { return _rot; }
        set
        {
            _lastRot = _rot;
            _rot = value;
        }
    }

    public  LQuaternion lastRot
    {
        get { return _lastRot; }
    }

    public void UpdateRotation()
    {
        if (Avatar.Player==null)
        {
            return;
        }
        _lastRot = _rot;
        viewBase?.SendMessage("OnSetRotation", _rot);
    }

    #endregion
}