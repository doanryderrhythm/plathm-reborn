using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SongCheckpoint : MonoBehaviour
{
    [SerializeField] SpriteRenderer checkpointTile;
    [SerializeField] GameObject wall;
    [SerializeField] ParticleSystem particles;

    private enum State
    {
        NONE,
        REACHED,
        PLAYED,
    }

    [SerializeField] State currentState;
    private bool isReached = false;
    private bool isAccessed = false;

    [Header("Chart File")]
    [SerializeField] string folderPath;
    [SerializeField] TextAsset chartInfo;
    [SerializeField] TextAsset pointChart;
    [SerializeField] TextAsset lineChart;
    [SerializeField] TextAsset triangleChart;
    [SerializeField] TextAsset squareChart;
    [SerializeField] Sprite jacketArt;
    [SerializeField] AudioClip music;

    void ImportChart()
    {
        if (string.IsNullOrEmpty(folderPath))
            return;

        chartInfo = Resources.Load<TextAsset>(folderPath + "/information");
        pointChart = Resources.Load<TextAsset>(folderPath + "/0");
        lineChart = Resources.Load<TextAsset>(folderPath + "/1");
        triangleChart = Resources.Load<TextAsset>(folderPath + "/2");
        squareChart = Resources.Load<TextAsset>(folderPath + "/3");
        jacketArt = Resources.Load<Sprite>(folderPath + "/jacket");
        music = Resources.Load<AudioClip>(folderPath + "/music");
    }

    void ShowUI()
    {
        string songName = "";
        string songArtist = "";

        if (chartInfo)
        {
            using (StringReader reader = new StringReader(chartInfo.text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == string.Empty)
                    {
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.songNameString))
                    {
                        songName = line.Substring(ValueStorer.songNameString.Length);
                        continue;
                    }

                    if (line.StartsWith(ValueStorer.songArtistString))
                    {
                        songArtist = line.Substring(ValueStorer.songArtistString.Length);
                        continue;
                    }
                }
            }
        }

        string pointDiff = "";
        string lineDiff = "";
        string triangleDiff = "";
        string squareDiff = "";

        if (pointChart)
            ReadDifficulty(pointChart, ref pointDiff);
        if (lineChart)
            ReadDifficulty(lineChart, ref lineDiff);
        if (triangleChart)
            ReadDifficulty(triangleChart, ref triangleDiff);
        if (squareChart)
            ReadDifficulty(squareChart, ref squareDiff);

        GameManager.Instance.AddChosenCharts(chartInfo, pointChart, lineChart, triangleChart, squareChart);
        GameManager.Instance.ShowChartInformation(ref isAccessed, songName, songArtist, jacketArt, music,
            pointDiff, lineDiff, triangleDiff, squareDiff);

        Time.timeScale = 0f;
    }

    void ReadDifficulty(TextAsset chart, ref string diff)
    {
        using (StringReader reader = new StringReader(chart.text))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line == string.Empty)
                {
                    continue;
                }

                if (line.StartsWith(ValueStorer.difficultyString))
                {
                    diff = line.Substring(ValueStorer.difficultyString.Length);
                    continue;
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ImportChart();

        currentState = State.NONE;
        ChangeState();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame
            && isReached && !GameManager.Instance.IsUIAppearing())
        {
            ShowUI();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == ValueStorer.playerLM)
        {
            isReached = true;

            if (currentState == State.NONE)
            {
                currentState = State.REACHED;
                GameManager.Instance.UpdateSafePosition(collision.transform.position);
            }

            ChangeState();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == ValueStorer.playerLM)
        {
            isReached = false;
        }
    }

    private void ChangeState()
    {
        var particleMain = particles.main;
        var startColor = particleMain.startColor;
        if (currentState == State.NONE)
        {
            checkpointTile.color = new Color32(0, 172, 255, 255);
            startColor.color = new Color32(114, 209, 255, 255);
            wall.SetActive(true);
        }
        else if (currentState == State.REACHED)
        {
            checkpointTile.color = new Color32(0, 255, 169, 255);
            startColor.color = new Color32(112, 255, 207, 255);
            wall.SetActive(true);
        }
        else if (currentState == State.PLAYED)
        {
            checkpointTile.color = new Color32(255, 141, 0, 255);
            startColor.color = new Color32(255, 192, 112, 255);
            wall.SetActive(false);
        }
        particleMain.startColor = startColor;
    }
}
