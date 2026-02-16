using UnityEngine;

public class MusicNote : MonoBehaviour
{
    //Other shenanigans
    EditorManager editorManager;
    AudioManager audioManager;
    TestPlayer player;

    //Note details
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

    public TimingGroup timingGroup;

    public bool isSelected = false;

    public float timing;
    public float temporaryTiming;

    [SerializeField] NoteType noteType;
    [SerializeField] bool isBlackActivated = false;
    public bool isAutoActivated = false;

    private void Awake()
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();
        player = GameObject.FindFirstObjectByType<TestPlayer>();
        audioManager = GameObject.FindFirstObjectByType<AudioManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!editorManager.playMode)
        {
            if (noteType == NoteType.BLACK_NOTE && isBlackActivated)
            {
                isBlackActivated = false;
            }
            isAutoActivated = false;
            return;
        }

        if (isAutoActivated)
        {
            return;
        }

        if (editorManager.autoplayToggle.isOn)
        {
            if (editorManager.audioSource.time < timing)
            {
                return;
            }

            if (isAutoActivated && editorManager.audioSource.time >= timing)
            {
                return;
            }

            isAutoActivated = true;

            if (noteType == NoteType.TAP_NOTE) audioManager.AddSound(audioManager.tapSound);
            else if (noteType == NoteType.BLACK_NOTE) audioManager.AddSound(audioManager.blackSound);
            else if (noteType == NoteType.SLICE_NOTE) audioManager.AddSound(audioManager.sliceSound);
            else if (noteType == NoteType.LEFT_TELEPORT || noteType == NoteType.RIGHT_TELEPORT) audioManager.AddSound(audioManager.teleportSound);
            else if (noteType == NoteType.SIDE_SPIKE || noteType == NoteType.MIDDLE_SPIKE) audioManager.AddSound(audioManager.spikeSound);

            SwitchToUsedFolder();

            return;
        }

        if (noteType == NoteType.BLACK_NOTE && isBlackActivated && editorManager.audioSource.time >= timing)
        {
            Instantiate(editorManager.blackCPerfectPrefab,
                new Vector3(this.transform.position.x, 0, 0),
                Quaternion.identity);
            audioManager.AddSound(audioManager.blackSound);
            SwitchToUsedFolder();
            return;
        }

        if (noteType == NoteType.MIDDLE_SPIKE && this.timing - editorManager.audioSource.time <= 0)
        {
            if (player.GetLanePosition() != EditorManager.LanePosition.MIDDLE_POS)
            {
                Instantiate(editorManager.spikesPrefab, ValueStorer.middleLanePosition, Quaternion.identity);
                audioManager.AddSound(audioManager.spikeSound);
                Debug.Log("Avoided MIDDLE SPIKE");
            }

            SwitchToUsedFolder();
            return;
        }

        if (noteType == NoteType.SIDE_SPIKE && this.timing - editorManager.audioSource.time <= 0)
        {
            if (player.GetLanePosition() == EditorManager.LanePosition.MIDDLE_POS)
            {
                Instantiate(editorManager.spikesPrefab, ValueStorer.leftLanePosition, Quaternion.identity);
                Instantiate(editorManager.spikesPrefab, ValueStorer.rightLanePosition, Quaternion.identity);
                audioManager.AddSound(audioManager.spikeSound);
                Debug.Log("Avoided SIDE SPIKE");
            }

            SwitchToUsedFolder();
            return;
        }

        //Check missed notes
        if (timing - editorManager.audioSource.time < -ValueStorer.goodJudgement && editorManager.playMode)
        {
            Debug.Log("MISSED, " + timingGroup);
            SwitchToUsedFolder();
        }    
    }

    public void ExecuteNote()
    {
        if (!editorManager)
        {
            return;
        }

        if (Mathf.Abs(timing - editorManager.audioSource.time) > ValueStorer.goodJudgement)
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
            if (this.noteType == NoteType.TAP_NOTE)
                audioManager.AddSound(audioManager.tapSound);
            else if (this.noteType == NoteType.SLICE_NOTE)
                audioManager.AddSound(audioManager.sliceSound);
            else if (this.noteType == NoteType.LEFT_TELEPORT || this.noteType == NoteType.RIGHT_TELEPORT)
                audioManager.AddSound(audioManager.teleportSound);

            InstantiateJudgementEffects();
        }

        SwitchToUsedFolder();
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

    void InstantiateJudgementEffects()
    {
        GameObject judgementSelected = null;

        if (Mathf.Abs(timing - editorManager.audioSource.time) <= ValueStorer.cPerfectJudgement)
        {
            judgementSelected = SelectJudgementVFX(EditorManager.JudgementType.CPERFECT);
            Instantiate(judgementSelected, new Vector3(transform.position.x, 0, 0), Quaternion.identity);
            return;
        }

        if (Mathf.Abs(timing - editorManager.audioSource.time) <= ValueStorer.perfectJudgement)
        {
            judgementSelected = SelectJudgementVFX(EditorManager.JudgementType.PERFECT);
            Instantiate(judgementSelected, new Vector3(transform.position.x, 0, 0), Quaternion.identity);
            return;
        }

        if (Mathf.Abs(timing - editorManager.audioSource.time) <= ValueStorer.goodJudgement)
        {
            judgementSelected = SelectJudgementVFX(EditorManager.JudgementType.GOOD);
            Instantiate(judgementSelected, new Vector3(transform.position.x, 0, 0), Quaternion.identity);
            return;
        }
    }

    GameObject SelectJudgementVFX(EditorManager.JudgementType judgementType)
    {
        if (judgementType == EditorManager.JudgementType.CPERFECT)
        {
            if (noteType == NoteType.TAP_NOTE) return editorManager.tapCPerfectPrefab;
            else if (noteType == NoteType.SLICE_NOTE) return editorManager.sliceCPerfectPrefab;
            else if (noteType == NoteType.LEFT_TELEPORT) return editorManager.leftTeleportCPerfectPrefab;
            else if (noteType == NoteType.RIGHT_TELEPORT) return editorManager.rightTeleportCPerfectPrefab;
        }
        else if (judgementType == EditorManager.JudgementType.PERFECT)
        {
            if (noteType == NoteType.TAP_NOTE) return editorManager.tapPerfectPrefab;
            else if (noteType == NoteType.SLICE_NOTE) return editorManager.slicePerfectPrefab;
            else if (noteType == NoteType.LEFT_TELEPORT) return editorManager.leftTeleportPerfectPrefab;
            else if (noteType == NoteType.RIGHT_TELEPORT) return editorManager.rightTeleportPerfectPrefab;
        }
        else if (judgementType == EditorManager.JudgementType.GOOD)
        {
            if (noteType == NoteType.TAP_NOTE) return editorManager.tapGoodPrefab;
            else if (noteType == NoteType.SLICE_NOTE) return editorManager.sliceGoodPrefab;
            else if (noteType == NoteType.LEFT_TELEPORT) return editorManager.leftTeleportGoodPrefab;
            else if (noteType == NoteType.RIGHT_TELEPORT) return editorManager.rightTeleportGoodPrefab;
        }

        return null;
    }

    public bool IsWithinArea(Vector3 mousePosition)
    {
        if (editorManager.playMode)
        {
            return false;
        }

        if (noteType == NoteType.TAP_NOTE)
        {
            if ((mousePosition.x > transform.position.x - ValueStorer.tapWidth * 0.5 && mousePosition.x < transform.position.x + ValueStorer.tapWidth * 0.5)
                && (mousePosition.y > transform.position.y - ValueStorer.tapHeight * 0.5 && mousePosition.y < transform.position.y + ValueStorer.tapHeight * 0.5))
            {
                return true;
            }
        }
        else if (noteType == NoteType.BLACK_NOTE)
        {
            if ((mousePosition.x > transform.position.x - ValueStorer.blackWidth * 0.5 && mousePosition.x < transform.position.x + ValueStorer.blackWidth * 0.5)
                && (mousePosition.y > transform.position.y - ValueStorer.blackHeight * 0.5 && mousePosition.y < transform.position.y + ValueStorer.blackHeight * 0.5))
            {
                return true;
            }
        }
        else if (noteType == NoteType.SLICE_NOTE)
        {
            if ((mousePosition.x > transform.position.x - ValueStorer.sliceWidth * 0.5 && mousePosition.x < transform.position.x + ValueStorer.sliceWidth * 0.5)
                && (mousePosition.y > transform.position.y - ValueStorer.sliceHeight * 0.5 && mousePosition.y < transform.position.y + ValueStorer.sliceHeight * 0.5))
            {
                return true;
            }
        }
        else if (noteType == NoteType.LEFT_TELEPORT || noteType == NoteType.RIGHT_TELEPORT)
        {
            if ((mousePosition.x > transform.position.x - ValueStorer.teleportWidth * 0.5 && mousePosition.x < transform.position.x + ValueStorer.teleportWidth * 0.5)
                && (mousePosition.y > transform.position.y - ValueStorer.teleportHeight * 0.5 && mousePosition.y < transform.position.y + ValueStorer.teleportHeight * 0.5))
            {
                return true;
            }
        }
        else if (noteType == NoteType.MIDDLE_SPIKE)
        {
            if ((mousePosition.x > transform.position.x - ValueStorer.spikeWidth * 0.5 && mousePosition.x < transform.position.x + ValueStorer.spikeWidth * 0.5)
                && (mousePosition.y > transform.position.y - ValueStorer.spikeHeight * 0.5 && mousePosition.y < transform.position.y + ValueStorer.spikeHeight * 0.5))
            {
                return true;
            }
        }
        else if (noteType == NoteType.SIDE_SPIKE)
        {
            if ((mousePosition.x > ValueStorer.minLeftLaneX && mousePosition.x < ValueStorer.maxRightLaneX)
                && (mousePosition.x < ValueStorer.maxLeftLaneX || mousePosition.x > ValueStorer.minRightLaneX)
                && (mousePosition.y > transform.position.y - ValueStorer.spikeHeight * 0.5 && mousePosition.y < transform.position.y + ValueStorer.spikeHeight * 0.5))
            {
                return true;
            }
        }

        return false;
    }

    public float DistanceFromPosition(Vector3 targetPosition)
    {
        return Mathf.Sqrt(
            Mathf.Pow(targetPosition.x - transform.position.x, 2) +
            Mathf.Pow(targetPosition.y - transform.position.y, 2));
    }

    public void ChangeSpeedPosition(float totalLength, float chartSpeed, float beginTiming, float speedMulti)
    {
        this.transform.position = new Vector3(
            this.transform.position.x, 
            ((this.timing * 1000f - beginTiming) / 1000f * speedMulti + totalLength) * chartSpeed, 0);
    }

    public void ResetSpeedPosition(float chartSpeed)
    {
        this.transform.position = new Vector3(this.transform.position.x, this.timing * chartSpeed, 0);
    }

    public NoteType GetNoteType()
    {
        return noteType;
    }

    public void ToggleSelected(bool toggle)
    {
        isSelected = toggle;
        if (isSelected)
        {
            GetComponent<SpriteRenderer>().color = ValueStorer.chosenNoteColor;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
