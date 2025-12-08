using UnityEngine;
using System.Collections.Generic;
public class MapGenerator: MonoBehaviour
{

    [Header("map info")]
    [SerializeField] int mapSize = 2;
    [SerializeField] int roomCount = 2;

    [Header("Spawn Settings")]
    public GameObject doorPrefab;
    public GameObject keyPrefab;
    public GameObject ammoPrefab;
    public GameObject monsterPrefab;
    [SerializeField] int keyCount = 2;
    [SerializeField] int ammoCount = 4;
    [SerializeField] int monsterCount = 1;


    public Vector3 spawnOffset = new Vector3(0, 0, 0);
    private Vector3 size = new Vector3(0, 0, 0);
    private int maxSpawnCount = 0;
    
    void Start()
    {
        size = GetComponent<MeshRenderer>().bounds.size * mapSize;
        maxSpawnCount = keyCount + ammoCount + monsterCount + 1;
        SpawnItems();
    }

    void SpawnItems()
    {
        int index = 0;
        List<GameObject> spawnList = new List<GameObject>();

        spawnList.Add(doorPrefab);
        for (int i = 0; i < keyCount; i++) spawnList.Add(keyPrefab);
        for (int i = 0; i < ammoCount; i++) spawnList.Add(ammoPrefab);
        for (int i = 0; i < monsterCount; i++) spawnList.Add(monsterPrefab);
        Shuffle(spawnList);

        GameObject[] spots = GameObject.FindGameObjectsWithTag("SpawnSpot");

        foreach (GameObject spot in spots)
        {
            Vector3 spawnPos = spot.transform.position;
            if (index >= spawnList.Count) break;

            Instantiate(spawnList[index], spawnPos, Quaternion.identity);
            index++;

            Destroy(spot);

        }
        while (index < spawnList.Count)
        {
            Vector3 pos = GetRandomPos();
            Instantiate(spawnList[index], pos, Quaternion.identity);
            index++;
        }
    }

    Vector3 GetRandomPos()
    {
        float randX = Random.Range(-size.x / 2f, size.x / 2f);
        float randZ = Random.Range(-size.z / 2f, size.z / 2f);

        return transform.position + new Vector3(randX, 0, randZ) + spawnOffset;
    }


    void Shuffle(List<GameObject> list){
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
