using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using SKLibrary;
using UnityEngine.UI;

public class CSVTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (!File.Exists("//172.18.133.1/GameLabo/student/たじまゼミ/各務/Test.csv"))
            {
                return;
            }

            //text.text = "ここまできたよ１";
            StreamWriter sw = new StreamWriter("//172.18.133.1/GameLabo/student/たじまゼミ/各務/Test.csv", true, Encoding.GetEncoding("utf-8"));
            // ヘッダー出力
            string[] s1 = { "プレイヤー名", "記録" };
            string s2 = string.Join(",", s1);
            sw.WriteLine(s2);
            sw.Close();
        }

        transform.position = new Vector3(transform.position.x,0,transform.position.z);
        transform.AddPositionX(10);
        DebugUtils.Log("a");
    }
}
