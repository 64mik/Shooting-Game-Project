using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIHUD : MonoBehaviour
{
    public static UIHUD I { get; private set; }

    [SerializeField] TMP_Text scoreText, timeText, accText ;
     [SerializeField] TMPro.TMP_Text ammoText;
    [SerializeField] TMPro.TMP_Text reloadHint;
    [SerializeField] CrosshairPulse crosshair;

    [Header("Stamina")]
    [SerializeField] Image staminaFill;   // StaminaBar_Fill 이미지


    void Awake() {
        if (I != null && I != this){ Destroy(gameObject);  return; }
        I = this; 
        }

    public void SetScore(int v) => scoreText.text = $"score: {v} ";

    public void SetTime(int totalSec)
    {
        int m = totalSec / 60;
        int s = totalSec % 60;
        timeText.text = $"{m:00}:{s:00}";
    }

    public void SetStamina(float cur, float max)
    {
        float t = Mathf.Clamp01(cur / max);       // 0~1 사이 값
        if (staminaFill)
        {
            // X 스케일만 줄이기 → 가운데 기준으로 양쪽에서 줄어듦
            staminaFill.rectTransform.localScale = new Vector3(t, 1f, 1f);
        }
    }

    public void SetAccuracy(float a) => accText.text = $"Acc: {a:0}%";

    public void CrosshairKick() => crosshair?.Kick();

     // ← 탄약 표시
    public void SetAmmo(int cur, int max)
    {
    if (ammoText) ammoText.text = $"{cur}/{max}";
    if (reloadHint) reloadHint.gameObject.SetActive(cur <= 0);
    }
}
