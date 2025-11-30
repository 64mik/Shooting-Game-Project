using UnityEngine;
using TMPro;

// 게임 시간 측정, 점수 관리 담당
// 몬스터에게 맞으면 점수 즉시 감소
// 클리어 시 시간/점수 표시
public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("결과 표시용 텍스트")]
    [SerializeField] TMP_Text clearResultText;

    [Header("점수 설정")]
    public int baseScore = 10000;         // 시작 점수
    public int perSecondPenalty = 1;      // 초당 감점
    public int hitPenalty = 200;          // 몬스터 피격 시 감점

    float playTime = 0f;
    int score = 0;
    bool isPlaying = true;

    void Awake()
    {
        // 싱글톤 등록
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;

        // 점수 초기화
        score = baseScore;

        // 시작 시 결과창 숨김
        if (clearResultText != null)
            clearResultText.gameObject.SetActive(false);

        // HUD 초기 세팅
        UIHUD.I?.SetTime(0);
        UIHUD.I?.SetScore(score);
    }

    void Update()
    {
        if (!isPlaying) return;

        // 시간 증가
        playTime += Time.deltaTime;

        // HUD에 시/분 표시
        int totalSec = Mathf.FloorToInt(playTime);
        UIHUD.I?.SetTime(totalSec);

        // 초당 점수 감소
        score -= perSecondPenalty * Mathf.FloorToInt(Time.deltaTime);
        if (score < 0) score = 0;

        // HUD 갱신
        UIHUD.I?.SetScore(score);
    }

    // 몬스터에게 맞으면 즉시 점수 차감
    public void RegisterMonsterHit()
    {
        if (!isPlaying) return;

        score -= hitPenalty;
        if (score < 0) score = 0;

        UIHUD.I?.SetScore(score);
        Debug.Log($"[GameManager] 몬스터 피격! 현재 점수: {score}");
    }

    // 클리어 처리 (아직 실제 호출 안 함 → 전체 주석 처리)
    /*
    public void RegisterGameClear()
    {
        if (!isPlaying) return;
        isPlaying = false;

        int totalSec = Mathf.FloorToInt(playTime);
        int m = totalSec / 60;
        int s = totalSec % 60;

        if (clearResultText != null)
        {
            clearResultText.gameObject.SetActive(true);
            clearResultText.text =
                $"CLEAR!\n" +
                $"Time : {m:00}:{s:00}\n" +
                $"Score: {score}";
        }

        var gameUI = FindObjectOfType<GameUI>();
        if (gameUI != null)
            gameUI.ShowGameOver();
    }
    */

    // 게임오버 처리 (미사용)
    /*
    public void RegisterGameOver()
    {
        if (!isPlaying) return;
        isPlaying = false;

        if (clearResultText != null)
        {
            clearResultText.gameObject.SetActive(true);
            clearResultText.text = "GAME OVER";
        }

        var gameUI = FindObjectOfType<GameUI>();
        if (gameUI != null)
            gameUI.ShowGameOver();
    }
    */

    public float PlayTime => playTime;
    public int Score => score;
}
