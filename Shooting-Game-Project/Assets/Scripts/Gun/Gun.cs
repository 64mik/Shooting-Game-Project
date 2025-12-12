using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("이펙트 오브젝트 (Scene에 있는 것 연결)")]
    public Transform firePoint;
    public GameObject lightningBeamObject;  // 번개 줄기 (LineRenderer 등)
    public GameObject muzzleFlashObject;    // 총구 화염

    [Header("이펙트 시간")]
    public float effectDuration = 0.1f;

    [Header("타격 이펙트 (프리팹)")]
    public GameObject hitEffectPrefab;

    [Header("카메라 & 사거리")]
    [SerializeField] Camera cam;
    [SerializeField] float maxDist = 100f; // 사거리 100
    [SerializeField] LayerMask hitMask = ~0;

    [Header("총 성능")]
    public float damage = 10f;
    public float fireRate = 0.5f;
    public int maxAmmo = 10;

    private int currentAmmo;
    private int ammoLeft;
    private float nextFireTime;

    private Coroutine disableEffectCoroutine;

    private void Awake()
    {
        ammoLeft = maxAmmo;
        currentAmmo = maxAmmo;
        if (!cam) cam = Camera.main;

        // 시작할 때 이펙트 꺼두기
        if (lightningBeamObject != null) lightningBeamObject.SetActive(false);
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(false);
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

        // 1. 히트스캔
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // 레이 발사
        if (Physics.Raycast(ray, out hit, maxDist, hitMask))
        {
            // 적 타격
            var target = hit.collider.GetComponent<IHittable>();
            if (target != null)
            {
            target.TakeHit(damage, hit.point, hit.normal);

            // ✅ 맞췄으니 +10점
             GameManager.I?.AddScore(10);
            }


            // 벽 타격 이펙트
            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        // 2. 이펙트 활성화 (기존 방식)
        if (disableEffectCoroutine != null) StopCoroutine(disableEffectCoroutine);

        if (lightningBeamObject != null) lightningBeamObject.SetActive(true);
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(true);

        disableEffectCoroutine = StartCoroutine(DisableEffects());

        // 3. 탄약 차감
        currentAmmo--;
        UIHUD.I?.SetAmmo(currentAmmo, ammoLeft);
    }

    IEnumerator DisableEffects()
    {
        yield return new WaitForSeconds(effectDuration);
        if (lightningBeamObject != null) lightningBeamObject.SetActive(false);
        if (muzzleFlashObject != null) muzzleFlashObject.SetActive(false);
    }

    public void Reload()
    {
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