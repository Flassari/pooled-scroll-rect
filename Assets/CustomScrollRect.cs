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

	private List<GameObject> pool;
	private List<VirtualListItem> virtualItems;
	private float spacing;

	public void Initialize(Func<int, GameObject, GameObject> createItemCallback)
	{
		this.createItemCallback = createItemCallback;
		
		contentRectTransform = content.GetComponent<RectTransform> ();
		spacing = content.GetComponent<VerticalLayoutGroup> ().spacing;
		pool = new List<GameObject> ();
		virtualItems = new List<VirtualListItem> ();

		StopMovement ();
		verticalNormalizedPosition = 0;

		while (contentRectTransform.offsetMin.y > 0)
		{
			AddChild ();
		}
	}

	protected void Update()
	{
		bool wasDragging = isDragging;

		// Remove children from top
		while (virtualItems.Count > 0 && verticalNormalizedPosition > 0 && contentRectTransform.offsetMax.y > ((RectTransform)virtualItems[0].gameObject.transform).rect.height + spacing)
		{
			RemoveChild();
		}

		// Remove children from bottom
		while (virtualItems.Count > 0 && verticalNormalizedPosition < 1 && contentRectTransform.offsetMin.y < -(((RectTransform)virtualItems[virtualItems.Count - 1].gameObject.transform).rect.height + spacing))
		{
			RemoveChild(removeLastChild: true);
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
			float childHeightAndSpacing = ((RectTransform)child.transform).rect.height + spacing;
			SetContentAnchoredPos (new Vector2(contentRectTransform.anchoredPosition.x, contentRectTransform.anchoredPosition.y + childHeightAndSpacing));
			virtualItems.Insert (0, new VirtualListItem(child, index));
		}
		else
		{
			virtualItems.Add (new VirtualListItem(child, index));
		}
	}

	private void RemoveChild(bool removeLastChild = false)
	{
		VirtualListItem itemToRemove = virtualItems [removeLastChild ? virtualItems.Count - 1 : 0];
		GameObject childToRemove = itemToRemove.gameObject;

		float childHeightAndSpacing = ((RectTransform)childToRemove.transform).rect.height + spacing;

		float newY = removeLastChild ? 
			contentRectTransform.anchoredPosition.y + childHeightAndSpacing : 
			contentRectTransform.anchoredPosition.y - childHeightAndSpacing;

		if (!removeLastChild)
		SetContentAnchoredPos (new Vector2(contentRectTransform.anchoredPosition.x, newY));

		Destroy (childToRemove);
		virtualItems.Remove (itemToRemove);

		Canvas.ForceUpdateCanvases ();
	}

	public void SetContentAnchoredPos(Vector2 pos)
	{
		if (isDragging)
		{
			OnEndDrag (new PointerEventData(null) { button = PointerEventData.InputButton.Left });
		}

		SetContentAnchoredPosition (pos);
	}

	override public void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		if (!IsActive())
			return;

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
