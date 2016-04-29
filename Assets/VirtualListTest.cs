using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VirtualListTest : MonoBehaviour
{
	public Child childPrefab;
	public CustomScrollRect scrollRect;

	void Start ()
	{
		scrollRect.Initialize (GetListItem);
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

