using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class RiddleBookScript : MonoBehaviour {

	public bool _paused = false;

	// Use this for initialization
	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (0)) {
			PauseGame ();
		}
			
	}

	// Pauses game, goes to Riddle Book Scene
	void PauseGame(){
		SceneManager.LoadScene ("RiddleScene");
		ResumeGame ();
	}

	void ResumeGame(){
		SceneManager.UnloadScene ("RiddleScene");
	}



}
