using UnityEngine;
using System.Collections;

public class NumUtil {
    /// <summary>
    /// 获得格式化输出,输入是字节大小
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static string GetFormatFileSize(long val)
    {
        double total = val;
        if (total < 1024)
        {
            return "1k";
        }

        string[] desc = new string[] { "k", "M", "G" };

        for (int i = 0; i < desc.Length; i++)
        {
            double vt = total / 1024.0f;
            if (vt < 1024)
            {
                vt = ((int)(vt * 100)) / 100.0f;

                return vt.ToString("#0.00") + desc[i];
            }
            total = total / 1024.0f;
        }
        total = ((int)(total * 100)) / 100.0f;
        return total.ToString("#0.00") + desc[2];
    }

}
