using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomScrollRect : ScrollRect, IBeginDragHandler, IEndDragHandler
{
	private bool isDragging = false;
	
	public void SetContentAnchoredPos(Vector2 pos)
	{
		SetContentAnchoredPosition (pos);
	}

	override public void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		if (!IsActive())
			return;

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

}
