using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.WSA;

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
        NOTE_ADD_BETWEEN,
    }

    //Chart shenanigans
    private Camera mainCamera;
    public Vector3 worldPosition;
    [Space(10.0f)]
    public float horizontalGridValue;
    public HorizontalGrid confirmedHorizontalGrid = null;
    public bool isTimingGridConfirm = false;
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
    private HashSet<Key> pressedKeys = new HashSet<Key>();

    //Note selection
    [SerializeField] bool isNoteTypeSelected;
    [SerializeField] NoteTypeGeneral selectedNoteType;
    public MusicNote draggedNote;

    [Space(10.0f)]
    [SerializeField] GameObject stepBeatPrefab;
    [SerializeField] GameObject mainBeatPrefab;
    [SerializeField] GameObject beatLinesFolder;

    [Header("Editor Settings")]
    public bool playMode = false;
    public float chartOffset = 0f;
    public float chartSpeed = 1f;
    public float speedMulti = 1f;
    public int beatDensity = 1;
    private float editorCurrentTiming = 0f;

    [Header("Input Actions")]
    [SerializeField] InputActionReference inputAnyKey;
    [SerializeField] InputActionReference inputLeftTeleport;
    [SerializeField] InputActionReference inputRightTeleport;
    [SerializeField] InputActionReference inputSlice;
    [Space(10.0f)]
    [SerializeField] InputActionReference editorInputScroll;

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
    [SerializeField] TestPlayer player;
    public int timingGroupIndex = 0;
    [SerializeField] GameObject timingGroupStorer;
    [SerializeField] TimingGroup timingGroupPrefab;
    [SerializeField] List<TimingGroup> timingGroups;

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

    [Header("Multiple Notes Adding")]
    [SerializeField] GameObject multipleNotesSignal;
    private GameObject addedSignal;
    private bool isMultipleNotesSignalAdded = false;
    private Vector3 firstAddingPosition;
    private Vector3 lastAddingPosition;
    private int noteAmount = 1;

    [Header("Select Note Area")]
    private float openNoteTempTiming;
    private float closeNoteTempTiming;
    private float areaTempTiming;
    [SerializeField] GameObject openNoteArea;
    [SerializeField] GameObject closeNoteArea;
    [SerializeField] GameObject noteArea;
    private List<Transform> foundNotesInArea;
    private bool isNoteAreaAdded = false;
    private Vector3 fixedAreaPosition;
    [SerializeField] Vector3 lockedAreaPosition;
    private Transform lowestNoteArea;

    [Header("UI")]
    [SerializeField] UIManager uiManager;
    [SerializeField] GameObject noteAmountUI;
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
    [SerializeField] TMP_InputField noteAmountInputField;

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

        timingGroups = new List<TimingGroup>();

        openNoteArea.SetActive(false);
        closeNoteArea.SetActive(false);
        noteArea.SetActive(false);

        foundNotesInArea = new List<Transform>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitiateUI();
        RebuildReservedKeys();

        UpdateGroupIndicators();
    }

    // Update is called once per frame
    void Update()
    {
        currentTimingText.text = ValueStorer.currentTimingText + ((int)(editorCurrentTiming * 1000)).ToString();

        ConvertFromMouseToWorld();

        if (playMode)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                foreach (var keyControl in keyboard.allKeys)
                {
                    Key finalKey;
                    if (!TryKeyControlToNewKey(keyControl, out finalKey))
                    {
                        continue;
                    }

                    if (keyControl.wasPressedThisFrame)
                    {
                        pressedKeys.Add(finalKey);
                        if (!reservedTapKeys.Contains(finalKey))
                        {
                            ExecuteInputAllTimingGroups(NoteTypeGeneral.TAP_NOTE);
                        }
                    }
                    else if (keyControl.wasReleasedThisFrame)
                    {
                        pressedKeys.Remove(finalKey);
                    }
                }
            }
        }

        if (!playMode && Keyboard.current != null)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.deleteKey.wasPressedThisFrame && IsNoteAreaFullyLocked())
                {
                    InsertNotesInArea();
                    ExecuteDeleteSelectedNotes(foundNotesInArea);
                }
            }
        }

        if (!playMode
            && editOption == EditOption.NOTE_ADD
            && isNoteTypeSelected
            && Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame
            && EventSystem.current != null 
            && !EventSystem.current.IsPointerOverGameObject())
        {
            if (worldPosition.y >= 0f)
            {
                InsertNote(worldPosition);
            }
        }

        if (!playMode
            && editOption == EditOption.NOTE_DRAG
            && timingGroups.Count > 0
            && Mouse.current != null
            && EventSystem.current != null
            && !EventSystem.current.IsPointerOverGameObject())
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                bool isFullyLocked = IsNoteAreaFullyLocked() && IsWithinOpenCloseNoteArea();

                if (!isFullyLocked)
                {
                    ResetNoteSelectArea(ref isNoteAreaAdded);

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
                else
                {
                    if (foundNotesInArea.Count <= 0
                        || worldPosition.x < ValueStorer.minLeftLaneX
                        || worldPosition.x > ValueStorer.maxRightLaneX)
                    {
                        InsertNotesInArea();
                    }

                    lowestNoteArea = GetLowestNote();
                    fixedAreaPosition = (openNoteArea.transform.position.y < closeNoteArea.transform.position.y ?
                        openNoteArea.transform.position : closeNoteArea.transform.position);
                    lockedAreaPosition = worldPosition;
                }
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && draggedNote)
            {
                /*
                CommandMoveOneNote commandMoveOneNote = new CommandMoveOneNote(
                    draggedNote.gameObject, 
                    noteOriginalTiming, 
                    draggedNote.timing, 
                    noteOriginalPosition.x, 
                    draggedNote.gameObject.transform.position.x);
                OverrideCommand(commandMoveOneNote);
                */

                draggedNote.temporaryTiming = draggedNote.timing;
                draggedNote = null;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame &&
                foundNotesInArea.All(e => e != null))
            {
                for (int i = 0; i < foundNotesInArea.Count; i++)
                {
                    if (foundNotesInArea[i] == null)
                    {
                        continue;
                    }

                    MusicNote note = foundNotesInArea[i].gameObject.GetComponent<MusicNote>();
                    if (note == null)
                    {
                        continue;
                    }

                    note.timing = foundNotesInArea[i].transform.localPosition.y / chartSpeed;
                    note.temporaryTiming = note.timing;
                }

                openNoteTempTiming = openNoteArea.transform.localPosition.y / chartSpeed;
                closeNoteTempTiming = closeNoteArea.transform.localPosition.y / chartSpeed;
                areaTempTiming = noteArea.transform.localPosition.y / chartSpeed;

                lowestNoteArea = null;
            }
            else if (Mouse.current.leftButton.isPressed && draggedNote)
            {
                MoveNote();
            }
            else if (Mouse.current.leftButton.isPressed && foundNotesInArea.All(e => e != null))
            {
                float alignValue = worldPosition.y - lockedAreaPosition.y;

                GameObject alignBeat = FindAlignArea(alignValue);

                if (alignBeat != null) alignValue = alignBeat.transform.localPosition.y - 
                        (openNoteTempTiming < closeNoteTempTiming ? openNoteTempTiming * chartSpeed : closeNoteTempTiming * chartSpeed);

                for (int i = 0; i < foundNotesInArea.Count; i++)
                {
                    if (foundNotesInArea[i] == null)
                    {
                        continue;
                    }

                    MusicNote note = foundNotesInArea[i].gameObject.GetComponent<MusicNote>();
                    if (note == null)
                    {
                        continue;
                    }

                    foundNotesInArea[i].localPosition = new Vector3(
                        foundNotesInArea[i].position.x,
                        note.temporaryTiming * chartSpeed + alignValue,
                        0);
                }

                openNoteArea.transform.localPosition = new Vector3(
                    0, openNoteTempTiming * chartSpeed + alignValue, 0);
                closeNoteArea.transform.localPosition = new Vector3(
                    0, closeNoteTempTiming * chartSpeed + alignValue, 0);
                noteArea.transform.localPosition = new Vector3(
                    0, areaTempTiming * chartSpeed + alignValue, 0);
            }
            else if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                ExecuteNoteAreaSignal(worldPosition, ref isNoteAreaAdded);
            }
        }

        if (!playMode
            && editOption == EditOption.NOTE_ADD_BETWEEN
            && Mouse.current != null
            && EventSystem.current != null
            && !EventSystem.current.IsPointerOverGameObject())
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                float posX = worldPosition.x;
                float posY = worldPosition.y;

                GameObject verticalGrid = GetHoveredVerticalGrid();
                if (verticalGrid)
                {
                    posX = verticalGrid.transform.position.x;

                    GameObject horizontalGrid = GetHoveredHorizontalGrid();
                    if (horizontalGrid)
                    {
                        posY = horizontalGrid.transform.position.y;
                    }

                    InsertMultipleNotesSignal(new Vector3(posX, posY, 0), ref isMultipleNotesSignalAdded);
                }
                else
                {
                    CancelMultipleNotesSignal();
                }
            }
        }

        if (!playMode && isNoteAreaAdded)
        {
            closeNoteArea.transform.position = new Vector3(0, worldPosition.y, 0);
            if (openNoteArea.transform.position.y < closeNoteArea.transform.position.y)
            {
                noteArea.transform.position = new Vector3(0, openNoteArea.transform.position.y, 0);
            }
            else
            {
                noteArea.transform.position = new Vector3(0, closeNoteArea.transform.position.y, 0);
            }
            noteArea.transform.localScale = new Vector3(
                1,
                Mathf.Abs(openNoteArea.transform.position.y - closeNoteArea.transform.position.y),
                1);
        }

        if (!playMode
            && Mouse.current != null 
            && Mouse.current.rightButton.isPressed
            && EventSystem.current != null
            && !EventSystem.current.IsPointerOverGameObject())
        {
            ExecuteDeleteInFolder(timingGroups[timingGroupIndex].tapFolder.transform);
            ExecuteDeleteInFolder(timingGroups[timingGroupIndex].blackFolder.transform);
            ExecuteDeleteInFolder(timingGroups[timingGroupIndex].sliceFolder.transform);
            ExecuteDeleteInFolder(timingGroups[timingGroupIndex].leftTeleportFolder.transform);
            ExecuteDeleteInFolder(timingGroups[timingGroupIndex].rightTeleportFolder.transform);
            ExecuteDeleteInFolder(timingGroups[timingGroupIndex].spikeFolder.transform);
        }

        if (playMode)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (pressedKeys.Count > 0)
                {
                    foreach (Key key in pressedKeys)
                    {
                        if (!reservedBlackKeys.Contains(key))
                        {
                            ExecuteInputAllTimingGroups(NoteTypeGeneral.BLACK_NOTE);
                            break;
                        }
                    }
                }
            }

            ChangeSpeedThroughTiming(audioSource.time * 1000f);
        }
        else
        {
            ExecuteScrolling();
        }
    }

    private void OnEnable()
    {
        inputLeftTeleport.action.performed  += _ => ExecuteInputAllTimingGroups(NoteTypeGeneral.LEFT_TELEPORT);
        inputRightTeleport.action.performed += _ => ExecuteInputAllTimingGroups(NoteTypeGeneral.RIGHT_TELEPORT);
        inputSlice.action.performed         += _ => ExecuteInputAllTimingGroups(NoteTypeGeneral.SLICE_NOTE);

        editorInputScroll.action.Enable();
    }

    private void OnDisable()
    {
        inputLeftTeleport.action.performed  -= _ => { };
        inputRightTeleport.action.performed -= _ => { };
        inputSlice.action.performed         -= _ => { };

        editorInputScroll.action.Disable();
    }

    bool TryKeyControlToNewKey(KeyControl keyControl, out Key finalKey)
    {
        finalKey = Key.None;

        if (keyControl == null)
        {
            return false;
        }

        finalKey = keyControl.keyCode;
        return finalKey != Key.None;
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
        ConfirmEditOption(0);

        offsetInputField.text = "0";
        speedInputField.text = "1";
        noteAmountInputField.text = "1";

        ChangeChartOffset();
        ChangeChartSpeed();
        ChangeNoteAmount();

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

    void ExecuteInputAllTimingGroups(NoteTypeGeneral noteType)
    {
        List<GameObject> notesInFolders = new List<GameObject>();

        for (int i = 0; i < timingGroups.Count; i++)
        {
            GameObject detectedFolder = null;

            if (noteType == NoteTypeGeneral.TAP_NOTE) detectedFolder = timingGroups[i].tapFolder;
            else if (noteType == NoteTypeGeneral.BLACK_NOTE) detectedFolder = timingGroups[i].blackFolder;
            else if (noteType == NoteTypeGeneral.SLICE_NOTE) detectedFolder = timingGroups[i].sliceFolder;
            else if (noteType == NoteTypeGeneral.LEFT_TELEPORT) detectedFolder = timingGroups[i].leftTeleportFolder;
            else if (noteType == NoteTypeGeneral.RIGHT_TELEPORT) detectedFolder = timingGroups[i].rightTeleportFolder;
            else if (noteType == NoteTypeGeneral.SPIKE) detectedFolder = timingGroups[i].spikeFolder;

            foreach (Transform detectedNote in detectedFolder.transform)
            {
                notesInFolders.Add(detectedNote.gameObject);
            }
        }

        ExecuteInput(notesInFolders);
    }

    void ExecuteInput(List<GameObject> notes)
    {
        if (!playMode)
        {
            return;
        }

        if (notes.Count == 0)
        {
            return;
        }

        MusicNote lowestNote = null;

        foreach (GameObject note in notes)
        {
            if (!note)
            {
                continue;
            }

            MusicNote musicNote = note.GetComponent<MusicNote>();
            if (!musicNote)
            {
                continue;
            }

            if (!lowestNote)
            {
                lowestNote = musicNote;
                continue;
            }

            if (musicNote.timing < lowestNote.timing)
            {
                lowestNote = musicNote;
            }
        }

        if (!lowestNote)
        {
            return;
        }

        lowestNote.ExecuteNote();
    }

    void ExecuteScrolling()
    {
        Vector2 value = editorInputScroll.action.ReadValue<Vector2>();
        editorCurrentTiming = editorCurrentTiming - value.y * ValueStorer.mouseScrollSpeed / chartSpeed;
        if (editorCurrentTiming < 0)
        {
            editorCurrentTiming = 0;
        }

        ChangeAllTimingGroupsScrolling();
    }

    void ChangeAllTimingGroupsScrolling()
    {
        timingGroupStorer.transform.position = new Vector3(0, -editorCurrentTiming * chartSpeed, 0);
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
                //CommandDeleteOneNote commandDeleteOneNote = new CommandDeleteOneNote(note.gameObject, note.timing, noteTransform.position.x);
                //OverrideCommand(commandDeleteOneNote);

                TimingGroup timingGroup = note.timingGroup;

                if (!timingGroup)
                {
                    return;
                }

                noteTransform.SetParent(timingGroup.undoRedoFolder.transform, false);
                note.gameObject.SetActive(false);
            }
        }
    }

    void ExecuteDeleteSelectedNotes(List<Transform> foundNotes)
    {
        foreach (Transform noteTransform in foundNotes)
        {
            MusicNote note = noteTransform.gameObject.GetComponent<MusicNote>();

            if (!note)
            {
                continue;
            }

            TimingGroup timingGroup = note.timingGroup;

            if (!timingGroup)
            {
                return;
            }

            noteTransform.SetParent(timingGroup.undoRedoFolder.transform, false);
            note.gameObject.SetActive(false);
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

        if (timingGroups.Count == 0)
        {
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

        float finalYPosition = targetPosition.y;
        GameObject hoveredHorizontalGrid = GetHoveredHorizontalGrid();
        if (hoveredHorizontalGrid)
        {
            finalYPosition = hoveredHorizontalGrid.transform.position.y;
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

                confirmedNote = Instantiate(tapNotePrefab, new Vector3(hoveredVerticalGrid.transform.position.x, finalYPosition, 0), Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].tapFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.BLACK_NOTE:
            {
                GameObject hoveredVerticalGrid = GetHoveredVerticalGrid();
                if (!hoveredVerticalGrid)
                {
                    return;
                }

                confirmedNote = Instantiate(blackNotePrefab, new Vector3(hoveredVerticalGrid.transform.position.x, finalYPosition, 0), Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].blackFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.LEFT_TELEPORT:
            {
                Vector3 confirmedPosition = Vector3.zero;

                if (confirmedLane == LanePosition.LEFT_POS)
                    confirmedPosition = new Vector3(ValueStorer.leftLanePosition.x, finalYPosition, 0);
                else if (confirmedLane == LanePosition.MIDDLE_POS)
                    confirmedPosition = new Vector3(ValueStorer.middleLanePosition.x, finalYPosition, 0);
                else if (confirmedLane == LanePosition.RIGHT_POS)
                    confirmedPosition = new Vector3(ValueStorer.rightLanePosition.x, finalYPosition, 0);

                confirmedNote = Instantiate(leftTeleportPrefab, confirmedPosition, Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].leftTeleportFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.RIGHT_TELEPORT:
            {
                Vector3 confirmedPosition = Vector3.zero;

                if (confirmedLane == LanePosition.LEFT_POS)
                    confirmedPosition = new Vector3(ValueStorer.leftLanePosition.x, finalYPosition, 0);
                else if (confirmedLane == LanePosition.MIDDLE_POS)
                    confirmedPosition = new Vector3(ValueStorer.middleLanePosition.x, finalYPosition, 0);
                else if (confirmedLane == LanePosition.RIGHT_POS)
                    confirmedPosition = new Vector3(ValueStorer.rightLanePosition.x, finalYPosition, 0);

                confirmedNote = Instantiate(rightTeleportPrefab, confirmedPosition, Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].rightTeleportFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.SLICE_NOTE:
            {
                confirmedNote = Instantiate(sliceNotePrefab, new Vector3(0, finalYPosition, 0), Quaternion.identity) as GameObject;
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].sliceFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.SPIKE:
            {
                if (targetPosition.x >= ValueStorer.minMiddleLaneX && targetPosition.x <= ValueStorer.maxMiddleLaneX)
                {
                    confirmedNote = Instantiate(middleSpikePrefab, new Vector3(0, finalYPosition, 0), Quaternion.identity) as GameObject;
                }
                else if ((targetPosition.x >= ValueStorer.minLeftLaneX && targetPosition.x <= ValueStorer.maxLeftLaneX)
                        || (targetPosition.x >= ValueStorer.minRightLaneX && targetPosition.x <= ValueStorer.maxRightLaneX))
                {
                    confirmedNote = Instantiate(sideSpikePrefab, new Vector3(0, finalYPosition, 0), Quaternion.identity) as GameObject;
                }
                confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].spikeFolder.transform, true);
                break;
            }
            default: break;
        }

        if (!confirmedNote)
        {
            return;
        }

        MusicNote musicNote = confirmedNote.GetComponent<MusicNote>();
        musicNote.timingGroup = timingGroups[timingGroupIndex];
        musicNote.temporaryTiming = musicNote.timing;

        //CommandAddOneNote commandAddOneNote = new CommandAddOneNote(confirmedNote, confirmedNote.GetComponent<MusicNote>().timing, confirmedNote.transform.position.x);
        //OverrideCommand(commandAddOneNote);
    }

    void InsertMultipleNotes(Vector3 startPosition, Vector3 endPosition, int numberOfNotes)
    {
        if (noteAmount == 0)
        {
            return;
        }

        float xStepOffset = (endPosition.x - startPosition.x) / (float)numberOfNotes;
        float yStepOffset = (endPosition.y - startPosition.y) / (float)numberOfNotes;

        LanePosition confirmedLane = CheckCorrectLane(startPosition);

        for (int i = 0; i <= numberOfNotes; i++)
        {
            if (chartSpeed == 0f)
            {
                Debug.Log("You can only run the chart with speed being 0.");
                return;
            }

            if (timingGroups.Count == 0)
            {
                return;
            }

            if (!isNoteTypeSelected)
            {
                return;
            }

            GameObject confirmedNote = null;
            Vector3 newPosition = new Vector3(startPosition.x + xStepOffset * i, startPosition.y + yStepOffset * i, 0);

            switch (selectedNoteType)
            {
                case NoteTypeGeneral.TAP_NOTE:
                    {
                        confirmedNote = Instantiate(tapNotePrefab, newPosition, Quaternion.identity) as GameObject;
                        confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                        confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].tapFolder.transform, true);
                        break;
                    }
                case NoteTypeGeneral.BLACK_NOTE:
                    {
                        confirmedNote = Instantiate(blackNotePrefab, newPosition, Quaternion.identity) as GameObject;
                        confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                        confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].blackFolder.transform, true);
                        break;
                    }
                case NoteTypeGeneral.LEFT_TELEPORT:
                    {
                        Vector3 confirmedPosition = Vector3.zero;

                        if (confirmedLane == LanePosition.LEFT_POS)
                            confirmedPosition = new Vector3(ValueStorer.leftLanePosition.x, newPosition.y, 0);
                        else if (confirmedLane == LanePosition.MIDDLE_POS)
                            confirmedPosition = new Vector3(ValueStorer.middleLanePosition.x, newPosition.y, 0);
                        else if (confirmedLane == LanePosition.RIGHT_POS)
                            confirmedPosition = new Vector3(ValueStorer.rightLanePosition.x, newPosition.y, 0);

                        confirmedNote = Instantiate(leftTeleportPrefab, confirmedPosition, Quaternion.identity) as GameObject;
                        confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                        confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].leftTeleportFolder.transform, true);
                        break;
                    }
                case NoteTypeGeneral.RIGHT_TELEPORT:
                    {
                        Vector3 confirmedPosition = Vector3.zero;

                        if (confirmedLane == LanePosition.LEFT_POS)
                            confirmedPosition = new Vector3(ValueStorer.leftLanePosition.x, newPosition.y, 0);
                        else if (confirmedLane == LanePosition.MIDDLE_POS)
                            confirmedPosition = new Vector3(ValueStorer.middleLanePosition.x, newPosition.y, 0);
                        else if (confirmedLane == LanePosition.RIGHT_POS)
                            confirmedPosition = new Vector3(ValueStorer.rightLanePosition.x, newPosition.y, 0);

                        confirmedNote = Instantiate(rightTeleportPrefab, confirmedPosition, Quaternion.identity) as GameObject;
                        confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                        confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].rightTeleportFolder.transform, true);
                        break;
                    }
                case NoteTypeGeneral.SLICE_NOTE:
                    {
                        confirmedNote = Instantiate(sliceNotePrefab, new Vector3(0, newPosition.y, 0), Quaternion.identity) as GameObject;
                        confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                        confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].sliceFolder.transform, true);
                        break;
                    }
                case NoteTypeGeneral.SPIKE:
                    {
                        if (startPosition.x >= ValueStorer.minMiddleLaneX && startPosition.x <= ValueStorer.maxMiddleLaneX)
                        {
                            confirmedNote = Instantiate(middleSpikePrefab, new Vector3(0, newPosition.y, 0), Quaternion.identity) as GameObject;
                        }
                        else if ((startPosition.x >= ValueStorer.minLeftLaneX && startPosition.x <= ValueStorer.maxLeftLaneX)
                                || (startPosition.x >= ValueStorer.minRightLaneX && startPosition.x <= ValueStorer.maxRightLaneX))
                        {
                            confirmedNote = Instantiate(sideSpikePrefab, new Vector3(0, newPosition.y, 0), Quaternion.identity) as GameObject;
                        }
                        confirmedNote.GetComponent<MusicNote>().timing = confirmedNote.transform.localPosition.y / chartSpeed;
                        confirmedNote.transform.SetParent(timingGroups[timingGroupIndex].spikeFolder.transform, true);
                        break;
                    }
                default: break;
            }

            if (!confirmedNote)
            {
                return;
            }

            MusicNote musicNote = confirmedNote.GetComponent<MusicNote>();
            musicNote.timingGroup = timingGroups[timingGroupIndex];
            musicNote.temporaryTiming = musicNote.timing;
        }
    }

    void InsertMultipleNotesSignal(Vector3 targetPosition, ref bool isPreSelected)
    {
        if (!isPreSelected)
        {
            firstAddingPosition = targetPosition;

            addedSignal = GameObject.Instantiate(multipleNotesSignal, targetPosition, Quaternion.identity) as GameObject;
            addedSignal.transform.SetParent(beatLinesFolder.transform, true);
            isPreSelected = true;
        }
        else
        {
            if (IsCorrectLane(targetPosition))
            {
                lastAddingPosition = targetPosition;
                InsertMultipleNotes(firstAddingPosition, lastAddingPosition, noteAmount);
            }

            if (addedSignal != null)
            {
                Destroy(addedSignal);
            }

            isPreSelected = false;
        }
    }

    void CancelMultipleNotesSignal()
    {
        if (addedSignal != null)
        {
            Destroy(addedSignal);
        }
        isMultipleNotesSignalAdded = false;
    }

    bool IsNoteAreaFullyLocked()
    {
        return Keyboard.current != null &&
                    openNoteArea.activeSelf &&
                    closeNoteArea.activeSelf &&
                    noteArea.activeSelf &&
                    !isNoteAreaAdded;
    }

    void ResetNoteSelectArea(ref bool isPreselected)
    {
        openNoteArea.SetActive(false);
        closeNoteArea.SetActive(false);
        noteArea.SetActive(false);
        isPreselected = false;

        foundNotesInArea.Clear();
    }


    bool IsWithinOpenCloseNoteArea()
    {
        return (openNoteArea.transform.position.y <= worldPosition.y && worldPosition.y <= closeNoteArea.transform.position.y)
            || (closeNoteArea.transform.position.y <= worldPosition.y && worldPosition.y <= openNoteArea.transform.position.y);
    }

    void InsertNotesInArea()
    {
        foundNotesInArea.Clear();

        foundNotesInArea.AddRange(FindAllNotesInRange(timingGroups[timingGroupIndex].tapFolder.transform));
        foundNotesInArea.AddRange(FindAllNotesInRange(timingGroups[timingGroupIndex].blackFolder.transform));
        foundNotesInArea.AddRange(FindAllNotesInRange(timingGroups[timingGroupIndex].sliceFolder.transform));
        foundNotesInArea.AddRange(FindAllNotesInRange(timingGroups[timingGroupIndex].leftTeleportFolder.transform));
        foundNotesInArea.AddRange(FindAllNotesInRange(timingGroups[timingGroupIndex].rightTeleportFolder.transform));
        foundNotesInArea.AddRange(FindAllNotesInRange(timingGroups[timingGroupIndex].spikeFolder.transform));
    }

    Transform GetLowestNote()
    {
        Transform lowestNote = null;
        float lowestTiming = 0f;
        foreach (Transform note in foundNotesInArea)
        {
            MusicNote musicNote = note.gameObject.GetComponent<MusicNote>();
            if (musicNote == null)
            {
                continue;
            }

            if (lowestNote == null)
            {
                lowestNote = note;
                lowestTiming = musicNote.timing;
            }
            else
            {
                if (musicNote.timing < lowestTiming)
                {
                    lowestNote = note;
                    lowestTiming = musicNote.timing;
                }
            }
        }
        return lowestNote;
    }

    Transform GetHighestNote()
    {
        Transform highestNote = null;
        float highestTiming = 0f;
        foreach (Transform note in foundNotesInArea)
        {
            MusicNote musicNote = note.gameObject.GetComponent<MusicNote>();
            if (musicNote == null)
            {
                continue;
            }

            if (highestNote == null)
            {
                highestNote = note;
                highestTiming = musicNote.timing;
            }
            else
            {
                if (musicNote.timing > highestTiming)
                {
                    highestNote = note;
                    highestTiming = musicNote.timing;
                }
            }
        }
        return highestNote;
    }

    void ExecuteNoteAreaSignal(Vector3 targetPosition, ref bool isPreselected)
    {
        if (targetPosition.x < ValueStorer.minLeftLaneX ||
            targetPosition.x > ValueStorer.maxRightLaneX)
        {
            ResetNoteSelectArea(ref isPreselected);
            return;
        }

        if (!isPreselected)
        {
            openNoteArea.transform.position = new Vector3(0, targetPosition.y, 0);
            openNoteTempTiming = openNoteArea.transform.localPosition.y / chartSpeed;
            openNoteArea.SetActive(true);

            closeNoteArea.transform.position = new Vector3(0, targetPosition.y, 0);
            closeNoteTempTiming = closeNoteArea.transform.localPosition.y / chartSpeed;
            closeNoteArea.SetActive(true);

            if (openNoteArea.transform.position.y < closeNoteArea.transform.position.y)
            {
                noteArea.transform.position = new Vector3(0, openNoteArea.transform.position.y, 0);
            }
            else
            {
                noteArea.transform.position = new Vector3(0, closeNoteArea.transform.position.y, 0);
            }
            areaTempTiming = noteArea.transform.localPosition.y / chartSpeed;
            noteArea.transform.localScale = new Vector3(
                1,
                Mathf.Abs(openNoteArea.transform.position.y - closeNoteArea.transform.position.y),
                1);
            noteArea.SetActive(true);

            foundNotesInArea.Clear();

            isPreselected = true;
        }
        else
        {
            ShrinkArea();

            openNoteArea.transform.localPosition = new Vector3(0, openNoteTempTiming * chartSpeed, 0);
            openNoteArea.SetActive(true);

            closeNoteArea.transform.localPosition = new Vector3(0, closeNoteTempTiming * chartSpeed, 0);
            closeNoteArea.SetActive(true);

            if (openNoteArea.transform.position.y < closeNoteArea.transform.position.y)
            {
                noteArea.transform.position = new Vector3(0, openNoteArea.transform.position.y, 0);
            }
            else
            {
                noteArea.transform.position = new Vector3(0, closeNoteArea.transform.position.y, 0);
            }
            noteArea.transform.localScale = new Vector3(
                1,
                Mathf.Abs(openNoteArea.transform.position.y - closeNoteArea.transform.position.y),
                1);
            areaTempTiming = noteArea.transform.localPosition.y / chartSpeed;
            noteArea.SetActive(true);

            isPreselected = false;
        }
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

        if (isTimingGridConfirm && confirmedHorizontalGrid)
        {
            yPos = horizontalGridValue >= 0 ? horizontalGridValue : 0;
        }

        draggedNote.gameObject.transform.position = new Vector3(xPos, yPos, 0);
        draggedNote.timing = draggedNote.gameObject.transform.localPosition.y / chartSpeed;
    }

    void ShrinkArea()
    {
        InsertNotesInArea();

        if (foundNotesInArea.Count == 0)
        {
            ResetNoteSelectArea(ref isNoteAreaAdded);
            return;
        }

        MusicNote foundLowestNote = null;
        MusicNote foundHighestNote = null;

        foreach (Transform noteTransform in foundNotesInArea)
        {
            if (noteTransform == null)
            {
                continue;
            }

            MusicNote musicNote = noteTransform.gameObject.GetComponent<MusicNote>();
            if (musicNote == null)
            {
                continue;
            }

            if (foundLowestNote == null && foundHighestNote == null)
            {
                foundLowestNote = musicNote;
                foundHighestNote = musicNote;
            }
            else
            {
                if (musicNote.timing < foundLowestNote.timing) foundLowestNote = musicNote;
                else if (musicNote.timing > foundHighestNote.timing) foundHighestNote = musicNote;
            }
        }

        openNoteTempTiming = foundLowestNote.timing;
        closeNoteTempTiming = foundHighestNote.timing;

        foundNotesInArea.Clear();
    }

    GameObject FindAlignArea(float totalPosition)
    {
        if (openNoteArea.activeSelf == false && closeNoteArea.activeSelf == false)
        {
            return null;
        }

        GameObject targetLowestGrid = null;

        GameObject[] grids = GameObject.FindGameObjectsWithTag(ValueStorer.tagHorizontalGrid);
        foreach (GameObject grid in grids)
        {
            if (Mathf.Abs(openNoteArea.transform.position.y - grid.transform.position.y) > ValueStorer.gridOffset)
            {
                continue;
            }

            if (targetLowestGrid == null)
            {
                targetLowestGrid = grid;
            }
            else
            {
                if (grid.transform.position.y < targetLowestGrid.transform.position.y)
                {
                    targetLowestGrid = grid;
                }
            }
        }

        if (targetLowestGrid && Mathf.Abs(targetLowestGrid.transform.position.y - fixedAreaPosition.y - totalPosition) > ValueStorer.gridOffset)
        {
            return null;
        }

        return targetLowestGrid;
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

    List<Transform> FindAllNotesInRange(Transform folder)
    {
        Vector3 openAreaPos = openNoteArea.transform.position;
        Vector3 closeAreaPos = closeNoteArea.transform.position;

        List<Transform> foundNotes = new List<Transform>();

        foreach (Transform note in folder)
        {
            bool isWithinArea = false;

            isWithinArea = ((openAreaPos.y <= note.position.y && note.position.y <= closeAreaPos.y) ||
                            (closeAreaPos.y <= note.position.y && note.position.y <= openAreaPos.y));

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

        List<Transform> foundTapNotes = FindAppropriateNotesInFolder(targetPosition, timingGroups[timingGroupIndex].tapFolder.transform);
        List<Transform> foundBlackNotes = FindAppropriateNotesInFolder(targetPosition, timingGroups[timingGroupIndex].blackFolder.transform);
        List<Transform> foundLeftTeleports = FindAppropriateNotesInFolder(targetPosition, timingGroups[timingGroupIndex].leftTeleportFolder.transform);
        List<Transform> foundRightTeleports = FindAppropriateNotesInFolder(targetPosition, timingGroups[timingGroupIndex].rightTeleportFolder.transform);
        List<Transform> foundSliceNotes = FindAppropriateNotesInFolder(targetPosition, timingGroups[timingGroupIndex].sliceFolder.transform);
        List<Transform> foundSpikes = FindAppropriateNotesInFolder(targetPosition, timingGroups[timingGroupIndex].spikeFolder.transform);

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

    private bool IsCorrectLane(Vector3 targetPosition)
    {
        if (targetPosition.x < ValueStorer.minLeftLaneX ||
            (targetPosition.x > ValueStorer.maxLeftLaneX && targetPosition.x < ValueStorer.minMiddleLaneX) ||
            (targetPosition.x > ValueStorer.maxMiddleLaneX && targetPosition.x < ValueStorer.minRightLaneX) ||
            targetPosition.x > ValueStorer.maxRightLaneX)
        {
            return false;
        }

        return true;
    }

    public void ConfirmEditOption(int index)
    {
        editOption = (EditOption)index;

        ResetNoteSelectArea(ref isNoteAreaAdded);

        if (editOption == EditOption.NOTE_ADD)
        {
            noteSelectDropDown.gameObject.SetActive(true);
            noteAmountUI.gameObject.SetActive(false);
        }
        else if (editOption == EditOption.NOTE_DRAG)
        {
            noteSelectDropDown.gameObject.SetActive(false);
            noteAmountUI.gameObject.SetActive(false);
        }
        else if (editOption == EditOption.NOTE_ADD_BETWEEN)
        {
            noteSelectDropDown.gameObject.SetActive(true);
            noteAmountUI.gameObject.SetActive(true);
        }
    }

    public void SetPlayMode(bool isPlayMode)
    {
        if (isPlayMode && uiManager.timingItems.Count <= 0)
        {
            return;
        }

        playMode = isPlayMode;

        if (playMode == false)
        {
            audioSource.Stop();

            foreach (TimingGroup group in timingGroups)
            {
                group.gameObject.transform.position = Vector3.zero;
            }
            ReenableNotes();

            RejectTimingSpeed();

            player.ChangePosition(player.originalPosition);
        }
        else
        {
            player.originalPosition = player.lanePosition;
            ApplyTimingSpeed();
            audioSource.Play();
        }
    }

    void ReenableNotes()
    {
        foreach (TimingGroup group in timingGroups)
        {
            var usedNotes = group.usedNotesFolder.transform;

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
                    case MusicNote.NoteType.TAP_NOTE: note.SetParent(group.tapFolder.transform, false); break;
                    case MusicNote.NoteType.BLACK_NOTE: note.SetParent(group.blackFolder.transform, false); break;
                    case MusicNote.NoteType.LEFT_TELEPORT: note.SetParent(group.leftTeleportFolder.transform, false); break;
                    case MusicNote.NoteType.RIGHT_TELEPORT: note.SetParent(group.rightTeleportFolder.transform, false); break;
                    case MusicNote.NoteType.SLICE_NOTE: note.SetParent(group.sliceFolder.transform, false); break;
                    case MusicNote.NoteType.MIDDLE_SPIKE: note.SetParent(group.spikeFolder.transform, false); break;
                    case MusicNote.NoteType.SIDE_SPIKE: note.SetParent(group.spikeFolder.transform, false); break;
                    default: continue;
                }

                note.gameObject.SetActive(true);
            }
        }
    }

    void ReenableOneNote(GameObject note)
    {
        MusicNote musicNote = note.GetComponent<MusicNote>();
        Transform noteTransform = musicNote.transform;

        GameObject timingGroupObj = note.transform.root.gameObject;
        TimingGroup group = timingGroupObj.GetComponent<TimingGroup>();

        switch (musicNote.GetNoteType())
        {
            case MusicNote.NoteType.TAP_NOTE: noteTransform.SetParent(group.tapFolder.transform, false); break;
            case MusicNote.NoteType.BLACK_NOTE: noteTransform.SetParent(group.blackFolder.transform, false); break;
            case MusicNote.NoteType.LEFT_TELEPORT: noteTransform.SetParent(group.leftTeleportFolder.transform, false); break;
            case MusicNote.NoteType.RIGHT_TELEPORT: noteTransform.SetParent(group.rightTeleportFolder.transform, false); break;
            case MusicNote.NoteType.SLICE_NOTE: noteTransform.SetParent(group.sliceFolder.transform, false); break;
            case MusicNote.NoteType.MIDDLE_SPIKE: noteTransform.SetParent(group.spikeFolder.transform, false); break;
            case MusicNote.NoteType.SIDE_SPIKE: noteTransform.SetParent(group.spikeFolder.transform, false); break;
            default: break;
        }

        musicNote.gameObject.SetActive(true);
    }

    public void AddTimingItem()
    {
        uiManager.AddTimingItem();
    }

    public void AddSpeedItem()
    {
        uiManager.AddSpeedItem(timingGroupIndex);
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

            note.localPosition = new Vector3(note.position.x, musicNote.timing * chartSpeed, 0);
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
            musicNote.temporaryTiming = musicNote.timing;
            note.localPosition = new Vector3(note.position.x, musicNote.timing * chartSpeed, 0);
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

    GameObject GetHoveredHorizontalGrid()
    {
        GameObject[] grids = GameObject.FindGameObjectsWithTag(ValueStorer.tagHorizontalGrid);
        foreach (GameObject grid in grids)
        {
            HorizontalGrid horizontalGrid = grid.GetComponent<HorizontalGrid>();
            if (horizontalGrid.isHovered && grid.activeSelf)
            {
                return grid;
            }
        }
        return null;
    }

    public void ApplyTimingBPM()
    {
        if (timingGroups.Count == 0)
        {
            return;
        }

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
        List<BPMStorer> timingItems = uiManager.timingItems;

        if (timingItems.Exists(item => item.BPM == 0))
        {
            return;
        }

        for (int i = 0; i < timingItems.Count; i++)
        {
            tempTiming = timingItems[i].timing;
            int mainBeatCount = 0;
            int stepBeatCount = 0;

            while (i + 1 != timingItems.Count &&
                tempTiming + ((float)mainBeatCount + (float)stepBeatCount / (float)beatDensity) * 60f / timingItems[i].BPM * 1000f < timingItems[i + 1].timing)
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

            while (i + 1 == timingItems.Count &&
                tempTiming + ((float)mainBeatCount + (float)stepBeatCount / (float)beatDensity) * 60f / timingItems[i].BPM * 1000f < audioSource.clip.length * 1000)
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
        timingGroupStorer.transform.position = Vector3.zero;
        beatLinesFolder.SetActive(false);

        for (int index = 0; index < timingGroups.Count; index++)
        {
            timingGroups[index].gameObject.SetActive(true);

            if (uiManager.speedItems[index].Count <= 0)
            {
                return;
            }

            List<SpeedStorer> speeds = uiManager.speedItems[index];
            if (speeds.Count <= 0)
            {
                continue;
            }

            List<MusicNote> notes = null;

            float tempSpeedMulti = 1f;
            float totalLength = 0f;

            for (int i = 0; i < speeds.Count; i++)
            {
                if (i == 0)
                {
                    tempSpeedMulti = 1f;
                    totalLength += (speeds[i].timing * tempSpeedMulti / 1000f);
                }
                else
                {
                    tempSpeedMulti = speeds[i - 1].speedMulti;
                    notes = FindAllNotesWithTiming(index, speeds[i - 1].timing, speeds[i].timing);
                    foreach (MusicNote note in notes)
                    {
                        note.ChangeSpeedPosition(totalLength, chartSpeed, speeds[i - 1].timing, tempSpeedMulti);
                    }

                    totalLength += (speeds[i].timing - speeds[i - 1].timing) / 1000f * tempSpeedMulti;
                }
            }
            tempSpeedMulti = speeds[speeds.Count - 1].speedMulti;
            notes = FindAllNotesWithTiming(index, speeds[speeds.Count - 1].timing, audioSource.clip.length * 1000f);
            foreach (MusicNote note in notes)
            {
                note.ChangeSpeedPosition(totalLength, chartSpeed, speeds[speeds.Count - 1].timing, tempSpeedMulti);
            }

            totalLength += (audioSource.clip.length - speeds[speeds.Count - 1].timing / 1000f) * tempSpeedMulti;
        }
    }

    public void RejectTimingSpeed()
    {
        for (int i = 0; i < timingGroups.Count; i++)
        {
            if (i == timingGroupIndex)
            {
                timingGroups[i].gameObject.SetActive(true);
            }
            else
            {
                timingGroups[i].gameObject.SetActive(false);
            }
            beatLinesFolder.SetActive(true);

            List<MusicNote> notes = FindAllNotesWithTiming(i, 0, audioSource.clip.length * 1000f);
            foreach (MusicNote note in notes)
            {
                note.ResetSpeedPosition(chartSpeed);
            }
        }
    }

    public void ChangeSpeedThroughTiming(float timing)
    {
        for (int index = 0; index < timingGroups.Count; index++)
        {
            List<SpeedStorer> speeds = uiManager.speedItems[index];

            float tempSpeedMulti = 1f;
            float totalLength = 0f;

            if (speeds.Count <= 0)
            {
                tempSpeedMulti = 1f;
                totalLength += (timing * tempSpeedMulti / 1000f);
                timingGroups[index].gameObject.transform.position = new Vector3(0, -totalLength * chartSpeed, 0);
                continue;
            }

            bool isChecked = false;
            for (int i = 0; i < speeds.Count; i++)
            {
                if (timing < speeds[i].timing)
                {
                    if (i == 0)
                    {
                        tempSpeedMulti = 1f;
                        totalLength += (timing * tempSpeedMulti / 1000f);
                    }
                    else
                    {
                        tempSpeedMulti = speeds[i - 1].speedMulti;
                        totalLength += (timing - speeds[i - 1].timing) / 1000f * tempSpeedMulti;
                    }
                    timingGroups[index].gameObject.transform.position = new Vector3(0, -totalLength * chartSpeed, 0);
                    isChecked = true;
                    break;
                }

                if (i == 0)
                {
                    tempSpeedMulti = 1f;
                    totalLength += (speeds[i].timing * tempSpeedMulti / 1000f);
                }
                else
                {
                    tempSpeedMulti = speeds[i - 1].speedMulti;
                    totalLength += (speeds[i].timing - speeds[i - 1].timing) / 1000f * tempSpeedMulti;
                }
            }

            if (!isChecked)
            {
                tempSpeedMulti = speeds[speeds.Count - 1].speedMulti;
                totalLength += (timing - speeds[speeds.Count - 1].timing) / 1000f * tempSpeedMulti;
                timingGroups[index].gameObject.transform.position = new Vector3(0, -totalLength * chartSpeed, 0);
            }
        }
    }

    List<MusicNote> FindAllNotesWithTiming(int index, float beginTiming, float endTiming)
    {
        List<MusicNote> foundTaps = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].tapFolder.transform);
        List<MusicNote> foundBlacks = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].blackFolder.transform);
        List<MusicNote> foundSlice = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].sliceFolder.transform);
        List<MusicNote> foundSpikes = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].spikeFolder.transform);
        List<MusicNote> foundLeftTeleports = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].leftTeleportFolder.transform);
        List<MusicNote> foundRightTeleport = FindAllNotesWithTimingFromFolder(beginTiming, endTiming, timingGroups[index].rightTeleportFolder.transform);

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

    void UpdateGroupIndicators()
    {
        int count = timingGroups.Count;
        if (count == 0)
        {
            uiManager.speedIndicator.text = ValueStorer.noSpeedGroupText;
        }
        else
        {
            if (timingGroupIndex >= count)
            {
                timingGroupIndex = count - 1;
            }
            else if (timingGroupIndex < 0)
            {
                timingGroupIndex = 0;
            }
            uiManager.speedIndicator.text = ValueStorer.hasSpeedGroupText + timingGroupIndex.ToString();
        }
    }

    public void AddSpeedGroup()
    {
        foreach (TimingGroup currentTG in timingGroups)
        {
            currentTG.gameObject.SetActive(false);
        }

        GameObject timingGroupObj = Instantiate(timingGroupPrefab.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
        timingGroupObj.transform.SetParent(timingGroupStorer.transform, false);

        TimingGroup newTimingGroup = timingGroupObj.GetComponent<TimingGroup>();
        timingGroups.Add(newTimingGroup);

        List<SpeedStorer> newSpeedItems = new List<SpeedStorer>();
        uiManager.speedItems.Add(newSpeedItems);

        timingGroupIndex = uiManager.speedItems.Count - 1;

        UpdateGroupIndicators();
        uiManager.RefreshSpeedItems(timingGroupIndex);
    }

    public void DeleteSpeedGroup()
    {
        if (timingGroupIndex < 0 || timingGroupIndex >= uiManager.speedItems.Count)
        {
            return;
        }

        uiManager.speedItems.RemoveAt(timingGroupIndex);

        TimingGroup targetTimingGroup = timingGroups[timingGroupIndex];
        timingGroups.RemoveAt(timingGroupIndex);
        Destroy(targetTimingGroup.gameObject);

        if (timingGroupIndex >= uiManager.speedItems.Count)
        {
            timingGroupIndex -= 1;
        }

        UpdateGroupIndicators();
        uiManager.RefreshSpeedItems(timingGroupIndex);

        for (int i = 0; i < timingGroups.Count; i++)
        {
            if (i == timingGroupIndex)
            {
                timingGroups[i].gameObject.SetActive(true);
            }
            else
            {
                timingGroups[i].gameObject.SetActive(false);
            }
        }
    }

    public void MoveTimingGroupLeft()
    {
        if (timingGroupIndex <= 0)
        {
            return;
        }

        timingGroupIndex -= 1;

        UpdateGroupIndicators();
        uiManager.RefreshSpeedItems(timingGroupIndex);

        for (int i = 0; i < timingGroups.Count; i++)
        {
            if (i == timingGroupIndex)
            {
                timingGroups[i].gameObject.SetActive(true);
            }
            else
            {
                timingGroups[i].gameObject.SetActive(false);
            }
        }
    }

    public void MoveTimingGroupRight()
    {
        if (timingGroupIndex >= uiManager.speedItems.Count - 1)
        {
            return;
        }

        timingGroupIndex += 1;

        UpdateGroupIndicators();
        uiManager.RefreshSpeedItems(timingGroupIndex);

        for (int i = 0; i < timingGroups.Count; i++)
        {
            if (i == timingGroupIndex)
            {
                timingGroups[i].gameObject.SetActive(true);
            }
            else
            {
                timingGroups[i].gameObject.SetActive(false);
            }
        }
    }

    /*
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
        noteObject.transform.SetParent(timingundoRedoFolder.transform, true);
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
    */
    #region Chart Properties

    void ReloadChartOffsetVisuals(float originalOffset)
    {
        chartOffsetText.text = ValueStorer.chartOffsetText + chartOffset.ToString();

        for (int i = 0; i < timingGroups.Count; i++)
        {
            ChangePositionsThroughOffset(timingGroups[i].tapFolder.transform, originalOffset);
            ChangePositionsThroughOffset(timingGroups[i].blackFolder.transform, originalOffset);
            ChangePositionsThroughOffset(timingGroups[i].sliceFolder.transform, originalOffset);
            ChangePositionsThroughOffset(timingGroups[i].leftTeleportFolder.transform, originalOffset);
            ChangePositionsThroughOffset(timingGroups[i].rightTeleportFolder.transform, originalOffset);
            ChangePositionsThroughOffset(timingGroups[i].spikeFolder.transform, originalOffset);
            ChangePositionsThroughOffset(timingGroups[i].usedNotesFolder.transform, originalOffset);
            ChangePositionsThroughOffset(timingGroups[i].undoRedoFolder.transform, originalOffset);
        }
    }

    void ReloadChartSpeedVisuals()
    {
        chartSpeedText.text = ValueStorer.chartSpeedText + chartSpeed.ToString();
        speedInputField.text = chartSpeed.ToString();

        for (int i = 0; i < timingGroups.Count; i++)
        {
            ChangePositionsThroughSpeed(timingGroups[i].tapFolder.transform);
            ChangePositionsThroughSpeed(timingGroups[i].blackFolder.transform);
            ChangePositionsThroughSpeed(timingGroups[i].sliceFolder.transform);
            ChangePositionsThroughSpeed(timingGroups[i].leftTeleportFolder.transform);
            ChangePositionsThroughSpeed(timingGroups[i].rightTeleportFolder.transform);
            ChangePositionsThroughSpeed(timingGroups[i].spikeFolder.transform);
            ChangePositionsThroughSpeed(timingGroups[i].usedNotesFolder.transform);
            ChangePositionsThroughSpeed(timingGroups[i].undoRedoFolder.transform);
        }
    }

    public void ChangeNoteAmount()
    {
        bool isParsed = int.TryParse(noteAmountInputField.text, out noteAmount);
        if (!isParsed)
        {
            noteAmount = 1;
        }
    }

    public void ChangeChartOffset()
    {
        float originalOffset = chartOffset;

        bool isParsed = float.TryParse(offsetInputField.text, out float newChartOffset);
        if (!isParsed)
        {
            chartOffset = 0;
        }
        else
        {
            chartOffset = newChartOffset / 1000f;
        }

        ReloadChartOffsetVisuals(originalOffset);
        ApplyTimingBPM();

        //CommandChangeOffset commandChangeOffset = new CommandChangeOffset(originalOffset, chartOffset);
        //OverrideCommand(commandChangeOffset);
    }

    public void ChangeChartSpeed()
    {
        float originalSpeed = chartSpeed;

        bool isParsed = float.TryParse(speedInputField.text, out chartSpeed);
        if (!isParsed || chartSpeed <= 0)
        {
            chartSpeed = 1;
        }

        ReloadChartSpeedVisuals();
        ApplyTimingBPM();

        timingGroupStorer.transform.position = new Vector3(0, -editorCurrentTiming * chartSpeed, 0);

        openNoteArea.transform.localPosition = new Vector3(0, openNoteTempTiming * chartSpeed, 0);
        closeNoteArea.transform.localPosition = new Vector3(0, closeNoteTempTiming * chartSpeed, 0);
        noteArea.transform.localPosition = new Vector3(0, areaTempTiming * chartSpeed, 0);
        noteArea.transform.localScale = new Vector3(1, Mathf.Abs(openNoteTempTiming - closeNoteTempTiming) * chartSpeed, 1);

        //CommandChangeSpeed commandChangeSpeed = new CommandChangeSpeed(originalSpeed, chartSpeed);
        //OverrideCommand(commandChangeSpeed);
    }

    #endregion
}
