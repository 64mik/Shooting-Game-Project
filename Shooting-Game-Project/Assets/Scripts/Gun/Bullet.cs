using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;     // 자동 제거 시간
    public float gravityScale = 0.1f; // 중력 약하게
    public float damage = 1f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // 관통 방지
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // 튕김 방지
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        //Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Bullet"),LayerMask.NameToLayer("Bullet"));
    }

    public void Setup(Vector3 dir, float speed, float dmg)
    {
        damage = dmg;
        rb.linearVelocity = dir * speed;
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        // 커스텀 중력
        rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision col)
    {
        // IHittable만 피격
        var hit = col.collider.GetComponent<IHittable>();
        if (hit != null)
        {
            var contact = col.GetContact(0);
            hit.TakeHit(damage, contact.point, contact.normal);
        }

        Destroy(gameObject);
    }
}
