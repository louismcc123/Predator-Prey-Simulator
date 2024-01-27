using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Environment : MonoBehaviour
{
    [Range(10, 200)]
    public float range;

    public Vector3 origin = new Vector3(0, 0, 0);

    public InitialPopulations[] initialPopulations;

    public float minTrees;
    public float minPlants;

    public LayerMask animalLayer;

    public GameObject plant;
    public GameObject tree;
    //public GameObject water;

    public float radius = 100;

    //private bool collisionFlag = false;

    // Start is called before the first frame update
    void Start()
    {
        foreach (InitialPopulations pop in initialPopulations)
        {
            for (int i = 0; i < pop.count; i++)
            {
                SpawnAnimal(pop.prefab);
            }
        }

        //SphereCollider trigger = gameObject.AddComponent<SphereCollider>();

        //trigger.radius = radius;
        //trigger.isTrigger = true;
    }

    void FixedUpdate()
    {
        GameObject[] Plant = GameObject.FindGameObjectsWithTag("Plant");
        GameObject[] Tree = GameObject.FindGameObjectsWithTag("Tree");

        int plantCount = Plant.Length;
        int treeCount = Tree.Length;

        if (plantCount < minPlants)
            SpawnPlant(plant);

        if (treeCount < minTrees)
            SpawnTree(tree);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawSphere(transform.position, range);
    }

    /*public Vector3 getRandomPosition()
    {
        Vector3 position = new Vector3(Random.Range(-range, range), 5, Random.Range(-range, range));
        if (!Physics.Raycast(position, Vector3.down, 10.0f))
        {
            position = getRandomPosition();
        }
    }
    */

    public Vector3 getRandomPosition(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;

        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }

        return finalPosition;
    }

    public void SpawnAnimal(GameObject prefab)
    {
        Vector3 spawnPoint = getRandomPosition(radius);

        spawnPoint = new Vector3(spawnPoint.x, prefab.transform.position.y, spawnPoint.z);

        Instantiate(prefab, spawnPoint, Quaternion.identity);
    }

    public void SpawnPlant(GameObject plant)
    {
        Vector3 spawnPoint = getRandomPosition(radius);

        spawnPoint = new Vector3(spawnPoint.x, plant.transform.position.y, spawnPoint.z);

        Instantiate(plant, spawnPoint, Quaternion.identity);

        /*if (collisionFlag == true)
        {
            Destroy(plant);
            Debug.Log("plant in water");
        }*/
    }

    public void SpawnTree(GameObject tree)
    {
        Vector3 spawnPoint = getRandomPosition(radius);

        spawnPoint = new Vector3(spawnPoint.x, tree.transform.position.y, spawnPoint.z);

        Instantiate(tree, spawnPoint, Quaternion.identity);

        /*if (collisionFlag == true)
        {
            Destroy(tree);
            Debug.Log("tree in water");
        }*/
    }

    /*void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            collisionFlag = true;
        }
    }*/
}

[System.Serializable]
public class InitialPopulations
{
    public string name;
    public GameObject prefab;
    public int count;
}