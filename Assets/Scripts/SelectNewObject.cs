using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SelectNewObject : MonoBehaviour
{
    //public NetworkPrefabRef prefabToSpawn;
    public GameObject prefabToSpawn;
    public Transform spawnPoint;
    public GameObject objectMenu;
    public Material spawnCubeContainerMaterial;    
    public AudioClip clip;
    private AudioSource audioSource;

    public OpenObjectMenu openObjectMenu;

    [HideInInspector] public GameObject spawnCubeContainer = null;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("IndexFingerCollider"))
            return;

        //NetworkRunner runner = BasicSpawner.Instance;
        //NetworkRunner runner = FindObjectOfType<NetworkRunner>();
        //if (runner == null)
        //{
            //Debug.LogError("No NetworkRunner");
            //return;
        //}
        Transform t = spawnPoint != null ? spawnPoint : transform;
        
        if (prefabToSpawn == null)
            return;

        /* spawn the object player selects */
        var spawned = Instantiate(prefabToSpawn, t.position, prefabToSpawn.transform.rotation);
        //NetworkObject networkObj = runner.Spawn(prefabToSpawn, t.position, Quaternion.identity, runner.LocalPlayer);
        //if (networkObj == null)
        //{
            //Debug.LogError("Failed to spawn prefab with runner spawner");
        //}
        //GameObject spawned = networkObj.gameObject;
        
        Collider prefabCol = spawned.gameObject.GetComponent<BoxCollider>();
        if (prefabCol == null)
        {
            prefabCol = spawned.gameObject.GetComponent<MeshCollider>();
            if (prefabCol == null)
            {
                Debug.LogError("No BoxCollider or MeshCollider found on prefab!");
                return;
            }
        }
        Bounds b = prefabCol.bounds;

        /* spawn its transaprent cube container */
        
        spawnCubeContainer = GameObject.CreatePrimitive(PrimitiveType.Cube);        
        spawnCubeContainer.transform.position = b.center;
        float maxExtent = 0;
        if (b.size.x > b.size.z)
            maxExtent = b.size.x;
        else
            maxExtent = b.size.z;

        spawnCubeContainer.transform.localScale = new Vector3(
            maxExtent, b.size.y + 0.1f, maxExtent);

        /* set the transaprent material */
        var rend = spawnCubeContainer.GetComponent<Renderer>();
        if (rend == null)
            return;

        if(spawnCubeContainerMaterial != null)
            rend.material = spawnCubeContainerMaterial;

        /* rotate the container */
        spawnCubeContainer.AddComponent<RotateObject>();
        
        Destroy(spawnCubeContainer.GetComponent<Collider>());

        /* pass the spawned object and container to OpenObjectMenu.cs */
        /* why? because once we disable the ObjectMenu, can no longer
            play sound effect or delete the container when the object is moved */
        
        openObjectMenu.objectSpawned = spawned;
        openObjectMenu.objectSpawnedCubeContainer = spawnCubeContainer;

        openObjectMenu.justSpawned = true;

        /* disable the object menu once the object is spawned */
        if (objectMenu != null)
            objectMenu.SetActive(false);
    }


    
}
