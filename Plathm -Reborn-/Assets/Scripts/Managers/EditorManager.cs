using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
    private Vector3 worldPosition;
    [SerializeField] EditOption editOption;

    [Space(10.0f)]
    //Gameplay shenanigans
    private HashSet<Key> reservedTapKeys;
    private HashSet<Key> reservedBlackKeys;
    private bool isAnyKeyHolding = false;
    private bool isBlackKeyReserved = false;

    //Note selection
    [SerializeField] bool isNoteTypeSelected;
    [SerializeField] NoteTypeGeneral selectedNoteType;
    [SerializeField] MusicNote draggedNote;

    [Header("Editor Settings")]
    public bool playMode = false;

    [Header("Input Actions")]
    [SerializeField] InputAction inputAnyKey;
    [SerializeField] InputAction inputLeftTeleport;
    [SerializeField] InputAction inputRightTeleport;
    [SerializeField] InputAction inputSlice;

    [Header("Note Folders")]
    [SerializeField] GameObject tapFolder;
    [SerializeField] GameObject blackFolder;
    [SerializeField] GameObject sliceFolder;
    [SerializeField] GameObject leftTeleportFolder;
    [SerializeField] GameObject rightTeleportFolder;
    [SerializeField] GameObject spikeFolder;
    [SerializeField] GameObject usedNotesFolder;

    [Header("Note Types")]
    [SerializeField] GameObject tapNotePrefab;
    [SerializeField] GameObject blackNotePrefab;
    [SerializeField] GameObject leftTeleportPrefab;
    [SerializeField] GameObject rightTeleportPrefab;
    [SerializeField] GameObject sliceNotePrefab;
    [SerializeField] GameObject middleSpikePrefab;
    [SerializeField] GameObject sideSpikePrefab;

    [Header("Gameplay")]
    public float scrollSpeed;
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
    [SerializeField] TMP_Dropdown noteSelectDropDown;

    private void Awake()
    {
        mainCamera = GameObject.FindFirstObjectByType<Camera>();

        isNoteTypeSelected = false;
        draggedNote = null;

        reservedTapKeys = new HashSet<Key>();
        reservedBlackKeys = new HashSet<Key>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RebuildReservedKeys();
    }

    // Update is called once per frame
    void Update()
    {
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
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
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
            scrollPlayfield.transform.position -= new Vector3(0, scrollSpeed * Time.deltaTime, 0);
        }

        if (isAnyKeyHolding && isBlackKeyReserved)
        {
            ExecuteInput(blackFolder);
        }
    }

    private void OnEnable()
    {
        inputAnyKey.Enable();

        inputLeftTeleport.Enable();
        inputRightTeleport.Enable();
        inputSlice.Enable();

        inputAnyKey.started     += OnAnyKeyStarted;
        inputAnyKey.canceled    += OnAnyKeyCanceled;
        inputAnyKey.performed   += CheckReservedTapKeys;

        inputLeftTeleport.performed     += _ => ExecuteInput(leftTeleportFolder);
        inputRightTeleport.performed    += _ => ExecuteInput(rightTeleportFolder);
        inputSlice.performed            += _ => ExecuteInput(sliceFolder);
    }

    private void OnDisable()
    {
        inputAnyKey.Disable();

        inputLeftTeleport.Disable();
        inputRightTeleport.Disable();
        inputSlice.Disable();

        inputAnyKey.performed -= CheckReservedTapKeys;

        inputLeftTeleport.performed     -= _ => { };
        inputRightTeleport.performed    -= _ => { };
        inputSlice.performed            -= _ => { };
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

        Transform lowestNote = null;

        foreach (Transform child in folder.transform)
        {
            if (!lowestNote)
            {
                lowestNote = child;
                continue;
            }

            if (child.position.y < lowestNote.position.y)
            {
                lowestNote = child;
            }
        }

        if (!lowestNote)
        {
            return;
        }

        MusicNote musicNote = lowestNote.gameObject.GetComponent<MusicNote>();
        musicNote?.ExecuteNote();
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
                Destroy(note.gameObject);
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
                confirmedNote = Instantiate(tapNotePrefab, targetPosition, Quaternion.identity) as GameObject;
                confirmedNote.transform.SetParent(tapFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.BLACK_NOTE:
            {
                confirmedNote = Instantiate(blackNotePrefab, targetPosition, Quaternion.identity) as GameObject;
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
                confirmedNote.transform.SetParent(rightTeleportFolder.transform, true);
                break;
            }
            case NoteTypeGeneral.SLICE_NOTE:
            {
                confirmedNote = Instantiate(sliceNotePrefab, new Vector3(0, targetPosition.y, 0), Quaternion.identity) as GameObject;
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
                confirmedNote.transform.SetParent(spikeFolder.transform, true);
                break;
            }
            default: break;
        }
    }

    void MoveNote()
    {
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
            else if (xPos > ValueStorer.maxLeftLaneX && xPos < ValueStorer.minMiddleLaneX)
            {
                if (currentXPos >= ValueStorer.minLeftLaneX && currentXPos <= ValueStorer.maxLeftLaneX) xPos = ValueStorer.maxLeftLaneX;
                else if (currentXPos >= ValueStorer.minMiddleLaneX && currentXPos <= ValueStorer.maxMiddleLaneX) xPos = ValueStorer.minMiddleLaneX;
            }
            else if (xPos > ValueStorer.maxMiddleLaneX && xPos < ValueStorer.minRightLaneX)
            {
                if (currentXPos >= ValueStorer.minMiddleLaneX && currentXPos <= ValueStorer.maxMiddleLaneX) xPos = ValueStorer.maxMiddleLaneX;
                else if (currentXPos >= ValueStorer.minRightLaneX && currentXPos <= ValueStorer.maxRightLaneX) xPos = ValueStorer.minRightLaneX;
            }
            else if (xPos > ValueStorer.maxRightLaneX)
            {
                xPos = ValueStorer.maxRightLaneX;
            }
        }

        draggedNote.gameObject.transform.position = new Vector3(xPos, yPos, 0);
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
        if (targetPosition.x >= ValueStorer.minLeftLaneX && targetPosition.x <= ValueStorer.maxLeftLaneX)
        {
            return LanePosition.LEFT_POS;
        }
        else if (targetPosition.x >= ValueStorer.minMiddleLaneX && targetPosition.x <= ValueStorer.maxMiddleLaneX)
        {
            return LanePosition.MIDDLE_POS;
        }
        else if (targetPosition.x >= ValueStorer.minRightLaneX && targetPosition.x <= ValueStorer.maxRightLaneX)
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
}
