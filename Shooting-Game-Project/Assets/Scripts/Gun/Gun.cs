using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // 코루틴 사용을 위해 필요

public class Gun : MonoBehaviour
{
    [Header("발사 관련 설정")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    // [수정됨] 프리팹이 아니라, 이미 자식으로 달려있는 오브젝트를 참조합니다.
    public GameObject muzzleFlashObject;

    // [추가됨] 번개 이펙트가 켜져있는 시간 (너무 길면 어색함)
    public float flashDuration = 0.1f;

    public AudioClip shootSound;

    [Header("카메라 참조")]
    [SerializeField] Camera cam;
    [SerializeField] float maxDist = 100f;
    [SerializeField] LayerMask hitMask = ~0;

    [Header("총알 설정")]
    public float bulletSpeed = 20f;
    public float damage = 1f;
    public float fireRate = 0.5f;
    public int maxAmmo = 10;
    private int currentAmmo;
    private int ammoLeft;
    private float nextFireTime;

    // 이펙트 끄는 코루틴을 저장할 변수
    private Coroutine disableFlashCoroutine;

    private void Awake()
    {
        ammoLeft = maxAmmo;
        currentAmmo = maxAmmo;
        if (!cam) cam = Camera.main;

        // 시작할 때 이펙트가 켜져있다면 확실하게 끕니다.
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
        // 1) 레이 계산
        // -------------------------------
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, hitMask))
            targetPoint = hit.point;
        else
            targetPoint = ray.origin + ray.direction * maxDist;

        Vector3 dir = (targetPoint - firePoint.position).normalized;

        // firePoint 회전 (총알 나가는 방향 동기화)
        firePoint.rotation = Quaternion.LookRotation(dir);

        // -------------------------------
        // 2) 총알 생성
        // -------------------------------
        var go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        var bullet = go.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Setup(dir, bulletSpeed, damage);
        }

        // -------------------------------
        // [수정됨] 3) 번개 이펙트 활성화 (껏다 켜기)
        // -------------------------------
        if (muzzleFlashObject != null)
        {
            // 이미 켜져서 끄려고 대기중인 코루틴이 있다면 취소함 (연사 시 깜빡임 방지)
            if (disableFlashCoroutine != null)
                StopCoroutine(disableFlashCoroutine);

            // 이펙트를 켬
            muzzleFlashObject.SetActive(true);

            // 파티클 시스템이라면 처음부터 다시 재생하도록 명령 (필요 시)
            // ParticleSystem ps = muzzleFlashObject.GetComponent<ParticleSystem>();
            // if(ps != null) ps.Play();

            // 일정 시간 뒤에 끄는 예약 걸기
            disableFlashCoroutine = StartCoroutine(DisableFlashRoutine());
        }

        currentAmmo--;
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
        Debug.Log($"발사, 남은 탄: {currentAmmo}");
    }

    // 지정된 시간 뒤에 이펙트를 끄는 함수
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
        else
        {
            Debug.Log("재장전 실패");
            return;
        }
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
        Debug.Log("재장전 완료!");
    }

    public void OnAttack(InputValue value)
    {
        if (GameUI.Paused) return;
        OnShoot();
    }
}