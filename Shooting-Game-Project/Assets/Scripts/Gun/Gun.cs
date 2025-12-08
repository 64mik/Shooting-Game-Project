using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("발사 이펙트 설정")]
    public Transform firePoint;         // 총구 위치
    public GameObject lightningPrefab;  // ⭐ 번개 프리팹 (여기에 에셋을 넣으세요)
    public GameObject muzzleFlashObject; // 총구 번쩍임 (선택)
    public float effectDuration = 0.2f; // 번개가 보여질 시간 (짧게 설정)

    [Header("타격 이펙트 (선택)")]
    public GameObject hitEffectPrefab;  // 벽에 맞았을 때 튀는 스파크

    [Header("카메라 참조")]
    [SerializeField] Camera cam;
    [SerializeField] float maxDist = 100f;
    [SerializeField] LayerMask hitMask = ~0;

    [Header("데미지 설정")]
    public float damage = 10f;
    public float fireRate = 0.5f;
    public int maxAmmo = 10;

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
        if (ammoLeft <= 0) return;
        Reload();
    }

    public void AddAmmo(int amount)
    {
        ammoLeft += amount;
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
    }

    private void Shoot()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("총알 부족!");
            return;
        }

        // -------------------------------
        // 1) 히트스캔 (레이 발사)
        // -------------------------------
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, maxDist, hitMask))
        {
            targetPoint = hit.point;

            // 데미지 주기
            var target = hit.collider.GetComponent<IHittable>();
            if (target != null)
                target.TakeHit(damage, hit.point, hit.normal);

            // 벽 타격 이펙트 (스파크)
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else
        {
            // 허공을 쏜 경우 (사거리 끝)
            targetPoint = ray.origin + ray.direction * maxDist;
        }

        // -------------------------------
        // 2) 번개 이펙트 생성 (총알 X)
        // -------------------------------
        if (lightningPrefab != null)
        {
            // 총구 위치에 번개 생성
            GameObject lightning = Instantiate(lightningPrefab, firePoint.position, Quaternion.identity);

            // 번개가 목표 지점을 바라보게 회전 (길게 뻗는 모델이라면 Z축이 정면이어야 함)
            lightning.transform.LookAt(targetPoint);

            // (선택) 만약 번개 길이를 코드로 늘려야 한다면 스케일 조절
            // float distance = Vector3.Distance(firePoint.position, targetPoint);
            // lightning.transform.localScale = new Vector3(1, 1, distance); 

            // 짧은 시간 뒤 삭제
            Destroy(lightning, effectDuration);
        }

        // 3) 머즐 플래시 (총구 번쩍임)
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
        yield return new WaitForSeconds(0.1f);
        muzzleFlashObject.SetActive(false);
    }

    public void Reload()
    {
        // ... (기존 재장전 로직과 동일) ...
        if (ammoLeft >= maxAmmo)
        {
            if (currentAmmo != 0) ammoLeft += currentAmmo;
            ammoLeft -= maxAmmo; currentAmmo = maxAmmo;
        }
        else if (ammoLeft > 0)
        {
            if (currentAmmo != 0) ammoLeft += currentAmmo;
            currentAmmo = ammoLeft; ammoLeft = 0;
        }
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
    }

    public void OnAttack(InputValue value)
    {
        if (GameUI.Paused) return;
        OnShoot();
    }
}