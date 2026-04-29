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
    [SerializeField] Image mirrorIndicatorImage;

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

    private float CPerfectCount = 0f, CPerfectCountMax;
    private float perfectCount = 0f, perfectCountMax;
    private float goodCount = 0f, goodCountMax;
    private float damageCount = 0f, damageCountMax;
    private float missCount = 0f, missCountMax;

    private float maxCombo = 0, maxComboMax;
    private float earlyCount = 0, earlyCountMax;
    private float lateCount = 0, lateCountMax;

    private float totalScore = 0f, totalScoreMax;
    
    //Count checker
    private bool isCPerfectCounted = false;
    private bool isPerfectCounted = false;
    private bool isGoodCounted = false;
    private bool isDamageCounted = false;
    private bool isMissCounted = false;

    private bool isMaxComboCounted = false;
    private bool isEarlyCounted = false;
    private bool isLateCounted = false;

    private bool isTotalScoreCounted = false;

    //Count Rates
    private float CPerfectCountRate;
    private float perfectCountRate;
    private float goodCountRate;
    private float damageCountRate;
    private float missCountRate;

    private float maxComboCountRate;
    private float earlyCountRate;
    private float lateCountRate;

    private float scoreCountRate;

    [Space(10.0f)]
    [SerializeField] float countDuration = 1f;
    private float additionValue;
    private bool isStarted = false;

    void Start()
    {
        if (PlayerPrefs.GetInt(ValueStorer.prefsIsMirror, 0) == 0)
            mirrorIndicatorImage.gameObject.SetActive(false);

        InsertResults();
        CalculateRate();
    }

    private void FixedUpdate()
    {
        if (isStarted)
        {
            if (!isCPerfectCounted)
            {
                CPerfectCount += CPerfectCountRate;
                if ((int)CPerfectCount >= CPerfectCountMax)
                {
                    CPerfectCount = CPerfectCountMax;
                    isCPerfectCounted = true;
                }
                CPerfectCountText.text = ((int)CPerfectCount).ToString();
            }

            if (!isPerfectCounted)
            {
                perfectCount += perfectCountRate;
                if ((int)perfectCount >= perfectCountMax)
                {
                    perfectCount = perfectCountMax;
                    isPerfectCounted = true;
                }
                perfectCountText.text = ((int)perfectCount).ToString();
            }

            if (!isGoodCounted)
            {
                goodCount += goodCountRate;
                if ((int)goodCount >= goodCountMax)
                {
                    goodCount = goodCountMax;
                    isGoodCounted = true;
                }
                goodCountText.text = ((int)goodCount).ToString();
            }

            if (!isDamageCounted)
            {
                damageCount += damageCountRate;
                if ((int)damageCount >= damageCountMax)
                {
                    damageCount = damageCountMax;
                    isDamageCounted = true;
                }
                damageCountText.text = ((int)damageCount).ToString();
            }

            if (!isMissCounted)
            {
                missCount += missCountRate;
                if ((int)missCount >= missCountMax)
                {
                    missCount = missCountMax;
                    isMissCounted = true;
                }
                missCountText.text = ((int)missCount).ToString();
            }

            if (!isEarlyCounted)
            {
                earlyCount += earlyCountRate;
                if ((int)earlyCount >= earlyCountMax)
                {
                    earlyCount = earlyCountMax;
                    isEarlyCounted = true;
                }
                earlyCountText.text = ((int)earlyCount).ToString();
            }

            if (!isLateCounted)
            {
                lateCount += lateCountRate;
                if ((int)lateCount >= lateCountMax)
                {
                    lateCount = lateCountMax;
                    isLateCounted = true;
                }
                lateCountText.text = ((int)lateCount).ToString();
            }

            if (!isMaxComboCounted)
            {
                maxCombo += maxComboCountRate;
                if ((int)maxCombo >= maxComboMax)
                {
                    maxCombo = maxComboMax;
                    isMaxComboCounted = true;
                }
                maxComboText.text = ((int)maxCombo).ToString();
            }

            if (!isTotalScoreCounted)
            {
                totalScore += scoreCountRate;
                if (totalScore >= totalScoreMax)
                {
                    totalScore = totalScoreMax;
                    isTotalScoreCounted = true;
                }
                scoreText.text = totalScore.ToString("0.0000") + "%";
            }

            if (isCPerfectCounted && isPerfectCounted && isGoodCounted && isDamageCounted && isMissCounted
                && isMaxComboCounted && isEarlyCounted && isLateCounted && isTotalScoreCounted)
                isStarted = false;
        }
    }

    void CalculateRate()
    {
        CPerfectCountRate = CPerfectCountMax / additionValue;
        perfectCountRate = perfectCountMax / additionValue;
        goodCountRate = goodCountMax / additionValue;
        damageCountRate = damageCountMax / additionValue;
        missCountRate = missCountMax / additionValue;

        maxComboCountRate = maxComboMax / additionValue;
        earlyCountRate = earlyCountMax / additionValue;
        lateCountRate = lateCountMax / additionValue;

        scoreCountRate = totalScoreMax / additionValue;
    }

    void InsertResults()
    {
        additionValue = countDuration / Time.fixedDeltaTime;

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

        CPerfectCountMax = (float)RGManager.CPerfectNotes;
        perfectCountMax = (float)RGManager.perfectNotes;
        goodCountMax = (float)RGManager.goodNotes;
        damageCountMax = (float)RGManager.damageNotes;
        missCountMax = (float)RGManager.missNotes;

        earlyCountMax = (float)RGManager.earlyCount;
        lateCountMax = (float)RGManager.lateCount;
        maxComboMax = (float)RGManager.maxComboCount;

        CPerfectCountText.text = ((int)CPerfectCount).ToString();
        perfectCountText.text = ((int)perfectCount).ToString();
        goodCountText.text = ((int)goodCount).ToString();
        damageCountText.text = ((int)damageCount).ToString();
        missCountText.text = ((int)missCount).ToString();

        earlyCountText.text = ((int)earlyCount).ToString();
        lateCountText.text = ((int)lateCount).ToString();
        maxComboText.text = ((int)maxCombo).ToString();

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

        totalScoreMax = RGManager.totalScore;
        scoreText.text = totalScore.ToString("0.0000") + "%";
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

    public void StartCounting()
    {
        isStarted = true;
    }
}
