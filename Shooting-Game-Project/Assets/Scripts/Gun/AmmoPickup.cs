using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 10; // 증가할 탄약 수

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Gun gun = collider.GetComponentInChildren<Gun>();

            if (gun != null)
            {
                gun.AddAmmo(ammoAmount);
            }
            else
            {
                Debug.Log("Gun 스크립트를 찾을 수 없습니다.");
            }
            Destroy(gameObject);
        }
    }
}
