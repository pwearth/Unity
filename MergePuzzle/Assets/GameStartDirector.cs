using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameStartDirector : MonoBehaviour
{
	// Audio
	[SerializeField] AudioClip seDrop;
	[SerializeField] AudioClip seMerge;

	// UI
	AudioSource audioSource;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		// サウンド再生用
		audioSource = GetComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update()
	{
		// SE再生
		if (Input.GetMouseButtonDown(0))
		{
			audioSource.PlayOneShot(seDrop);
		}
	}

	public void StartButton()
	{
		// Load the game scene
		SceneManager.LoadScene("MergePuzzleScene");
	}

}
