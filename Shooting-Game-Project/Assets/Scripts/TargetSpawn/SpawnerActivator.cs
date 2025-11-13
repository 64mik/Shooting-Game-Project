using UnityEngine;

public class SpawnerActivator : MonoBehaviour
{
    [Header("연결할 스포너")]
    public TargetSpawner spawner;     // 타겟 랜덤 생성 스크립트

    [Header("플레이어 설정")]
    public Transform player;          // 플레이어 Transform
    public float interactDistance = 2f; // 이 거리 이내일 때만 상호작용

    [Header("입력 키")]
    public KeyCode key = KeyCode.E;

    [Header("한 번만 작동할지 여부")]
    public bool onlyOnce = true;

    private bool hasActivated = false;

    void Update()
    {
        if (spawner == null || player == null) return;
        if (onlyOnce && hasActivated) return;

        // E 키 눌렀는지
        if (Input.GetKeyDown(key))
        {
            float dist = Vector3.Distance(player.position, transform.position);

            // 버튼 ↔ 플레이어 간 거리가 interactDistance 이하일 때만 작동
            if (dist <= interactDistance)
            {
                spawner.Activate();
                hasActivated = true;

                Debug.Log("타겟 생성");
            }
        }
    }
}
