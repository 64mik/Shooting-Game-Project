using UnityEngine;
using TMPro;

// ���� �ð� ����, ���� ���� ���
// ���Ϳ��� ������ ���� ��� ����
// Ŭ���� �� �ð�/���� ǥ��
public class GameManager : MonoBehaviour
{
     public static GameManager I { get; private set; }

    [Header("��� ǥ�ÿ� �ؽ�Ʈ")]
    [SerializeField] TMP_Text clearResultText;

    [Header("���� ����")]
    public int baseScore = 10000;         // ���� ����
    public int perSecondPenalty = 1;      // �ʴ� ����
    public int hitPenalty = 200;          // ���� �ǰ� �� ����

    float playTime = 0f;
    int score = 0;
    public int keysCollected = 0; //������ ���� ��
    bool isPlaying = true;

    //씬 로드 전에 GameManager가 없으면 자동 생성(+DontDestroyOnLoad)하여 싱글톤을 항상 보장
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Ensure()
    {
        if (FindFirstObjectByType<GameManager>() == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            Object.DontDestroyOnLoad(go);
        }
    }

    void Awake()
    {
        // �̱��� ���
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;

        // ���� �ʱ�ȭ
        score = baseScore;

        // ���� �� ���â ����
        if (clearResultText != null)
            clearResultText.gameObject.SetActive(false);

        // HUD �ʱ� ����
        UIHUD.I?.SetTime(0);
        UIHUD.I?.SetScore(score);
    }

    void Update()
    {
        if (!isPlaying) return;

        // �ð� ����
        playTime += Time.deltaTime;

        // HUD�� ��/�� ǥ��
        int totalSec = Mathf.FloorToInt(playTime);
        UIHUD.I?.SetTime(totalSec);

        // �ʴ� ���� ����
        score -= perSecondPenalty * Mathf.FloorToInt(Time.deltaTime);
        if (score < 0) score = 0;

        // HUD ����
        UIHUD.I?.SetScore(score);
    }
    public void AddKey()
    {
        keysCollected++;
        Debug.Log($"[GameManager] ���� ȹ��! ���� ���� ����: {keysCollected}");
    }
    public bool UseKeys(int count)
    {
        if (keysCollected >= count)
        {
            keysCollected -= count;
            Debug.Log($"[GameManager] �� ����! ���� ���� ����: {keysCollected}");
            return true;
        }
        else
        {
            Debug.Log($"[GameManager] ���� ����! ���� ���� ����: {keysCollected}, �ʿ��� ����: {count}");
            return false;
        }
    }
    // ���Ϳ��� ������ ��� ���� ����
    public void RegisterMonsterHit()
    {
        if (!isPlaying) return;

        score -= hitPenalty;
        if (score < 0) score = 0;

        UIHUD.I?.SetScore(score);
        Debug.Log($"[GameManager] ���� �ǰ�! ���� ����: {score}");
    }
    // Ŭ���� ó�� (���� ���� ȣ�� �� �� �� ��ü �ּ� ó��)
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

    // ���ӿ��� ó�� (�̻��)
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
