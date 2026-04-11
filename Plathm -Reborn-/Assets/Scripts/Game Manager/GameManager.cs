using NUnit.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] GameObject player;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Vector2 safePosition;

    [Header("Song Information UI")]
    [SerializeField] Canvas songInformationCanvas;
    [SerializeField] TMP_Text songNameText;
    [SerializeField] TMP_Text artistText;
    [SerializeField] Image jacketArtImage;
    [SerializeField] AudioSource musicSource;

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
        MovingPlatform[] movingPlatforms = GameObject.FindObjectsByType<MovingPlatform>(FindObjectsSortMode.None);
        foreach (MovingPlatform platform in movingPlatforms)
        {
            if (platform.isTrigger)
            {
                platform.ResetPosition();
            }
        }

        GameObject newPlayer = Instantiate(player, safePosition, Quaternion.identity);

        CameraController cam = GameObject.FindFirstObjectByType<CameraController>();
        cam.player = newPlayer.GetComponent<PlatformerPlayer>();
        cam.ResetCameraOffset();
    }

    public void UpdateSafePosition(Vector2 pos)
    {
        safePosition = pos;
    }

    public void ShowChartInformation(string songName, string artist, Sprite jacketArt, AudioClip music)
    {
        songNameText.text = songName;
        artistText.text = artist;
        jacketArtImage.sprite = jacketArt;
        musicSource.clip = music;

        songInformationCanvas.gameObject.SetActive(true);
        musicSource.Play();
    }
}
