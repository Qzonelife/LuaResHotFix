
using UnityEngine;
using System.Collections;

namespace LuaFramework {

    /// <summary>
    /// </summary>
    public class Main : MonoBehaviour {

        void Start() {
            PreInit.GameStartInit();
            AppFacade.Instance.StartUp();   //启动游戏
        }
    }
}