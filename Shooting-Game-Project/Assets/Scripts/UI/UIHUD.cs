using TMPro;
using UnityEngine;


public class UIHUD : MonoBehaviour
{
    public static UIHUD I { get; private set; }

    [SerializeField] TMP_Text scoreText, timeText, accText ;
     [SerializeField] TMPro.TMP_Text ammoText;
    [SerializeField] TMPro.TMP_Text reloadHint;
    [SerializeField] CrosshairPulse crosshair;

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

    public void SetAccuracy(float a) => accText.text = $"Acc: {a:0}%";

    public void CrosshairKick() => crosshair?.Kick();

     // ← 탄약 표시
    public void SetAmmo(int cur, int max)
    {
    if (ammoText) ammoText.text = $"{cur}/{max}";
    if (reloadHint) reloadHint.gameObject.SetActive(cur <= 0);
    }
}
