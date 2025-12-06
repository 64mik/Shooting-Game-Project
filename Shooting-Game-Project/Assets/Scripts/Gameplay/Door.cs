using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Door : MonoBehaviour
{
    [SerializeField] bool onlyPlayerTag = true;
    [SerializeField] int keysRequired = 1;   // 0이면 키 불필요
    bool done;

    void OnTriggerEnter(Collider other)
{
    if (done) return;
    if (onlyPlayerTag && !other.CompareTag("Player")) return;

    if (keysRequired <= 0) {
        Debug.Log("[Door] keysRequired=0 → free pass");
        Pass();
        return;
    }

    if (GameManager.I == null) {
        Debug.LogWarning("[Door] GameManager.I == null → block");
        return;
    }

    bool consumed = GameManager.I.UseKeys(keysRequired);
    Debug.Log($"[Door] need={keysRequired}, have(after?)={GameManager.I.keysCollected}, consumed={consumed}");
    if (!consumed) return;

    Pass();
}

void Pass()
{
    done = true;
    (GameUI.I ?? FindFirstObjectByType<GameUI>())?.ShowClear();
}


    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;

        // CharacterController와 트리거 충돌 보장을 위해 권장
        if (!TryGetComponent<Rigidbody>(out var rb))
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
    }
}

