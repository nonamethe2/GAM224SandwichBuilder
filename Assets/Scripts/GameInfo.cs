using UnityEngine;

public class GameInfo : MonoBehaviour {

	public static PlayerInfo thePlayer;

	void Start () {
		DontDestroyOnLoad (gameObject);
		thePlayer = new PlayerInfo ();
	}
}
