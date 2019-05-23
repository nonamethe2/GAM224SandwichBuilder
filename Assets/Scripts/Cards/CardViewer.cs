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

	private bool isLerping = false;

	//private Task task;

	[Header("Expand to the right")]
	public bool moveRight = false;

	private int siblingIndex = -1;

	void Start ()
	{
		startPos = GetComponent<RectTransform> ().localPosition;
		defScale = new Vector2 (133, 186);
		siblingIndex = transform.GetSiblingIndex ();
	}

	public void OnPointerEnter (PointerEventData eventData)
	{
		//if (task == null || !task.Running)
		//{
		//	if (GetComponent<RectTransform> ().localPosition != startPos)
		//	{
		//		task = new Task (MoveToPos (startPos, scaleVector, 0.6f));
		//		return;
		//	}

		//	if (moveRight)
		//		task = new Task (MoveToPos (GetComponent<RectTransform> ().localPosition + Vector3.right * 90, new Vector3 (133 * scale, 186 * scale), 0.8f));
		//	else
		//		task = new Task (MoveToPos (GetComponent<RectTransform> ().localPosition + Vector3.up * 150, new Vector2 (133 * scale, 186 * scale), 0.8f));

		//	task.Finished += delegate (bool man)
		//	{
		//		task = new Task (MoveToPos (startPos, scaleVector, 0.6f));
		//	};
		//}

		Vector3 curPos = GetComponent<RectTransform> ().localPosition;

		if (curPos != startPos)
			return;

		if (moveRight)
			GetComponent<RectTransform> ().localPosition += Vector3.right * 90;
		else
			GetComponent<RectTransform> ().localPosition += Vector3.up * 150;

		GetComponent<RectTransform> ().sizeDelta = new Vector2 (133 * scale, 186 * scale);
		transform.SetAsLastSibling ();
	}

	public void OnPointerExit (PointerEventData eventData)
	{
		//if (!task.Running)
		//	task = new Task (MoveToPos (startPos, scaleVector, 0.6f));

		GetComponent<RectTransform> ().localPosition = startPos;
		GetComponent<RectTransform> ().sizeDelta = defScale;
		transform.SetSiblingIndex (siblingIndex);
	}

	IEnumerator MoveToPos (Vector3 newPosition, Vector2 newScale, float time)
	{
		float elapsedTime = 0;
		Vector3 startingPos = GetComponent<RectTransform> ().localPosition;
		Vector2 startScale = GetComponent<RectTransform> ().sizeDelta;
		while (elapsedTime < time)
		{
			GetComponent<RectTransform> ().localPosition = Vector3.Lerp (startingPos, newPosition, (elapsedTime / time));
			GetComponent<RectTransform> ().sizeDelta = Vector2.Lerp (startScale, newScale, (elapsedTime / time));
			elapsedTime += Time.deltaTime;
			yield return new WaitForSeconds (delay);
		}
	}
}
