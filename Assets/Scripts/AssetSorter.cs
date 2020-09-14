using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AssetSorter : MonoBehaviour
{
	public enum SortType
	{
		Name,
		zPos_Positive,
		zPos_Negative,
	}

	public SortType m_SortType;
	[ContextMenu("Sort Children")]
	void SortChildren()
	{
		// Get the index of this object
		int startingIndex = transform.GetSiblingIndex() + 1;

		// Get all the children
		List<Transform> assetsToSort = new List<Transform>();
		foreach (Transform child in transform)
		{
			assetsToSort.Add(child);
		}

		// Remove this object so it's just the children
		assetsToSort.Remove(transform);

		// Sort
		switch (m_SortType)
		{
			case SortType.Name:
				assetsToSort = assetsToSort.OrderBy(go => go.name).ToList();
				break;
			case SortType.zPos_Positive:
				assetsToSort = assetsToSort.OrderBy(go => go.transform.position.z).ToList();
				break;
			case SortType.zPos_Negative:
				assetsToSort = assetsToSort.OrderBy(go => go.transform.position.z).Reverse().ToList();
				break;
			default:
				break;
		}

		// Reorder
		for (int i = 0; i < assetsToSort.Count; i++)
		{
			assetsToSort[i].SetSiblingIndex(startingIndex + i);
		}
	}
}
