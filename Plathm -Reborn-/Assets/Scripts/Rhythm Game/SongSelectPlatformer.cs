using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SongSelectPlatformer : MonoBehaviour
{
    [SerializeField] Sprite difficultyNonHighlighted;
    [SerializeField] Sprite difficultyHighlighted;

    [SerializeField] Image[] diffImages;

    int chosenDiff = 0;
    int minDiff = 0;
    int maxDiff = 3;

    private void Start()
    {
        ChangeDifficultyUI();
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                ChangeDifficulty(false);
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                ChangeDifficulty(true);
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                GameManager.Instance.isSongReached = false;
                Time.timeScale = 1f;
                gameObject.SetActive(false);
            }
            else if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                Time.timeScale = 1f;
                GameManager.Instance.SelectChart(chosenDiff);
                GameManager.Instance.musicClip = GameManager.Instance.musicSource.clip;
                SceneManager.LoadScene("Rhythm Game");
            }
        }
    }

    void ChangeDifficulty(bool isUp)
    {
        if (isUp && chosenDiff < maxDiff)
            chosenDiff += 1;
        else if (!isUp && chosenDiff > minDiff)
            chosenDiff -= 1;

        ChangeDifficultyUI();
    }

    void ChangeDifficultyUI()
    {
        for (int i = minDiff; i <= maxDiff; i++)
        {
            if (chosenDiff == i)
                diffImages[i].sprite = difficultyHighlighted;
            else
                diffImages[i].sprite = difficultyNonHighlighted;
        }
    }
}
