using UnityEngine;
using System.Collections;

public class Video : MonoBehaviour {

	public MovieTexture movie;

	// Use this for initialization
	void Start () {

		GetComponent<Renderer>().material.mainTexture = movie as MovieTexture;
		movie.loop = true;
		movie.Play ();

	
	}
}
