using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;


public class OverheadSwap : MonoBehaviour
{
    [HideInInspector] public bool isOverhead = false;
    private bool itemsCloned = false;
    private List<GameObject> items = new List<GameObject>();
    private List<GameObject> activeOverheadItems = new List<GameObject>();
    private Dictionary<GameObject, GameObject> itemToOverheadMap = new Dictionary<GameObject, GameObject>();
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    private Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>();
    private float offset = 16.5f;
    private GameObject tabletopItems;
    private List<Collider> dividerColliders = new List<Collider>();
    public Transform tabletopCenter; 
    public int numWedges = 4;
    public Material ghostMaterial;
    public Transform playerTransform;
    public int playerNum = 1;
    private Dictionary<GameObject, List<Material[]>> originalMaterials = new Dictionary<GameObject, List<Material[]>>();
    private Transform leftWrist;
    private Transform rightWrist;
    private Transform leftIndex;
    private Transform rightIndex;
    private Collider leftWristCollider;
    private Collider rightWristCollider;
    private Collider leftIndexCollider;
    private Collider rightIndexCollider;
    private bool isLoadingPrefabs = false;

 
    void Start()
    {
        leftWrist = GameObject.Find("XR Origin Hands (XR Rig)/Camera Offset/Left Hand/Left Hand Interaction Visual/L_Wrist").transform;
        rightWrist = GameObject.Find("XR Origin Hands (XR Rig)/Camera Offset/Right Hand/Right Hand Interaction Visual/R_Wrist").transform;
        leftIndex = GameObject.Find("XR Origin Hands (XR Rig)/Camera Offset/Left Hand/Left Hand Interaction Visual/L_Wrist/L_IndexMetacarpal/L_IndexProximal/L_IndexIntermediate/L_IndexDistal/LeftIndexDistalCollider").transform;
        rightIndex = GameObject.Find("XR Origin Hands (XR Rig)/Camera Offset/Right Hand/Right Hand Interaction Visual/R_Wrist/R_IndexMetacarpal/R_IndexProximal/R_IndexIntermediate/R_IndexDistal/RightIndexDistalCollider").transform;
        leftWristCollider = leftWrist.GetComponent<Collider>();
        rightWristCollider = rightWrist.GetComponent<Collider>();
        leftIndexCollider = leftIndex.GetComponent<Collider>();
        rightIndexCollider = rightIndex.GetComponent<Collider>();
                        
        tabletopItems = GameObject.Find("Tabletop Objects");

        if (tabletopItems == null)
            return;
        foreach (Transform child in tabletopItems.transform)
        {
            if (child.gameObject.activeSelf)
            items.Add(child.gameObject);
            Collider childCollider = child.gameObject.GetComponent<Collider>();
            GameObject centralMat = GameObject.Find("Central Tabletop Mat");
            foreach (Transform div in centralMat.transform)
            {
                if (div.gameObject.name == "Table Divider") {
                    Collider dividerCollider = div.gameObject.GetComponent<Collider>();
                    if (dividerCollider != null && !dividerColliders.Contains(dividerCollider))
                        dividerColliders.Add(dividerCollider);
                }
            }
        }

        StartCoroutine(PreloadPrefabs());
    }

    private IEnumerator PreloadPrefabs()
    {
        isLoadingPrefabs = true;
        List<AsyncOperationHandle<GameObject>> handles = new List<AsyncOperationHandle<GameObject>>();

        foreach (GameObject child in items)
        {
            string baseName = child.gameObject.name;
            baseName = baseName.Replace("(Clone)", "").Trim();
            int parenIndex = baseName.IndexOf(" (");
            if (parenIndex > 0)
                baseName = baseName.Substring(0, parenIndex);
            string prefabName = "Assets/Prefabs/" + baseName + ".prefab";
            
            if (!prefabCache.ContainsKey(prefabName))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(prefabName);
                handles.Add(handle);
            }
        }

        yield return new WaitUntil(() => {
            foreach (var handle in handles)
            {
                if (!handle.IsDone) return false;
            }
            return true;
        });

        foreach (var handle in handles)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                string prefabName = handle.Result.name;
                prefabCache[prefabName] = handle.Result;
                

                if (!objectPool.ContainsKey(prefabName))
                {
                    objectPool[prefabName] = new Queue<GameObject>();

                    for (int i = 0; i < 2; i++)
                    {
                        GameObject pooledObj = Instantiate(handle.Result);
                        pooledObj.SetActive(false);
                        objectPool[prefabName].Enqueue(pooledObj);
                    }
                }
            }
        }

        isLoadingPrefabs = false;
    }

    private GameObject GetPooledObject(string prefabName)
    {
        if (!objectPool.ContainsKey(prefabName))
            return null;

        Queue<GameObject> pool = objectPool[prefabName];
        
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        else
        {

            if (prefabCache.TryGetValue(prefabName, out GameObject prefab))
            {
                GameObject newObj = Instantiate(prefab);
                return newObj;
            }
        }
        return null;
    }

    private void ReturnToPool(GameObject obj, string prefabName)
    {
        if (!objectPool.ContainsKey(prefabName))
            objectPool[prefabName] = new Queue<GameObject>();


        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;


        obj.SetActive(false);
        objectPool[prefabName].Enqueue(obj);
    }

    void Update()
    {
        if (isLoadingPrefabs) return;

        if (isOverhead && !itemsCloned)
        {
            SwapItems();
            itemsCloned = true;
        }
        else if (!isOverhead && itemsCloned)
        {
            DestroyItems();
            itemsCloned = false;
        }

        UpdateObjectStates(playerNum);
        }

    void SwapItems()
    {
        if (isOverhead)
        {
            foreach (GameObject child in items)
            {
                string baseName = child.gameObject.name;
                baseName = baseName.Replace("(Clone)", "").Trim();
                int parenIndex = baseName.IndexOf(" (");
                if (parenIndex > 0)
                    baseName = baseName.Substring(0, parenIndex);

                if (prefabCache.TryGetValue(baseName, out GameObject prefab))
                {
                    GameObject newObject = GetPooledObject(baseName);
                    if (newObject != null)
                    {
                        newObject.SetActive(true);
                        Vector3 newPosition = new Vector3(0, 0, 0);
                        if (playerNum == 1) {
                            newPosition = new Vector3(child.transform.position.x - offset, child.transform.position.y, child.transform.position.z + offset);
                        } else if (playerNum == 2) {
                            newPosition = new Vector3(child.transform.position.x + offset, child.transform.position.y, child.transform.position.z + offset);
                        } else if (playerNum == 3) {
                            newPosition = new Vector3(child.transform.position.x + offset, child.transform.position.y, child.transform.position.z - offset);
                        } else if (playerNum == 4) {
                            newPosition = new Vector3(child.transform.position.x - offset, child.transform.position.y, child.transform.position.z - offset);
                        }
                        newObject.transform.position = newPosition;
                        newObject.transform.rotation = child.transform.rotation;
                        
                        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable grabInteractable = newObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
                        if (grabInteractable != null)
                        {
                            Destroy(grabInteractable);
                        }
                        if (newObject.GetComponent<GrabPushRotate>() == null)
                            newObject.AddComponent<GrabPushRotate>();
                        if (newObject.GetComponent<Outline>() == null)
                            newObject.AddComponent<Outline>();

                        Outline outline = newObject.GetComponent<Outline>();
                        if (outline != null) {
                            outline.enabled = false;
                            outline.enabled = true;  
                            outline.enabled = false; 
                        }

                        activeOverheadItems.Add(newObject);
                        itemToOverheadMap[child] = newObject;

                        Collider[] newObjectColliders = newObject.GetComponentsInChildren<Collider>();
                        foreach (var dividerCollider in dividerColliders)
                        {
                            foreach (var objCollider in newObjectColliders)
                            {
                                Physics.IgnoreCollision(objCollider, dividerCollider);
                            }
                        }

                        Renderer[] renderers = newObject.GetComponentsInChildren<Renderer>();
                        List<Material[]> matsList = new List<Material[]>();
                        foreach (var rend in renderers)
                            matsList.Add((Material[])rend.materials.Clone());
                        originalMaterials[newObject] = matsList;
                    }
                }
                else
                {
                    Debug.LogWarning($"[OverheadSwap] Prefab NOT found for: {baseName}");
                }
            }
            itemsCloned = true;
        }
    }

    void DestroyItems()
    {
        if (activeOverheadItems.Count > 0)
        {
            foreach (var kvp in itemToOverheadMap)
            {
                GameObject original = kvp.Key;
                GameObject overhead = kvp.Value;

                Vector3 updatedPosition = new Vector3(0, 0, 0);
                if (playerNum == 1) {
                    updatedPosition = new Vector3(overhead.transform.position.x + offset, overhead.transform.position.y, overhead.transform.position.z - offset);
                } else if (playerNum == 2) {
                    updatedPosition = new Vector3(overhead.transform.position.x - offset, overhead.transform.position.y, overhead.transform.position.z - offset);
                } else if (playerNum == 3) {
                    updatedPosition = new Vector3(overhead.transform.position.x - offset, overhead.transform.position.y, overhead.transform.position.z + offset);
                } else if (playerNum == 4) {
                    updatedPosition = new Vector3(overhead.transform.position.x + offset, overhead.transform.position.y, overhead.transform.position.z + offset);
                }


                original.transform.position = updatedPosition;
                original.transform.rotation = overhead.transform.rotation;

                string baseName = original.name;
                baseName = baseName.Replace("(Clone)", "").Trim();
                int parenIndex = baseName.IndexOf(" (");
                if (parenIndex > 0)
                    baseName = baseName.Substring(0, parenIndex);
                ReturnToPool(overhead, baseName);
            }

            foreach (GameObject item in items)
            {
                if (!itemToOverheadMap.ContainsKey(item))
                {
                    foreach (GameObject overhead in activeOverheadItems)
                    {
                        if (overhead.name.StartsWith(item.name))
                        {
                            Vector3 updatedPosition = new Vector3(0, 0, 0);
                            if (playerNum == 1) {
                                updatedPosition = new Vector3(overhead.transform.position.x + offset, overhead.transform.position.y, overhead.transform.position.z - offset);
                            } else if (playerNum == 2) {
                                updatedPosition = new Vector3(overhead.transform.position.x - offset, overhead.transform.position.y, overhead.transform.position.z - offset);
                            } else if (playerNum == 3) {
                                updatedPosition = new Vector3(overhead.transform.position.x - offset, overhead.transform.position.y, overhead.transform.position.z + offset);
                            } else if (playerNum == 4) {
                                updatedPosition = new Vector3(overhead.transform.position.x + offset, overhead.transform.position.y, overhead.transform.position.z + offset);
                            }
                            
                            item.transform.position = updatedPosition;
                            item.transform.rotation = overhead.transform.rotation;
                            break;
                        }
                    }
                }
            }

            itemToOverheadMap.Clear();
            activeOverheadItems.Clear();
            originalMaterials.Clear();
        }
    }

    int GetWedgeIndex(int playerNum)
    {
        switch (playerNum)
        {
            case 1: return 0;
            case 2: return 1;
            case 3: return 2; 
            case 4: return 3; 
            default: return 1;
        }
    }

    bool IsObjectPartiallyInWedge(GameObject obj, int playerWedge)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        Vector3 center = tabletopCenter.position;

        foreach (var rend in renderers)
        {
            Bounds bounds = rend.bounds;
            Vector3[] points = new Vector3[]
            {
                bounds.center,
                bounds.min,
                bounds.max,
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
            };

            foreach (var point in points)
            {
                Vector3 dir = point - center;
                float absX = Mathf.Abs(dir.x);
                float absZ = Mathf.Abs(dir.z);
                int wedge;
                if (absX >= absZ)
                {
                    wedge = dir.x >= 0 ? 2 : 0;
                }
                else
                {
                    wedge = dir.z >= 0 ? 1 : 3;
                }
                if (wedge == playerWedge)
                    return true;
            }
        }
        return false;
    }

    void UpdateObjectStates(int playerNum)
    {
        int playerWedge = GetWedgeIndex(playerNum);

        foreach (GameObject obj in activeOverheadItems)
        {
            bool inWedge = IsObjectPartiallyInWedge(obj, playerWedge);

            if (!inWedge)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
                if (originalMaterials.ContainsKey(obj))
                {
                    var matsList = originalMaterials[obj];
                    Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; i++)
                        renderers[i].materials = matsList[i];
                }
                Collider[] colliders = obj.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                    col.enabled = true;
            }
        }
    }

    public void AddItem(GameObject newItem)
    {
        if (!items.Contains(newItem))
        {
            items.Add(newItem);
            Debug.Log("Added item: " + newItem.name);
        }


    }
}
