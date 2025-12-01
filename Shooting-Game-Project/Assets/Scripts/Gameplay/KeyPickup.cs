using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            GameManager.I.AddKey();
            Destroy(gameObject);
        }
        Destroy(gameObject);
    }
}