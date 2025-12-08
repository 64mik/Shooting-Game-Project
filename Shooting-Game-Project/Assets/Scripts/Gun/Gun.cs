using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("발사 관련 설정")]
    public GameObject bulletTrailPrefab; // (구 bulletPrefab) 날아가는 궤적 이펙트용 프리팹
    public Transform firePoint;
    public GameObject muzzleFlashObject;
    public float flashDuration = 0.1f;
    public ParticleSystem hitEffectPrefab; // (선택) 벽에 맞았을 때 튈 파티클

    [Header("카메라 참조")]
    [SerializeField] Camera cam;
    [SerializeField] float maxDist = 100f;
    [SerializeField] LayerMask hitMask = ~0;

    [Header("총알 설정")]
    public float damage = 1f;
    public float fireRate = 0.5f;
    public int maxAmmo = 10;

    // 히트스캔이지만 눈에 보이는 총알 속도 (궤적 이동 속도)
    public float visualSpeed = 100f;

    private int currentAmmo;
    private int ammoLeft;
    private float nextFireTime;
    private Coroutine disableFlashCoroutine;

    private void Awake()
    {
        ammoLeft = maxAmmo;
        currentAmmo = maxAmmo;
        if (!cam) cam = Camera.main;

        if (muzzleFlashObject != null)
            muzzleFlashObject.SetActive(false);
    }

    void Start()
    {
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
    }

    public void OnShoot()
    {
        if (GameUI.Paused) return;

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    public void OnReload()
    {
        if (ammoLeft <= 0)
        {
            Debug.Log("재장전 실패: 여분의 탄약이 없습니다!");
            return;
        }
        Reload();
    }

    public void AddAmmo(int amount)
    {
        ammoLeft += amount;
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
    }

    private void Shoot()
    {
        if (firePoint == null) return;
        if (currentAmmo <= 0)
        {
            Debug.Log("총알 부족!");
            return;
        }

        // -------------------------------
        // [핵심] 히트스캔 로직 (즉시 판정)
        // -------------------------------
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint; // 총알 궤적이 날아갈 목표 지점

        // 1. 레이 발사
        if (Physics.Raycast(ray, out hit, maxDist, hitMask))
        {
            targetPoint = hit.point;

            // 2. 맞은 대상에게 즉시 데미지 적용
            var target = hit.collider.GetComponent<IHittable>();
            if (target != null)
            {
                target.TakeHit(damage, hit.point, hit.normal);
            }

            // (선택) 벽에 맞은 이펙트 생성
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            // 허공을 쏜 경우
            targetPoint = ray.origin + ray.direction * maxDist;
        }

        // -------------------------------
        // 3. 시각 효과 (궤적) 생성
        // -------------------------------
        if (bulletTrailPrefab != null)
        {
            // 총구에서 생성
            GameObject trail = Instantiate(bulletTrailPrefab, firePoint.position, Quaternion.identity);

            // 궤적 스크립트에 "여기까지 날아가라"고 명령
            BulletTracer tracer = trail.GetComponent<BulletTracer>();
            if (tracer != null)
            {
                tracer.Init(targetPoint, visualSpeed);
            }
        }

        // -------------------------------
        // 4. 머즐플래시 (기존 동일)
        // -------------------------------
        if (muzzleFlashObject != null)
        {
            if (disableFlashCoroutine != null) StopCoroutine(disableFlashCoroutine);
            muzzleFlashObject.SetActive(true);
            disableFlashCoroutine = StartCoroutine(DisableFlashRoutine());
        }

        currentAmmo--;
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
    }

    IEnumerator DisableFlashRoutine()
    {
        yield return new WaitForSeconds(flashDuration);
        muzzleFlashObject.SetActive(false);
    }

    public void Reload()
    {
        if (ammoLeft >= maxAmmo)
        {
            if (currentAmmo != 0) ammoLeft += currentAmmo;
            ammoLeft -= maxAmmo;
            currentAmmo = maxAmmo;
        }
        else if (ammoLeft > 0)
        {
            if (currentAmmo != 0) ammoLeft += currentAmmo;
            currentAmmo = ammoLeft;
            ammoLeft = 0;
        }
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
    }

    public void OnAttack(InputValue value)
    {
        if (GameUI.Paused) return;
        OnShoot();
    }
}