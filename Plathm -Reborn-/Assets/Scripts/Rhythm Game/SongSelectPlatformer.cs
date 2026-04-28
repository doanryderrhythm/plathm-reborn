using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class SongSelectPlatformer : MonoBehaviour
{
    [SerializeField] Sprite difficultyNonHighlighted;
    [SerializeField] Sprite difficultyHighlighted;

    [SerializeField] Image[] diffImages;

    int chosenDiff = 0;
    int minDiff = 0;
    int maxDiff = 3;

    bool isDifficultyChosen = false;

    int chosenConfig = 0;
    [SerializeField] NumericalItem[] configItems;

    [SerializeField] TMP_Text titleText;
    [SerializeField] GameObject difficultySelectScreen;
    [SerializeField] GameObject configurationScreen;

    [SerializeField] Canvas songTransitionCanvas;
    [SerializeField] InputPlaceholder inputPlaceholder;

    private void Start()
    {
        titleText.text = ValueStorer.songInfoTitle;
        difficultySelectScreen.SetActive(true);
        configurationScreen.SetActive(false);

        ChangeDifficultyUI();

        ChangeConfigUI();
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (GameManager.Instance.isRhythmStarting)
                return;

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                if (!isDifficultyChosen)
                    ChangeDifficulty(false);
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            { 
                if (!isDifficultyChosen)
                    ChangeDifficulty(true);
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                if (isDifficultyChosen && chosenConfig > 0)
                {
                    chosenConfig -= 1;
                    ChangeConfigUI();
                }
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                if (isDifficultyChosen && chosenConfig < configItems.Length - 1)
                {
                    chosenConfig += 1;
                    ChangeConfigUI();
                }
            }
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (isDifficultyChosen)
                {
                    isDifficultyChosen = false;

                    titleText.text = ValueStorer.songInfoTitle;
                    difficultySelectScreen.SetActive(true);
                    configurationScreen.SetActive(false);

                    inputPlaceholder.UpdatePage(0);
                }
                else
                {
                    GameManager.Instance.isSongReached = false;
                    Time.timeScale = 1f;
                    gameObject.SetActive(false);
                }
            }
            else if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                if (!isDifficultyChosen)
                {
                    isDifficultyChosen = true;

                    titleText.text = ValueStorer.configurationTitle;
                    difficultySelectScreen.SetActive(false);
                    configurationScreen.SetActive(true);

                    inputPlaceholder.UpdatePage(1);
                }
                else
                {
                    GameManager.Instance.musicSource.Stop();

                    GameManager.Instance.SelectChart(chosenDiff);
                    GameManager.Instance.musicClip = GameManager.Instance.musicSource.clip;

                    GameManager.Instance.isRhythmStarting = true;
                    GameManager.Instance.songTransitionCanvas = songTransitionCanvas;
                    songTransitionCanvas.gameObject.SetActive(true);
                    songTransitionCanvas.GetComponent<Animator>().Play("Song Transition Start");
                    DontDestroyOnLoad(songTransitionCanvas.gameObject);
                }
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

    void ChangeConfigUI()
    {
        for (int i = 0; i < configItems.Length; i++)
        {
            if (chosenConfig == i)
                configItems[i].Toggle(true);
            else
                configItems[i].Toggle(false);
        }
    }
}
