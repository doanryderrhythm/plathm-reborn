using System;
using System.Collections.Generic;
using UnityEngine;
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

    //Chart shenanigans
    private Camera mainCamera;
    private Vector3 worldPosition;

    //Gameplay shenanigans
    private HashSet<Key> reservedTapKeys;
    private HashSet<Key> reservedBlackKeys;
    private bool isAnyKeyHolding = false;
    private bool isBlackKeyReserved = false;

    //Note selection
    [SerializeField] bool isNoteTypeSelected;
    [SerializeField] NoteTypeGeneral selectedNoteType;

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

    private void Awake()
    {
        mainCamera = GameObject.FindFirstObjectByType<Camera>();

        isNoteTypeSelected = false;

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
        if (isNoteTypeSelected && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = mainCamera.nearClipPlane;
            worldPosition = mainCamera.ScreenToWorldPoint(mousePos);

            InsertNote(worldPosition);
        }

        //scrollPlayfield.transform.position -= new Vector3(0, scrollSpeed * Time.deltaTime, 0);

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
}
