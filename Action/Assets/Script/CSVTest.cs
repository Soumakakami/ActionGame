using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.UI;

public class CSVTest : MonoBehaviour
{
    public Text text;
    void Start()
    {
        if (File.Exists("//172.18.133.1/GameLabo/student/たじまゼミ/各務/Test.csv"))
        {
            //text.text = "通ったよ";
        }
        //text.text = "ここまできたよ１";
        StreamWriter sw = new StreamWriter("//172.18.133.1/GameLabo/student/たじまゼミ/各務/Test.csv", true, Encoding.GetEncoding("utf-8"));
        // ヘッダー出力
        text.text = "ここまできたよ２";
        string[] s1 = { "プレイヤー名", "記録" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
        text.text = "ここまできたよ３";
        // データ出力
        for (int i = 0; i < 3; i++)
        {
            text.text = "ここまできたよ４";
            string[] str = { "tatsu", "" + (i + 1) };
            string str2 = string.Join(",", str);
            sw.WriteLine(str2);
        }
        // StreamWriterを閉じる
        sw.Close();
        text.text = "ここまできたよ５";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {

        }
    }

}
