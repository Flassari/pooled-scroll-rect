using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomScrollRect : ScrollRect, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	private Func<int, GameObject, GameObject> createItemCallback;
	
	private bool isDragging = false;
	private PointerEventData lastBeginDragEventData;

	private RectTransform contentRectTransform;

	private List<VirtualListItem> virtualItems;
	private float spacing;

	public void Initialize(Func<int, GameObject, GameObject> createItemCallback)
	{
		this.createItemCallback = createItemCallback;
		
		contentRectTransform = content.GetComponent<RectTransform> ();
		spacing = content.GetComponent<VerticalLayoutGroup> ().spacing;
		virtualItems = new List<VirtualListItem> ();

		StopMovement ();
		verticalNormalizedPosition = 0;

		while (contentRectTransform.offsetMin.y > 0)
		{
			AddChild ();
		}
	}

	private void AddChild(bool setAsFirstSibling = false)
	{
		int index = 0;
		if (virtualItems.Count > 0)
		{
			if (setAsFirstSibling)
			{
				index = virtualItems [0].index - 1;
			}
			else
			{
				index = virtualItems[virtualItems.Count - 1].index + 1;
			}
		}

		GameObject child = createItemCallback (index, null);
		child.transform.SetParent (content.transform);

		if (setAsFirstSibling)
		{
			child.transform.SetAsFirstSibling ();
		}

		Canvas.ForceUpdateCanvases ();

		if (setAsFirstSibling)
		{
			float childHeightAndSpacing = child.GetComponent<RectTransform> ().rect.height + spacing;
			SetContentAnchoredPos (new Vector2(contentRectTransform.anchoredPosition.x, contentRectTransform.anchoredPosition.y + childHeightAndSpacing));
			virtualItems.Insert (0, new VirtualListItem(child, index));

		}
		else
		{
			virtualItems.Add (new VirtualListItem(child, index));

		}
	}
	
	public void SetContentAnchoredPos(Vector2 pos)
	{
		if (isDragging)
		{
			OnEndDrag (new PointerEventData(null) { button = PointerEventData.InputButton.Left });
		}

		SetContentAnchoredPosition (pos);
	}

	protected void Update()
	{
		bool wasDragging = isDragging;

		// Remove children from top
		while (virtualItems.Count > 0 && verticalNormalizedPosition > 0 && contentRectTransform.offsetMax.y > ((RectTransform)virtualItems[0].gameObject.transform).rect.height + spacing)
		{
			Debug.Log ("Item should be removed." + contentRectTransform.offsetMax.y + " > " + virtualItems[0].gameObject.GetComponent<RectTransform>().rect.height  + " + "  + spacing);

			RemoveChildFromTop();
		}

		// Add children to bottom
		while (contentRectTransform.offsetMin.y > 0)
		{
			AddChild ();
		}

		// Add children to top
		while (contentRectTransform.offsetMax.y < 0)
		{
			AddChild (setAsFirstSibling: true);
		}

		if (wasDragging && !isDragging)
		{
			lastBeginDragEventData.position = Input.mousePosition;
			Rebuild (CanvasUpdate.PostLayout);
			OnBeginDrag(lastBeginDragEventData);
		}
	}

	private void RemoveChildFromTop()
	{
		VirtualListItem itemToRemove = virtualItems [0];
		GameObject childToRemove = itemToRemove.gameObject;

		float childHeightAndSpacing = childToRemove.GetComponent<RectTransform> ().rect.height + spacing;

		SetContentAnchoredPos (new Vector2(contentRectTransform.anchoredPosition.x, contentRectTransform.anchoredPosition.y - childHeightAndSpacing));

		Destroy (childToRemove);
		virtualItems.Remove (itemToRemove);

		Canvas.ForceUpdateCanvases ();
	}

	override public void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		if (!IsActive())
			return;

		Vector2 mouse = Input.mousePosition;

		lastBeginDragEventData = eventData;
		isDragging = true;
		base.OnBeginDrag (eventData);
	}

	override public void OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		isDragging = false;
		base.OnEndDrag (eventData);
	}

	override public void OnDrag(PointerEventData eventData)
	{
		if (!isDragging) return;
		base.OnDrag (eventData);
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
