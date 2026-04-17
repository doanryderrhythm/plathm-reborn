using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas newDiscoveryCanvas;
    [SerializeField] TMP_Text newSongNameText;
    [SerializeField] TMP_Text newSongArtistText;
    [SerializeField] Image newJacketArtImage;
    [Space(10.0f)]
    [SerializeField] Canvas songInformationCanvas;
    [SerializeField] TMP_Text songNameText;
    [SerializeField] TMP_Text artistText;
    [SerializeField] Image jacketArtImage;
    [SerializeField] AudioSource musicSource;
    [Space(10.0f)]
    public Image transitionBackground;
    public Image transitionDifficultyIndicator;
    [SerializeField] TMP_Text transitionDifficultyText;
    [SerializeField] TMP_Text transitionSongNameText;
    [SerializeField] TMP_Text transitionSongArtistText;
    [SerializeField] Image transitionJacketArtImage;
    [Space(10.0f)]
    [SerializeField] TMP_Text pointDifficultyText;
    [SerializeField] TMP_Text lineDifficultyText;
    [SerializeField] TMP_Text triangleDifficultyText;
    [SerializeField] TMP_Text squareDifficultyText;

    private void Start()
    {
        GameManager.Instance.newDiscoveryCanvas = newDiscoveryCanvas;
        GameManager.Instance.newSongNameText = newSongNameText;
        GameManager.Instance.newSongArtistText = newSongArtistText;
        GameManager.Instance.newJacketArtImage = newJacketArtImage;

        GameManager.Instance.songInformationCanvas = songInformationCanvas;
        GameManager.Instance.songNameText = songNameText;
        GameManager.Instance.artistText = artistText;
        GameManager.Instance.jacketArtImage = jacketArtImage;
        GameManager.Instance.musicSource = musicSource;

        GameManager.Instance.transitionDifficultyText = transitionDifficultyText;
        GameManager.Instance.transitionSongNameText = transitionSongNameText;
        GameManager.Instance.transitionSongArtistText = transitionSongArtistText;
        GameManager.Instance.transitionJacketArtImage = transitionJacketArtImage;

        GameManager.Instance.pointDifficultyText = pointDifficultyText;
        GameManager.Instance.lineDifficultyText = lineDifficultyText;
        GameManager.Instance.triangleDifficultyText = triangleDifficultyText;
        GameManager.Instance.squareDifficultyText = squareDifficultyText;
    }
}
