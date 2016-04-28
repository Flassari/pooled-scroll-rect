using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VirtualListTest : MonoBehaviour
{
	public Child childPrefab;
	public ScrollRect scrollRect;

	private RectTransform rectTransform;
	private float spacing;
	private List<VirtualListItem> virtualItems;

	void Start ()
	{
		virtualItems = new List<VirtualListItem> ();
		rectTransform = GetComponent<RectTransform> ();

		spacing = GetComponent<VerticalLayoutGroup> ().spacing;

		scrollRect.StopMovement ();
		scrollRect.verticalNormalizedPosition = 0;

		while (rectTransform.offsetMin.y > 0)
		{
			AddChildToBottom ();
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.N))
		{
			AddChildToBottom ();
		}

		while (scrollRect.verticalNormalizedPosition < 1 && rectTransform.offsetMax.y > virtualItems[0].gameObject.GetComponent<RectTransform>().rect.height + spacing)
		{
			Debug.Log ("Item should be removed." + rectTransform.offsetMax.y + " > " + virtualItems[0].gameObject.GetComponent<RectTransform>().rect.height  + " + "  + spacing);

			RemoveChildFromTop();
		}

		/*if (Input.GetKeyDown(KeyCode.D))
		{
			GameObject childToRemove = virtualItems [0];

			float childHeightAndSpacing = childToRemove.GetComponent<RectTransform> ().rect.height + spacing;
			float totalScrollableHeight = rectTransform.rect.height - transform.parent.GetComponent<RectTransform>().rect.height;
			float delta = childHeightAndSpacing / totalScrollableHeight;

			scrollRect.verticalNormalizedPosition += delta;
		}*/

		/*while (rectTransform.offsetMin.y > 0)
		{
			AddChildToBottom ();
		}*/
	}

	void AddChildToBottom()
	{
		int index = virtualItems.Count == 0 ? 0 : virtualItems [virtualItems.Count - 1].index + 1;
		
		GameObject child;

		child = GetListItem (0, null);
		child.transform.SetParent (transform);

		child.GetComponent<Child> ().text.text = "#" + index;

		Canvas.ForceUpdateCanvases ();
		Debug.Log (GetComponent<RectTransform>().offsetMin);
		Debug.Log (GetComponent<RectTransform>().offsetMax);

		virtualItems.Add (new VirtualListItem(child, index));
	}

	void RemoveChildFromTop()
	{
		VirtualListItem itemToRemove = virtualItems [0];
		GameObject childToRemove = itemToRemove.gameObject;

		float childHeightAndSpacing = childToRemove.GetComponent<RectTransform> ().rect.height + spacing;
		float totalScrollableHeight = rectTransform.rect.height - transform.parent.GetComponent<RectTransform>().rect.height;
		float delta = childHeightAndSpacing / totalScrollableHeight;

		Vector2 currentVelocity = scrollRect.velocity;
		scrollRect.verticalNormalizedPosition += delta;
		scrollRect.velocity = currentVelocity;

		Destroy (childToRemove);
		virtualItems.Remove (itemToRemove);

		Canvas.ForceUpdateCanvases ();
	}

	private GameObject GetListItem(int index, GameObject pooledObject)
	{
		if (pooledObject == null)
		{
			pooledObject = Instantiate (childPrefab).gameObject;
		}

		Child child = pooledObject.GetComponent<Child> ();
		child.text.text = "#" + index;

		child.GetComponent<LayoutElement> ().preferredHeight = Random.Range (20, 300);

		return child.gameObject;
	}

	class VirtualListItem
	{
		public VirtualListItem(GameObject gameObject, int index)
		{
			this.gameObject = gameObject;
			this.index = index;
		}
		
		public GameObject gameObject;
		public int index;
	}
}

