using UnityEngine;

public class Door : MonoBehaviour
{
    public int keysRequired = 1; //문을 열기 위해 필요한 열쇠 수
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (GameManager.I.UseKeys(keysRequired))
            {
                Destroy(gameObject);
            }
        }
        Destroy(gameObject);
    }
}