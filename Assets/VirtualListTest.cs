using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VirtualListTest : MonoBehaviour
{
	public Child childPrefab;
	public CustomScrollRect scrollRect;

	private RectTransform rectTransform;
	private float spacing;

	void Start ()
	{
		scrollRect.Initialize (GetListItem);
	}

	void Update()
	{
		/*if (Input.GetKeyDown(KeyCode.N))
		{
			AddChildToBottom ();
		}*/

		/*if (Input.GetKeyDown(KeyCode.D))
		{
			GameObject childToRemove = virtualItems [0];

			float childHeightAndSpacing = childToRemove.GetComponent<RectTransform> ().rect.height + spacing;
			float totalScrollableHeight = rectTransform.rect.height - transform.parent.GetComponent<RectTransform>().rect.height;
			float delta = childHeightAndSpacing / totalScrollableHeight;

			scrollRect.verticalNormalizedPosition += delta;
		}*/

		/*while (rectTransform.offsetMin.y > 0)
		{
			AddChildToBottom ();
		}*/
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


}

