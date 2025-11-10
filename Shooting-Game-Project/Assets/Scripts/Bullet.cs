using UnityEngine;

// 총에서 Setup으로 속도/데미지를 주입받음
// 'Target' 태그를 가진 오브젝트에만 반응하도록 수정

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    //[Header("수명(선택)")]
    //[Tooltip("허공으로 쏜 탄을 자동 정리하는 안전장치. 0이면 비활성화.")]
    public float lifeTime = 3f; // 발사 후 3초 뒤 삭제

    public bool useCustomGravity = true; // 중력
    public float gravityScale = 0.2f;    // 중력 약하게 설정

    // 총에서 세팅받을 값
    private float damage = 1f;
    private Vector3 initVelocity;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // 기본 중력은 끔
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    // 총에서 발사 직후 호출: 방향, 속도, 데미지 주입

    public void Setup(Vector3 direction, float speed, float setDamage)
    {
        damage = setDamage;
        initVelocity = direction.normalized * speed;
        rb.linearVelocity = initVelocity;

        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime); // 허공 발사 자동 정리
    }

    private void FixedUpdate()
    {
        if (useCustomGravity && gravityScale != 0f)
        {
            // 기본 중력 = Physics.gravity. 배율로 약화/강화
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 태그 필터: Target에만 반응
        if (!collision.collider.CompareTag("Target"))
            return;

        // IHittable이면 데미지 전달
        var hit = collision.collider.GetComponent<IHittable>();
        if (hit != null)
        {
            var contact = collision.GetContact(0);
            hit.TakeHit(damage, contact.point, contact.normal);
        }

        Destroy(gameObject); // 타겟에 명중하면 소멸
    }
}
