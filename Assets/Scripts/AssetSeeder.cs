using UnityEngine;

public class AssetSeeder : MonoBehaviour
{
    public bool instantiateAsPrefab;

    public string parentName;
    public GameObject prefab;
    public Bounds spawnBounds;
    public float yHeight;

    [Min(0)]
    public int maxTries;
    [Min(0)]
    public float spacing;

    public LayerMask layerMask;

    int prefabCount;
    GameObject parentObject;

    [ContextMenu("Seed Object")]
    public void SeedObject()
    {
        if (spacing == 0)
        {
            Debug.LogError("Spacing cannot be zero!");
            return;
        }
        prefabCount = Mathf.Min(Mathf.FloorToInt(spawnBounds.extents.x * spawnBounds.extents.z / (spacing * 3)), 5000);
        if (parentObject)
            DestroyImmediate(parentObject);


        parentObject = new GameObject(string.IsNullOrWhiteSpace(parentName) ? $"{prefab.name}Parent" : parentName);
        parentObject.transform.position = spawnBounds.center;
        for (int i = 0; i < prefabCount; i++)
        {
            int tries = 0;
            Vector3 randPoint;
            // Gets a random point. Repeats if there is food or an obstacle too close
            do
            {
                randPoint = new Vector3(
                    Random.Range(spawnBounds.min.x, spawnBounds.max.x),
                    yHeight,
                    Random.Range(spawnBounds.min.z, spawnBounds.max.z)
                );

                tries++;
                if (tries > maxTries) break;
            } while (Physics.CheckSphere(randPoint, spacing, layerMask)); // Invalid check
            if (tries <= maxTries)
            {
#if UNITY_EDITOR
                if (instantiateAsPrefab)
                {
                    GameObject obj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parentObject.transform);
                    obj.transform.position = randPoint;
                    obj.transform.rotation = Quaternion.Euler(0, Random.Range(-180f, 180f), 0);
                }
                else
                {
                    Instantiate(prefab, randPoint, Quaternion.Euler(0, Random.Range(-180f, 180f), 0), parentObject.transform);
                }
#endif
            }
            else
            {
                Debug.LogWarning($"Couldn't find a place to put the {prefab.name}! Seeded {i} {prefab.name} out of a targeted {prefabCount}");
                return;
            }
        }
        Debug.Log($"Sucessfully planted {prefabCount} {prefab.name}");
    }

    [ContextMenu("Finalise Placement")]
    public void ClearPrev()
    {
        parentObject = null;
    }

}
