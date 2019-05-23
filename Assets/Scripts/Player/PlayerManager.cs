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
	private int ingrInd = 0;
	private int evntInd = 0;

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
			GameObject.Find ("Canvas/Button End Turn").GetComponent<Button> ().onClick.AddListener (() => endTurn ());
			GameObject.Find ("Canvas/Button Display Cards").GetComponent<Button> ().onClick.AddListener (() => drawCards ());

			//add listener for the ingredient card selectors (next and previous)
			HUD.transform.GetChild (0).GetChild (0).GetComponent<Button> ().onClick.AddListener (() => btnUpdateIngredients (-1));
			HUD.transform.GetChild (0).GetChild (1).GetComponent<Button> ().onClick.AddListener (() => btnUpdateIngredients (1));

			//add listener for the event card selectors (next and previous)
			HUD.transform.GetChild (1).GetChild (0).GetComponent<Button> ().onClick.AddListener (() => btnUpdateEvents (-1));
			HUD.transform.GetChild (1).GetChild (1).GetComponent<Button> ().onClick.AddListener (() => btnUpdateEvents (1));

		}
	}

	public void btnUpdateEvents(int dir)
	{
		CmdUpdateEvents (dir);
	}

	[Command]
	public void CmdUpdateEvents(int dir)
	{
		RpcUpdateEventCardsDisplay (dir);
	}

	// either +/- 1 for either right / left
	// 0 just pushes cards from the right onto the display
	[ClientRpc]
	public void RpcUpdateEventCardsDisplay (int dir)
	{
		if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
			return;

		evntInd += dir;
		//int x = 0;

		//Debug.Log (evntInd);

		//List<Button> btns = new List<Button> ();
		Button btn = GameObject.Find ("Canvas/Panel Player HUD/Panel Events/Button CRD7").GetComponent<Button> ();

		if (evntInd >= evntCrds.Count || evntInd < 0)
		{
			btn.GetComponent<Image> ().sprite = Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;
			return;
		}

		//Debug.Log (evntCrds[evntInd].ToString () + Card.getEvntImg (evntInd));
		btn.GetComponent<Image> ().sprite =
			Resources.Load ("Cards/Events/" + Card.getEvntImg (evntCrds[evntInd]), typeof (Sprite)) as Sprite;
		btn.onClick.AddListener (() => playEvntCard (evntInd));
	}

	public void btnUpdateIngredients(int dir)
	{
		CmdUpdateIngredients (dir);
	}

	[Command]
	public void CmdUpdateIngredients(int dir)
	{
		RpcUpdateIngredientCardsDisplay (dir);
	}

	[ClientRpc]
	public void RpcUpdateIngredientCardsDisplay (int dir)
	{
		if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
			return;

		ingrInd += dir;
		int x = 0;

		List<Button> btns = new List<Button> ();
		btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD1").GetComponent<Button> ());
		btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD2").GetComponent<Button> ());
		btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD3").GetComponent<Button> ());
		btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD4").GetComponent<Button> ());
		btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD5").GetComponent<Button> ());
		foreach (Button btn in btns)
		{
			int _x = 0;

			if (ingrInd < 0 || ingrInd + x >= ingCrds.Count)
			{
				//Debug.Log (ingrInd.ToString () + x.ToString () + ingCrds.Count.ToString ());
				//btn.transform.GetChild (0).GetComponent<Text> ().text = "-1";
				btn.GetComponent<Image> ().sprite = Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;
				_x = ++x;
				continue;
			}

			btn.GetComponent<Image> ().sprite = 
				Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[ingrInd + x]), typeof (Sprite)) as Sprite;

			if(dir == -1)
				btn.onClick.AddListener (() => playIngrCard (_x - 1));

			else if(dir == 1)
				btn.onClick.AddListener (() => playIngrCard (_x + 1));

			//if (dir == -1)
			//{
			//	//btn.transform.GetChild (0).GetComponent<Text> ().text = ingCrds[ingrInd + x].ToString ();
			//	btn.onClick.AddListener (() => playIngrCard (_x - 1));
			//}
			////else if (dir == 0)
			////{
			////	btn.onClick.AddListener (() => playIngrCard (_x));
			////}
			//else if (dir == 1)
			//{
			//	//btn.transform.GetChild (0).GetComponent<Text> ().text = ingCrds[ingrInd + x].ToString ();
			//	btn.onClick.AddListener (() => playIngrCard (_x + 1));
			//}

			_x = ++x;
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
			newImg.sprite = Resources.Load (Card.getIngrdntImg (i), typeof (Sprite)) as Sprite;

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
		GameObject.Find ("Canvas/Button Display Cards").SetActive (false);
	}

	//should only be called once at setup
	[Command]
	public void CmdDrawNewCards ()
	{
		//add the cards to the sync lists and then call the RPC function to update the UI
		ScoreManager table = GameObject.FindGameObjectWithTag ("Table").GetComponent<ScoreManager> ();

		for (int x = 0; x < maxIngredientsAllowed; x += 1)
		{
			if (x < maxEventCardsAllowed /*&& (evntCrds.Count < maxEventCardsAllowed || table.evntCrdsDeck.Count > 0)*/)
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
		GameObject.Find("Canvas/Panel Player HUD/Panel Events/Button CRD7").GetComponent<Image> ().sprite =
					Resources.Load ("Cards/Events/" + Card.getEvntImg (evntCrds[0]), typeof (Sprite)) as Sprite;
		for (int x = 0; x < maxIngredientsAllowed-1; x += 1)
		{
			HUD.transform.GetChild (0).GetChild (x+2).GetComponent<Image> ().sprite =
				Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[x]), typeof (Sprite)) as Sprite;
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