using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Linq;


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
    public float tabletopHeight = 0.9328f;

    public Material outlineMaskMaterial; // Assign in Inspector
    public Material outlineFillMaterial; // Assign in Inspector

 
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
            GameObject centralMat = GameObject.Find("Overhead Table");
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
                yield return null;
            }
        }

        yield return new WaitUntil(() => handles.All(h => h.IsDone));

        foreach (var handle in handles)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                string prefabName = handle.Result.name;
                prefabCache[prefabName] = handle.Result;

                if (!objectPool.ContainsKey(prefabName))
                {
                    objectPool[prefabName] = new Queue<GameObject>();
                }
            }
            yield return null; 

        isLoadingPrefabs = false;
    }

    private GameObject GetPooledObject(string prefabName)
    {
        if (!objectPool.ContainsKey(prefabName))
            objectPool[prefabName] = new Queue<GameObject>();

        Queue<GameObject> pool = objectPool[prefabName];

        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            return obj;
        }
 
        if (prefabCache.TryGetValue(prefabName, out GameObject prefab))
        {
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            return newObj;
        }
        return null;
    }

    private void ReturnToPool(GameObject obj, string prefabName)
    {
        if (!objectPool.ContainsKey(prefabName))
            objectPool[prefabName] = new Queue<GameObject>();




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
            itemToOverheadMap.Clear();
            activeOverheadItems.Clear();

            foreach (GameObject child in items)
            {
                if (!IsObjectPartiallyInWedge(child, GetWedgeIndex(playerNum)))
                    continue;

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
                        newObject.transform.SetParent(transform);
                        newObject.transform.localPosition = child.transform.localPosition;
                        newObject.transform.localRotation = child.transform.localRotation;

                        GrabPushRotate grabPushRotate = newObject.GetComponent<GrabPushRotate>();
                        if (grabPushRotate == null)
                        {
                            grabPushRotate = newObject.AddComponent<GrabPushRotate>();
                        }

                        GrabPushRotate originalGrabPushRotate = child.GetComponent<GrabPushRotate>();
                        if (originalGrabPushRotate != null)
                        {
                            grabPushRotate.rotationSpeed = originalGrabPushRotate.rotationSpeed;
                            grabPushRotate.moveSpeed = originalGrabPushRotate.moveSpeed;
                            grabPushRotate.tableHeight = originalGrabPushRotate.tableHeight;
                            grabPushRotate.rotationSensitivity = originalGrabPushRotate.rotationSensitivity;
                        }

                        var outline = newObject.GetComponent<Outline>();
                        if (outline == null)
                            outline = newObject.AddComponent<Outline>();

                        var outlineMaskField = typeof(Outline).GetField("OutlineMaskMaterial", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                        var outlineFillField = typeof(Outline).GetField("OutlineFillMaterial", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

                        if (outlineMaskField != null)
                            outlineMaskField.SetValue(outline, outlineMaskMaterial);
                        if (outlineFillField != null)
                            outlineFillField.SetValue(outline, outlineFillMaterial);

                        outline.enabled = true;
                        outline.OutlineMode = Outline.Mode.OutlineAll;
                        outline.OutlineColor = Color.yellow;
                        outline.OutlineWidth = 5f;

                        newObject.layer = LayerMask.NameToLayer("Default");


                        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable = newObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                        if (grabInteractable == null)
                        {
                            grabInteractable = newObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                        }
                        grabInteractable.enabled = false;

                        if (newObject.GetComponent<Outline>() == null)
                            newObject.AddComponent<Outline>();

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
                    updatedPosition = new Vector3(overhead.transform.position.x + offset, 1.122f, overhead.transform.position.z - offset);
                } else if (playerNum == 2) {
                    updatedPosition = new Vector3(overhead.transform.position.x - offset, 1.122f, overhead.transform.position.z - offset);
                } else if (playerNum == 3) {
                    updatedPosition = new Vector3(overhead.transform.position.x - offset, 1.122f, overhead.transform.position.z + offset);
                } else if (playerNum == 4) {
                    updatedPosition = new Vector3(overhead.transform.position.x + offset, 1.122f, overhead.transform.position.z + offset);
                }


                original.transform.position = updatedPosition;
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
            Vector3 objCenter = bounds.center;


                Vector3 dir = objCenter - center;
                float dx = Mathf.Abs(dir.x);
                float dz = Mathf.Abs(dir.z);
                int wedge;

                if (playerNum == 1 && objCenter.x < 0 && Mathf.Abs(objCenter.x) > Mathf.Abs(objCenter.z))
                    if (playerWedge == 0)
                        return true;
                if (playerNum == 3 && objCenter.x > 0 && Mathf.Abs(objCenter.x) > Mathf.Abs(objCenter.z))
                    if (playerWedge == 2)
                        return true;
                if (playerNum == 2 && objCenter.z  > 0 && Mathf.Abs(objCenter.z) > Mathf.Abs(objCenter.x))
                {
                    if (playerWedge == 1)
                        return true;
                }
                if (playerNum == 4 && objCenter.z < 0 && Mathf.Abs(objCenter.z) >= Mathf.Abs(objCenter.x))
                    if (playerWedge == 3)
                        return true;
        }
        return false;
    }

    void UpdateObjectStates(int playerNum)
    {
        int playerWedge = GetWedgeIndex(playerNum);

        foreach (GameObject obj in activeOverheadItems)
        {
            GameObject origObj = itemToOverheadMap.FirstOrDefault(kv => kv.Value == obj).Key;
            if (origObj == null)
                continue;
            bool inWedge = IsObjectPartiallyInWedge(origObj, playerWedge);

            if (!inWedge)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.transform.position = new Vector3(obj.transform.position.x, tabletopHeight, obj.transform.position.z);
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
            items.Add(newItem);
    }
}
