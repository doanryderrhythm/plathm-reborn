using UnityEngine;

public class MusicNote : MonoBehaviour
{
    //Other shenanigans
    EditorManager editorManager;
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

    [SerializeField] NoteType noteType;
    [SerializeField] bool isBlackActivated = false;

    private void Awake()
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();
        player = GameObject.FindFirstObjectByType<TestPlayer>();
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
            return;
        }

        //Not checking negative timing in notes
        if (this.transform.position.y > 0)
        {
            return;
        }

        if (noteType == NoteType.BLACK_NOTE && isBlackActivated)
        {
            Instantiate(editorManager.blackCPerfectPrefab,
                new Vector3(this.transform.position.x, 0, 0),
                Quaternion.identity);
            SwitchToUsedFolder();
            return;
        }

        if (noteType == NoteType.MIDDLE_SPIKE)
        {
            if (player.GetLanePosition() == EditorManager.LanePosition.MIDDLE_POS)
            {
                Debug.Log("Damaged by MIDDLE SPIKE");
            }
            else
            {
                Instantiate(editorManager.spikesPrefab, ValueStorer.middleLanePosition, Quaternion.identity);
            }

            SwitchToUsedFolder();
            return;
        }

        if (noteType == NoteType.SIDE_SPIKE)
        {
            if (player.GetLanePosition() != EditorManager.LanePosition.MIDDLE_POS)
            {
                Debug.Log("Damaged by SIDE SPIKE");
            }
            else
            {
                Instantiate(editorManager.spikesPrefab, ValueStorer.leftLanePosition, Quaternion.identity);
                Instantiate(editorManager.spikesPrefab, ValueStorer.rightLanePosition, Quaternion.identity);
            }

            SwitchToUsedFolder();
            return;
        }

        //Check missed notes
        if (this.transform.position.y < -ValueStorer.goodJudgement * editorManager.scrollSpeed && editorManager.playMode)
        {
            SwitchToUsedFolder();
        }    
    }

    public void ExecuteNote()
    {
        if (!editorManager)
        {
            return;
        }

        if (Mathf.Abs(this.transform.position.y) > ValueStorer.goodJudgement * editorManager.scrollSpeed)
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
            InstantiateJudgementEffects();
        }

        SwitchToUsedFolder();
    }

    void SwitchToUsedFolder()
    {
        editorManager.SwitchToUsedFolder(transform);
        gameObject.SetActive(false);
    }

    void InstantiateJudgementEffects()
    {
        GameObject judgementSelected = null;

        if (Mathf.Abs(this.transform.position.y) <= ValueStorer.cPerfectJudgement * editorManager.scrollSpeed)
        {
            judgementSelected = SelectJudgementVFX(EditorManager.JudgementType.CPERFECT);
            Instantiate(judgementSelected, new Vector3(transform.position.x, 0, 0), Quaternion.identity);
            return;
        }

        if (Mathf.Abs(this.transform.position.y) <= ValueStorer.perfectJudgement * editorManager.scrollSpeed)
        {
            judgementSelected = SelectJudgementVFX(EditorManager.JudgementType.PERFECT);
            Instantiate(judgementSelected, new Vector3(transform.position.x, 0, 0), Quaternion.identity);
            return;
        }

        if (Mathf.Abs(this.transform.position.y) <= ValueStorer.goodJudgement * editorManager.scrollSpeed)
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
}
