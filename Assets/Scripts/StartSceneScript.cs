using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartSceneScript : MonoBehaviour {
	public GameObject HeartEyesEmoji, YummyEmoji, OKEmoji, OopsEmoji, ScreamEmoji;
	public Button StartButton;

	// Use this for initialization
	void Start () {
		StartButton.onClick.AddListener (() =>
			SceneManager.LoadScene ("MainScene"));
			

		HeartEyesEmoji.SetActive (true);
		YummyEmoji.SetActive (false);
		OKEmoji.SetActive (false);
		OopsEmoji.SetActive (false);
		ScreamEmoji.SetActive (false);

		StartCoroutine (ChangeEmojis ());
	}

	IEnumerator ChangeEmojis(){
		while (true) {
			yield return new WaitForSeconds(1f);
			HeartEyesEmoji.SetActive (false);
			YummyEmoji.SetActive (true);
			yield return new WaitForSeconds (1f);
			YummyEmoji.SetActive (false);
			OKEmoji.SetActive (true);
			yield return new WaitForSeconds (1f);
			OKEmoji.SetActive (false);
			OopsEmoji.SetActive (true);
			yield return new WaitForSeconds (1f);
			OopsEmoji.SetActive (false);
			ScreamEmoji.SetActive (true);
			yield return new WaitForSeconds (1f);
			ScreamEmoji.SetActive (false);
			HeartEyesEmoji.SetActive (true);

		}
	}


	
	// Update is called once per frame
	void Update () {
			
	}
}
