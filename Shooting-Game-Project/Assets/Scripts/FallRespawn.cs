using UnityEngine;

// 맵 아래 트리거에 붙이는 스크립트
// 플레이어가 떨어지면 지정한 플레이어 오브젝트를 리스폰 위치로 순간이동
public class FallRespawn : MonoBehaviour
{
    [Header("텔레포트 대상 (보통 PlayerRig)")]
    public Transform playerRoot;      // 실제로 움직이는 플레이어 루트 오브젝트
    [Header("리스폰 위치")]
    public Transform respawnPoint;    // 되살아날 위치

    [Header("플레이어 태그 이름")]
    public string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그가 아니면 무시
        if (!other.CompareTag(playerTag))
            return;

        // 이동시킬 대상 결정:
        // Inspector에 playerRoot가 지정되어 있으면 그걸 사용,
        // 아니면 트리거에 닿은 오브젝트의 루트 사용
        Transform target = playerRoot != null ? playerRoot : other.transform.root;

        if (respawnPoint == null)
        {
            Debug.LogWarning("FallRespawn: respawnPoint가 비어 있습니다.");
            return;
        }

        Debug.Log($"FallRespawn: 텔레포트 시도 {target.name} -> {respawnPoint.position}");

        // CharacterController가 있으면 잠시 끄고 위치 옮긴 뒤 다시 켜기
        var cc = target.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            target.position = respawnPoint.position;
            target.rotation = respawnPoint.rotation;
            cc.enabled = true;
        }
        else
        {
            // 일반 Transform 이동
            target.position = respawnPoint.position;
            target.rotation = respawnPoint.rotation;
        }

        // Rigidbody가 있으면 속도도 0으로
        var rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("FallRespawn: 리스폰 완료");
    }
}
