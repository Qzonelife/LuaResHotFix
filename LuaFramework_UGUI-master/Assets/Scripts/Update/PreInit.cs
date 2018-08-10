using UnityEngine;
using System.Collections;


/// <summary>
/// 游戏加载助手，
/// </summary>
public class PreInit  {

    static void Log(string str)
    {
        Debug.Log("==================================="+str+ "===================================");
    }
    /// <summary>
    /// 游戏初始化开始
    /// </summary>
    public static void GameStartInit()
    {
        Log("开始初始化游戏");
    }
    /// <summary>
    /// 释放streamingAssets下的资源
    /// </summary>
    public static void CheckExtRes()
    {
        Log("检查释放资源");
    }
    /// <summary>
    /// 释放lua资源
    /// </summary>
    public static void ReleaseLuaRes()
    {
        Log("释放lua资源");
    }
    /// <summary>
    /// 释放游戏资源
    /// </summary>
    public static void ReleaseGameRes()
    {
        Log("释放游戏资源");
    }

    /// <summary>
    /// 资源释放完毕
    /// </summary>
    public static void ExtResFinish()
    {
        Log("资源释放完毕");
    }
    /// <summary>
    /// 开始更新下载游戏资源
    /// </summary>
    public static void StartUpdateGameRes()
    {
        Log("开始更新游戏资源");
    }
    /// <summary>
    /// 更新游戏资源完毕
    /// </summary>
    public static void UpdateGameResFinish(){

        Log("游戏资更新完毕");
    }

    /// <summary>
    /// 开始更新下载lua资源
    /// </summary>
    public static void StartUpdateLuaRes()
    {
        Log("开始更新lua资源");
    }
    /// <summary>
    /// 资源下载完毕
    /// </summary>
    public static void UpdateLuaResFinish()
    {
        Log("Lua资源更新完毕");
    }
    /// <summary>
    /// 开始加载资源
    /// </summary>
    public static void StartLoadRes()
    {
        Log("开始加载资源");
    }
    /// <summary>
    /// 资源加载完毕
    /// </summary>
    public static void LoadResFinish()
    {
        Log("资源加载完毕");
    }

    /// <summary>
    /// 开始加载lua文件
    /// </summary>
    public static void StartLoadLua()
    {
        Log("开始加载lua文件");
    }

    /// <summary>
    /// 游戏加载lua结束
    /// </summary>
    public static void InitLuaFinish()
    {
        Log("lua文件加载完毕");
    }
    /// <summary>
    /// 处理消息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="body"></param>
    public static void OnMessageInfo(string type,object body)
    {
        Log("OnMessage");
        Log(body.ToString());
    }

}
