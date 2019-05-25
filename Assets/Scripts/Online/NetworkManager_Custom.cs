using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkManager_Custom : NetworkManager {

	const int MAXPLAYERS = 2;

	public PlayerManager[] playerSlots = new PlayerManager[MAXPLAYERS];
	
	public string ipAddress = "localhost";
	public int port = 7777;

	//host game button
	public void StartupHost ()
	{
		singleton.networkPort = port;
		singleton.StartHost ();
	}

	//join game button
	public void JoinGame ()
	{
		//assume that if ip address is unchanged then singleton field is not valid (might be, need to check)
		if (ipAddress.Equals ("localhost"))
			singleton.networkAddress = ipAddress;

		singleton.StartClient ();
	}
	
	//properly setting the ip address through user input
	public void SetIPAddress (string newAddress)
	{
		ipAddress = newAddress;
		singleton.networkAddress = newAddress;
	}

	public void SetPort(string newPort)
	{
		try
		{
			singleton.networkPort = int.Parse (newPort);
			port = singleton.networkPort;
		}
		catch (System.Exception)
		{
			Debug.LogError ("something went wrong, you should probably do something");
		}
	}

	public int getMaxPlayers ()
	{
		return MAXPLAYERS;
	}

	//private Vector3 getSpawnPos ()
	//{
	//	int rand = Random.Range (0, GameObject.Find ("Spawners").transform.GetChild (selectedMap).childCount);
	//	return GameObject.Find ("Spawners").transform.GetChild (selectedMap).GetChild (rand).position;
	//}

	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
	{
		// find empty player slot
		for (int slot = 0; slot < MAXPLAYERS; slot++)
		{
			if (playerSlots[slot] == null)
			{
				var playerObj = Instantiate (playerPrefab, Vector3.zero, Quaternion.identity);
				var player = playerObj.GetComponent<PlayerManager> ();
				playerSlots[slot] = player;//.GetComponent<PlayerManager> ();

				//assign client authority to gamemanager

				NetworkServer.AddPlayerForConnection (conn, playerObj, playerControllerId);
				return;
			}
		}

		//TODO: graceful  disconnect
		conn.Disconnect ();
	}

	public override void OnServerRemovePlayer (NetworkConnection conn, PlayerController playerController)
	{
		// remove players from slots
		var player = playerController.gameObject.GetComponent<PlayerManager> ();
		int id = 1;
		foreach (PlayerManager PM in playerSlots)
		{
			if (PM.UID == player.UID)
				id = PM.UID;
		}
		//foreach (PlayerController PAM in playerSlots)
		//{
		//	if (PAM.gameObject.GetComponent<PlayerAnimatorManager> ().UID == player.UID)
		//		id = PAM.gameObject.GetComponent<PlayerAnimatorManager> ().UID;
		//}

		playerSlots[id] = null;
		//CardManager.singleton.RemovePlayer (player); //TODO

		base.OnServerRemovePlayer (conn, playerController);
	}

	public override void OnServerDisconnect (NetworkConnection conn)
	{
		for(int x = 0; x < playerSlots.Length; x += 1)
		{
			playerSlots[x] = null;
		}

		base.OnServerDisconnect (conn);
	}
}
