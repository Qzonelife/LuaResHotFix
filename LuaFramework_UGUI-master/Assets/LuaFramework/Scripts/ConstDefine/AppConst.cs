using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LuaFramework {
    public class AppConst
    {
        public const bool DebugMode = false; //调试模式-用于内部测试

        /// <summary>
        /// 如果想删掉框架自带的例子，那这个例子模式必须要
        /// 关闭，否则会出现一些错误。
        /// </summary>
        public const bool ExampleMode = true; //例子模式 

        /// <summary>
        /// 如果开启更新模式，前提必须启动框架自带服务器端。
        /// 否则就需要自己将StreamingAssets里面的所有内容
        /// 复制到自己的Webserver上面，并修改下面的WebUrl。
        /// </summary>
        public const bool UpdateMode = true; //更新模式-默认关闭 

        public const bool LuaByteMode = false; //Lua字节码模式-默认关闭 
        public const bool LuaBundleMode = true; //Lua代码AssetBundle模式

        public const int TimerInterval = 1;
        public const int GameFrameRate = 30; //游戏帧频

        public const string AppName = "LuaFramework"; //应用程序名称
        public const string LuaTempDir = "Lua/"; //临时目录
        public const string AppPrefix = AppName + "_"; //应用程序前缀
        public const string ExtName = ".unity3d"; //素材扩展名
        public const string AssetDir = "StreamingAssets"; //素材目录 
        public const string WebUrl = "http://192.168.10.218//"; //测试更新地址

        public const string versionBase = "1000"; //基础版本号


        public static string UserId = string.Empty; //用户ID
        public static int SocketPort = 0; //Socket服务器端口
        public static string SocketAddress = string.Empty; //Socket服务器地址

        public static string FrameworkRoot
        {
            get { return Application.dataPath + "/" + AppName; }
        }

        /// <summary>
        /// 外部lua读取地址，需要手动更新，测试从服务器下载更新资源
        /// </summary>
        public static string LuaOutPath
        {
            get
            {
                string str = Application.dataPath;
                str = str.Substring(0, str.LastIndexOf('/'));
                str = str + "/ExtFolder/";
                return  str;
                
            }
        }
        /// <summary>
        /// 外部lua生成地址，生成lua的地址，最新的lua资源在这个路径生成
        /// </summary>
        /// <returns></returns>
        public static string LuaGenPath
        {
            get
                {
                string str = Application.dataPath;
                str = str.Substring(0, str.LastIndexOf('/'));
                str = str + "/ExtFolder/luaGen/";
                return str;

            }
        }

        /// <summary>
        /// 版本文件存放目录
        /// </summary>
        /// <returns></returns>
        public static string VersionPath
        {
            get
            {
                string str = Application.dataPath;
                str = str.Substring(0, str.LastIndexOf('/'));
                str = str + "/Version";
                return str;

            }
        }



    }
}