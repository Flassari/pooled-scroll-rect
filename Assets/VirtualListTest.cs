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
		// Simulate 300 items. Remove check for unlimited scrolling! 
		if (index > 300)
			return null;
		
		if (pooledObject == null)
		{
			pooledObject = Instantiate(childPrefab).gameObject;
			StartCoroutine(GrowAndShrink(pooledObject, Random.Range(0f, Mathf.PI * 2)));
		}

		Child child = pooledObject.GetComponent<Child>();
		child.text.text = "#" + index;

		child.GetComponent<Image>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f),Random.Range(0f, 1f));

		return child.gameObject;
	}

	private IEnumerator GrowAndShrink(GameObject child, float startSeed)
	{
		while (true)
		{
			if (child.activeInHierarchy)
			{
				child.GetComponent<LayoutElement>().preferredHeight = 20 + (1 + Mathf.Sin(startSeed + Time.time)) * 50;
			}
			yield return null;
		}
	}
}

