using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [Header("발사 관련 설정")]
    public GameObject bulletPrefab; //총알 프리팹
    public Transform firePoint; //총구 위치
    public ParticleSystem muzzleFlash;  //총 쏘면 번쩍이는 거, 나중에 추가 요함
    public AudioClip shootSound;    //총 소리, 총 소리도 아직 없음

    [Header("카메라 참조")]
    [SerializeField] Camera cam;
    [SerializeField] float maxDist = 100f;      // 레이 최대 거리
    [SerializeField] LayerMask hitMask = ~0;    // 맞출 레이어 (원하면 설정)

    [Header("총알 설정")]
    public float bulletSpeed = 20f; //생성할 총알 속도
    public float damage = 1f;
    public float fireRate = 0.5f;   //총 발사 후 지연 시간
    public int maxAmmo = 10;    //장전가능한 최대 장탄 수
    private int currentAmmo; //현재 남은 총알
    private int ammoLeft;    //여분 탄약 수
    private float nextFireTime; //다음 총 발사 가능 시간

    private void Awake()
    {
        ammoLeft = maxAmmo; //초기 여분 탄약 수 설정
        currentAmmo = maxAmmo; //기본적으로 최대 장탄 수로 세팅
        if (!cam) cam = Camera.main;
    }

    void Start()
    {
        // 추가: 시작하자마자 HUD를 10/10으로 세팅하고 힌트 끄기
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
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
        if(ammoLeft <= 0)
        {
            Debug.Log("재장전 실패: 여분의 탄약이 없습니다!");
            return;
        }
        Reload();
    }

    public void AddAmmo(int amount)
    {
        ammoLeft += amount;
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);   // 탄약 추가 후 갱신
        Debug.Log($"탄약 추가: {amount}, 현재 소유 탄약: {ammoLeft}");
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("bulletPrefab 또는 firePoint가 연결되지 않았습니다!");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("총알 부족!");
            return;
        }

        // -------------------------------
        // 1) 화면 중앙에서 나가는 레이 계산
        // -------------------------------
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPoint;

        // 뭔가 맞으면 그 위치, 아니면 일정 거리 앞을 목표로 사용
        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, hitMask))
            targetPoint = hit.point;
        else
            targetPoint = ray.origin + ray.direction * maxDist;

        // firePoint에서 그 목표 지점을 향하는 방향
        Vector3 dir = (targetPoint - firePoint.position).normalized;

        // 총구 방향도 맞추고 싶다면(선택 사항)
        firePoint.rotation = Quaternion.LookRotation(dir);

        // -------------------------------
        // 2) 총알 생성 + 방향/속도/데미지 세팅
        // -------------------------------
        var go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        var bullet = go.GetComponent<Bullet>();
        if (bullet != null)
        {
            // 🔴 예전: firePoint.forward
            // bullet.Setup(firePoint.forward, bulletSpeed, damage);

            // ✅ 수정: 카메라 중앙 기준으로 계산한 dir 사용
            bullet.Setup(dir, bulletSpeed, damage);
        }
        
        if (muzzleFlash != null) muzzleFlash.Play();

        currentAmmo--;

        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);   // 발사 후 갱신
        Debug.Log($"발사, 남은 탄: {currentAmmo}");
    }


    public void Reload()
    {
        if (ammoLeft >= maxAmmo)    //총알 수 여유있음
        {
            if(currentAmmo != 0)
            {
                ammoLeft += currentAmmo; //현재 탄창에 남은 탄약을 여분 탄약에 다시 더함
            }
            ammoLeft -= maxAmmo;
            currentAmmo = maxAmmo;
            UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);   // 재장전 후 갱신
            Debug.Log("재장전 완료!");
        }
        else if(ammoLeft > 0)   //남은 탄약으로 재장전
        {
            if (currentAmmo != 0)
            {
                ammoLeft += currentAmmo; //현재 탄창에 남은 탄약을 여분 탄약에 다시 더함
            }
            currentAmmo = ammoLeft;
            ammoLeft = 0;
            UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);   // 재장전 후 갱신
            Debug.Log("탄약이 부족하여 남은 탄약으로 재장전 완료!");
        }
        else
        {
            Debug.Log("재장전 실패: 여분의 탄약이 없습니다!");
        }
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);   // 발사 후 갱신
        Debug.Log($"발사, 남은 탄: {currentAmmo}");
    }


       public void OnAttack(InputValue value)
        {
            // 버튼이 눌렸을 때마다 발사
            OnShoot();
        }
}
