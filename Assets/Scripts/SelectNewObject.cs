using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SelectNewObject : MonoBehaviour
{
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
        Transform t = spawnPoint != null ? spawnPoint : transform;
        
        if (prefabToSpawn == null)
            return;

        /* spawn the object player selects */
        var spawned = Instantiate(prefabToSpawn, t.position, t.rotation);
        BoxCollider prefabCol = spawned.gameObject.GetComponent<BoxCollider>();
        Bounds b = prefabCol.bounds;

        /* spawn its transaprent cube container */
        spawnCubeContainer = GameObject.CreatePrimitive(PrimitiveType.Cube);        
        spawnCubeContainer.transform.position = b.center;
        spawnCubeContainer.transform.localScale = new Vector3(
            b.size.x * 1.2f, 
            b.size.y * 1.2f,  
            b.size.z * 1.2f   
        );

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
