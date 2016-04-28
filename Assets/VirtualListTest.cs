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

	private LayoutGroup layoutGroup;

	void Start ()
	{
		virtualItems = new List<VirtualListItem> ();
		rectTransform = GetComponent<RectTransform> ();
		layoutGroup = GetComponent<LayoutGroup> ();

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

		// Remove children from top
		while (scrollRect.verticalNormalizedPosition < 1 && rectTransform.offsetMax.y - layoutGroup.padding.top > virtualItems[0].gameObject.GetComponent<RectTransform>().rect.height + spacing)
		{
			RemoveChildFromTop();
		}

		// Add children to bottom
		while (rectTransform.offsetMin.y > 0)
		{
			AddChildToBottom ();
		}

		// Add children to top

		//while (rectTransform.offsetMax.y - layoutGroup.padding.top

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

		layoutGroup.padding.top += Mathf.RoundToInt(childHeightAndSpacing);

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

