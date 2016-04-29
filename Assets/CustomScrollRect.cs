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

	private Stack<GameObject> pool;
	private List<VirtualListItem> virtualItems;
	private float spacing;
	private int maxIndex = -1;

	public void Initialize(Func<int, GameObject, GameObject> createItemCallback)
	{
		this.createItemCallback = createItemCallback;
		
		contentRectTransform = (RectTransform)content.transform;
		spacing = content.GetComponent<VerticalLayoutGroup>().spacing;
		pool = new Stack<GameObject>();
		virtualItems = new List<VirtualListItem>();

		StopMovement();
		verticalNormalizedPosition = 0;

		while (contentRectTransform.offsetMin.y > 0)
		{
			AddChild();
		}
	}

	protected void Update()
	{
		bool wasDragging = isDragging;

		// Remove children from beginning
		while (virtualItems.Count > 0 && verticalNormalizedPosition > 0 && 
			contentRectTransform.offsetMax.y > ((RectTransform)virtualItems[0].gameObject.transform).rect.height + spacing)
		{
			RemoveChild();
		}

		// Remove children from end
		while (virtualItems.Count > 0 && verticalNormalizedPosition < 1 && 
			contentRectTransform.offsetMin.y < -(((RectTransform)virtualItems[virtualItems.Count - 1].gameObject.transform).rect.height + spacing))
		{
			RemoveChild(removeLastChild: true);
		}

		// Add children to beginning
		while (CanAddChildAt(ChildPosition.First))
		{
			AddChild(position: ChildPosition.First);
		}

		// Add children to end
		while (CanAddChildAt(ChildPosition.Last))
		{
			AddChild(position: ChildPosition.Last);
		}

		if (wasDragging && !isDragging)
		{
			lastBeginDragEventData.position = Input.mousePosition;
			Rebuild(CanvasUpdate.PostLayout);
			OnBeginDrag(lastBeginDragEventData);
		}
	}

	private bool CanAddChildAt(ChildPosition position = ChildPosition.Last)
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

	private void AddChild(ChildPosition position = ChildPosition.Last)
	{
		int index = GetNextIndex(position);

		GameObject newChild = null;
		if (pool.Count > 0)
		{
			newChild = pool.Pop();
			newChild.SetActive(true);
		}

		newChild = createItemCallback(index, newChild);
		if (newChild == null)
		{
			// End of the list
			maxIndex = index;
			return;
		}

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

	private void RemoveChild(bool removeLastChild = false)
	{
		VirtualListItem itemToRemove = virtualItems [removeLastChild ? virtualItems.Count - 1 : 0];
		GameObject childToRemove = itemToRemove.gameObject;

		float childHeightAndSpacing = ((RectTransform)childToRemove.transform).rect.height + spacing;

		if (!removeLastChild)
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
