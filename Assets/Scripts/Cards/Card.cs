using System.Collections.Generic;
using System.Xml;

/// <summary>
/// Base type for cards. 
/// Stores references to ID, type, and image
/// </summary>
public class Card {

	public int ID = -1;
	public CardType cardType;
	public IngredientType IngredientType;

	public int meaty, salty, sour, spicy, sweet = 0;
	public int crunch, juicy, soft = 0;

	public string name = "";

	//only used for category cards to store the generic values
	public int flavor = 0;
	public int texture = 0;
	public string bonus = "";

	//only used for event cards
	public string effect = "";
	public string option = "";

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="newType">the Type of the card from the enum</param>
	public Card (CardType newType)
	{
		cardType = newType;
		genID ();
	}

	/// <summary>
	/// Constructor overload to set the ID and type. 
	/// </summary>
	/// <param name="CID">Unique ID for the card, will NOT be checked to ensure uniqueness. </param>
	public Card(int CID)
	{
		ID = CID;
	}
	
	/// <summary>
	/// Generates the ID of the card based on its type. 
	/// Will not change ID if it has been previously set. 
	/// </summary>
	/// <returns>The ID of the card</returns>
	private int genID ()
	{
		if (ID != -1)
			return ID;

		switch (cardType)
		{
			case CardType.Ingredient:
				ID = UnityEngine.Random.Range (0, 28); 
				break;

			case CardType.Event:
				ID = UnityEngine.Random.Range (0, 7);
				break;

			case CardType.Category:
				ID = UnityEngine.Random.Range (0, 5);
				break;
		}

		return ID;
	}

	/// <summary>
	/// Gets the sum of all the flavor values of the card. 
	/// </summary>
	/// <returns>The total flavor. </returns>
	public int getTotalFlavor ()
	{
		return meaty + salty + sour + spicy + sweet;
	}

	/// <summary>
	/// Gets the sum of all the texture valus of the card. 
	/// </summary>
	/// <returns>The total Texture. </returns>
	public int getTotalTexture ()
	{
		return crunch + juicy + soft;
	}

	/// <summary>
	/// Returns all the flavors on the card as a List with type Flavor. 
	/// </summary>
	/// <returns>List of all flavors on the card. </returns>
	public List<Flavor> allFlavors ()
	{
		List<Flavor> flavors = new List<Flavor> ();
		if (meaty > 0)
			flavors.Add (Flavor.Meaty);

		if (salty > 0)
			flavors.Add (Flavor.Salty);

		if (sour > 0)
			flavors.Add (Flavor.Sour);

		if (spicy > 0)
			flavors.Add (Flavor.Spicy);

		if (sweet > 0)
			flavors.Add (Flavor.Sweet);

		return flavors;
	}

	/// <summary>
	/// Returns all the textures on the card as a list with type Texture. 
	/// </summary>
	/// <returns>List of all flavors on the card. </returns>
	public List<Texture> allTextures ()
	{
		List<Texture> textures = new List<Texture> ();
		if (crunch > 0)
			textures.Add (Texture.Crunch);

		if (juicy > 0)
			textures.Add (Texture.Juicy);

		if (soft > 0)
			textures.Add (Texture.Soft);

		return textures;
	}

	/// <summary>
	/// Gets the image associated with the ID. 
	/// Called in a static context to allow getting the image without a whole card object. 
	/// </summary>
	/// <param name="CID">Card ID</param>
	/// <returns>Image type of the card image</returns>
	public static string getIngrdntImg (int CID)
	{
		return getIngredientStats (CID).name;
	}

	public static string getEvntImg(int CID)
	{
		return getEventCardStats (CID).name;
	}

	/// <summary>
	/// Compares the values of two cards to determine the "winner". 
	/// Called in a static context to allow comparing the values without an object reference. 
	/// -1 for p1 victury, 1 for p2 victory, and 0 for a tie.
	/// </summary>
	/// <param name="crd1">Player 1. </param>
	/// <param name="crd2">Player 2. </param>
	/// <returns>-1 for p1 victury, 1 for p2 victory, and 0 for a tie. </returns>
	public static int cmprCards(int crd1, int crd2)
	{

		return -1;
	}

	public static Card getEventCardStats (int CID)
	{
		Card newCard = new Card (CID);
		newCard.cardType = CardType.Event;

		//UnityEngine.Debug.Log (CID);

		UnityEngine.TextAsset sandwichesTXT = UnityEngine.Resources.Load ("EventCards", typeof (UnityEngine.TextAsset)) as UnityEngine.TextAsset;
		XmlDocument xmlDoc = new XmlDocument ();
		xmlDoc.LoadXml (sandwichesTXT.text);

		XmlNodeList evntCards = xmlDoc.GetElementsByTagName ("Card");
		foreach (XmlNode node in evntCards)
		{
			if (node.ChildNodes[0].InnerXml == CID.ToString ())
				newCard.name = node.ChildNodes[1].InnerXml;

		}

		//UnityEngine.Debug.Log ("STATIC FUNC  " + newCard.name);
		return newCard;
	}

	public static Card getIngredientStats(int CID)
	{
		Card newCard = new Card (CID);
		newCard.cardType = CardType.Ingredient;

		UnityEngine.TextAsset sandwichesTXT = UnityEngine.Resources.Load ("IngredientCards", typeof (UnityEngine.TextAsset)) as UnityEngine.TextAsset;
		XmlDocument xmlDoc = new XmlDocument ();
		xmlDoc.LoadXml (sandwichesTXT.text);

		XmlNodeList ingCards = xmlDoc.GetElementsByTagName ("Card");
		foreach (XmlNode node in ingCards)
		{
			//skip cards that don't match the one requested
			if (newCard.ID != int.Parse (node.ChildNodes[0].InnerXml))
				continue;

			//set name and card type
			newCard.name = node.ChildNodes[1].InnerXml;
			//newCard.cardType = (CardType)System.Enum.Parse (typeof (CardType), node.ChildNodes[2].InnerXml);

			newCard.IngredientType = (IngredientType)System.Enum.Parse (typeof (IngredientType), node.ChildNodes[3].InnerXml);
			foreach (XmlNode childNode in node.ChildNodes)
			{
				int value = int.TryParse (childNode.InnerXml, out value) ? value : -1;

				if (childNode.Name == "Meaty")
					newCard.meaty = value;

				if (childNode.Name == "Salty")
					newCard.salty = value;

				if (childNode.Name == "Sour")
					newCard.salty = value;

				if (childNode.Name == "Spicy")
					newCard.spicy = value;

				if (childNode.Name == "Sweet")
					newCard.sweet = value;


				if (childNode.Name == "Crunch")
					newCard.crunch = value;

				if (childNode.Name == "Juicy")
					newCard.juicy = value;

				if (childNode.Name == "Soft")
					newCard.soft = value;
			}

			break;
		}

		return newCard;
	}

	//public static Card getStats (int CID, CardType cType)
	//{
	//	Card newCard = new Card (CID);
	//	newCard.cardType = cType;

	//	UnityEngine.TextAsset sandwichesTXT = UnityEngine.Resources.Load ("Cards", typeof (UnityEngine.TextAsset)) as UnityEngine.TextAsset;
	//	XmlDocument xmlDoc = new XmlDocument ();
	//	xmlDoc.LoadXml (sandwichesTXT.text);

	//	switch (cType)
	//	{
	//		case CardType.Ingredient:
	//			XmlNodeList ingCards = xmlDoc.GetElementsByTagName ("IngredientCard");
	//			foreach (XmlNode node in ingCards)
	//			{
	//				//skip cards that don't match the one requested
	//				if (newCard.ID != int.Parse (node.ChildNodes[0].InnerXml))
	//					continue;

	//				//set name and card type
	//				newCard.name = node.ChildNodes[1].InnerXml;
	//				//newCard.cardType = (CardType)System.Enum.Parse (typeof (CardType), node.ChildNodes[2].InnerXml);

	//				newCard.IngredientType = (IngredientType)System.Enum.Parse (typeof (IngredientType), node.ChildNodes[3].InnerXml);
	//				foreach (XmlNode childNode in node.ChildNodes)
	//				{
	//					int value = int.TryParse (childNode.InnerXml, out value) ? value : -1;

	//					if (childNode.Name == "Meaty")
	//						newCard.meaty = value;

	//					if (childNode.Name == "Salty")
	//						newCard.salty = value;

	//					if (childNode.Name == "Sour")
	//						newCard.salty = value;

	//					if (childNode.Name == "Spicy")
	//						newCard.spicy = value;

	//					if (childNode.Name == "Sweet")
	//						newCard.sweet = value;


	//					if (childNode.Name == "Crunch")
	//						newCard.crunch = value;

	//					if (childNode.Name == "Juicy")
	//						newCard.juicy = value;

	//					if (childNode.Name == "Soft")
	//						newCard.soft = value;
	//				}
	//			}
	//			break;

	//		case CardType.Event:
	//			XmlNodeList evntCards = xmlDoc.GetElementsByTagName ("EventCard");
	//			foreach (XmlNode node in evntCards)
	//			{

	//			}
	//			break;

	//		case CardType.Category:
	//			XmlNodeList catCards = xmlDoc.GetElementsByTagName ("CategoryCard");
	//			foreach (XmlNode node in catCards)
	//			{

	//			}
	//			break;
	//	}
		

	//	return newCard;
	//}
}

/// <summary>
/// Enum of the possible types of cards
/// None included for error handling-  it is not considered a valid card type for gameplay
/// </summary>
public enum CardType
{
	None, Ingredient, Event, Category
}

public enum IngredientType
{
	None, Bread, Cheese, Meat, Veggies
}

public enum Flavor
{
	None, Meaty, Salty, Sour, Spicy, Sweet
}

public enum Texture
{
	None, Crunch, Juicy, Soft
}