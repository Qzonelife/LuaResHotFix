using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using LuaFramework;

public class VersionManager:MonoBehaviour
{
    private Thread thread;
    public Action versionCompleteCallBack;
    private List<string> downLs = new List<string>();
    private string currDownFile = string.Empty;
    private string currentDataPath = ""; //由于线程内不允许调用unity的功能，在外部先将此赋值
    delegate void ThreadSyncEvent(VFStruct data);
    private ThreadSyncEvent m_SyncEvent;
    private Action<VFStruct> func;
    public VFStruct currentDownVf;
    private long totalSize = 0; //需要下载总大小，字节大小
    private long currentSize = 0; //当前下载大小，字节大小
    private VersionVo remoteVersionVo;
    void Awake()
    {

        currentDataPath = Util.DataPath;
        m_SyncEvent = OnSyncEvent;
        thread = new Thread(OnUpdate);
    }
   
    /// <summary>
    /// 通知事件
    /// </summary>
    /// <param name="state"></param>
    private void OnSyncEvent(VFStruct data)
    {
        if (this.func != null) func(data);  //回调逻辑层
    }
    void Start()
    {
        thread.Start();
        CheckVersion(null);
    }
    /// <summary>
    /// 添加到事件队列
    /// </summary>
    public void AddEvent(VFStruct target, Action<VFStruct> func)
    {
        lock (m_lockObject)
        {
            this.func = func;
            events.Enqueue(target);
        }
    }
    static readonly object m_lockObject = new object();
    static Queue<VFStruct> events = new Queue<VFStruct>();
    void OnUpdate()
    {
        while (true)
        {
            lock (m_lockObject)
            {
                if (events.Count > 0)
                {
                    VFStruct e = events.Dequeue();
                    try
                    {
                        OnDownloadFile(e);
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError(ex.Message);
                    }
                }
            }
            Thread.Sleep(1);
        }

    }
    /// <summary>
    /// 下载文件
    /// </summary>
    void OnDownloadFile(VFStruct vf)
    {
        string url = AppConst.RemoteUpdatePath + vf.resId + "/" + vf.file;
        currDownFile = currentDataPath + "/" + vf.file;
        currentDownVf = vf;
        using (WebClient client = new WebClient())
        {
           // client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            client.DownloadFileAsync(new System.Uri(url), currDownFile);
            client.DownloadFileCompleted += FileDownComplete;
           
        }
    }

    private void FileDownComplete(object sender, AsyncCompletedEventArgs e)
    {
        m_SyncEvent(currentDownVf);
    }

    //    private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        //    {
        //        //UnityEngine.Debug.Log(e.ProgressPercentage);
        //        /*
        //        UnityEngine.Debug.Log(string.Format("{0} MB's / {1} MB's",
        //            (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
        //            (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00")));
        //        */
        //        //float value = (float)e.ProgressPercentage / 100f;
        //
        //        if (e.ProgressPercentage == 100 && e.BytesReceived == e.TotalBytesToReceive)
        //        {
        //            NotiData data = new NotiData(NotiConst.UPDATE_DOWNLOAD, currDownFile);
        //            if (m_SyncEvent != null) m_SyncEvent(data);
        //        }
        //    }










        /// <summary>
        /// 检测版本更新
        /// </summary>
        /// <param name="completeCallBack"></param>
        public void CheckVersion(Action completeCallBack)
    {
        downLs.Clear();
        totalSize = 0;
        currentSize = 0;
        remoteVersionVo = null; //清空当前缓存的远端
        versionCompleteCallBack = completeCallBack;
        StartCoroutine(CheckRemoteXML());
    }


    /// <summary>
    /// 对比检测远程的xml信息
    /// </summary>
    IEnumerator  CheckRemoteXML()
    {
        WWW apkInfo = new WWW(AppConst.RemoteApkInfo);
        yield return apkInfo;
        if (apkInfo.isDone)
        {
            if (string.IsNullOrEmpty(apkInfo.error)) //没有错误
            {
                XmlDocument apkXml = new XmlDocument();
                apkXml.LoadXml(apkInfo.text);
                XmlNode verNode = apkXml.SelectSingleNode("verInfo");
                string remoteVersion = verNode.Attributes["curVersion"].Value;
                int remoteResId = int.Parse(verNode.Attributes["curResId"].Value);
                CompareVersion(remoteVersion,remoteResId);
            }
            else
            {
                //此处校验文件失败，需要重新检查
                Debug.LogError("down apkInfo error:"+apkInfo.error);
            }
        }
    }
    /// <summary>
    /// 获取当前本地的版本信息
    /// </summary>
    /// <returns></returns>
    public VersionVo GetCurrentVo()
    {
        string currentVersionPath = "";

#if UNITY_EDITOR_WIN
        currentVersionPath = Util.DataPath + "/versionInfo.xml";

#elif UNITY_ANDROID
        currentVersionPath = Application.persistentDataPath+"/versionInfo.xml";
#endif
        VersionVo currentVo = new VersionVo(currentVersionPath);
        return currentVo;
    }

    public void CompareVersion(string remoteVersion,int resId)
    {

        VersionVo currentVo = GetCurrentVo();
        if (currentVo.isVersionNeedUpdate(remoteVersion))
        {
            Debug.Log("需要进行大版本更新");
        }
        else if (currentVo.isResVersionNeedUpdate(resId.ToString()))
        {
            Debug.Log("资源需要进行差异更新");
            StartCoroutine(StartDownDiffList(remoteVersion, resId));
        }
        else
        {
            Debug.Log("无更新内容");
            if (versionCompleteCallBack != null)
            {
                versionCompleteCallBack();
            }
        }
    }


    /// <summary>
    /// 开始进行差异更新,下载当前最新版本的差异文件
    /// </summary>
    IEnumerator  StartDownDiffList(string version,int resId)
    {
        WWW remoteVersion = new WWW(AppConst.RemoteUpdatePath+resId+"/versionInfo.xml");
        yield return remoteVersion;
        if (remoteVersion.isDone)
        {
            if (string.IsNullOrEmpty(remoteVersion.error))
            {
                remoteVersionVo = new VersionVo(remoteVersion.text,true);
                VersionVo curretnVo = GetCurrentVo();
                remoteVersionVo.InitDifferDict(); //初始化所有版本的更新内容
                Dictionary<string, VFStruct> updateDict = remoteVersionVo.GetDiffFromBeginVer(curretnVo.ResVersion);
                List<VFStruct> vfList = new List<VFStruct>();
                foreach (KeyValuePair<string,VFStruct> dict in updateDict)
                {
                    vfList.Add(dict.Value);
                }
                totalSize = remoteVersionVo.CalculateVFLSize(vfList);
                List<string> downedLs = GetDownedList(); //获取已经下载了的文件列表
                //开始下载文件
                for (int i = 0; i < vfList.Count; i++)
                {
                    if (downedLs.Contains(vfList[i].file))
                    {
                        Debug.Log("文件已下载："+vfList[i].file);
                        DownFileFinish(vfList[i]);
                        continue;
                    }
                    else
                    {
                        StartDownFile(vfList[i]);
                        while (!IsDownFinish(vfList[i].file))
                        {
                            OnDownFinisCallByWWW(vfList[i]);
                            yield return new WaitForEndOfFrame();
                        }
                    }
                }
                ResDownFinish(true);
            }
            else
            {
                Debug.LogError("下载更新列表失败："+remoteVersion.error);
                ResDownFinish(false);
            }
        }
    }

    public bool IsDownFinish(string file)
    {
        return downLs.Contains(file);
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="vf"></param>
    public void StartDownFile(VFStruct vf)
    {
      AddEvent(vf,OnDownOneFileComplete);
    }

    /// <summary>
    /// 文件下载完成,网络下载事件的回调，将文件添加到缓存列表，下载完成再删除缓存列表
    /// </summary>
    /// <param name="vf"></param>
    public void OnDownOneFileComplete(VFStruct vf)
    {
        downLs.Add(vf.file);
    }

    /// <summary>
    /// 协程判断下载完成的回调，主线程不能判断事件，所以从子线程进行判断
    /// </summary>
    /// <param name="vf"></param>
    public void OnDownFinisCallByWWW(VFStruct vf)
    {
        downLs.Add(vf.file);
        SaveToTemp(vf.file);
        DownFileFinish(vf);
    }

    /// <summary>
    /// 一个文件下载完成，更新通知当前文件下载完毕，包括更新下载进度等
    /// </summary>
    /// <param name="vf"></param>
    public void DownFileFinish(VFStruct vf)
    {
        string tts = NumUtil.GetFormatFileSize(totalSize);
        currentSize += vf.size;
        string crs = NumUtil.GetFormatFileSize(currentSize);
        Debug.Log("下载中：" + crs + "/" + tts);
    }
    /// <summary>
    /// 资源更新结束
    /// </summary>
    public void ResDownFinish(bool isSucc)
    {
        if (isSucc) //资源下载成功，更新本地版本信息
        {
            Debug.Log("==============下载成功==================");
            //将版本差异文件信息写入本地
            OnUpdateFinish(isSucc);
        }
    }

    /// <summary>
    /// 更新结束，更新本地版本号以及资源内容
    /// </summary>
    /// <param name="isSucc"></param>
    public void OnUpdateFinish(bool isSucc)
    {

        ClearDownTmp();//清除下载缓存记录
        remoteVersionVo.WriteVersionInfo(Util.DataPath+"versionInfo.xml");

        if (versionCompleteCallBack != null)
        {
            versionCompleteCallBack();
        }
    }
    
    public void SaveToTemp(string file)
    {
        if (File.Exists(downRecordFile))
        {
            File.AppendAllText(downRecordFile, "\n"+file);
        }
        else
        {
            File.AppendAllText(downRecordFile, file);
        }
    }

    public void ClearDownTmp()
    {
        if (File.Exists(downRecordFile))
        {
            File.Delete(downRecordFile);
        }
    }
    /// <summary>
    /// 获取已经下载了的文件列表
    /// </summary>
    public List<string> GetDownedList()
    {
        List<string> strLs = new List<string>();
        if (File.Exists(downRecordFile))
        {
            string st = File.ReadAllText(downRecordFile);
            st.Replace("\r", "");
            string[] str = st.Split('\n');
            for (int i = 0; i < str.Length; i++)
            {
                strLs.Add(str[i]);
            }
        }
        return strLs;
    }

    private string downRecordFile 
    {
        get
        {
            return Util.DataPath + remoteVersionVo.CurrentVersion + "_" + remoteVersionVo.ResVersion + "_downTmp.txt";
        }
       
    }

    void OnDestroy()
    {
        if (thread != null)
        {
            thread.Abort();
        }
    }



}
