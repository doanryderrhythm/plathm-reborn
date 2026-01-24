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
        //Not checking negative timing in notes
        if (this.transform.position.y > 0)
        {
            return;
        }

        if (noteType == NoteType.BLACK_NOTE && isBlackActivated)
        {
            SwitchToUsedFolder();
            return;
        }

        if (noteType == NoteType.MIDDLE_SPIKE)
        {
            if (player.GetLanePosition() == EditorManager.LanePosition.MIDDLE_POS)
            {
                Debug.Log("Damaged by MIDDLE SPIKE");
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

            SwitchToUsedFolder();
            return;
        }

        //Check missed notes
        if (this.transform.position.y < -ValueStorer.goodJudgement * editorManager.scrollSpeed)
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

        SwitchToUsedFolder();
    }

    void SwitchToUsedFolder()
    {
        editorManager.SwitchToUsedFolder(transform);
        gameObject.SetActive(false);
    }
}
