using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCharacter : MonoBehaviour
{
    public GameObject[] characterPrefabs;
    public Transform spawnPoint;
    
    public float radius = 100;

    Environment environment;

    // Start is called before the first frame update
    void Start()
    {
        environment = FindObjectOfType<Environment>();

        int selectedCharacter = PlayerPrefs.GetInt("selectedCharacter");
        GameObject prefab = characterPrefabs[selectedCharacter];
        //GameObject clone = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        SpawnPlayer(prefab);
    }

    public void SpawnPlayer(GameObject go)
    {
        Vector3 spawnPoint = environment.getRandomPosition(radius);

        spawnPoint = new Vector3(spawnPoint.x, go.transform.position.y, spawnPoint.z);

        Instantiate(go, spawnPoint, Quaternion.identity);
    }
}
