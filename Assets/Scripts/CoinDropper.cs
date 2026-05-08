using UnityEngine;
using UnityEngine.InputSystem;

public class CoinDropper : MonoBehaviour
{
    [SerializeField] GameObject coinPrefab;
    [SerializeField] float dropY = 4.5f;
    [SerializeField] float minX  = -3.5f;
    [SerializeField] float maxX  =  3.5f;

    Camera mainCamera;

    void Start() => mainCamera = Camera.main;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
        if (GameManager.Instance == null) return;

        var screenPos = mouse.position.ReadValue();
        float depth   = Mathf.Abs(mainCamera.transform.position.z);
        var worldPos  = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, depth));
        float x       = Mathf.Clamp(worldPos.x, minX, maxX);

        if (GameManager.Instance.TrySpendCoin())
        {
            Instantiate(coinPrefab, new Vector3(x, dropY, 0f), Quaternion.identity);
            GameManager.Instance.RegisterCoinSpawned();
            GameSFX.Instance?.PlayDrop();
        }
    }
}
