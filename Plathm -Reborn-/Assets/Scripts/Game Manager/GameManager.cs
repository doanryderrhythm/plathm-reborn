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

    [Header("New Discovery UI")]
    public Canvas newDiscoveryCanvas;
    public TMP_Text newSongNameText;
    public TMP_Text newSongArtistText;
    public Image newJacketArtImage;
    [Header("Song Information UI")]
    public bool isSongReached = false;
    public Canvas songInformationCanvas;
    public TMP_Text songNameText;
    public TMP_Text artistText;
    public Image jacketArtImage;
    public AudioSource musicSource;
    [Space(10.0f)]
    public TMP_Text pointDifficultyText;
    public TMP_Text lineDifficultyText;
    public TMP_Text triangleDifficultyText;
    public TMP_Text squareDifficultyText;

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

    public void ShowChartInformation(ref bool isAccessed, string songName, string artist, Sprite jacketArt, AudioClip music,
        string pointDiff, string lineDiff, string triangleDiff, string squareDiff)
    {
        newSongNameText.text = songName;
        newSongArtistText.text = artist;
        newJacketArtImage.sprite = jacketArt;

        songNameText.text = songName;
        artistText.text = artist;
        jacketArtImage.sprite = jacketArt;
        musicSource.clip = music;

        if (!string.IsNullOrEmpty(pointDiff))
            pointDifficultyText.text = pointDiff;
        else
            pointDifficultyText.text = "-1";
        if (!string.IsNullOrEmpty(lineDiff))
            lineDifficultyText.text = lineDiff;
        else
            lineDifficultyText.text = "-1";
        if (!string.IsNullOrEmpty(triangleDiff))
            triangleDifficultyText.text = triangleDiff;
        else
            triangleDifficultyText.text = "-1";
        if (!string.IsNullOrEmpty(squareDiff))
            squareDifficultyText.text = squareDiff;
        else
            squareDifficultyText.text = "-1";

        isSongReached = true;
        if (isAccessed) songInformationCanvas.gameObject.SetActive(true);
        else
        {
            isAccessed = true;
            newDiscoveryCanvas.gameObject.SetActive(true);
            newDiscoveryCanvas.GetComponent<Animator>().Play("New Discovery Canvas");
        }
    }

    //SELECTED CHART FILES
    public TextAsset chosenSongInfo;

    TextAsset chosenPointChart;
    TextAsset chosenLineChart;
    TextAsset chosenTriangleChart;
    TextAsset chosenSquareChart;
    public TextAsset chosenChart;
    public AudioClip musicClip;

    public void AddChosenCharts(TextAsset songInfo, TextAsset pointChart, TextAsset lineChart, TextAsset triangleChart, TextAsset squareChart)
    {
        chosenSongInfo = songInfo;

        chosenPointChart = pointChart;
        chosenLineChart = lineChart;
        chosenTriangleChart = triangleChart;
        chosenSquareChart = squareChart;
    }

    public void SelectChart(int diff)
    {
        switch (diff)
        {
            case 0: chosenChart = chosenPointChart; break;
            case 1: chosenChart = chosenLineChart; break;
            case 2: chosenChart = chosenTriangleChart; break;
            case 3: chosenChart = chosenSquareChart; break;
            default: chosenChart = chosenPointChart; break;
        }
    }

    public bool IsUIAppearing()
    {
        return newDiscoveryCanvas.gameObject.activeSelf
            || songInformationCanvas.gameObject.activeSelf;
    }
}
