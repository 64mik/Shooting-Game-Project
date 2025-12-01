using UnityEngine;
using System.Collections.Generic;
public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject doorPrefab;
    public GameObject keyPrefab;
    public GameObject ammoPrefab;   // ¼ÒÈ¯ÇÒ ÅºÈ¯ ÇÁ¸®ÆÕ
    public int doorCount = 1;
    public int keyCount = 1;
    public int ammoCount = 5;
    public int maxSpawnCount = 10;
    public Vector3 spawnOffset = new Vector3(0, 1f, 0); // ¹Ù´Ú¿¡¼­ »ìÂ¦ ¶ç¿ì±â
    private Vector3 size;              // ¹Ù´Ú Å©±â
    private int index = 0;
    void Start()
    {
        size = GetComponent<MeshRenderer>().bounds.size;
        SpawnItems();
    }

    void SpawnItems()
    {
        List<GameObject> spawnList = new List<GameObject>();
        for (int i = 0; i < doorCount; i++) spawnList.Add(doorPrefab);
        for (int i = 0; i < keyCount; i++) spawnList.Add(keyPrefab);

        int remainSlots = maxSpawnCount - spawnList.Count;
        for (int i = 0; i < remainSlots && i < ammoCount; i++)
        {
            spawnList.Add(ammoPrefab);
        }
        Shuffle(spawnList);

        GameObject[] spots = GameObject.FindGameObjectsWithTag("SpawnSpot");

        foreach (GameObject spot in spots)
        {
            Vector3 spawnPos = spot.transform.position + spawnOffset;
            if (index >= spawnList.Count) break;

            Instantiate(spawnList[index], spot.transform.position + spawnOffset, Quaternion.identity);
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


    void Shuffle(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
