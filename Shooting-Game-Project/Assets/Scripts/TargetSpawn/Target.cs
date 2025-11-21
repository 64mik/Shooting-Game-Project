using UnityEngine;

public class Target : MonoBehaviour, IHittable
{
    // 타겟 hp 설정
    // hp와 스피드를 늘림으로서 난이도 조절 가능
    public float maxHp = 1f;

    private float currentHp;

    // 이펙트
    // 부셔질 시 파편화 되는 이펙트 넣으면 좋을 것 같음
    // hp를 높인 고난이도에서는 피격한곳에 탄흔 이펙트 남기면 좋을 것 같음
    public GameObject hitEffectPrefab;

    private void OnEnable()
    {
        // 리스폰되거나 씬 재활성화 시 HP 초기화
        currentHp = maxHp;
    }

    // IHittable 인터페이스 구현 (총알에서 호출됨)
    public void TakeHit(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        currentHp -= damage;

        // 피격 이펙트 생성
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPoint, Quaternion.LookRotation(hitNormal));
        }

        // HP가 0 이하가 되면 완전 파괴
        if (currentHp <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
