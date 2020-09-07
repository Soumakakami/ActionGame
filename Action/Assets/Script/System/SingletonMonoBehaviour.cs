using UnityEngine;
using System;
using UnityEngine.SceneManagement;


/// <summary>
/// シングルトンさせたい奴が継承するようクラス
/// </summary>
/// <typeparam name="T">何かしらのクラスが入る</typeparam>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	//何かしらのクラスが入る
	private static T instance;

	public static T Instance
	{
		get
		{
			//instanceの中身がなにもないか確認
			if (instance == null)
			{
				
				Type t = typeof(T);

				//instanceにクラスを渡す
				instance = (T)FindObjectOfType(t);

				//そもそもクラスがなければエラー
				if (instance == null)
				{
					Debug.LogError(t + " をアタッチしているGameObjectはありません");
				}
			}

			return instance;
		}
	}

	virtual protected void Awake()
	{
		// アタッチされている場合は破棄する。
		CheckInstance();
	}

	/// <summary>
	/// 他のゲームオブジェクトにアタッチされているか調べる
	/// </summary>
	/// <returns></returns>
	protected bool CheckInstance()
	{
		//instanceになにもなければ
		if (instance == null)
		{
			instance = this as T;
			DontDestroyOnLoad(instance.gameObject);
			return true;
		}
		//自身であるかチェック
		else if (Instance == this)
		{
			return true;
		}
		//自分以外になにかあったので
		Destroy(this.gameObject);
		return false;
	}
}