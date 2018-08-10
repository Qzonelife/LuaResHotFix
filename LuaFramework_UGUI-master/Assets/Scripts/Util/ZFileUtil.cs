using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ZFileUtil  {


    static List<string> files = new List<string>();
    /// <summary>
    /// 遍历目录下所有文件
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllFiles(string path)
    {
        files.Clear();
        Recursive(path);
        return files;
    }
    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            Recursive(dir);
        }
    }
}
