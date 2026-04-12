using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas songInformationCanvas;
    [SerializeField] TMP_Text songNameText;
    [SerializeField] TMP_Text artistText;
    [SerializeField] Image jacketArtImage;
    [SerializeField] AudioSource musicSource;
    [Space(10.0f)]
    [SerializeField] TMP_Text pointDifficultyText;
    [SerializeField] TMP_Text lineDifficultyText;
    [SerializeField] TMP_Text triangleDifficultyText;
    [SerializeField] TMP_Text squareDifficultyText;

    private void Start()
    {
        GameManager.Instance.songInformationCanvas = songInformationCanvas;
        GameManager.Instance.songNameText = songNameText;
        GameManager.Instance.artistText = artistText;
        GameManager.Instance.jacketArtImage = jacketArtImage;
        GameManager.Instance.musicSource = musicSource;

        GameManager.Instance.pointDifficultyText = pointDifficultyText;
        GameManager.Instance.lineDifficultyText = lineDifficultyText;
        GameManager.Instance.triangleDifficultyText = triangleDifficultyText;
        GameManager.Instance.squareDifficultyText = squareDifficultyText;
    }
}
