using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] GameObject player;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Vector2 safePosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        safePosition = new Vector2(spawnPoint.position.x, spawnPoint.position.y);
        StartCoroutine(RespawnPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(ValueStorer.playerRespawnTime);
        GameObject newPlayer = Instantiate(player, safePosition, Quaternion.identity);

        CameraController cam = GameObject.FindFirstObjectByType<CameraController>();
        cam.player = newPlayer.GetComponent<PlatformerPlayer>();
        cam.ResetCameraOffset();
    }

    public void UpdateSafePosition(Vector2 pos)
    {
        safePosition = pos;
    }
}
