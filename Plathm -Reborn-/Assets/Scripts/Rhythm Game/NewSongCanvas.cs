using UnityEngine;
using UnityEngine.SceneManagement;

public class NewSongCanvas : MonoBehaviour
{
    [SerializeField] Canvas songSelectCanvas;
    public void TurnOffSongCanvas()
    {
        songSelectCanvas.gameObject.SetActive(true);
        songSelectCanvas.GetComponent<Animator>().Play("Song Select Canvas Open");
        gameObject.SetActive(false);
    }

    public void SwitchScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void DestroyItself()
    {
        Destroy(gameObject);
    }
}
