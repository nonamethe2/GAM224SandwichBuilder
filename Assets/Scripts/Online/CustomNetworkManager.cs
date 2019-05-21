using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
	const int maxPlayers = 5;
	PlayerInfo[] playerSlots = new PlayerInfo[5];

	public override void OnServerAddPlayer (NetworkConnection conn, short playerControllerId)
	{
		// find empty player slot
		for (int slot = 0; slot < maxPlayers; slot++)
		{
			if (playerSlots[slot] == null)
			{
				var playerObj = (GameObject)GameObject.Instantiate (playerPrefab, Vector3.zero, Quaternion.identity);
				var player = playerObj.GetComponent<PlayerInfo> ();

				player.PID = slot;
				playerSlots[slot] = player;

				Debug.Log ("Adding player in slot " + slot);
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
		var player = playerController.gameObject.GetComponent<PlayerInfo> ();
		playerSlots[player.PID] = null;
		//CardManager.singleton.RemovePlayer (player); //TODO

		base.OnServerRemovePlayer (conn, playerController);
	}

	public override void OnServerDisconnect (NetworkConnection conn)
	{
		foreach (var playerController in conn.playerControllers)
		{
			var player = playerController.gameObject.GetComponent<PlayerInfo> ();
			playerSlots[player.PID] = null;
			//CardManager.singleton.RemovePlayer (player); //TODO
		}

		base.OnServerDisconnect (conn);
	}

	public override void OnStartClient (NetworkClient client)
	{
		//client.RegisterHandler (CardConstants.CardMsgId, OnCardMsg);
	}

	//void OnCardMsg (NetworkMessage netMsg)
	//{
	//	var msg = netMsg.ReadMessage<CardConstants.CardMessage> ();

	//	var other = ClientScene.FindLocalObject (msg.playerId);
	//	var player = other.GetComponent<PlayerInfo> ();
	//	player.MsgAddCard (msg.cardId);
	//}
}
