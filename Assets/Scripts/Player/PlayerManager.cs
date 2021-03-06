﻿using UnityEngine;
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
		RpcScrollEventCards (dir);
	}

	// either +/- 1 for either right / left
	// 0 just pushes cards from the right onto the display
	[ClientRpc]
	public void RpcScrollEventCards (int dir)
	{
		if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
			return;

		Button btn = GameObject.Find ("Canvas/Panel Player HUD/Panel Events/Button CRD7").GetComponent<Button> ();
		btn.onClick.RemoveListener (() => playEvntCard (evntInd));
		evntInd += dir;

		if (evntInd >= evntCrds.Count || evntInd < 0)
		{
			btn.GetComponent<Image> ().sprite = Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;
			return;
		}

		btn.GetComponent<Image> ().sprite =
			Resources.Load ("Cards/Events/" + evntCrds[evntInd], typeof (Sprite)) as Sprite;
		btn.onClick.AddListener (() => playEvntCard (evntInd));
	}

	public void btnUpdateIngredients(int dir)
	{
		CmdUpdateIngredients (dir);
	}

	[Command]
	public void CmdUpdateIngredients(int dir)
	{
		RpcScrollIngCards (dir);
	}

	[ClientRpc]
	public void RpcScrollIngCards (int dir)
	{
		if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
			return;

		ingrInd += dir;
		//int x = 0;

		//Debug.Log (dir);

		//yes this looks bad but this area of code is buggy (understatement, multiple issues exist)
		//also lambdas capture values weirdly and i'm trying to isolate the issue
		if (ingrInd + 0 >= 0 && ingrInd + 0 < ingCrds.Count)
		{
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD1").GetComponent<Button> ().onClick.RemoveAllListeners ();
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD1").GetComponent<Button> ().onClick.AddListener (() => playIngrCard (ingrInd + 0));
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD1").GetComponent<Image> ().sprite =
				Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[ingrInd + 0]), typeof (Sprite)) as Sprite;
		}
		else
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD1").GetComponent<Image> ().sprite = Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;

		if (ingrInd + 1 >= 0 && ingrInd + 1 < ingCrds.Count)
		{
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD2").GetComponent<Button> ().onClick.RemoveAllListeners ();
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD2").GetComponent<Button> ().onClick.AddListener (() => playIngrCard (ingrInd + 1));
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD2").GetComponent<Image> ().sprite =
				Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[ingrInd + 1]), typeof (Sprite)) as Sprite;
		}
		else
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD2").GetComponent<Image> ().sprite = Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;

		if (ingrInd + 2 >= 0 && ingrInd + 2 < ingCrds.Count)
		{
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD3").GetComponent<Button> ().onClick.RemoveAllListeners ();
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD3").GetComponent<Button> ().onClick.AddListener (() => playIngrCard (ingrInd + 2));
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD3").GetComponent<Image> ().sprite =
				Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[ingrInd + 2]), typeof (Sprite)) as Sprite;
		}
		else
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD3").GetComponent<Image> ().sprite = Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;

		if (ingrInd + 3 >= 0 && ingrInd + 3 < ingCrds.Count)
		{
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD4").GetComponent<Button> ().onClick.RemoveAllListeners ();
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD4").GetComponent<Button> ().onClick.AddListener (() => playIngrCard (ingrInd + 3));
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD4").GetComponent<Image> ().sprite =
				Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[ingrInd + 3]), typeof (Sprite)) as Sprite;
		}
		else
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD4").GetComponent<Image> ().sprite = Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;

		if (ingrInd + 4 >= 0 && ingrInd + 4 < ingCrds.Count)
		{
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD5").GetComponent<Button> ().onClick.RemoveAllListeners ();
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD5").GetComponent<Button> ().onClick.AddListener (() => playIngrCard (ingrInd + 4));
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD5").GetComponent<Image> ().sprite =
				Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[ingrInd + 4]), typeof (Sprite)) as Sprite;
		}
		else
			GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD5").GetComponent<Image> ().sprite = Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;

		//List<Button> btns = new List<Button> ();
		//btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD1").GetComponent<Button> ());
		//btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD2").GetComponent<Button> ());
		//btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD3").GetComponent<Button> ());
		//btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD4").GetComponent<Button> ());
		//btns.Add (GameObject.Find ("Canvas/Panel Player HUD/Panel Ingredients/Button CRD5").GetComponent<Button> ());
		//foreach (Button btn in btns)
		//{
		//	int _x = 0;

		//	if (ingrInd < 0 || ingrInd + x >= ingCrds.Count)
		//	{
		//		btn.GetComponent<Image> ().sprite = Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;
		//		_x = ++x;
		//		continue;
		//	}

		//	//Debug.Log (btn.name + x.ToString () + _x.ToString ());

		//	btn.GetComponent<Image> ().sprite = 
		//		Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[ingrInd + x]), typeof (Sprite)) as Sprite;

		//	btn.onClick.RemoveListener (() => playIngrCard (ingrInd - x));
		//	//btn.onClick.RemoveListener (() => playIngrCard (_x));
		//	btn.onClick.RemoveListener (() => playIngrCard (ingrInd + x));

		//	//Debug.Log (ingrInd + x);
		//	if (dir == -1 || dir == 0)
		//	{
		//		//btn.onClick.RemoveListener (() => playIngrCard (_x - 1));
		//		//Debug.Log (ingrInd - x);
		//		btn.onClick.AddListener (() => playIngrCard (ingrInd + x));

		//	}
		//	else if (dir == 1)
		//	{
		//		//btn.onClick.RemoveListener (() => playIngrCard (_x + 1));
		//		//Debug.Log (ingrInd + x);
		//		btn.onClick.AddListener (() => playIngrCard (ingrInd + x));
		//	}

		//	_x = ++x;
		//}
	}

	/// <summary>
	/// Button response - ends the players turn and adds one event card to their deck. 
	/// </summary>
	public void endTurn ()
	{
		CmdEndTurn ();
	}

	[Command]
	public void CmdEndTurn ()
	{
		//ScoreManager table = GetComponent<ScoreManager> ();

		//evntCrds.Add (table.evntCrdsDeck[0]);
		evntCrds.Add (new Card (CardType.Event).ID);
		//table.evntCrdsDeck.RemoveAt (0);
	}

	/// <summary>
	/// plays the ingredient card at the given index in the players hand. 
	/// </summary>
	/// <param name="ind">Index of the ingredient card to play. </param>
	public void playIngrCard(int ind)
	{
		//Debug.Log (ind);
		//Debug.Log (ingCrds[ind]);
		//Debug.Log (GameObject.Find("Canvas/Panel Player HUD/Panel Ingredients/Button CRD1").GetComponent<Button> ().onClick.GetPersistentEventCount ());
		//check if selected card exists or that adding the card won't exceed the ingredient limit
		if (ingCrds.Count <= ind || (sandwich.Count + 1) >= maxIngredientsAllowed)
			return;

		sandwich.Add (ingCrds[ind]);
		ingCrds.RemoveAt (ind);

		string tmp = "";
		foreach (int i in sandwich)
		{
			tmp += i.ToString () + "|";
		}

		tmp = tmp.Substring (0, tmp.Length - 1);
		CmdPlayIngredient (tmp, UID);
	}

	[Command]
	public void CmdPlayIngredient (string tmp, int PID)
	{
		//Debug.Log (tmp);

		//sandwich.Add (ingCrds[ind]);
		//ingCrds.RemoveAt (ind);

		//List<int> tmp = new List<int> ();
		//foreach(int i in sandwich)
		//{
		//	tmp.Add (i);
		//}

		RpcDisplayCards ();
		RpcDrawSandwich (tmp, PID);
	}

	[ClientRpc]
	public void RpcDrawSandwich (string sndwch, int PID)
	{
		//if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
		//	return;

		//decide which panel to use
		CanvasGroup sndwchPanel = PID == 1 ?
				GameObject.Find ("Canvas/Panel Category/Sandwich P1").GetComponent<CanvasGroup> () :
				GameObject.Find ("Canvas/Panel Category/Sandwich P2").GetComponent<CanvasGroup> ();

		//first destroy all the old images before adding new ones
		foreach (Transform trans in sndwchPanel.GetComponentsInChildren<Transform> ())
		{
			if (trans.tag.Equals ("IngredientImage"))
				Destroy (trans.gameObject);
		}

		string[] tmp = sndwch.Split ('|');
		List<int> betterSandwich = new List<int> ();
		foreach(string s in tmp)
		{
			betterSandwich.Add (int.Parse (s));
		}

		int cnt = 0;
		foreach (int i in betterSandwich)
		{
			//get the associated image and add it
			//add and destroy image objects as necessary?? removing cards?
			Image newImg = Instantiate (Resources.Load ("Image Ingredient", typeof (Image)) as Image);
			newImg.transform.SetParent (sndwchPanel.transform);
			newImg.rectTransform.localPosition = Vector3.zero - Vector3.up * (40 * cnt);
			newImg.GetComponent<CardViewer> ().updateInfoForSndwchImgs ();
			newImg.sprite = Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (i), typeof (Sprite)) as Sprite;
			cnt += 1;

		}
	}

	/// <summary>
	/// Plays the event card at the given index. 
	/// </summary>
	/// <param name="ind">Index of the ingredient card to play. </param>
	public void playEvntCard(int ind)
	{
		Debug.Log (ind);
		//Debug.Log (evntCrds[ind]);
		switch (evntCrds[ind]) //call function action based on event ID
		{
			case 0:
				CmdBakery ();
				break;

			case 1:
				CmdGrocer ();
				break;

			case 2:
				CmdGardenFreshVeggies ();
				break;

			case 3:
				CmdFishMarket ();
				break;

			case 4:
				CmdButcher ();
				break;
		}

		//if (evntCrds.Count > ind && evntInd > 0)
		//	evntInd -= 1;
		CmdDelPlayedEvnt (ind);
	}

	[Command]
	public void CmdDelPlayedEvnt(int ind)
	{
		evntCrds.RemoveAt (ind);
	}

	//iterate over deck and add first bread item found
	[Command]
	public void CmdBakery ()
	{
		//ScoreManager table = GameObject.FindGameObjectWithTag ("Table").GetComponent<ScoreManager> ();
		//ScoreManager table = GetComponent<ScoreManager> ();

		ingCrds.Add (new Card (IngredientType.Bread).ID);
		//foreach (int i in table.ingrCrdsDeck)
		//{
		//	if(Card.getIngredientStats(i).IngredientType == IngredientType.Bread)
		//	{
		//		ingCrds.Add (i);
		//		table.ingrCrdsDeck.Remove (i);
		//	}
		//}
	}

	//add the top two ingredient cards to your hand
	[Command]
	public void CmdGrocer ()
	{
		//ScoreManager table = GetComponent<ScoreManager> ();

		ingCrds.Add (new Card (CardType.Ingredient).ID);
		ingCrds.Add (new Card (CardType.Ingredient).ID);

		//ingCrds.Add (table.ingrCrdsDeck[0]);
		//ingCrds.Add (table.ingrCrdsDeck[1]);

		//removes first two items
		//table.ingrCrdsDeck.RemoveAt (0);
		//table.ingrCrdsDeck.RemoveAt (0);
	}

	//adds a veggie
	[Command]
	public void CmdGardenFreshVeggies ()
	{
		//ScoreManager table = GetComponent<ScoreManager> ();

		//foreach (int i in table.ingrCrdsDeck)
		//{
		//	if (Card.getIngredientStats (i).IngredientType == IngredientType.Veggies)
		//	{
		//		ingCrds.Add (i);
		//		table.ingrCrdsDeck.Remove (i);
		//	}
		//}

		ingCrds.Add (new Card (IngredientType.Veggies).ID);
	}

	//draw random card from set of first 3 cards in deck
	[Command]
	public void CmdFishMarket ()
	{
		//ScoreManager table = GetComponent<ScoreManager> ();

		//int rand = Random.Range (0, 3);

		//ingCrds.Add (table.ingrCrdsDeck[rand]);
		//table.ingrCrdsDeck.RemoveAt (rand);

		ingCrds.Add (new Card (CardType.Ingredient).ID);
	}

	//adds a meat
	[Command]
	public void CmdButcher ()
	{
		//ScoreManager table = GetComponent<ScoreManager> ();

		//foreach (int i in table.ingrCrdsDeck)
		//{
		//	if (Card.getIngredientStats (i).IngredientType == IngredientType.Meat)
		//	{
		//		ingCrds.Add (i);
		//		table.ingrCrdsDeck.Remove (i);
		//	}
		//}

		ingCrds.Add (new Card (IngredientType.Meat).ID);
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
		//ScoreManager table = GetComponent<ScoreManager> ();

		//for (int x = 0; x < maxIngredientsAllowed; x += 1)
		//{
		//	if (x < maxEventCardsAllowed /*&& (evntCrds.Count < maxEventCardsAllowed || table.evntCrdsDeck.Count > 0)*/)
		//	{
		//		evntCrds.Add (table.evntCrdsDeck[0]);
		//		table.evntCrdsDeck.RemoveAt (0);
		//	}

		//	//ingCrds.Add (new Card (Type.Ingredient).ID);
		//	ingCrds.Add (table.ingrCrdsDeck[0]);
		//	table.ingrCrdsDeck.RemoveAt (0);
		//}

		RpcAddCards ();
		RpcDisplayCards ();
	}

	[ClientRpc]
	public void RpcAddCards ()
	{
		//ScoreManager table = GetComponent<ScoreManager> ();

		for (int x = 0; x < maxIngredientsAllowed; x += 1)
		{
			if (x < maxEventCardsAllowed /*&& (evntCrds.Count < maxEventCardsAllowed || table.evntCrdsDeck.Count > 0)*/)
			{
				evntCrds.Add (new Card (CardType.Event).ID);
				//evntCrds.Add (table.evntCrdsDeck[0]);
				//table.evntCrdsDeck.RemoveAt (0);
			}

			//ingCrds.Add (new Card (Type.Ingredient).ID);
			ingCrds.Add (new Card (CardType.Ingredient).ID);
			//ingCrds.Add (table.ingrCrdsDeck[0]);
			//table.ingrCrdsDeck.RemoveAt (0);
		}
	}

	[ClientRpc]
	public void RpcDisplayCards ()
	{
		//only update the UI for the player that initiated the call
		if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
			return;

		//ScoreManager table = GetComponent<ScoreManager> ();

		//for (int x = 0; x < maxIngredientsAllowed; x += 1)
		//{
		//	if (x < maxEventCardsAllowed /*&& (evntCrds.Count < maxEventCardsAllowed || table.evntCrdsDeck.Count > 0)*/)
		//	{
		//		evntCrds.Add (table.evntCrdsDeck[0]);
		//		table.evntCrdsDeck.RemoveAt (0);
		//	}

		//	//ingCrds.Add (new Card (Type.Ingredient).ID);
		//	ingCrds.Add (table.ingrCrdsDeck[0]);
		//	table.ingrCrdsDeck.RemoveAt (0);
		//}

		HUD = GameObject.Find ("Canvas/Panel Player HUD").GetComponent<CanvasGroup> ();
		GameObject.Find("Canvas/Panel Player HUD/Panel Events/Button CRD7").GetComponent<Image> ().sprite =
					Resources.Load ("Cards/Events/" + Card.getEvntImg (evntCrds[0]), typeof (Sprite)) as Sprite;
		//GameObject.Find ("Canvas/Panel Player HUD/Panel Events/Button CRD7").GetComponent<Button> ().onClick.AddListener (() => playEvntCard (0));

		//Debug.Log (ingCrds.Count);
		for (int x = 2; x < 7; x += 1)
		{
			//Debug.Log (x - 2);
			//if (ingCrds.Count > (x - 2))
			//{
			//	HUD.transform.GetChild (0).GetChild (x).GetComponent<Image> ().sprite =
			//		Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[x - 2]), typeof (Sprite)) as Sprite;
			//}
			//else
			//{
			//	HUD.transform.GetChild (0).GetChild (x).GetComponent<Image> ().sprite =
			//		Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;
			//}

			HUD.transform.GetChild (0).GetChild (x).GetComponent<Image> ().sprite = ingCrds.Count > (x-2) ?
				Resources.Load ("Cards/Ingredients/" + Card.getIngrdntImg (ingCrds[x - 2]), typeof (Sprite)) as Sprite :
				Resources.Load ("Cards/Blank", typeof (Sprite)) as Sprite;

			//HUD.transform.GetChild (0).GetChild (x).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (x));

		}

		//HUD.transform.GetChild (0).GetChild (2).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (0));
		//HUD.transform.GetChild (0).GetChild (3).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (1));
		//HUD.transform.GetChild (0).GetChild (4).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (2));
		//HUD.transform.GetChild (0).GetChild (5).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (3));
		//HUD.transform.GetChild (0).GetChild (6).GetComponent<Button> ().onClick.AddListener (() => playIngrCard (4));

		//RpcUpdateEventCardsDisplay (0);
		//RpcUpdateIngredientCardsDisplay (0);
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