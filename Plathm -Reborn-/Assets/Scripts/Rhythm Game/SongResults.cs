using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongResults : MonoBehaviour
{
    [SerializeField] Camera cam;

    [Header("Song Information UI")]
    [SerializeField] TMP_Text songTitleText;
    [SerializeField] TMP_Text songArtistText;
    [SerializeField] Image jacketArtImage;
    [SerializeField] Image difficultyIndicatorImage;
    [SerializeField] TMP_Text levelText;

    [Header("Judgements")]
    [SerializeField] TMP_Text CPerfectCountText;
    [SerializeField] TMP_Text perfectCountText;
    [SerializeField] TMP_Text goodCountText;
    [SerializeField] TMP_Text damageCountText;
    [SerializeField] TMP_Text missCountText;
    [Space(10.0f)]
    [SerializeField] TMP_Text maxComboText;
    [SerializeField] TMP_Text earlyCountText;
    [SerializeField] TMP_Text lateCountText;
    [Space(10.0f)]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] Image rankImage;

    [Header("Indicators")]
    [SerializeField] Image clearImage;
    [SerializeField] Image allComboImage;
    [SerializeField] Image fullPerfectImage;

    [Header("Rank Sprites")]
    [SerializeField] Sprite PPlusRankSprite;
    [SerializeField] Sprite PRankSprite;
    [SerializeField] Sprite SPlusRankSprite;
    [SerializeField] Sprite SRankSprite;
    [SerializeField] Sprite APlusRankSprite;
    [SerializeField] Sprite ARankSprite;
    [SerializeField] Sprite BPlusRankSprite;
    [SerializeField] Sprite BRankSprite;
    [SerializeField] Sprite CPlusRankSprite;
    [SerializeField] Sprite CRankSprite;
    [SerializeField] Sprite DRankSprite;

    [Header("Indicator Sprites")]
    [SerializeField] Sprite clearSprite;
    [SerializeField] Sprite allComboSprite;
    [SerializeField] Sprite fullPerfectSprite;

    private int difficulty;
    private RhythmGameManager.IndicatorType indicator;
    void Start()
    {
        InsertResults();    
    }

    void InsertResults()
    {
        RhythmGameManager RGManager = FindFirstObjectByType<RhythmGameManager>();
        if (RGManager == null)
            return;

        songTitleText.text = RGManager.songName;
        songArtistText.text = RGManager.songArtist;
        jacketArtImage.sprite = RGManager.jacketArtRaw;

        difficulty = RGManager.difficulty;
        if (difficulty == 0)
            difficultyIndicatorImage.color = ValueStorer.pointDifficultyColor;
        else if (difficulty == 1)
            difficultyIndicatorImage.color = ValueStorer.lineDifficultyColor;
        else if (difficulty == 2)
            difficultyIndicatorImage.color = ValueStorer.triangleDifficultyColor;
        else if (difficulty == 3)
            difficultyIndicatorImage.color = ValueStorer.squareDifficultyColor;
        levelText.text = RGManager.level.ToString();

        CPerfectCountText.text = RGManager.CPerfectNotes.ToString();
        perfectCountText.text = RGManager.perfectNotes.ToString();
        goodCountText.text = RGManager.goodNotes.ToString();
        damageCountText.text = RGManager.damageNotes.ToString();
        missCountText.text = RGManager.missNotes.ToString();

        earlyCountText.text = RGManager.earlyCount.ToString();
        lateCountText.text = RGManager.lateCount.ToString();
        maxComboText.text = RGManager.maxComboCount.ToString();

        indicator = RGManager.indicatorType;
        if (indicator != RhythmGameManager.IndicatorType.FAILED)
        {
            if (difficulty == 0)
                cam.backgroundColor = ValueStorer.pointDifficultyBackground;
            else if (difficulty == 1)
                cam.backgroundColor = ValueStorer.lineDifficultyBackground;
            else if (difficulty == 2)
                cam.backgroundColor = ValueStorer.triangleDifficultyBackground;
            else if (difficulty == 3)
                cam.backgroundColor = ValueStorer.squareDifficultyBackground;
        }

        if (indicator == RhythmGameManager.IndicatorType.NORMAL)
        {
            clearImage.sprite = clearSprite;
        }
        else if (indicator == RhythmGameManager.IndicatorType.ALL_COMBO)
        {
            clearImage.sprite = clearSprite;
            allComboImage.sprite = allComboSprite;
        }
        else if (indicator == RhythmGameManager.IndicatorType.FULL_PERFECT)
        {
            clearImage.sprite = clearSprite;
            allComboImage.sprite = allComboSprite;
            fullPerfectImage.sprite = fullPerfectSprite;
        }

        scoreText.text = RGManager.totalScore.ToString("0.0000") + "%";
        if (RGManager.totalScore == 101.0f) rankImage.sprite = PPlusRankSprite;
        else if (RGManager.totalScore >= 100.0f && RGManager.totalScore < 101.0f) rankImage.sprite = PRankSprite;
        else if (RGManager.totalScore >= 95.0f && RGManager.totalScore < 100.0f) rankImage.sprite = SPlusRankSprite;
        else if (RGManager.totalScore >= 90.0f && RGManager.totalScore < 95.0f) rankImage.sprite = SRankSprite;
        else if (RGManager.totalScore >= 85.0f && RGManager.totalScore < 90.0f) rankImage.sprite = APlusRankSprite;
        else if (RGManager.totalScore >= 80.0f && RGManager.totalScore < 85.0f) rankImage.sprite = ARankSprite;
        else if (RGManager.totalScore >= 75.0f && RGManager.totalScore < 80.0f) rankImage.sprite = BPlusRankSprite;
        else if (RGManager.totalScore >= 70.0f && RGManager.totalScore < 75.0f) rankImage.sprite = BRankSprite;
        else if (RGManager.totalScore >= 65.0f && RGManager.totalScore < 70.0f) rankImage.sprite = CPlusRankSprite;
        else if (RGManager.totalScore >= 60.0f && RGManager.totalScore < 65.0f) rankImage.sprite = CRankSprite;
        else rankImage.sprite = DRankSprite;

        Destroy(RGManager.gameObject);
    }
}
