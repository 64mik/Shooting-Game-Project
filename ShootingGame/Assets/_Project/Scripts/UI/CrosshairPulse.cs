using UnityEngine;

public class CrosshairPulse : MonoBehaviour
{
    [SerializeField] RectTransform dot;
    [SerializeField] float pulseScale = 1.25f, recover = 12f;
    Vector3 baseScale;
    void Awake()
    {
        baseScale = dot.localScale;
    }

    public void Kick()
    {
        dot.localScale = baseScale * pulseScale;
    }

    void Update()
    {
        dot.localScale = Vector3.Lerp(dot.localScale, baseScale, Time.deltaTime * recover);
    }
    
}
