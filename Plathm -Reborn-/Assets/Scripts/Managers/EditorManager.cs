using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    public enum LanePosition
    {
        NONE,
        LEFT_POS,
        MIDDLE_POS,
        RIGHT_POS,
    }

    public enum JudgementType
    {
        CPERFECT,
        PERFECT,
        GOOD,
        DAMAGE,
        MISS,
    }

    public enum NoteTypeGeneral
    {
        TAP_NOTE,
        BLACK_NOTE,
        LEFT_TELEPORT,
        RIGHT_TELEPORT,
        SLICE_NOTE,
        SPIKE,
    }

    public enum EditOption
    {
        NOTE_ADD,
        NOTE_DRAG,
    }

    //Chart shenanigans
    private Camera mainCamera;
    public Vector3 worldPosition;
    public float verticalGridValue;
    [SerializeField] EditOption editOption;

    //Undo & Redo System shenanigans
    private Stack<EditorCommand> undoCommandStack;
    private Stack<EditorCommand> redoCommandStack;
    private float noteOriginalTiming;     //move note
    private Vector3 noteOriginalPosition; //move note

    [Space(10.0f)]
    //Gameplay shenanigans
    private HashSet<Key> reservedTapKeys;
    private HashSet<Key> reservedBlackKeys;
    private bool isAnyKeyHolding = false;
    private bool isBlackKeyReserved = false;
    private List<SpeedStorer> speedItems = null;

    //Note selection
    [SerializeField] bool isNoteTypeSelected;
    [SerializeField] NoteTypeGeneral selectedNoteType;
    public MusicNote draggedNote;

    [Space(10.0f)]
    [SerializeField] GameObject stepBeatPrefab;
    [SerializeField] GameObject mainBeatPrefab;

    [Header("Editor Settings")]
    public bool playMode = false;
    public float chartOffset = 0f;
    public float chartSpeed = 1f;
    public float speedMulti = 1f;
    public int beatDensity = 1;

    [Header("Input Actions")]
    [SerializeField] InputActionReference inputAnyKey;
    [SerializeField] InputActionReference inputLeftTeleport;
    [SerializeField] InputActionReference inputRightTeleport;
    [SerializeField] InputActionReference inputSlice;

    [Header("Note Folders")]
    [SerializeField] GameObject tapFolder;
    [SerializeField] GameObject blackFolder;
    [SerializeField] GameObject sliceFolder;
    [SerializeField] GameObject leftTeleportFolder;
    [SerializeField] GameObject rightTeleportFolder;
    [SerializeField] GameObject spikeFolder;
    [SerializeField] GameObject usedNotesFolder;
    [SerializeField] GameObject undoRedoFolder;
    [SerializeField] GameObject beatLinesFolder;

    [Header("Note Types")]
    [SerializeField] GameObject tapNotePrefab;
    [SerializeField] GameObject blackNotePrefab;
    [SerializeField] GameObject leftTeleportPrefab;
    [SerializeField] GameObject rightTeleportPrefab;
    [SerializeField] GameObject sliceNotePrefab;
    [SerializeField] GameObject middleSpikePrefab;
    [SerializeField] GameObject sideSpikePrefab;

    [Header("Music")]
    public AudioSource audioSource;

    [Header("Gameplay")]
    [SerializeField] GameObject scrollPlayfield;

    [Header("Judgement VFX")]
    public GameObject tapCPerfectPrefab;
    public GameObject tapPerfectPrefab;
    public GameObject tapGoodPrefab;
    [Space(10.0f)]
    public GameObject blackCPerfectPrefab;
    [Space(10.0f)]
    public GameObject sliceCPerfectPrefab;
    public GameObject slicePerfectPrefab;
    public GameObject sliceGoodPrefab;
    [Space(10.0f)]
    public GameObject leftTeleportCPerfectPrefab;
    public GameObject leftTeleportPerfectPrefab;
    public GameObject leftTeleportGoodPrefab;
    [Space(10.0f)]
    public GameObject rightTeleportCPerfectPrefab;
    public GameObject rightTeleportPerfectPrefab;
    public GameObject rightTeleportGoodPrefab;
    [Space(10.0f)]
    public GameObject spikesPrefab;

    [Header("UI")]
    [SerializeField] UIManager uiManager;
    [SerializeField] TMP_Dropdown noteSelectDropDown;
    //Texts
    [Space(10.0f)]
    [SerializeField] TMP_Text currentTimingText;
    [SerializeField] TMP_Text chartOffsetText;
    [SerializeField] TMP_Text chartSpeedText;
    //Input Fields
    [Space(10.0f)]
    [SerializeField] TMP_InputField offsetInputField;
    [SerializeField] TMP_InputField speedInputField;

    //[Header("Undo & Redo")]

    private void Awake()
    {
        mainCamera = GameObject.FindFirstObjectByType<Camera>();

        isNoteTypeSelected = false;
        draggedNote = null;

        reservedTapKeys = new HashSet<Key>();
        reservedBlackKeys = new HashSet<Key>();

        undoCommandStack = new Stack<EditorCommand>();
        undoCommandStack.Clear();
        redoCommandStack = new Stack<EditorCommand>();
        redoCommandStack.Clear();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitiateUI();
        RebuildReservedKeys();
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            currentTimingText.text = ValueStorer.currentTimingText + ((int)(audioSource.time * 1000)).ToString();
        }
        else
        {
            currentTimingText.text = ValueStorer.currentTimingText + "0";
        }

        ConvertFromMouseToWorld();

        if (!playMode
            && editOption == EditOption.NOTE_ADD
            && isNoteTypeSelected
            && Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (worldPosition.y >= 0f)
            {
                InsertNote(worldPosition);
            }
        }

        if (!playMode
            && editOption == EditOption.NOTE_DRAG
            && Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                draggedNote = FindSmallestDistanceNote(worldPosition);
                if (draggedNote)
                {
                    noteOriginalTiming = draggedNote.timing;
                    noteOriginalPosition = draggedNote.gameObject.transform.position;
                    if (draggedNote.GetNoteType() == MusicNote.NoteType.TAP_NOTE ||
                        draggedNote.GetNoteType() == MusicNote.NoteType.BLACK_NOTE)
                    {
                        verticalGridValue = noteOriginalPosition.x;
                    }
                }
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && draggedNote)
            {
                CommandMoveOneNote commandMoveOneNote = new CommandMoveOneNote(
                    draggedNote.gameObject, 
                    noteOriginalTiming, 
                    draggedNote.timing, 
                    noteOriginalPosition.x, 
                    draggedNote.gameObject.transform.position.x);
                OverrideCommand(commandMoveOneNote);

                draggedNote = null;
            }
            else if (Mouse.current.leftButton.isPressed && draggedNote)
            {
                MoveNote();
            }
        }

        if (!playMode && Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            ExecuteDeleteInFolder(tapFolder.transform);
            ExecuteDeleteInFolder(blackFolder.transform);
            ExecuteDeleteInFolder(sliceFolder.transform);
            ExecuteDeleteInFolder(leftTeleportFolder.transform);
            ExecuteDeleteInFolder(rightTeleportFolder.transform);
            ExecuteDeleteInFolder(spikeFolder.transform);
        }

        if (playMode)
        {
            ChangeSpeedThroughTiming(audioSource.time * 1000f);
        }

        if (isAnyKeyHolding && isBlackKeyReserved)
        {
            ExecuteInput(blackFolder);
        }
    }

    private void OnEnable()
    {
        inputAnyKey.action.started     += OnAnyKeyStarted;
        inputAnyKey.action.canceled    += OnAnyKeyCanceled;
        inputAnyKey.action.performed   += CheckReservedTapKeys;

        inputLeftTeleport.action.performed  += _ => ExecuteInput(leftTeleportFolder);
        inputRightTeleport.action.performed += _ => ExecuteInput(rightTeleportFolder);
        inputSlice.action.performed         += _ => ExecuteInput(sliceFolder);
    }

    private void OnDisable()
    {
        inputAnyKey.action.performed -= CheckReservedTapKeys;

        inputLeftTeleport.action.performed  -= _ => { };
        inputRightTeleport.action.performed -= _ => { };
        inputSlice.action.performed         -= _ => { };
    }

    void AddReservedKeys(InputAction inputAction, ref HashSet<Key> reservedKeys)
    {
        foreach (var binding in inputAction.bindings)
        {
            if (!binding.effectivePath.StartsWith("<Keyboard>/"))
            {
                continue;
            }

            string keyName = binding.effectivePath.Replace("<Keyboard>/", "");
            if (System.Enum.TryParse(keyName, true, out Key key))
            {
                reservedKeys.Add(key);
            }
        }
    }

    void InitiateUI()
    {
        chartOffsetText.text = ValueStorer.chartOffsetText + chartOffset.ToString();
        chartSpeedText.text = ValueStorer.chartSpeedText + chartSpeed.ToString();
    }

    void RebuildReservedKeys()
    {
        reservedTapKeys.Clear();
        AddReservedKeys(inputLeftTeleport, ref reservedTapKeys);
        AddReservedKeys(inputRightTeleport, ref reservedTapKeys);
        AddReservedKeys(inputSlice, ref reservedTapKeys);

        reservedBlackKeys.Clear();
        AddReservedKeys(inputLeftTeleport, ref reservedBlackKeys);
        AddReservedKeys(inputRightTeleport, ref reservedBlackKeys);
    }

    void CheckReservedTapKeys(InputAction.CallbackContext context)
    {
        if (IsAnyKeyReserved(reservedTapKeys))
        {
            ExecuteInput(tapFolder);
        }
    }

    void OnAnyKeyStarted(InputAction.CallbackContext context)
    {
        if (!playMode)
        {
            return;
        }

        isAnyKeyHolding = true;

        isBlackKeyReserved = IsAnyKeyReserved(reservedBlackKeys);
        if (isBlackKeyReserved)
        {
            ExecuteInput(blackFolder);
        }
    }

    void OnAnyKeyCanceled(InputAction.CallbackContext context)
    {
        isAnyKeyHolding = false;
        isBlackKeyReserved = false;
    }

    private bool IsAnyKeyReserved(HashSet<Key> keySet)
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        foreach (var keyControl in keyboard.allKeys)
        {
            if (keyControl == null)
            {
                continue;
            }

            if (!keyControl.wasPressedThisFrame)
            {
                continue;
            }

            if (keySet.Contains(keyControl.keyCode))
            {
                return false;
            }
        }

        return true;
    }

    void ExecuteInput(GameObject folder)
    {
        if (!playMode)
        {
            return;
        }

        MusicNote lowestNote = null;

        foreach (Transform child in folder.transform)
        {
            if (!child)
            {
                continue;
            }

            MusicNote note = child.gameObject.GetComponent<MusicNote>();
            if (!note)
            {
                continue;
            }

            if (!lowestNote)
            {
                lowestNote = note;
                continue;
            }

            if (note.timing < lowestNote.timing)
            {
                lowestNote = note;
            }
        }

        if (!lowestNote)
        {
            return;
        }

        lowestNote.ExecuteNote();
    }

    public void SwitchToUsedFolder(Transform usedNote)
    {
        usedNote.SetParent(usedNotesFolder.transform);
    }

    public void SelectNoteType(int index)
    {
        if (index == 0)
        {
            isNoteTypeSelected = false;
        }
        else
        {
            isNoteTypeSelected = true;
            if (index == 1) selectedNoteType = NoteTypeGeneral.TAP_NOTE;
            else if (index == 2) selectedNoteType = NoteTypeGeneral.BLACK_NOTE;
            else if (index == 3) selectedNoteType = NoteTypeGeneral.LEFT_TELEPORT;
            else if (index == 4) selectedNoteType = NoteTypeGeneral.RIGHT_TELEPORT;
            else if (index == 5) selectedNoteType = NoteTypeGeneral.SLICE_NOTE;
            else if (index == 6) selectedNoteType = NoteTypeGeneral.SPIKE;
        }
    }

    public void ExecuteDeleteInFolder(Transform folder)
    {
        bool isWithinArea;
        foreach (Transform noteTransform in folder.transform)
        {
            MusicNote note = noteTransform.gameObject.GetComponent<MusicNote>();

            if (!note)
            {
                continue;
            }

            isWithinArea = note.IsWithinArea(worldPosition);

            if (isWithinArea)
            {
                CommandDeleteOneNote commandDeleteOneNote = new CommandDeleteOneNote(note.gameObject, note.timing, noteTransform.position.x);
                OverrideCommand(commandDeleteOneNote);

                noteTransform.SetParent(undoRedoFolder.transform, true);
                note.gameObject.SetActive(false);

                Debug.Log(undoCommandStack.Count);
            }
        }
    }

    void ConvertFromMouseToWorld()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = mainCamera.nearClipPlane;
        worldPosition = mainCamera.ScreenToWorldPoint(mousePos);
    }

    void InsertNote(Vector3 targetPosition)
    {
        if (chartSpeed == 0f)
        {
            Debug.Log("You can only run the chart with speed being 0.");
            return;
        }

        if (!isNoteTypeSelected)
        {
            return;
        }

        LanePosition confirmedLane = CheckCorrectLane(targetPosition);
        GameObject confirmedNote = null;

        if (confirmedLane == LanePosition.NONE)
        {
            return;
        }

        switch (selectedNoteType)
        {
            case NoteTypeGeneral.TAP_NOTE:
            {
                GameObject hoveredVerticalGrid = GetHoveredVerticalGrid();
                if (!hoveredVerticalGrid)
                {
                    return;
                }

                confirmedNote = Instantiate(tapNotePrefab, new Vector3(hoveredVerticalGrid.transform.position.x, targetPosition.y, 0), Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(tapFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.BLACK_NOTE:
            {
                GameObject hoveredVerticalGrid = GetHoveredVerticalGrid();
                if (!hoveredVerticalGrid)
                {
                    return;
                }

                confirmedNote = Instantiate(blackNotePrefab, new Vector3(hoveredVerticalGrid.transform.position.x, targetPosition.y, 0), Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(blackFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.LEFT_TELEPORT:
            {
                Vector3 confirmedPosition = Vector3.zero;

                if (confirmedLane == LanePosition.LEFT_POS)
                    confirmedPosition = new Vector3(ValueStorer.leftLanePosition.x, targetPosition.y, 0);
                else if (confirmedLane == LanePosition.MIDDLE_POS)
                    confirmedPosition = new Vector3(ValueStorer.middleLanePosition.x, targetPosition.y, 0);
                else if (confirmedLane == LanePosition.RIGHT_POS)
                    confirmedPosition = new Vector3(ValueStorer.rightLanePosition.x, targetPosition.y, 0);

                confirmedNote = Instantiate(leftTeleportPrefab, confirmedPosition, Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(leftTeleportFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.RIGHT_TELEPORT:
            {
                Vector3 confirmedPosition = Vector3.zero;

                if (confirmedLane == LanePosition.LEFT_POS)
                    confirmedPosition = new Vector3(ValueStorer.leftLanePosition.x, targetPosition.y, 0);
                else if (confirmedLane == LanePosition.MIDDLE_POS)
                    confirmedPosition = new Vector3(ValueStorer.middleLanePosition.x, targetPosition.y, 0);
                else if (confirmedLane == LanePosition.RIGHT_POS)
                    confirmedPosition = new Vector3(ValueStorer.rightLanePosition.x, targetPosition.y, 0);

                confirmedNote = Instantiate(rightTeleportPrefab, confirmedPosition, Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(rightTeleportFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.SLICE_NOTE:
            {
                confirmedNote = Instantiate(sliceNotePrefab, new Vector3(0, targetPosition.y, 0), Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(sliceFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.SPIKE:
            {
                if (targetPosition.x >= ValueStorer.minMiddleLaneX && targetPosition.x <= ValueStorer.maxMiddleLaneX)
                {
                    confirmedNote = Instantiate(middleSpikePrefab, new Vector3(0, targetPosition.y, 0), Quaternion.identity) as GameObject;
                }
                else if ((targetPosition.x >= ValueStorer.minLeftLaneX && targetPosition.x <= ValueStorer.maxLeftLaneX)
                        || (targetPosition.x >= ValueStorer.minRightLaneX && targetPosition.x <= ValueStorer.maxRightLaneX))
                {
                    confirmedNote = Instantiate(sideSpikePrefab, new Vector3(0, targetPosition.y, 0), Quaternion.identity) as GameObject;
                }
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(spikeFolder.transform, true);
                break;
            }
            default: break;
        }

        CommandAddOneNote commandAddOneNote = new CommandAddOneNote(confirmedNote, confirmedNote.GetComponent<MusicNote>().timing, confirmedNote.transform.position.x);
        OverrideCommand(commandAddOneNote);
    }

    void MoveNote()
    {
        if (chartSpeed == 0f)
        {
            Debug.Log("You can only run the chart with speed being 0.");
            return;
        }

        float currentXPos = draggedNote.gameObject.transform.position.x;

        float xPos = worldPosition.x;
        float yPos = worldPosition.y >= 0 ? worldPosition.y : 0;

        if (draggedNote.GetNoteType() == MusicNote.NoteType.MIDDLE_SPIKE
            || draggedNote.GetNoteType() == MusicNote.NoteType.SIDE_SPIKE
            || draggedNote.GetNoteType() == MusicNote.NoteType.SLICE_NOTE)
        {
            xPos = 0;
        }
        else if (draggedNote.GetNoteType() == MusicNote.NoteType.LEFT_TELEPORT
            || draggedNote.GetNoteType() == MusicNote.NoteType.RIGHT_TELEPORT)
        {
            if (xPos >= ValueStorer.minLeftLaneX && xPos <= ValueStorer.maxLeftLaneX) xPos = ValueStorer.leftLanePosition.x;
            else if (xPos >= ValueStorer.minMiddleLaneX && xPos <= ValueStorer.maxMiddleLaneX) xPos = ValueStorer.middleLanePosition.x;
            else if (xPos >= ValueStorer.minRightLaneX && xPos <= ValueStorer.maxRightLaneX) xPos = ValueStorer.rightLanePosition.x;
            else if (xPos < ValueStorer.minLeftLaneX) xPos = ValueStorer.leftLanePosition.x;
            else if (xPos > ValueStorer.maxLeftLaneX && xPos < ValueStorer.minMiddleLaneX)
            {
                if (currentXPos >= ValueStorer.minLeftLaneX && currentXPos <= ValueStorer.maxLeftLaneX) xPos = ValueStorer.leftLanePosition.x;
                else if (currentXPos >= ValueStorer.minMiddleLaneX && currentXPos <= ValueStorer.maxMiddleLaneX) xPos = ValueStorer.middleLanePosition.x;
            }
            else if (xPos > ValueStorer.maxMiddleLaneX && xPos < ValueStorer.minRightLaneX)
            {
                if (currentXPos >= ValueStorer.minMiddleLaneX && currentXPos <= ValueStorer.maxMiddleLaneX) xPos = ValueStorer.middleLanePosition.x;
                else if (currentXPos >= ValueStorer.minRightLaneX && currentXPos <= ValueStorer.maxRightLaneX) xPos = ValueStorer.rightLanePosition.x;
            }
            else if (xPos > ValueStorer.maxRightLaneX) xPos = ValueStorer.rightLanePosition.x;
        }
        else if (draggedNote.GetNoteType() == MusicNote.NoteType.TAP_NOTE
            || draggedNote.GetNoteType() == MusicNote.NoteType.BLACK_NOTE)
        {
            if (xPos < ValueStorer.minLeftLaneX)
            {
                xPos = ValueStorer.minLeftLaneX;
            }
            else if (xPos > ValueStorer.maxRightLaneX)
            {
                xPos = ValueStorer.maxRightLaneX;
            }
            else
            {
                xPos = verticalGridValue;
            }
        }

        draggedNote.gameObject.transform.position = new Vector3(xPos, yPos, 0);
        draggedNote.timing = draggedNote.gameObject.transform.localPosition.y / chartSpeed;
    }

    List<Transform> FindAppropriateNotesInFolder(Vector3 targetPosition, Transform folder)
    {
        List<Transform> foundNotes = new List<Transform>();

        foreach (Transform note in folder)
        {
            MusicNote musicNote = note.gameObject.GetComponent<MusicNote>();

            if (!musicNote) continue;

            bool isWithinArea = musicNote.IsWithinArea(targetPosition);
            if (isWithinArea)
            {
                foundNotes.Add(note);
            }
        }

        return foundNotes;
    }

    MusicNote FindSmallestDistanceNote(Vector3 targetPosition)
    {
        List<Transform> foundNotes = new List<Transform>();

        List<Transform> foundTapNotes = FindAppropriateNotesInFolder(targetPosition, tapFolder.transform);
        List<Transform> foundBlackNotes = FindAppropriateNotesInFolder(targetPosition, blackFolder.transform);
        List<Transform> foundLeftTeleports = FindAppropriateNotesInFolder(targetPosition, leftTeleportFolder.transform);
        List<Transform> foundRightTeleports = FindAppropriateNotesInFolder(targetPosition, rightTeleportFolder.transform);
        List<Transform> foundSliceNotes = FindAppropriateNotesInFolder(targetPosition, sliceFolder.transform);
        List<Transform> foundSpikes = FindAppropriateNotesInFolder(targetPosition, spikeFolder.transform);

        foreach (Transform note in foundTapNotes) foundNotes.Add(note);
        foreach (Transform note in foundBlackNotes) foundNotes.Add(note);
        foreach (Transform note in foundLeftTeleports) foundNotes.Add(note);
        foreach (Transform note in foundRightTeleports) foundNotes.Add(note);
        foreach (Transform note in foundSliceNotes) foundNotes.Add(note);
        foreach (Transform note in foundSpikes) foundNotes.Add(note);

        MusicNote inConfirmedNote = null;
        float distanceFromMouseToNote = 0f;
        foreach (Transform note in foundNotes)
        {
            MusicNote musicNote = note.gameObject.GetComponent<MusicNote>();

            if (!musicNote)
            {
                continue;
            }

            if (!inConfirmedNote)
            {
                inConfirmedNote = musicNote;
                distanceFromMouseToNote = musicNote.DistanceFromPosition(worldPosition);
                continue;
            }

            if (musicNote.DistanceFromPosition(worldPosition) >= distanceFromMouseToNote)
            {
                continue;
            }

            inConfirmedNote = musicNote;
        }

        return inConfirmedNote;
    }

    LanePosition CheckCorrectLane(Vector3 targetPosition)
    {
        if (targetPosition.x >= ValueStorer.minLeftLaneX - ValueStorer.gridOffset &&
            targetPosition.x <= ValueStorer.maxLeftLaneX + ValueStorer.gridOffset)
        {
            return LanePosition.LEFT_POS;
        }
        else if (targetPosition.x >= ValueStorer.minMiddleLaneX - ValueStorer.gridOffset &&
            targetPosition.x <= ValueStorer.maxMiddleLaneX + ValueStorer.gridOffset)
        {
            return LanePosition.MIDDLE_POS;
        }
        else if (targetPosition.x >= ValueStorer.minRightLaneX - ValueStorer.gridOffset &&
            targetPosition.x <= ValueStorer.maxRightLaneX + ValueStorer.gridOffset)
        {
            return LanePosition.RIGHT_POS;
        }

        return LanePosition.NONE;
    }

    public void ConfirmEditOption(int index)
    {
        editOption = (EditOption)index;

        if (editOption == EditOption.NOTE_ADD)
        {
            noteSelectDropDown.gameObject.SetActive(true);
        }
        else if (editOption == EditOption.NOTE_DRAG)
        {
            noteSelectDropDown.gameObject.SetActive(false);
        }
    }

    public void SetPlayMode(bool isPlayMode)
    {
        playMode = isPlayMode;

        if (playMode == false)
        {
            audioSource.Stop();

            scrollPlayfield.transform.position = Vector3.zero;
            ReenableNotes();

            RejectTimingSpeed();
            speedMulti = 1;
        }
        else
        {
            ApplyTimingSpeed();
            audioSource.Play();
        }
    }

    void ReenableNotes()
    {
        var usedNotes = usedNotesFolder.transform;

        for (int i = usedNotes.childCount - 1; i >= 0; i--)
        {
            Transform note = usedNotes.GetChild(i);
            if (!note)
            {
                continue;
            }

            MusicNote musicNote = note.gameObject.GetComponent<MusicNote>();
            if (!musicNote)
            {
                continue;
            }

            switch (musicNote.GetNoteType())
            {
                case MusicNote.NoteType.TAP_NOTE: note.SetParent(tapFolder.transform, false); break;
                case MusicNote.NoteType.BLACK_NOTE: note.SetParent(blackFolder.transform, false); break;
                case MusicNote.NoteType.LEFT_TELEPORT: note.SetParent(leftTeleportFolder.transform, false); break;
                case MusicNote.NoteType.RIGHT_TELEPORT: note.SetParent(rightTeleportFolder.transform, false); break;
                case MusicNote.NoteType.SLICE_NOTE: note.SetParent(sliceFolder.transform, false); break;
                case MusicNote.NoteType.MIDDLE_SPIKE: note.SetParent(spikeFolder.transform, false); break;
                case MusicNote.NoteType.SIDE_SPIKE: note.SetParent(spikeFolder.transform, false); break;
                default: continue;
            }

            note.gameObject.SetActive(true);
        }
    }

    void ReenableOneNote(GameObject note)
    {
        MusicNote musicNote = note.GetComponent<MusicNote>();
        Transform noteTransform = musicNote.transform;

        switch (musicNote.GetNoteType())
        {
            case MusicNote.NoteType.TAP_NOTE: noteTransform.SetParent(tapFolder.transform, false); break;
            case MusicNote.NoteType.BLACK_NOTE: noteTransform.SetParent(blackFolder.transform, false); break;
            case MusicNote.NoteType.LEFT_TELEPORT: noteTransform.SetParent(leftTeleportFolder.transform, false); break;
            case MusicNote.NoteType.RIGHT_TELEPORT: noteTransform.SetParent(rightTeleportFolder.transform, false); break;
            case MusicNote.NoteType.SLICE_NOTE: noteTransform.SetParent(sliceFolder.transform, false); break;
            case MusicNote.NoteType.MIDDLE_SPIKE: noteTransform.SetParent(spikeFolder.transform, false); break;
            case MusicNote.NoteType.SIDE_SPIKE: noteTransform.SetParent(spikeFolder.transform, false); break;
            default: break;
        }

        musicNote.gameObject.SetActive(true);
    }

    void ChangePositionsThroughSpeed(Transform folder)
    {
        foreach (Transform note in folder.transform)
        {
            MusicNote musicNote = note.gameObject.GetComponent<MusicNote>();
            if (!musicNote)
            {
                continue;
            }

            note.position = new Vector3(note.position.x, musicNote.timing * chartSpeed, 0);
        }
    }

    void ChangePositionsThroughOffset(Transform folder, float originalOffset)
    {
        foreach (Transform note in folder.transform)
        {
            MusicNote musicNote = note.gameObject.GetComponent<MusicNote>();
            if (!musicNote)
            {
                continue;
            }

            musicNote.timing = musicNote.timing - originalOffset + chartOffset;
            note.position = new Vector3(note.position.x, musicNote.timing * chartSpeed, 0);
        }
    }

    GameObject GetHoveredVerticalGrid()
    {
        GameObject[] grids = GameObject.FindGameObjectsWithTag(ValueStorer.tagVerticalGrid);
        foreach (GameObject grid in grids)
        {
            VerticalGrid verticalGrid = grid.GetComponent<VerticalGrid>();
            if (verticalGrid.isHovered)
            {
                return grid;
            }
        }
        return null;
    }

    public void ApplyTimingBPM()
    {
        foreach (Transform beatLineT in beatLinesFolder.transform)
        {
            if (beatLineT)
            {
                Destroy(beatLineT.gameObject);
            }
        }
        
        if (!audioSource.clip || chartSpeed == 0)
        {
            return;
        }

        float tempTiming = 0;
        List<BPMStorer> timingItems = uiManager.GetTimingItems();

        if (timingItems.Exists(item => item.BPM == 0))
        {
            return;
        }

        for (int i = 0; i < timingItems.Count; i++)
        {
            tempTiming = timingItems[i].timing;
            int mainBeatCount = 0;
            int stepBeatCount = 0;

            while (i + 1 != timingItems.Count && tempTiming + ((float)mainBeatCount + (float)stepBeatCount / (float)beatDensity) * 60f / timingItems[i].BPM * 1000f < timingItems[i + 1].timing)
            {
                float totalTiming = tempTiming + ((float)mainBeatCount + (float)stepBeatCount / (float)beatDensity) * 60f / timingItems[i].BPM * 1000f;
                GameObject beat = null;
                if (stepBeatCount != 0)
                {
                    beat = Instantiate(stepBeatPrefab, beatLinesFolder.transform, false);
                }
                else
                {
                    beat = Instantiate(mainBeatPrefab, beatLinesFolder.transform, false);
                }
                beat.transform.localPosition = new Vector3(0, chartSpeed * (totalTiming / 1000f), 0);
                stepBeatCount++;
                if (stepBeatCount == beatDensity)
                {
                    stepBeatCount = 0;
                    mainBeatCount++;
                }
            }

            mainBeatCount = 0;
            stepBeatCount = 0;

            while (i + 1 == timingItems.Count && tempTiming + ((float)mainBeatCount + (float)stepBeatCount / (float)beatDensity) * 60f / timingItems[i].BPM * 1000f < audioSource.clip.length * 1000)
            {
                float totalTiming = tempTiming + ((float)mainBeatCount + (float)stepBeatCount / (float)beatDensity) * 60f / timingItems[i].BPM * 1000f;
                GameObject beat = null;
                if (stepBeatCount != 0)
                {
                    beat = Instantiate(stepBeatPrefab, beatLinesFolder.transform, false);
                }
                else
                {
                    beat = Instantiate(mainBeatPrefab, beatLinesFolder.transform, false);
                }
                beat.transform.localPosition = new Vector3(0, chartSpeed * (totalTiming / 1000f), 0);
                stepBeatCount++;
                if (stepBeatCount == beatDensity)
                {
                    stepBeatCount = 0;
                    mainBeatCount++;
                }
            }

            mainBeatCount = 0;
            stepBeatCount = 0;
        }
    }

    public void ApplyTimingSpeed()
    {
        beatLinesFolder.SetActive(false);

        speedItems = uiManager.GetSpeedItems();

        if (speedItems.Count <= 0)
        {
            return;
        }

        List<MusicNote> notes = null;

        float tempSpeedMulti = 1f;
        float totalLength = 0f;

        for (int i = 0; i < speedItems.Count; i++)
        {
            if (i == 0)
            {
                tempSpeedMulti = 1f;
                totalLength += (speedItems[i].timing * tempSpeedMulti / 1000f);
            }
            else
            {
                tempSpeedMulti = speedItems[i - 1].speedMulti;
                notes = FindAllNotesWithTiming(speedItems[i - 1].timing, speedItems[i].timing);
                foreach (MusicNote note in notes)
                {
                    note.ChangeSpeedPosition(totalLength, chartSpeed, speedItems[i - 1].timing, tempSpeedMulti);
                }

                totalLength += (speedItems[i].timing - speedItems[i - 1].timing) / 1000f * tempSpeedMulti;
            }
            Debug.Log(totalLength);
        }
        tempSpeedMulti = speedItems[speedItems.Count - 1].speedMulti;
        notes = FindAllNotesWithTiming(speedItems[speedItems.Count - 1].timing, audioSource.clip.length * 1000f);
        foreach (MusicNote note in notes)
        {
            note.ChangeSpeedPosition(totalLength, chartSpeed, speedItems[speedItems.Count - 1].timing, tempSpeedMulti);
        }

        totalLength += (audioSource.clip.length - speedItems[speedItems.Count - 1].timing / 1000f) * tempSpeedMulti;
        Debug.Log(totalLength);
    }

    public void RejectTimingSpeed()
    {
        speedItems = null;
        beatLinesFolder.SetActive(true);
        scrollPlayfield.transform.position = Vector3.zero;

        List<MusicNote> notes = FindAllNotesWithTiming(0, audioSource.clip.length * 1000f);
        foreach (MusicNote note in notes)
        {
            note.ResetSpeedPosition(chartSpeed);
        }
    }

    public void ChangeSpeedThroughTiming(float timing)
    {
        if (speedItems.Count <= 0)
        {
            speedMulti = 1f;
            return;
        }

        float totalLength = 0f;
        for (int i = 0; i < speedItems.Count; i++)
        {
            if (timing < speedItems[i].timing)
            {
                if (i == 0)
                {
                    speedMulti = 1f;
                    totalLength += (timing * speedMulti / 1000f);
                }
                else
                {
                    speedMulti = speedItems[i - 1].speedMulti;
                    totalLength += (timing - speedItems[i - 1].timing) / 1000f * speedMulti;
                }
                scrollPlayfield.transform.position = new Vector3(0, -totalLength * chartSpeed, 0);
                return;
            }

            if (i == 0)
            {
                speedMulti = 1f;
                totalLength += (speedItems[i].timing * speedMulti / 1000f);
            }
            else
            {
                speedMulti = speedItems[i - 1].speedMulti;
                totalLength += (speedItems[i].timing - speedItems[i - 1].timing) / 1000f * speedMulti;
            }
        }
        speedMulti = speedItems[speedItems.Count - 1].speedMulti;
        totalLength += (timing - speedItems[speedItems.Count - 1].timing) / 1000f * speedMulti;
        scrollPlayfield.transform.position = new Vector3(0, -totalLength * chartSpeed, 0);
    }

    List<MusicNote> FindAllNotesWithTiming(float beginTiming, float endTiming)
    {
        List<MusicNote> foundTaps = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, tapFolder.transform);
        List<MusicNote> foundBlacks = FindAllNotesWithTimingFromFolder(beginTiming, endTiming,  blackFolder.transform);
        List<MusicNote> foundSlice = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, sliceFolder.transform);
        List<MusicNote> foundSpikes = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, spikeFolder.transform);
        List<MusicNote> foundLeftTeleports = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, leftTeleportFolder.transform);
        List<MusicNote> foundRightTeleport = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, rightTeleportFolder.transform);

        List<MusicNote> foundAllNotes = new List<MusicNote>();
        foundAllNotes.AddRange(foundTaps);
        foundAllNotes.AddRange(foundBlacks);
        foundAllNotes.AddRange(foundSlice);
        foundAllNotes.AddRange(foundSpikes);
        foundAllNotes.AddRange(foundLeftTeleports);
        foundAllNotes.AddRange(foundRightTeleport);

        return foundAllNotes;
    }

    List<MusicNote> FindAllNotesWithTimingFromFolder(float beginTiming, float endTiming, Transform folder)
    {
        List<MusicNote> foundNotes = new List<MusicNote>();
        foreach (Transform note in folder)
        {
            MusicNote musicNote = note.GetComponent<MusicNote>();
            if (musicNote != null && musicNote.timing * 1000f >= beginTiming && musicNote.timing * 1000f < endTiming)
            {
                foundNotes.Add(musicNote);
            }
        }
        return foundNotes;
    }

    #region Undo & Redo Stack

    public void UndoCommand()
    {
        if (undoCommandStack.Count == 0)
        {
            return;
        }

        EditorCommand poppedCommand = undoCommandStack.Pop();
        poppedCommand.UndoCommand();
        redoCommandStack.Push(poppedCommand);
    }

    public void RedoCommand()
    {
        if (redoCommandStack.Count == 0)
        {
            return;
        }

        EditorCommand poppedCommand = redoCommandStack.Pop();
        poppedCommand.RedoCommand();
        undoCommandStack.Push(poppedCommand);
    }

    void OverrideCommand(EditorCommand command)
    {
        undoCommandStack.Push(command);
        foreach (var cmd in redoCommandStack)
        {
            cmd.FreeCommand();
        }
        redoCommandStack.Clear();
    }

    #endregion

    #region Undo

    public void UndoAddOneNote(GameObject noteObject)
    {
        noteObject.SetActive(false);
        noteObject.transform.SetParent(undoRedoFolder.transform, true);
    }

    public void UndoMoveOneNote(GameObject noteObject, float originalTiming, float noteOriginalPosition)
    {
        noteObject.transform.position = new Vector3(noteOriginalPosition, originalTiming * chartSpeed, 0);
    }

    public void UndoDeleteOneNote(GameObject noteObject, float timing, float notePosition)
    {
        ReenableOneNote(noteObject);
        noteObject.transform.position = new Vector3(notePosition, timing * chartSpeed, 0);
    }

    public void UndoChangeOffset(float offset)
    {
        float originalOffset = chartOffset;
        chartOffset = offset;

        offsetInputField.text = chartOffset.ToString();

        ReloadChartOffsetVisuals(originalOffset);
    }

    public void UndoChangeSpeed(float speed)
    {
        chartSpeed = speed;

        speedInputField.text = chartSpeed.ToString();

        ReloadChartSpeedVisuals();
    }

    #endregion

    #region Redo

    public void RedoAddOneNote(GameObject noteObject, float timing, float notePosition)
    {
        ReenableOneNote(noteObject);
        noteObject.transform.position = new Vector3(notePosition, timing * chartSpeed, 0);
    }

    public void RedoMoveOneNote(GameObject noteObject, float newTiming, float noteNewPosition)
    {
        noteObject.transform.position = new Vector3(noteNewPosition, newTiming * chartSpeed, 0);
    }

    public void RedoDeleteOneNote(GameObject noteObject)
    {
        noteObject.SetActive(false);
        noteObject.transform.SetParent(undoRedoFolder.transform, true);
    }

    public void RedoChangeOffset(float offset)
    {
        float originalOffset = chartOffset;
        chartOffset = offset;

        offsetInputField.text = chartOffset.ToString();

        ReloadChartOffsetVisuals(originalOffset);
    }

    public void RedoChangeSpeed(float speed)
    {
        chartSpeed = speed;

        speedInputField.text = chartSpeed.ToString();

        ReloadChartSpeedVisuals();
    }

    #endregion

    #region Chart Properties

    void ReloadChartOffsetVisuals(float originalOffset)
    {
        chartOffsetText.text = ValueStorer.chartOffsetText + chartOffset.ToString();

        ChangePositionsThroughOffset(tapFolder.transform, originalOffset);
        ChangePositionsThroughOffset(blackFolder.transform, originalOffset);
        ChangePositionsThroughOffset(sliceFolder.transform, originalOffset);
        ChangePositionsThroughOffset(leftTeleportFolder.transform, originalOffset);
        ChangePositionsThroughOffset(rightTeleportFolder.transform, originalOffset);
        ChangePositionsThroughOffset(spikeFolder.transform, originalOffset);
        ChangePositionsThroughOffset(usedNotesFolder.transform, originalOffset);
        ChangePositionsThroughOffset(undoRedoFolder.transform, originalOffset);
    }

    void ReloadChartSpeedVisuals()
    {
        chartSpeedText.text = ValueStorer.chartSpeedText + chartSpeed.ToString();

        ChangePositionsThroughSpeed(tapFolder.transform);
        ChangePositionsThroughSpeed(blackFolder.transform);
        ChangePositionsThroughSpeed(sliceFolder.transform);
        ChangePositionsThroughSpeed(leftTeleportFolder.transform);
        ChangePositionsThroughSpeed(rightTeleportFolder.transform);
        ChangePositionsThroughSpeed(spikeFolder.transform);
        ChangePositionsThroughSpeed(usedNotesFolder.transform);
        ChangePositionsThroughSpeed(undoRedoFolder.transform);
    }

    public void ChangeChartOffset()
    {
        float originalOffset = chartOffset;

        bool isParsed = float.TryParse(offsetInputField.text, out chartOffset);
        if (!isParsed)
        {
            chartOffset = 0;
        }

        ReloadChartOffsetVisuals(originalOffset);

        CommandChangeOffset commandChangeOffset = new CommandChangeOffset(originalOffset, chartOffset);
        OverrideCommand(commandChangeOffset);
    }

    public void ChangeChartSpeed()
    {
        float originalSpeed = chartSpeed;

        bool isParsed = float.TryParse(speedInputField.text, out chartSpeed);
        if (!isParsed)
        {
            chartSpeed = 1;
        }

        ReloadChartSpeedVisuals();

        CommandChangeSpeed commandChangeSpeed = new CommandChangeSpeed(originalSpeed, chartSpeed);
        OverrideCommand(commandChangeSpeed);
    }

    #endregion
}
