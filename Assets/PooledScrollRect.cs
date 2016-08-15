﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PooledScrollRect : ScrollRect, IBeginDragHandler, IEndDragHandler
{
	public delegate GameObject CreateItemCallbackDelegate(int index, GameObject pooledObject);
	public CreateItemCallbackDelegate CreateItemCallback;
	
	private bool isDragging = false;
	private PointerEventData lastBeginDragEventData;
	private RectTransform contentRectTransform;

	private Stack<GameObject> pool = new Stack<GameObject>();
	private LinkedList<VirtualListItem> virtualItems = new LinkedList<VirtualListItem>();
	private float spacing;
	private int maxIndex = -1;

	override protected void Start()
	{
		base.Start();
		StopMovement();
		contentRectTransform = (RectTransform)content.transform;
		spacing = content.GetComponent<VerticalLayoutGroup>().spacing;
	}

	override protected void LateUpdate()
	{
		base.LateUpdate();

		if (CreateItemCallback == null || !content || !contentRectTransform) return;
		
		bool wasDragging = isDragging;

		// Add children to beginning
		while (CanAddChildAt(ChildPosition.First))
		{
			AddChild(ChildPosition.First);
		}

		// Add children to end
		while (CanAddChildAt(ChildPosition.Last))
		{
			AddChild(ChildPosition.Last);
		}

		// Remove children from beginning
		while (virtualItems.Count > 0 && verticalNormalizedPosition >= 0 &&
			contentRectTransform.offsetMax.y > ((RectTransform)virtualItems.First.Value.gameObject.transform).rect.height + spacing)
		{
			RemoveChild(ChildPosition.First);
		}

		// Remove children from end
		while (virtualItems.Count > 0 && verticalNormalizedPosition <= 1 &&
			contentRectTransform.offsetMin.y < -(((RectTransform)virtualItems.Last.Value.gameObject.transform).rect.height + spacing))
		{
			RemoveChild(ChildPosition.Last);
		}
		
		if (wasDragging && !isDragging)
		{
			lastBeginDragEventData.position = Input.mousePosition;
			Rebuild(CanvasUpdate.PostLayout);
			OnBeginDrag(lastBeginDragEventData);
		}
	}

	private bool CanAddChildAt(ChildPosition position)
	{
		if (position == ChildPosition.Last)
		{
			if (maxIndex != -1 && GetNextIndex(position) >= maxIndex)
				return false;
			
			return contentRectTransform.offsetMin.y > 0;
		}
		else
		{
			if (GetNextIndex(position) < 0)
				return false;
			
			return contentRectTransform.offsetMax.y < 0;
		}
	}

	private int GetNextIndex(ChildPosition position)
	{
		int index = 0;
		if (virtualItems.Count > 0)
		{
			if (position == ChildPosition.First)
			{
				index = virtualItems.First.Value.index - 1;
			}
			else
			{
				index = virtualItems.Last.Value.index + 1;
			}
		}

		return index;
	}

	private void AddChild(ChildPosition position)
	{
		int index = GetNextIndex(position);

		GameObject pooledChild = null;
		if (pool.Count > 0)
		{
			pooledChild = pool.Pop();
		}

		GameObject newChild = CreateItemCallback(index, pooledChild);
		if (newChild == null)
		{
			// End of the list
			maxIndex = index;

			// If we popped a pooled child
			if (pooledChild != null)
            {
				// Return it back to the pool
				pool.Push(pooledChild);
				// Make sure it wasn't activated in createItemCallback
				pooledChild.SetActive(false);
			}
			
			return;
		}

		newChild.SetActive(true);
		newChild.transform.SetParent(content.transform);

		if (position == ChildPosition.First)
		{
			newChild.transform.SetAsFirstSibling();
		}
		else
		{
			newChild.transform.SetAsLastSibling();
		}

		Canvas.ForceUpdateCanvases();

		if (position == ChildPosition.First)
		{
			float childHeightAndSpacing = ((RectTransform)newChild.transform).rect.height + spacing;
			SetContentAnchoredPos(new Vector2(contentRectTransform.anchoredPosition.x, contentRectTransform.anchoredPosition.y + childHeightAndSpacing));
			virtualItems.AddFirst(new VirtualListItem(newChild, index));
		}
		else
		{
			virtualItems.AddLast(new VirtualListItem(newChild, index));
		}
	}

	private void RemoveChild(ChildPosition position)
	{
		VirtualListItem itemToRemove = (position == ChildPosition.Last ? virtualItems.Last : virtualItems.First).Value;
		GameObject childToRemove = itemToRemove.gameObject;

		float childHeightAndSpacing = ((RectTransform)childToRemove.transform).rect.height + spacing;

		if (position == ChildPosition.First)
		{
			SetContentAnchoredPos(new Vector2(contentRectTransform.anchoredPosition.x, 
				contentRectTransform.anchoredPosition.y - childHeightAndSpacing));
		}

		childToRemove.SetActive(false);

		if (position == ChildPosition.Last)
		{
			virtualItems.RemoveLast();
		}
		else
		{
			virtualItems.RemoveFirst();
		}

		pool.Push(childToRemove);

		Canvas.ForceUpdateCanvases();
	}

	public void SetContentAnchoredPos(Vector2 pos)
	{
		// ScrollRect bases dragging on the content's beginning position,
		// so when we move the content the dragging goes haywire. To fix this we
		// simply restart the drag in LateUpdate if the content was moved during the frame.
		if (isDragging)
		{
			OnEndDrag(new PointerEventData(null) { button = PointerEventData.InputButton.Left });
		}

		SetContentAnchoredPosition(pos);
	}

	#region Drag overrides
	override public void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		if (!IsActive())
			return;

		lastBeginDragEventData = eventData;
		isDragging = true;
		base.OnBeginDrag(eventData);
	}

	override public void OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		isDragging = false;
		base.OnEndDrag(eventData);
	}
	#endregion

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

	enum ChildPosition
	{
		First,
		Last
	}
}
