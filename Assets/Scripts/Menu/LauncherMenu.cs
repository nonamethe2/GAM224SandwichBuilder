using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LauncherMenu : MonoBehaviour {

	void Start () {
		
	}
	
	public void setName(string newName)
	{
		GameInfo.thePlayer.pName = newName;
	}
}
