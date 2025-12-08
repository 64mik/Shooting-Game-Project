using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;

    // Gun 스크립트에서 호출해줌
    public void Init(Vector3 target, float moveSpeed)
    {
        targetPosition = target;
        speed = moveSpeed;

        // 목표 방향을 바라보게 회전
        transform.LookAt(targetPosition);

        // 안전장치: 2초 뒤에는 무조건 삭제 (혹시 모를 잔여물 방지)
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        // 목표 지점까지 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 목표 지점에 거의 도달했으면 삭제
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}