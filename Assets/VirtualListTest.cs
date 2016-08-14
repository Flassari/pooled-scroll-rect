using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class VirtualListTest : MonoBehaviour
{
	public Child childPrefab;
	public PooledScrollRect scrollRect;

	void Start()
	{
		scrollRect.CreateItemCallback = GetListItem;
	}

	private GameObject GetListItem(int index, GameObject pooledObject)
	{
		if (index < 0 || index > 100)
			return null;
		
		if (pooledObject == null)
		{
			pooledObject = Instantiate(childPrefab).gameObject;
		}

		Child child = pooledObject.GetComponent<Child>();
		child.text.text = "#" + index;

		child.GetComponent<LayoutElement>().preferredHeight = Random.Range(20, 300);

		return child.gameObject;
	}
}

