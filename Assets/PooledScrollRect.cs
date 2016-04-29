using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PooledScrollRect : ScrollRect, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	private Func<int, GameObject, GameObject> createItemCallback;
	
	private bool isDragging = false;
	private PointerEventData lastBeginDragEventData;
	private RectTransform contentRectTransform;

	private Stack<GameObject> pool = new Stack<GameObject>();
	private List<VirtualListItem> virtualItems = new List<VirtualListItem>();
	private float spacing;
	private int maxIndex = -1;

	public void Initialize(Func<int, GameObject, GameObject> createItemCallback)
	{
		this.createItemCallback = createItemCallback;
		
		contentRectTransform = (RectTransform)content.transform;
		spacing = content.GetComponent<VerticalLayoutGroup>().spacing;

		StopMovement();
		verticalNormalizedPosition = 0;

		while (CanAddChildAt(ChildPosition.Last))
		{
			AddChild(ChildPosition.Last);
		}
	}

	protected void Update()
	{
		if (!content || !contentRectTransform) return;
		
		bool wasDragging = isDragging;

		// Remove children from beginning
		while (virtualItems.Count > 0 && verticalNormalizedPosition > 0 && 
			contentRectTransform.offsetMax.y > ((RectTransform)virtualItems[0].gameObject.transform).rect.height + spacing)
		{
			RemoveChild(ChildPosition.First);
		}

		// Remove children from end
		while (virtualItems.Count > 0 && verticalNormalizedPosition < 1 && 
			contentRectTransform.offsetMin.y < -(((RectTransform)virtualItems[virtualItems.Count - 1].gameObject.transform).rect.height + spacing))
		{
			RemoveChild(ChildPosition.Last);
		}

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
				index = virtualItems[0].index - 1;
			}
			else
			{
				index = virtualItems[virtualItems.Count - 1].index + 1;
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

		GameObject newChild = createItemCallback(index, pooledChild);
		if (newChild == null)
		{
			// End of the list
			maxIndex = index;
			// Return it back to the pool
			pool.Push (pooledChild);
			// Make sure it wasn't activated in createItemCallback
			pooledChild.SetActive(false);
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
			virtualItems.Insert(0, new VirtualListItem(newChild, index));
		}
		else
		{
			virtualItems.Add(new VirtualListItem(newChild, index));
		}
	}

	private void RemoveChild(ChildPosition position)
	{
		VirtualListItem itemToRemove = virtualItems [position == ChildPosition.Last ? virtualItems.Count - 1 : 0];
		GameObject childToRemove = itemToRemove.gameObject;

		float childHeightAndSpacing = ((RectTransform)childToRemove.transform).rect.height + spacing;

		if (position == ChildPosition.First)
		{
			SetContentAnchoredPos(new Vector2(contentRectTransform.anchoredPosition.x, 
				contentRectTransform.anchoredPosition.y - childHeightAndSpacing));
		}

		childToRemove.SetActive(false);
		virtualItems.Remove(itemToRemove);

		pool.Push(childToRemove);

		Canvas.ForceUpdateCanvases();
	}

	public void SetContentAnchoredPos(Vector2 pos)
	{
		// Base ScrollRect class bases dragging on the content's beginning position,
		// so when we move the content the dragging goes haywire. To fix this we
		// simply restart the drag if the content was moved during the frame.
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

	override public void OnDrag(PointerEventData eventData)
	{
		if (!isDragging) return;
		base.OnDrag(eventData);
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
