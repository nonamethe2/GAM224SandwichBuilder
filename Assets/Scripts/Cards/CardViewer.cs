using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardViewer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

	public float scale = 2.6f;
	public float delay = 0.01f;

	private Vector3 startPos;
	private Vector2 defScale;

	[Header("Direction to offset card")]
	public bool moveRight = false;
	public bool noMove = false;


	private int siblingIndex = -1;
	private Transform prnt; //parent of the object at start

	void Start ()
	{
		startPos = GetComponent<RectTransform> ().localPosition;
		defScale = new Vector2 (133, 186);
		siblingIndex = transform.GetSiblingIndex ();
		prnt = transform.parent;
	}

	public void updateInfoForSndwchImgs ()
	{
		startPos = GetComponent<RectTransform> ().localPosition;
		siblingIndex = transform.GetSiblingIndex ();
		prnt = transform.parent;
	}

	public void OnPointerEnter (PointerEventData eventData)
	{
		Vector3 curPos = GetComponent<RectTransform> ().localPosition;

		if (curPos != startPos)
			return;

		GetComponent<RectTransform> ().sizeDelta = new Vector2 (133 * scale, 186 * scale);
		transform.SetParent (GameObject.Find ("Canvas").GetComponent<Transform> ());
		transform.SetAsLastSibling ();

		if (noMove)
			return;

		if (moveRight)
			GetComponent<RectTransform> ().localPosition += Vector3.right * 90;
		else
			GetComponent<RectTransform> ().localPosition += Vector3.up * 150;
	}

	public void OnPointerExit (PointerEventData eventData)
	{
		transform.SetParent (prnt);
		transform.SetSiblingIndex (siblingIndex);

		GetComponent<RectTransform> ().localPosition = startPos;
		GetComponent<RectTransform> ().sizeDelta = defScale;
	}

	/// <summary>
	/// Attached to the buttons action listener so it's called when clicked. 
	/// Should be called before other action listeners. 
	/// </summary>
	public void btnResetTrans ()
	{
		transform.SetParent (prnt);
		transform.SetSiblingIndex (siblingIndex);
	}

	//IEnumerator MoveToPos (Vector3 newPosition, Vector2 newScale, float time)
	//{
	//	float elapsedTime = 0;
	//	Vector3 startingPos = GetComponent<RectTransform> ().localPosition;
	//	Vector2 startScale = GetComponent<RectTransform> ().sizeDelta;
	//	while (elapsedTime < time)
	//	{
	//		GetComponent<RectTransform> ().localPosition = Vector3.Lerp (startingPos, newPosition, (elapsedTime / time));
	//		GetComponent<RectTransform> ().sizeDelta = Vector2.Lerp (startScale, newScale, (elapsedTime / time));
	//		elapsedTime += Time.deltaTime;
	//		yield return new WaitForSeconds (delay);
	//	}
	//}
}
