using System.Collections.Generic;
using UnityEngine;
using SKLibrary;
using SKLibrary.SaveAndLoad;
using System;
using System.Collections;

public class SoundManager : MonoBehaviour
{
	const string SAVE_FILE_NAME = "SoundInformation";		//セーブデータのファイル名
	const float BGM_FADE_TIME = 1f;				//フェードにかかる秒数

	[Serializable]
	public class SoundInfo
	{
		public float MainVolume;
		public float BackMusicVolume;
		public float SfxVolume;
	}
	SoundInfo soundInfo = new SoundInfo();

	[SerializeField, Header("BGM")]
	AudioClip backMusic;

	public float MainVolume { set { SoundVolumeApplication(); MainVolume = value; } get { return MainVolume; } }

	public float BackMusicVolume { set { SoundVolumeApplication(); BackMusicVolume = value; } get { return BackMusicVolume; } }

	public float SfxVolume { set { SoundVolumeApplication(); SfxVolume = value; } get { return SfxVolume; } }

	GameObject backMusicObject;

	public List<AudioSource> sfxList = new List<AudioSource>();

	public AudioClip clip;


	private void Start()
	{
		Initialize();

	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			GetComponent<AudioSource>().Play();
			Debug.Log(Mathf.Clamp(5, 0, 2));
		}
	}

	/// <summary>
	/// 初期化
	/// </summary>
	private void Initialize()
	{
		//サウンドの設定データがなければ作成
		if (SaveLoadSystem.Load(SAVE_FILE_NAME) == null)
		{
			Debug.Log("セーブデータを作成");
			soundInfo.BackMusicVolume = 1;
			soundInfo.MainVolume = 1;
			soundInfo.SfxVolume = 1;
			VolumeAttach();
		}
		//サウンドの設定データを呼び出す
		else
		{
			soundInfo = (SoundInfo)SaveLoadSystem.Load(SAVE_FILE_NAME);
			VolumeAttach();
		}

		//BGMが最初からアタッチしてあればそちらを流す
		if (backMusic != null)
		{
			RequestBackMusic(backMusic);
		}
	}


	/// <summary>
	/// バックミュージックをリクエストする
	/// </summary>
	/// <param name="_backMusic">流したいBGM</param>
	/// <param name="_overwrite">今流れているBGMを上書きするかどうか</param>
	/// <returns>バックミュージックを流しているAudioSource</returns>
	public AudioSource RequestBackMusic(AudioClip _backMusic, bool _overwrite = false, float _pitch = 1f)
	{
		AudioSource audioSource = null;
		//何も流れていなければ再生
		if (backMusicObject == null)
		{
			backMusicObject = new GameObject("BackMusicObject");
			audioSource = backMusicObject.AddComponent<AudioSource>();
			StartCoroutine(BackGroundMusicFade(_backMusic,audioSource,FadeMode.FadeIn));
			audioSource.loop = true;
			audioSource.pitch = Mathf.Clamp(_pitch, -3, 3);
		}
		//バックミュージックを上書き
		else if (backMusicObject != null && _overwrite == true)
		{
			audioSource.Stop();
			audioSource.loop = true;
			audioSource.time = 0;
			audioSource.pitch = Mathf.Clamp(_pitch, -3, 3);
			StartCoroutine(BackGroundMusicFade(_backMusic, audioSource, FadeMode.FadeOut));
		}

		return audioSource;
	}

	/// <summary>
	/// バックミュージックを止める
	/// </summary>
	/// <returns>バックミュージックを止められたかの結果 true or false</returns>
	public bool StopBackMusic()
	{
		bool result = false;
		if (backMusicObject != null)
		{
			backMusicObject.GetComponent<AudioSource>().Stop();
			result = true;
		}
		return result;
	}

	/// <summary>
	/// サウンドをリクエスとする
	/// </summary>
	/// <param name="_sound">流したいサウンド</param>
	/// <param name="_localPos">再生させたい位置</param>
	/// <param name="_loop">ループ</param>
	/// <param name="_pitch">ピッチ(-3～3)</param>
	/// <param name="_spatial">立体音響(0～1)</param>
	/// <returns>再生させているサウンドのAudioSource</returns>
	public AudioSource RequestSound(AudioClip _sound, Vector3 _localPos = default, bool _loop = false, float _pitch = 1f, float _spatial = 0f)
	{

		//Soundを鳴らす用のオブジェクトを生成
		GameObject tempAudio = new GameObject("TempAudio");
		//鳴らす位置を決める
		tempAudio.transform.position = _localPos;
		//AudioSourceをAddする
		AudioSource audioSource = tempAudio.AddComponent<AudioSource>() as AudioSource;
		//リストに格納
		sfxList.Allocation(audioSource, true);

		//ループさせるかどうか
		audioSource.loop = _loop;
		//音声を追加
		audioSource.clip = _sound;
		//音声を再生
		audioSource.Play();
		//ピッチ調整
		audioSource.pitch = Mathf.Clamp(_pitch, -3, 3);
		//立体音響
		audioSource.spatialBlend = Mathf.Clamp(_spatial, 0, 1);

		//ループでなければ
		if (_loop == false)
		{
			//音声が再生され終えたら削除
			Destroy(tempAudio, _sound.length);
		}

		//AudioSourceを返す
		return audioSource;
	}

	/// <summary>
	/// 現在なっている音に変更後の音量を適用させる
	/// </summary>
	public void SoundVolumeApplication()
	{

		soundInfo.MainVolume = MainVolume;
		soundInfo.BackMusicVolume = BackMusicVolume;
		soundInfo.SfxVolume = SfxVolume;

		SaveLoadSystem.Save(soundInfo, SAVE_FILE_NAME);

		//バックミュージックがなければ処理しない
		if (backMusicObject != null)
		{
			//音量再適用
			backMusicObject.GetComponent<AudioSource>().volume = MainVolume * BackMusicVolume;
		}

		//音量再適用
		for (int i = 0; i < sfxList.Count; i++)
		{
			if (sfxList[i] == null)
			{
				continue;
			}
			else
			{
				sfxList[i].volume = MainVolume * BackMusicVolume;
			}
		}
	}

	/// <summary>
	/// セーブしてあった値を読み込む
	/// </summary>
	public void VolumeAttach()
	{
		MainVolume = soundInfo.MainVolume;
		BackMusicVolume = soundInfo.BackMusicVolume;
		SfxVolume = soundInfo.SfxVolume;
	}

	public enum FadeMode
	{
		FadeIn,
		FadeOut
	}

	IEnumerator BackGroundMusicFade(AudioClip _clip,AudioSource _audioSource,FadeMode _fadeMode)
	{
		float volume;
		switch (_fadeMode)
		{
			case FadeMode.FadeIn:
				volume = MainVolume*BackMusicVolume;
				_audioSource.volume = 0;
				_audioSource.clip = _clip;
				_audioSource.Play();
				while (_audioSource.volume >= volume)
				{
					_audioSource.volume += volume / Time.deltaTime / BGM_FADE_TIME;
					yield return null;
				}
				_audioSource.volume = volume;
				break;
			case FadeMode.FadeOut:
				volume = _audioSource.volume;
				while (_audioSource.volume <= 0)
				{
					volume -= Time.deltaTime / BGM_FADE_TIME;
					_audioSource.volume = volume;
					yield return null;
				}
				if (_clip != null)
				{
					StartCoroutine(BackGroundMusicFade(_clip, _audioSource, FadeMode.FadeIn));
				}
				break;
			default:
				break;
		}
		yield break;
	}
}