using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Player Manager. 
/// TODO
/// </summary>
public class PlayerManager : NetworkBehaviour
{
	//private const int MAX_EVENT_CARDS = 2;
	//private const int MAX_INGREDIENT_CARDS = 6;

	private NetworkIdentity NetID;

	//sync lists for the cards in the players hand
	public SyncListInt ingCrds = new SyncListInt (); //list of ingredient cards
	public SyncListInt evntCrds = new SyncListInt (); //list of event cards
	public SyncListInt sandwich = new SyncListInt (); //sandwich

	public int maxIngredientsAllowed = 6; //not a const because it can change
	public int maxEventCardsAllowed = 2;
	public int maxSaucesAllowed = 2; //???

	[SyncVar]
	[HideInInspector]
	public int UID = 0;

	public PlayerInfo thePlayer;

	private CanvasGroup HUD;

	void Start ()
	{
		NetID = GetComponent<NetworkIdentity> ();

		if (!NetID.isLocalPlayer)
			transform.GetChild (0).GetComponent<Camera> ().enabled = false;

		else //is the local player
		{
			thePlayer = GameInfo.thePlayer;
			CmdGimmeControl ();
			HUD = GameObject.Find ("Canvas/Panel Player HUD").GetComponent<CanvasGroup> ();

			foreach (GameObject obj in GameObject.FindGameObjectsWithTag ("Player"))
			{
				UID += 1;
			}
			
			CmdChangeMyName (thePlayer.pName, UID);

			//this works it just doesn't display the listener in the inspector
			HUD.transform.GetChild (2).GetComponent<Button> ().onClick.AddListener (() => drawCards ());
			HUD.transform.parent.GetChild (2).GetComponent<Button> ().onClick.AddListener (() => endTurn ());

			//looping over the values created issues with the parameter it was passing in
			//HUD.transform.GetChild (0).GetChild (0).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (0));
			//HUD.transform.GetChild (0).GetChild (1).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (1));
			//HUD.transform.GetChild (0).GetChild (2).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (2));
			//HUD.transform.GetChild (0).GetChild (3).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (3));
			//HUD.transform.GetChild (0).GetChild (4).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (4));
			//HUD.transform.GetChild (0).GetChild (5).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (5));

			//HUD.transform.GetChild (1).GetChild (0).GetComponent<Button> ().onClick.AddListener (() => playEvntCard (0));
			//HUD.transform.GetChild (1).GetChild (1).GetComponent<Button> ().onClick.AddListener (() => playEvntCard (1));
		}
	}

	// either +/- 1 for either right / left
	// 0 just pushes cards from the right onto the display
	[ClientRpc]
	public void RpcUpdateEventCardsDisplay (int dir)
	{

	}

	[ClientRpc]
	public void RpcUpdateIngredientCardsDisplay (int dir)
	{
		if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
			return;

		//TODO: set next and previous buttons

		int x = 0;
		List<Button> btns = new List<Button> ();
		btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD1").GetComponent<Button> ());
		btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD2").GetComponent<Button> ());
		btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD3").GetComponent<Button> ());
		btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD4").GetComponent<Button> ());
		foreach(Button btn in btns)
		{
			int _x = ++x;
			if (ingCrds.Count >= _x + 1) //display some none card?
				continue;

			if (dir == -1)
			{
				btn.onClick.AddListener (() => playIngrCard (_x - 1));
			}
			else if (dir == 0)
			{
				btn.onClick.AddListener (() => playIngrCard (_x));
			}
			else if (dir == 1)
			{
				btn.onClick.AddListener (() => playIngrCard (_x + 1));
			}
		}
	}

	/// <summary>
	/// Button response - ends the players turn and adds one event card to their deck. 
	/// </summary>
	public void endTurn ()
	{

	}

	/// <summary>
	/// plays the ingredient card at the given index in the players hand. 
	/// </summary>
	/// <param name="ind">Index of the ingredient card to play. </param>
	public void playIngrCard(int ind)
	{
		//check if selected card exists or that adding the card won't exceed the ingredient limit
		if (ingCrds.Count <= ind || (ingCrds.Count + 1) <= maxIngredientsAllowed)
			return;

		CmdPlayIngredient (ind);
	}

	[Command]
	public void CmdPlayIngredient (int ind)
	{
		sandwich.Add (ingCrds[ind]);
		ingCrds.RemoveAt (ind);
		RpcDisplayCards ();
		RpcDrawSandwich ();
	}

	[ClientRpc]
	public void RpcDrawSandwich ()
	{
		if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
			return;

		//Debug.Log (UID);
		CanvasGroup sndwchPanel = UID == 1 ?
				GameObject.Find ("Canvas/Panel Category/Sandwich P1").GetComponent<CanvasGroup> () :
				GameObject.Find ("Canvas/Panel Category/Sandwich P2").GetComponent<CanvasGroup> ();

		//Image imgRef = Resources.Load ("Image Ingredient", typeof (Image)) as Image;
		foreach(int i in sandwich)
		{
			//get the associated image and add it
			//add and destroy image objects as necessary?? removing cards?
			Image newImg = Instantiate (Resources.Load ("Image Ingredient", typeof (Image)) as Image);
			newImg.transform.SetParent (sndwchPanel.transform);
			newImg.rectTransform.localPosition = Vector3.zero;
			newImg.sprite = Resources.Load (Card.getImg (i), typeof (Sprite)) as Sprite;

		}
	}

	/// <summary>
	/// Plays the event card at the given index. 
	/// </summary>
	/// <param name="ind">Index of the ingredient card to play. </param>
	public void playEvntCard(int ind)
	{
		Debug.Log (ind);
		switch (evntCrds[ind]) //call function action based on event ID
		{
			case 0:
				CmdBakery ();
				break;

			case 1:
				CmdGrocer ();
				break;
		}
	}

	[Command]
	public void CmdBakery ()
	{
		//iterate over deck and add first bread item found
	}

	[Command]
	public void CmdGrocer ()
	{
		//add the top two ingredient cards to your hand
	}

	public void drawCards ()
	{
		CmdDrawNewCards ();
	}

	[Command]
	public void CmdDrawNewCards ()
	{
		//add the cards to the sync lists and then call the RPC function to update the UI
		ScoreManager table = GameObject.FindGameObjectWithTag ("Table").GetComponent<ScoreManager> ();
		//if (evntCrds.Count >= maxEventCardsAllowed || table.evntCrdsDeck.Count == 0)
			//return;

		for (int x = 0; x < maxIngredientsAllowed; x += 1)
		{
			if (x < maxEventCardsAllowed && (evntCrds.Count < maxEventCardsAllowed || table.evntCrdsDeck.Count > 0))
			{
				evntCrds.Add (table.evntCrdsDeck[0]);
				table.evntCrdsDeck.RemoveAt (0);
			}

			//ingCrds.Add (new Card (Type.Ingredient).ID);
			ingCrds.Add (table.ingrCrdsDeck[0]);
			table.ingrCrdsDeck.RemoveAt (0);
		}
		
		RpcDisplayCards ();
	}

	[ClientRpc]
	public void RpcDisplayCards ()
	{
		//only update the UI for the player that initiated the call
		if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
			return;

		HUD = GameObject.Find ("Canvas/Panel Player HUD").GetComponent<CanvasGroup> ();
		for (int x = 0; x < maxIngredientsAllowed; x += 1)
		{
			if (x < maxEventCardsAllowed)
			{
				//evntCrds.Add (new Card (Type.Event).ID); //just add some event cards (capped at 2)
				HUD.transform.GetChild (1).GetChild (x).GetChild (0).GetComponent<Text> ().text = evntCrds[x].ToString ();

			}

			//ingCrds.Add (new Card (Type.Ingredient).ID);
			string tmp = ingCrds.Count > x ? ingCrds[x].ToString () : "-1";
			HUD.transform.GetChild (0).GetChild (x).GetChild (0).GetComponent<Text> ().text = tmp;
			//HUD.transform.GetChild (0).GetChild (x).GetComponent<Image> ().sprite = Card.getImg (ingCrds[x]); //TODO UNCOMMENT
		}
	}

	[Command]
	public void CmdChangeMyName (string name, int id)
	{
		RpcChangeMyName (name, id);
	}

	[ClientRpc]
	public void RpcChangeMyName (string name, int id)
	{
		transform.name = name + id.ToString ();
	}

	[Command]
	public void CmdGimmeControl ()
	{
		NetID.AssignClientAuthority (connectionToClient);
	}

	IEnumerator waitToSwap (GameObject wep)
	{
		yield return new WaitForSeconds (0.5f);
		if(wep.gameObject != null)
			wep.SetActive (true);
	}

	void Update ()
	{
		if (!NetID.isLocalPlayer)
			return;

		
	}

	[Command]
	public void CmdRespawn ()
	{
		RpcRespawn ();
	}

	[ClientRpc]
	public void RpcRespawn ()
	{
		if (NetID.isLocalPlayer)
		{
			//health = MAXHEALTH; //Max health
			transform.position = Vector3.zero; //move to spawn
		}
	}
}