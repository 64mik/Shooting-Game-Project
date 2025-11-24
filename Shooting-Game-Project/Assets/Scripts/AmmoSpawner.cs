using UnityEngine;

public class AmmoSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject ammoPrefab;   // ¼ÒÈ¯ÇÒ ÅºÈ¯ ÇÁ¸®ÆÕ
    public int spawnCount = 10;        // ¼ÒÈ¯ °³¼ö
    public Vector3 spawnOffset = new Vector3(0, 1f, 0); // ¹Ù´Ú¿¡¼­ »ìÂ¦ ¶ç¿ì±â

    private Vector3 size;              // ¹Ù´Ú Å©±â

    void Start()
    {
        // ¹Ù´Ú MeshÀÇ ½ÇÁ¦ Å©±â(Scale ¹Ý¿µ)
        size = GetComponent<MeshRenderer>().bounds.size;

        SpawnBullets();
    }

    void SpawnBullets()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            // ¹Ù´Ú ¿µ¿ª ·£´ý À§Ä¡
            float randX = Random.Range(-size.x / 2f, size.x / 2f);
            float randZ = Random.Range(-size.z / 2f, size.z / 2f);

            Vector3 spawnPos = transform.position + new Vector3(randX, 0, randZ) + spawnOffset;

            Instantiate(ammoPrefab, spawnPos, Quaternion.identity);
        }
    }
}
