using UnityEngine;

public class NewSongCanvas : MonoBehaviour
{
    [SerializeField] Canvas songSelectCanvas;
    public void TurnOffSongCanvas()
    {
        gameObject.SetActive(false);
        songSelectCanvas.gameObject.SetActive(true);
    }
}
