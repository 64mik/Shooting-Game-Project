using UnityEngine;

public class CrosshairPulse : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] RectTransform dot;     // 중앙 점(Image)의 RectTransform

    [Header("Effect")]
    [SerializeField] [Range(1f, 3f)] float pulseScale = 1.3f; // 튈 때 배수
    [SerializeField] float recover = 14f;                      // 원래 크기로 복귀 속도

    Vector3 baseScale = Vector3.one;

    void Awake()
    {
        if (dot == null)
        {
            // 안전장치: 자식 중 첫 RectTransform을 자동 할당 시도
            dot = GetComponentInChildren<RectTransform>();
        }
        if (dot != null) baseScale = dot.localScale;
    }

    void Update()
    {
        if (dot == null) return;
        // 부드럽게 원래 크기로 복귀 (TimeScale 무시)
        dot.localScale = Vector3.Lerp(dot.localScale, baseScale, Time.unscaledDeltaTime * recover);
    }

    /// <summary>크로스헤어를 한 번 '툭' 튀게 합니다. power 1이 기본.</summary>
    public void Kick(float power = 1f)
    {
        if (dot == null) return;
        float p = Mathf.Clamp01(power);
        dot.localScale = baseScale * Mathf.Lerp(1f, pulseScale, p);
    }
}


