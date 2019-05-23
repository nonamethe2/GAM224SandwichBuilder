using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ScoreManager : NetworkBehaviour {

	//constants for the size of the decks
	private const int MAX_EVENT_CARDS = 30;
	private const int MAX_INGREDIENT_CARDS = 70;

	//stores the currently active category cards
	public SyncListInt catCrds = new SyncListInt (); 

	//the two decks cards are drawn from
	public SyncListInt ingrCrdsDeck = new SyncListInt (); 
	public SyncListInt evntCrdsDeck = new SyncListInt ();

	//sandwiches currently on the table
	//public SyncListInt sandwichP1 = new SyncListInt ();
	//public SyncListInt sandwichP2 = new SyncListInt ();

	//not sure if bools sync?
	[SyncVar(hook = "UpdateTurn")]
	public int activeTurn = 0; //0 means the active turn is player 1

	private int activeCat = 0; //the currently displayed category card (checking all active categories will not affect others)

	void Start () {
		DontDestroyOnLoad (gameObject);
	}

	public void btnNextCatCrd ()
	{
		CmdNextCatCrd ();
	}

	[Command]
	public void CmdNextCatCrd ()
	{
		RpcNextCatCrd ();
	}

	[ClientRpc]
	public void RpcNextCatCrd ()
	{
		if (!GetComponent<NetworkIdentity> ().isLocalPlayer)
			return;

		activeCat += 1;
		if (activeCat >= catCrds.Count)
			activeCat = 0;

		//disable current sandwiches and category card
		//enable the next one
	}

	public void btnPrevCatCrd ()
	{

	}

	/// <summary>
	/// Hook function caled when activeTurn is changed. 
	/// </summary>
	/// <param name="val">The new value. </param>
	public void UpdateTurn (int val)
	{
		activeTurn = val;
	}

	/// <summary>
	/// Function linked to the GUI button that will begin to create the decks.
	/// </summary>
	public void genDeckBtnResponse ()
	{
		CmdGenDeck ();
	}

	//server command to generate the deck and enforce synchronisity
	[Command]
	public void CmdGenDeck ()
	{
		for (int x = 0; x < MAX_INGREDIENT_CARDS; x += 1)
		{
			ingrCrdsDeck.Add (new Card (CardType.Ingredient).ID);
		}

		for (int x = 0; x < MAX_EVENT_CARDS; x += 1)
		{
			evntCrdsDeck.Add (new Card (CardType.Event).ID);
		}

		RpcGenDeck ();
	}

	//disables the button
	[ClientRpc]
	public void RpcGenDeck ()
	{
		GameObject.Find ("Canvas/Button Create Deck").SetActive (false);
	}

	//adds a new ACTIVE category card. It will be placed on the table in the following RPC call.
	[Command]
	public void CmdAddNewCatCard ()
	{
		catCrds.Add (new Card (CardType.Category).ID);
		RpcAddNewCatCard ();
	}

	//TODO: needs to be rewritten
	//currently places only the first category card on the table
	[ClientRpc]
	public void RpcAddNewCatCard ()
	{
		//GameObject.Find ("Canvas/Panel Category/Text TEMP Cat").GetComponent<Text> ().text = catCrds[0].ToString ();
		GameObject.Find ("Canvas/Panel Category/Image Cat Card").GetComponent<Image> ().sprite = 
			Resources.Load ("Cards/Category/" + catCrds[0].ToString (), typeof (Sprite)) as Sprite;
	}

	/// <summary>
	/// Linked to the GUI button that ends the players turn. 
	/// Currently only updates the counter. 
	/// </summary>
	public void endTurn ()
	{
		activeTurn += 1;
		if (activeTurn > 1)
			activeTurn = 0;
	}

	/// <summary>
	/// TODO: needs to enforce only being able to play a new one when a sandwich is completed
	/// Linked to the GUI button that will play a new category card. 
	/// </summary>
	public void playNewCat ()
	{
		CmdAddNewCatCard ();
	}
}
