using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceTest : MonoBehaviour
{
	public string a;
	public Object[] voices;
	public AudioSource audioSource;
	public SoundManager soundManager;
	void Start()
	{
		voices = Resources.LoadAll("Voice",typeof(AudioClip));
		//audioSource.clip = (AudioClip)voices[0];
		//audioSource.Play();

	}

	void Update()
    {
		if (Input.GetKeyDown(KeyCode.A))
		{
			StartCoroutine(ReadAloud());
		}
    }

	IEnumerator ReadAloud()
	{
		for (int i = 0; i < a.Length; i++)
		{
			for (int j = 0; j < voices.Length; j++)
			{
				if (voices[j].name==a[i].ToString())
				{
					//audioSource.clip = (AudioClip)voices[j];
					//audioSource.Play();
					AudioClip test = (AudioClip)voices[j];
					soundManager.RequestSound(test);
					yield return new WaitForSeconds(test.length);
					break;
				}
			}
		}
	}
}
