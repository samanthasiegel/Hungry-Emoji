using UnityEngine;
using System.Collections;

public class StartMenuScript : MonoBehaviour {

	public GameObject Book;
	public GameObject StartMenu;

	// Use this for initialization
	void Start () {
		Book.SetActive (false);
		StartMenu.SetActive (true);


	}

	// Update is called once per frame
	void Update () {

	}

	public void GoToBook(){
		Book.SetActive (true);
		StartMenu.SetActive (false);
	}

	public void GoToStart(){
		Start ();
	}
}

