using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [Header("발사 관련 설정")]
    public GameObject bulletPrefab; //총알 프리팹
    public Transform firePoint; //총구 위치
    public ParticleSystem muzzleFlash;  //총 쏘면 번쩍이는 거, 나중에 추가 요함
    public AudioClip shootSound;    //총 소리, 총 소리도 아직 없음

    [Header("총알 설정")]
    public int maxBullet = 10;    //최대 장탄 수
    public float fireRate = 0.5f;   //총 발사 후 지연 시간
    public float bulletSpeed = 20f; //생성할 총알 속도
    public float damage = 1f;      
 
    private int bulletsLeft;    //남은 총알 수
    private float nextFireTime; //다음 총 발사 가능 시간

    private void Awake()
    {
        bulletsLeft = maxBullet;
    }

    void Start()
    {
        // ▼ 추가: 시작하자마자 HUD를 10/10으로 세팅하고 힌트 끄기
        UIHUD.I?.SetAmmo(bulletsLeft, maxBullet);
    }
    public void OnShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
        
    }

    public void OnReload()
    {
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
        {
            Debug.Log("총알 부족!");
            return;
        }

        var go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 총알에 속도/데미지 주입
        var bullet = go.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Setup(firePoint.forward, bulletSpeed, damage);
        }
        if (muzzleFlash != null) muzzleFlash.Play();

        bulletsLeft--;

        UIHUD.I?.SetAmmo(bulletsLeft, maxBullet);   // ← 발사 후 갱신
        Debug.Log($"발사, 남은 탄: {bulletsLeft}");
    }


    public void Reload()
    {
        bulletsLeft = maxBullet;

        UIHUD.I?.SetAmmo(bulletsLeft, maxBullet);   // ← 재장전 후 갱신
        
        Debug.Log("재장전 완료!");
    }


       public void OnAttack(InputValue value)
        {
            // 버튼이 눌렸을 때마다 발사
            OnShoot();
        }
}
