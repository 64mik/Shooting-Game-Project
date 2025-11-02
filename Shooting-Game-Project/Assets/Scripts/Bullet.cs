using UnityEngine;

// 총알
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("총알 설정")]
    public float speed = 40f;        // 날아가는 속도
    public float damage = 1f;        // 타겟 HP와 연동되는 데미지
    public float lifeTime = 3f;      // 3초 뒤 자동 파괴
    //public bool useGravity = false;  // 중력 적용 여부

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //rb.useGravity = useGravity;

        // 빠른 속도로 움직일 때 관통 방지
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void OnEnable()
    {
        // 생성 즉시 앞으로 발사
        rb.linearVelocity = transform.forward * speed;

        // 일정 시간 후 자동 제거
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 맞은 오브젝트가 IHittable 구현했는지 확인
        IHittable hittable = collision.collider.GetComponent<IHittable>();
        if (hittable != null)
        {
            // 충돌 지점 정보 얻기
            ContactPoint contact = collision.GetContact(0);

            // 데미지 전달
            hittable.TakeHit(damage, contact.point, contact.normal);
        }

        // 충돌 즉시 총알 제거
        Destroy(gameObject);
    }
}
