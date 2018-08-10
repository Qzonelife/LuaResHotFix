using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Xml;

public class VersionVo {

    XmlDocument xdoc = new XmlDocument();
    public VersionVo(string path)
    {
        xdoc.Load(path);

        XmlNode node = xdoc.SelectSingleNode("version");
        currentVersion = node.Attributes["currentVersion"].Value;
        resVersion = node.Attributes["resVersion"].Value;
        
        Debug.Log(currentVersion);
        Debug.Log(resVersion);
    }

    //a.b.c ,三位比较方式从高中低
    public string currentVersion;
    //abcd ,四位
    public string resVersion;


    /// <summary>
    /// 是否需要进行大版本更新
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public  bool isVersionNeedUpdate(string version)
    {
        string[] mvs = this.currentVersion.Split('.');
        string[] tarVs = version.Split('.');
        if (mvs.Length == tarVs.Length)
        {
            for (int i = 0; i < mvs.Length; i++)
            {
                if (mvs[i] == tarVs[i])
                {
                    continue;
                }
                return int.Parse(mvs[i]) < int.Parse(tarVs[i]);
            }
        }
        
        return false;
    }
    /// <summary>
    /// 是否存在资源更新
    /// </summary>
    /// <param name="resVersion"></param>
    /// <returns></returns>
    public bool isResVersionNeedUpdate(string resVersion)
    {
        
        return int.Parse(this.resVersion)<int.Parse(resVersion);
    }

}
