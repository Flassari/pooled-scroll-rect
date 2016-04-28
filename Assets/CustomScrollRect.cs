using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomScrollRect : ScrollRect, IBeginDragHandler, IEndDragHandler
{
	private bool isDragging = false;
	private PointerEventData lastBeginDragEventData;

	
	public void SetContentAnchoredPos(Vector2 pos)
	{
		bool wasDragging = isDragging;
		
		if (wasDragging)
		{
			OnEndDrag (new PointerEventData(null) { button = PointerEventData.InputButton.Left });
		}

		SetContentAnchoredPosition (pos);

		if (wasDragging)
		{
			lastBeginDragEventData.position = Input.mousePosition;
			OnBeginDrag(lastBeginDragEventData);
		}
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

}
