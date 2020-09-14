using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

/// <summary>
/// フォルダの場所(\\172.18.133.1\GameLabo\student\たじまゼミ\各務)
/// ID q0169 PW LodpXT8o
/// </summary>
public class CSVController : MonoBehaviour
{
    const string folderPath = "//172.18.133.1/GameLabo/student/たじまゼミ/各務/";
    private string path;
    public string[] Load(string _fileName,int _line)
    {
        //ファイルがなければここで処理を止める
        if (!CheckFile(_fileName)) return null;
        StreamReader sr = new StreamReader(GetPath(_fileName), Encoding.GetEncoding("utf-8"));

        //指定した行を取得
        for (int i = 0; i < _line-1; i++)
        {
            sr.ReadLine();
        }
        string str=sr.ReadLine();
        sr.Close();
        return str.Split(',');
    }

    public string[][] AllLoad(string _fileName)
    {
        if (!CheckFile(_fileName)) return null;
        string[][] table = new string[GetLength(_fileName)][];
        StreamReader sr = new StreamReader(GetPath(_fileName), Encoding.GetEncoding("utf-8"));
        for (int i = 0; i < table.Length-1; i++)
        {
            table[i] = sr.ReadLine().Split(',');
        }
        sr.Close();
        return table;
    }

    /// <summary>
    /// 追加保存
    /// </summary>
    /// <param name="_fileName">ファイルの名前</param>
    /// <param name="_strs">追加する文字列</param>
    /// <param name="_callback">コールバック</param>
    public IEnumerator AddSave(string _fileName , string[] _strs,Action<bool> _callback=null)
    {
        //ファイルがなければここで処理を止める
        if (!CheckFile(_fileName)){ _callback(false); yield break;}

        StreamWriter sw = null;

        //書き込めるまで動くよ
        while (true)
        {
            //エラーガ発生していればtrueになるよ
            bool error = false;

            //エラーガでるかもしれない処理
            try
            {
                sw = new StreamWriter(GetPath(_fileName), true, Encoding.GetEncoding("utf-8"));
            }

            //エラーガでたときの処理
            catch (System.Exception)
            {
                Debug.Log("誰かがファイルを開いています");
                error = true;
            }
            //エラーが出たら1秒止めてもう一度
            if (error == true)
            {
                yield return new WaitForSeconds(1f);
            }
            else
            {
                //書き込む
                sw.WriteLine(string.Join(",", _strs));

                Debug.Log("書き込みました");
                //ファイルを閉じる
                sw.Close();
                //コールバックを呼び出す
                _callback(true);
                break;
            }
        }    
    }

    /// <summary>
    /// 上書き保存
    /// </summary>
    /// <param name="_fileName">ファイル名</param>
    /// <param name="_table">文字テーブル</param>
    /// <param name="_callback">コールバック</param>
    /// <returns></returns>
    public IEnumerator OverwriteSave(string _fileName, string[][] _table, Action<bool> _callback = null)
    {
        int count = 0;
        //ファイルがなければここで処理を止める
        if (!CheckFile(_fileName)) { _callback(false); yield break; }
        StreamWriter sw = null;
        while (true)
        {
            //エラーガ発生していればtrueになるよ
            bool error = false;
            count++;
            //エラーガでるかもしれない処理
            try
            {
                sw = new StreamWriter(GetPath(_fileName), false, Encoding.GetEncoding("utf-8"));
            }

            //エラーガでたときの処理
            catch (System.Exception)
            {
                Debug.Log("誰かがファイルを開いています");
                error = true;
            }
            //エラーが出たら1秒止めてもう一度
            if (error == true)
            {
                yield return new WaitForSeconds(1f);
            }
            else
            {
                Debug.Log("書き込みました");
                for (int i = 0; i < _table.Length - 1; i++)
                {
                    sw.WriteLine(string.Join(",", _table[i]));
                }
                sw.Close();
                _callback(true);
                break;
            }
        }
    }

    /// <summary>
    /// 現在どのぐらいデータが書き込まれているかの長さを取得
    /// </summary>
    /// <param name="_fileName">ファイルの名前</param>
    /// <returns></returns>
    public int GetLength(string _fileName)
    {
        int count=0;
        //ファイルがなければここで処理を止める
        if (!CheckFile(_fileName)) return 0;

        StreamReader sr = new StreamReader(GetPath(_fileName), Encoding.GetEncoding("utf-8"));

        //nullが検知できるまでループする
        while (true)
        {
            count++;
            if (sr.ReadLine()==null)
            {
                break;
            }
        }

        return count;
    }

    /// <summary>
    /// Fileがあるかチェックする
    /// </summary>
    /// <param name="_fileName">ファイル名</param>
    /// <returns></returns>
    private bool CheckFile(string _fileName)
    {
        return File.Exists(GetPath(_fileName));
    }

    /// <summary>
    /// ファイルのパスを作る
    /// </summary>
    /// <param name="_fileName"></param>
    /// <returns></returns>
    private string GetPath(string _fileName)
    {

        return folderPath + _fileName + ".csv";
    }
}
