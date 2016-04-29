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
			AddChildToBottom ();
		}
	}

	private void AddChildToBottom()
	{
		int index = virtualItems.Count == 0 ? 0 : virtualItems [virtualItems.Count - 1].index + 1;

		GameObject child = createItemCallback (0, null);

		child.transform.SetParent (content.transform);
		child.GetComponent<Child> ().text.text = "#" + index;

		Canvas.ForceUpdateCanvases ();

		virtualItems.Add (new VirtualListItem(child, index));
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

		while (virtualItems.Count > 0 && verticalNormalizedPosition > 0 && contentRectTransform.offsetMax.y > ((RectTransform)virtualItems[0].gameObject.transform).rect.height + spacing)
		{
			Debug.Log ("Item should be removed." + contentRectTransform.offsetMax.y + " > " + virtualItems[0].gameObject.GetComponent<RectTransform>().rect.height  + " + "  + spacing);

			RemoveChildFromTop();
		}

		while (contentRectTransform.offsetMin.y > 0)
		{
			AddChildToBottom ();
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
