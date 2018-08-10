using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using LuaFramework;
using UnityEditor;
using Object = UnityEngine.Object;

public class PackUtil : Editor {

    [MenuItem("Zone/设置assetbundleName")]
    public static void SetAllAbName()
    {
        string tarFolder = "Assets/Resources";

        string[] files = Directory.GetFiles(tarFolder, "*.prefab", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string rp = FormatPath(file);
            Object obj = AssetDatabase.LoadMainAssetAtPath(rp);
            SetAssetName(rp);
            //设置引用打包
            if (obj != null)
            {
                Object[] depends = EditorUtility.CollectDependencies(new Object[] {obj});

                foreach (var depend in depends)
                {
                    string tarPath = AssetDatabase.GetAssetPath(depend);
                    if (tarPath != rp)
                    {
                        if (tarPath.IndexOf(".prefab") != -1 || tarPath.IndexOf(".TTF") != -1)
                        {
                            SetAssetName(tarPath);
                        }
                    }
                }
            }
        }

    }

    static string FormatPath(string s)
    {
        return s.Replace("\\", "/");
    }

    static void SetAssetName(string file)
    {
        file = FormatPath(file);
        if (NeedPack(file))
        {
            SetBundleName(file, file);
        }
        
    }


    /// <summary>
    /// 设置bundle名字的方法
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bundleName"></param>
    /// <param name="variant"></param>
    static void SetBundleName(string path,string bundleName,string variant = "")
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            Debug.Log("Res not found:"+path);
            return;
        }
        //打包场景文件
        if (!string.IsNullOrEmpty(path) && path.Length > 6 && path.Substring(path.Length - 6) == ".unity")
        {
            string sceneName = Path.GetFileNameWithoutExtension(path);
            bundleName = sceneName+".unity3d";
            assetImporter.assetBundleName = bundleName;
            return;
        }

        int len = @"Assets/".Length;
        if (path.Length < len || path.Substring(0, len) != "Assets/")
        {
            Debug.Log("Create ab faild,error path");
            return;
        }
        if (bundleName.Length >= len && bundleName.Substring(0, len) == "Assets/")
        {
            bundleName = bundleName.Substring(len);
        }
        assetImporter.assetBundleName = bundleName;
        if (variant != "")
        {
            assetImporter.assetBundleVariant = variant;
        }
    }



    [MenuItem("Zone/清空所有assetbundleName")]
    static void ClearAllAbName()
    {
        int length = AssetDatabase.GetAllAssetBundleNames().Length;
        string[] allAbName = new string[length];
        for (int i = 0; i < length; i++)
        {
            allAbName[i] = AssetDatabase.GetAllAssetBundleNames()[i];
        }

        for (int j = 0; j < allAbName.Length; j++)
        {
            AssetDatabase.RemoveAssetBundleName(allAbName[j], true);
        }
    }

   
    static bool NeedPack(string file)
    {
        return true;
    }


    [MenuItem("Zone/打包所有assetbundle")]
    public static void PackAb()
    {
        string tarDir = "Assets/StreamingAssets/" + "Android";
        if (Directory.Exists(tarDir))
        {
            Directory.Delete(tarDir);
        }
        Directory.CreateDirectory(tarDir);
        BuildPipeline.BuildAssetBundles(tarDir, BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
    }

    private static List<string> files = new List<string>();
    [MenuItem("Zone/生成resourcesIndex")]
    public static void BuildResIndex()
    {
        string resPath = Application.streamingAssetsPath + "/Android/";
        ///----------------------创建文件列表-----------------------
        string newFilePath = Application.streamingAssetsPath + "/resIndex.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);


        files = ZFileUtil.GetAllFiles(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);
        }
        sw.Close(); fs.Close();
        Debug.Log("生成resIndex完毕!");
    }

    /// <summary>
    /// 一键打包lua，资源，生成索引
    /// </summary>
    [MenuItem("Zone/一键打包所有资源")]
    public static void PackageOneKey()
    {
        Packager.BuildAndroidResource();
        SetAllAbName();
        PackAb(); 
        BuildResIndex();
        Debug.Log("一键打包资源完毕");
    }
 
    /// <summary>
    /// 生成版本差异文件
    /// </summary>
    [MenuItem("Zone/生成版本差异文件")]
    static void GenVersionFiles()
    {
        if (!Directory.Exists(AppConst.VersionPath))
        {
            Directory.CreateDirectory(AppConst.VersionPath);
        }
        VersionVo versionInfo = new VersionVo(Application.streamingAssetsPath + "/versionInfo.xml");
        versionInfo.resVersion = "1000";
        Debug.Log(versionInfo.isResVersionNeedUpdate("1000"));
    
        return;
        string[] dirs = Directory.GetDirectories(AppConst.VersionPath);
        string sourcePath = Application.streamingAssetsPath;
        
        if (dirs.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有发现版本文件，直接备份文件资源", "继续");
            string tarPath = AppConst.VersionPath + "/" + AppConst.versionBase;
            CopyVersionInfoToTarget(tarPath);
            return;
          
//            Directory.CreateDirectory(tarPath);
//            List<string> files = ZFileUtil.GetAllFiles(sourcePath); //获取所有文件
//            //将文件移到对应目录
//            foreach (var file in files)
//            {
//                string fName = Path.GetFileName(file); //文件名
//                string pathComb = file.Replace(sourcePath, "");
//                string targetFolder = tarPath +"/"+ Path.GetDirectoryName(file).Replace(sourcePath,"");
//                if (!Directory.Exists(targetFolder))
//                {
//                    Directory.CreateDirectory(targetFolder);
//                }
//                File.Copy(file,targetFolder+"/"+fName,true);
//            }

        }

        int baseDir = int.Parse(AppConst.versionBase); //最大版本的版本文件夹，用于与当前文件夹做对比
        foreach (var dir in dirs)
        {
            string tarDir = dir.Replace("\\","/");
            int lId = tarDir.LastIndexOf("/") + 1;
            string dirName = tarDir.Substring(lId, tarDir.Length- lId);

            if (!IsNumeric(dirName))
            {
                continue;
            }

            int versionId = int.Parse(dirName);
            
            if (versionId > baseDir)
            {
                baseDir = versionId;
            }
        }
        string compareDir = AppConst.VersionPath + "/" + baseDir;
        string nextVersionDir = AppConst.VersionPath + "/UpdateRes/"+(baseDir + 1).ToString(); //差异生成文件夹
        if (!Directory.Exists(nextVersionDir))
        {
            Directory.CreateDirectory(nextVersionDir);
        }
        string sourceFileIndex = File.ReadAllText(sourcePath + "/" + "files.txt");
        string compFileIndex = File.ReadAllText(compareDir + "/files.txt");

        string[] sFiles = sourceFileIndex.Split('\n');
        
        string appenInfo = "";
        for (int i = 0; i < sFiles.Length; i++)
        {
            if (String.IsNullOrEmpty(sFiles[i]))
            {
                continue;
            }
            string[] kv = sFiles[i].Split('|');
            string f = kv[0].Trim();
            string md5 = kv[1].Trim();
            string localFile = compareDir+"/"+f; //对比文件
            string newFilePath = nextVersionDir + "/" + f;  //新的文件位置 

            if (!File.Exists(localFile)) //如果没有资源，则是新增的内容，直接复制差异资源
            {
                appenInfo += "\n";
                appenInfo += sFiles[i];
                string dir = Path.GetDirectoryName(newFilePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.Copy(sourcePath+"/"+f,newFilePath);
            }
            else
            {
                string oldMd5 = Util.md5file(localFile);
                if (!oldMd5.Equals(md5)) //md5信息变更，更新文件信息
                {
                    appenInfo += "\n";
                    appenInfo += sFiles[i];
                    string dir = Path.GetDirectoryName(newFilePath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.Copy(sourcePath + "/" + f, newFilePath);
                }
            }
        }
         //appenInfo;
        //File.WriteAllText(nextVersionDir + "/files.txt",compFileIndex); //更新资源
        File.Copy(sourcePath + "/" + "files.txt", nextVersionDir + "/files.txt",true); //直接更新file.txt文件

        //更新游戏资源
        string sourceResIndex = File.ReadAllText(sourcePath + "/" + "resIndex.txt");
        string[] rFiles = sourceResIndex.Split('\n');
        for (int i = 0; i < rFiles.Length; i++)
        {
            if (String.IsNullOrEmpty(rFiles[i]))
            {
                continue;
            }
            string[] kv = rFiles[i].Split('|');
            string f = kv[0].Trim();
            string md5 = kv[1].Trim();
            string localFile = compareDir + "/Android/" + f; //对比文件
            string newFilePath = nextVersionDir + "/Android/" + f;  //新的文件位置 
            if (!File.Exists(localFile)) //如果没有资源，则是新增的内容，直接复制差异资源
            {
                appenInfo += "\n";
                appenInfo += rFiles[i];
                string dir = Path.GetDirectoryName(newFilePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.Copy(sourcePath + "/Android/" + f, newFilePath);
            }
            else
            {
                string oldMd5 = Util.md5file(localFile);
                if (!oldMd5.Equals(md5)) //md5信息变更，更新文件信息
                {
                    appenInfo += "\n";
                    appenInfo += rFiles[i];
                    string dir = Path.GetDirectoryName(newFilePath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.Copy(sourcePath + "/Android/" + f, newFilePath);
                }
            }
        }
        //appenInfo = "version:" + (baseDir + 1) + "/n" + appenInfo;
        appenInfo = appenInfo+"\n"+ "version:" + (baseDir + 1);
        File.Copy(sourcePath + "/" + "resIndex.txt", nextVersionDir + "/resIndex.txt", true); //直接更新file.txt文件
        File.WriteAllText(nextVersionDir + "/resUpdate.txt",appenInfo); //更新差异文件资源
        
        //最后将当前版本完整资源进行备份
        CopyVersionInfoToTarget(AppConst.VersionPath + "/"+ (baseDir + 1));
    }

    /// <summary>
    /// 复制版本信息到对应的路径
    /// </summary>
    /// <param name="path"></param>
    public static void CopyVersionInfoToTarget(string tarPath)
    {
        string sourcePath = Application.streamingAssetsPath;
        Directory.CreateDirectory(tarPath);
        List<string> files = ZFileUtil.GetAllFiles(sourcePath); //获取所有文件
        //将文件移到对应目录
        foreach (var file in files)
        {
            string fName = Path.GetFileName(file); //文件名
            string pathComb = file.Replace(sourcePath, "");
            string targetFolder = tarPath + "/" + Path.GetDirectoryName(file).Replace(sourcePath, "");
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            File.Copy(file, targetFolder + "/" + fName, true);
        }
    }
    public static bool IsNumeric(string value)
    {
        const string pattern = "^[0-9]*$";
        Regex rx = new Regex(pattern);
        return rx.IsMatch(value);
    }


}

