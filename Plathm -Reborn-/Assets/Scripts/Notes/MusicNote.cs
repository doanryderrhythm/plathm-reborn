using UnityEngine;

public class MusicNote : MonoBehaviour
{
    //Other shenanigans
    EditorManager editorManager;

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

    private void Awake()
    {
        editorManager = GameObject.FindFirstObjectByType<EditorManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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

        SwitchToUsedFolder();
    }

    void SwitchToUsedFolder()
    {
        editorManager.SwitchToUsedFolder(transform);
        gameObject.SetActive(false);
    }
}
