using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class CustomScrollRect : ScrollRect
{
	public void SetContentAnchoredPos(Vector2 pos)
	{
		SetContentAnchoredPosition (pos);
	}
}
