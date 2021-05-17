using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KBEngine;
using Lockstep.Math;
using UnityEngine.TextCore;
using static UnityRoyale.Placeable;
using System;

public enum GameState
{
    GAME_PREPARE,
    GAME_START,
    GAME_OVER,
}

public class Avatar : AvatarBase
{
    public GameState gameState;
    public int frameId;
    public List<FRAME_SYNC> frames = new List<FRAME_SYNC>();
    public uint seed; //服务器下发的随机数种子
    public int consumedFrameCount = 0;//已经处理（消费）了的帧数
    public byte frameRate;//逻辑帧率
    public LFloat fixedDeltaTime;//逻辑帧间隔时间，按秒（比如200ms）
    public int roundFrames;//一局游戏的总帧数

    public int clntIdx = -1;//客户端编号，客户端都有一个服务器分配给的编号

    //我方阵营
    public Faction MyFaction
    {
        get
        {
            if (clntIdx == 0)
                return Faction.Red;
            else if (clntIdx==1)
                return Faction.Blue;
            else
                return Faction.None;
        }
    }

    //敌方阵营
    public Faction HisFaction
    {
        get
        {
            if (clntIdx == 0)
                return Faction.Blue;
            else if (clntIdx == 1)
                return Faction.Red;
            else
                return Faction.None;
        }
    }

    public bool isFastForward;//是否快进（追帧）
    public int clntFrameId;//客户端模拟帧号，（同步到第几帧了）
    /// <summary>
    /// 从客户端开局总共运行了多少时间。按秒(在渲染帧累加)，（）
    /// </summary>
    public LFloat clntAccFrameTime;//客户端从启动到现在总共运行了多少时间。按秒

    public LFloat lerpT //插值系数 
    {
        get
        {
            LFloat f = (clntAccFrameTime - clntFrameTime) / fixedDeltaTime;
            return f;
        }
    }

    public readonly object frameLocker = new object();

    //服务器发过来的帧号算出来的帧时间（服务器帧和客户端模拟帧会有帧数差）
    public LFloat frameTime
    {
        get
        {
            return Avatar.Player.frameId * Avatar.Player.fixedDeltaTime;
        }
    }
    
    //按照运行的帧数和每帧时间算出来的当前帧的起始时间（上一帧的结束时间）
    public LFloat clntFrameTime
    {
        get
        {
            return Avatar.Player.clntFrameId * Avatar.Player.fixedDeltaTime;
        }
    }

    //客户端模拟的本帧结束时间（下帧开始时间）
    public LFloat clntNextFrameTime
    {
        get
        {
            return (Avatar.Player.clntFrameId + 1) * Avatar.Player.fixedDeltaTime;
        }
    }
    
    //表示本地玩家实体
    /// <summary>
    /// 一个快捷属性，方便取得玩家对象
    /// </summary>
    public static Avatar Player
    {
        get
        {
            if (KBEngineApp.app==null )
            {
                return null;
            }
            return KBEngineApp.app.player() as Avatar;
        }
    }
    public override void __init__()
    {
        base.__init__();

        if (isPlayer())
        {
            KBEngine.Event.registerIn("EnterRoom",this, "EnterRoom");
            KBEngine.Event.registerIn("PlaceCard", this, "PlaceCard");
        }

        gameState=GameState.GAME_PREPARE;
        frameId = 0;
    }

    #region Events In
    public void EnterRoom(bool isRestart)
    {
        Debug.Log($"#Avatar# EnterRoom()");
        //条用base暴露给客户端的EnterRoom()（发送匹配信息）
        baseEntityCall.EnterRoom(Convert.ToByte(isRestart));
    }

    public void LeaveRoom()
    {
        Debug.Log($"#Avatar# LeaveRoom()");
        baseEntityCall.LeaveRoom();
    }

    public void PlaceCard(CMD cMD)
    {
        if (cellEntityCall !=null)
        {
            Debug.Log($"#Avatar# <color=red>PlaceCard({cMD.ToStringEx()})</color>");
            //（发送出牌信息）
            cellEntityCall.PlaceCard(cMD);
        } 
    }
    #endregion

    #region ClientMethods
    public override void OnFrameSync(int frameid , FRAME_SYNC fs)
    {
        Debug.Log($"#Avatar# onFrameSync({frameid},{fs})");
        //生成帧
        this.frameId = frameid;

        frames.Add(fs);

        Debug.Log($"#Avatar# END onFrameSync()");

    }

    public override void OnGameOver()
    {
        Debug.Log($"#Avatar# 房间帧已经跑完");
        DoGameOver(Faction.None);
        frames.Clear();

    }

    public void DoGameOver( Faction winner)
    {
        Debug.Log($"Avatar  DoGameOver({winner})" );

        gameState = GameState.GAME_OVER;

        KBEngine.Event.fireOut("OnGameOver",winner);
        UIPage.ShowPageAsync<GameOverPage>(winner);

        //停止更新（节省性能）
        //MonoBridge.instance.OnUpdate = null;
        //MonoBridge.instance.OnIMGUI = null;
    }

    public override void OnGameReady(uint seed, byte frameRate, int idx,FRAME_LIST frameList,int roundFrames)
    {
        Debug.Log($"#Avatar# OnGameReady({seed })");
        //保存服务器发过来的战斗初始化数据
        gameState = GameState.GAME_START;
        frameId = 0;
        this.seed = seed;
        this.frameRate = frameRate;
        this.fixedDeltaTime = LFloat.one / (uint)frameRate;//计算服务器帧间隔频率
        Time.fixedDeltaTime = this.fixedDeltaTime.ToFloat();//没用到
        this.clntIdx = idx;//客户端编号（服务器分配）
        this.roundFrames = roundFrames;//

        //初始化客户端only的参数
        this.clntFrameId = 0;
        this.clntAccFrameTime = 0;
        this.isFastForward = false;
        this.consumedFrameCount = 0;//roundFrames(总帧数)==consumedFrameCount（消费的帧数）：则游戏平局

        //通知游戏开始了
        KBEngine.Event.fireOut("OnGameReady",seed,frameRate,idx); 

        //补帧
        //断线从联的时候，服务器会发送OnGameReady ，并把帧列表发过来，把帧列表放到要消费的帧列表里就能追帧
        Debug.Log("#Avatar# 补帧开始--->>>");
        for (int i = 0; i < frameList.frames.Count; i++)
        {
            var frame = frameList.frames[i];
            OnFrameSync(i, frame);
        }
        Debug.Log("#Avatar# 补帧结束<<<----");
    }


    #endregion
}
