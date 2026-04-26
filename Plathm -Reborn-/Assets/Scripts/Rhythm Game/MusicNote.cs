using UnityEngine;

public class MusicNote : MonoBehaviour
{
    RhythmGameManager rhythmManager;
    RhythmPlayer player;

    public enum NoteType
    {
        TAP_NOTE,
        BLACK_NOTE,
        SLICE_NOTE,
        LEFT_TELEPORT,
        RIGHT_TELEPORT,
        MIDDLE_SPIKE,
        SIDE_SPIKE,
    }

    [SerializeField] NoteType noteType;

    public TimingGroup timingGroup;
    public double timing;

    [SerializeField] bool isBlackActivated = false;

    private void Start()
    {
        rhythmManager = GameObject.FindFirstObjectByType<RhythmGameManager>();
        player = GameObject.FindFirstObjectByType<RhythmPlayer>();
    }

    private void Update()
    {
        if (noteType == NoteType.BLACK_NOTE && isBlackActivated && rhythmManager.currentTiming / 1000f >= timing)
        {
            Instantiate(rhythmManager.blackCPerfectPrefab,
                new Vector3(this.transform.position.x, 0, 0),
                Quaternion.identity);
            //audioManager.AddSound(audioManager.blackSound);
            rhythmManager.CPerfectNotes += 1;
            rhythmManager.comboCount += 1;
            if (rhythmManager.comboCount > rhythmManager.maxComboCount)
                rhythmManager.maxComboCount += 1;
            rhythmManager.CalculateScore();
            SwitchToUsedFolder();
            return;
        }

        if (noteType == NoteType.MIDDLE_SPIKE && this.timing - rhythmManager.currentTiming / 1000f <= 0)
        {
            if (player.lanePosition != RhythmGameManager.LanePosition.MIDDLE_POS)
            {
                Instantiate(rhythmManager.spikesPrefab, ValueStorer.middleLanePosition, Quaternion.identity);
                //audioManager.AddSound(audioManager.spikeSound);
                Debug.Log("Avoided MIDDLE SPIKE");
                rhythmManager.CPerfectNotes += 1;
                rhythmManager.comboCount += 1;
                if (rhythmManager.comboCount > rhythmManager.maxComboCount)
                    rhythmManager.maxComboCount += 1;
                rhythmManager.CalculateScore();
            }
            else
            {
                rhythmManager.damageNotes += 1;
                rhythmManager.comboCount = 0;
                rhythmManager.DeductHealth(5f);
                rhythmManager.CalculateScore(true);
            }

            SwitchToUsedFolder();
            return;
        }

        if (noteType == NoteType.SIDE_SPIKE && this.timing - rhythmManager.currentTiming / 1000f <= 0)
        {
            if (player.lanePosition == RhythmGameManager.LanePosition.MIDDLE_POS)
            {
                Instantiate(rhythmManager.spikesPrefab, ValueStorer.leftLanePosition, Quaternion.identity);
                Instantiate(rhythmManager.spikesPrefab, ValueStorer.rightLanePosition, Quaternion.identity);
                //audioManager.AddSound(audioManager.spikeSound);
                Debug.Log("Avoided SIDE SPIKE");
                rhythmManager.CPerfectNotes += 1;
                rhythmManager.comboCount += 1;
                if (rhythmManager.comboCount > rhythmManager.maxComboCount)
                    rhythmManager.maxComboCount += 1;
                rhythmManager.CalculateScore();
            }
            else
            {
                rhythmManager.damageNotes += 1;
                rhythmManager.comboCount = 0;
                rhythmManager.DeductHealth(5f);
                rhythmManager.CalculateScore(true);
            }

            SwitchToUsedFolder();
            return;
        }

        //Check missed notes
        if (timing - rhythmManager.currentTiming / 1000f < -ValueStorer.goodJudgement && rhythmManager.isStarted)
        {
            Debug.Log("MISSED, " + timingGroup);
            rhythmManager.comboCount = 0;
            if (noteType == NoteType.SLICE_NOTE)
            {
                rhythmManager.damageNotes += 1;
                rhythmManager.DeductHealth(10f);
            }
            else
            {
                rhythmManager.missNotes += 1;
            }
            rhythmManager.CalculateScore(true);
            SwitchToUsedFolder();
        }
    }

    public void ChangeSpeedPosition(double totalLength, double chartSpeed, double beginTiming, double speedMulti)
    {
        this.transform.position = new Vector3(
            this.transform.position.x,
            (float)(((this.timing * 1000f - beginTiming) / 1000f * speedMulti + totalLength) * chartSpeed), 0);
    }

    public void ExecuteNote()
    {
        if (Mathf.Abs((float)(timing - rhythmManager.currentTiming / 1000f)) > ValueStorer.goodJudgement)
        {
            return;
        }

        if (noteType == NoteType.BLACK_NOTE)
        {
            isBlackActivated = true;
            return;
        }
        else
        {
        /*
            if (this.noteType == NoteType.TAP_NOTE)
                audioManager.AddSound(audioManager.tapSound);
            else if (this.noteType == NoteType.SLICE_NOTE)
                audioManager.AddSound(audioManager.sliceSound);
            else if (this.noteType == NoteType.LEFT_TELEPORT || this.noteType == NoteType.RIGHT_TELEPORT)
                audioManager.AddSound(audioManager.teleportSound);

        */
            InstantiateJudgementEffects();
        }

        SwitchToUsedFolder();
    }

    void InstantiateJudgementEffects()
    {
        GameObject judgementSelected = null;

        if (Mathf.Abs((float)(timing - rhythmManager.currentTiming / 1000f)) <= ValueStorer.cPerfectJudgement)
        {
            judgementSelected = SelectJudgementVFX(RhythmGameManager.JudgementType.CPERFECT);
            Instantiate(judgementSelected, new Vector3(transform.position.x, 0, 0), Quaternion.identity);
            rhythmManager.CPerfectNotes += 1;
            rhythmManager.comboCount += 1;
            if (rhythmManager.comboCount > rhythmManager.maxComboCount)
                rhythmManager.maxComboCount += 1;
            rhythmManager.CalculateScore();
            return;
        }

        if (Mathf.Abs((float)(timing - rhythmManager.currentTiming / 1000f)) <= ValueStorer.perfectJudgement)
        {
            if (rhythmManager.currentTiming / 1000f < timing) rhythmManager.earlyCount += 1;
            else if (rhythmManager.currentTiming / 1000f > timing) rhythmManager.lateCount += 1;

            judgementSelected = SelectJudgementVFX(RhythmGameManager.JudgementType.PERFECT);
            Instantiate(judgementSelected, new Vector3(transform.position.x, 0, 0), Quaternion.identity);
            rhythmManager.perfectNotes += 1;
            rhythmManager.comboCount += 1;
            if (rhythmManager.comboCount > rhythmManager.maxComboCount)
                rhythmManager.maxComboCount += 1;
            rhythmManager.CalculateScore();
            return;
        }

        if (Mathf.Abs((float)(timing - rhythmManager.currentTiming / 1000f)) <= ValueStorer.goodJudgement)
        {
            if (rhythmManager.currentTiming / 1000f < timing) rhythmManager.earlyCount += 1;
            else if (rhythmManager.currentTiming / 1000f > timing) rhythmManager.lateCount += 1;

            judgementSelected = SelectJudgementVFX(RhythmGameManager.JudgementType.GOOD);
            Instantiate(judgementSelected, new Vector3(transform.position.x, 0, 0), Quaternion.identity);
            rhythmManager.goodNotes += 1;
            rhythmManager.comboCount += 1;
            if (rhythmManager.comboCount > rhythmManager.maxComboCount)
                rhythmManager.maxComboCount += 1;
            rhythmManager.CalculateScore();
            return;
        }
    }

    GameObject SelectJudgementVFX(RhythmGameManager.JudgementType judgementType)
    {
        if (judgementType == RhythmGameManager.JudgementType.CPERFECT)
        {
            if (noteType == NoteType.TAP_NOTE) return rhythmManager.tapCPerfectPrefab;
            else if (noteType == NoteType.SLICE_NOTE) return rhythmManager.sliceCPerfectPrefab;
            else if (noteType == NoteType.LEFT_TELEPORT) return rhythmManager.leftTeleportCPerfectPrefab;
            else if (noteType == NoteType.RIGHT_TELEPORT) return rhythmManager.rightTeleportCPerfectPrefab;
        }
        else if (judgementType == RhythmGameManager.JudgementType.PERFECT)
        {
            if (noteType == NoteType.TAP_NOTE) return rhythmManager.tapPerfectPrefab;
            else if (noteType == NoteType.SLICE_NOTE) return rhythmManager.slicePerfectPrefab;
            else if (noteType == NoteType.LEFT_TELEPORT) return rhythmManager.leftTeleportPerfectPrefab;
            else if (noteType == NoteType.RIGHT_TELEPORT) return rhythmManager.rightTeleportPerfectPrefab;
        }
        else if (judgementType == RhythmGameManager.JudgementType.GOOD)
        {
            if (noteType == NoteType.TAP_NOTE) return rhythmManager.tapGoodPrefab;
            else if (noteType == NoteType.SLICE_NOTE) return rhythmManager.sliceGoodPrefab;
            else if (noteType == NoteType.LEFT_TELEPORT) return rhythmManager.leftTeleportGoodPrefab;
            else if (noteType == NoteType.RIGHT_TELEPORT) return rhythmManager.rightTeleportGoodPrefab;
        }

        return null;
    }

    public void SwitchToUsedFolder()
    {
        if (!timingGroup)
        {
            return;
        }

        transform.SetParent(timingGroup.usedNotesFolder.transform);
        gameObject.SetActive(false);
    }
}
