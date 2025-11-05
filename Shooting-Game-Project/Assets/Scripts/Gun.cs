using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab; //총알 프리팹
    public Transform firePoint; //총구 위치
    public float bulletSpeed = 20f; //생성할 총알 속도
    public float fireRate = 0.5f;   //총 발사 후 지연 시간
    public int maxAmmo = 10;    //최대 장탄 수
    public ParticleSystem muzzleFlash;  //총 쏘면 번쩍이는 거, 나중에 추가 요함
    public AudioClip shootSound;    //총 소리, 총 소리도 아직 없음

    private int bulletsLeft;    //남은 총알 수
    private float nextFireTime; //다음 총 발사 가능 시간

    private void Awake()
    {
        Debug.Log("start");
        bulletsLeft = maxAmmo;

    }
    public void OnShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Debug.Log("action called");
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
        
    }

    public void OnReload()
    {
            Debug.Log("action called");
            Reload();
     
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("bulletPrefab 또는 firePoint가 연결되지 않았습니다!");
            return;
        }
        if (bulletsLeft <= 0)
            Debug.Log("총알 부족!");
        else
        {
            {
                Debug.Log("발사");
                Debug.Log(bulletsLeft - 1 + "남음");
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = firePoint.forward * bulletSpeed;

                if (muzzleFlash != null)
                    muzzleFlash.Play();


                bulletsLeft--;
            }
        }

    }

    public void Reload()
    {
        bulletsLeft = maxAmmo;
        Debug.Log("재장전 완료!");
    }
}
