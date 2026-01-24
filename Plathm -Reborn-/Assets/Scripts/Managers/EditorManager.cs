using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EditorManager : MonoBehaviour
{
    //Other shenanigans
    private HashSet<Key> reservedTapKeys;
    private HashSet<Key> reservedBlackKeys;
    private bool isAnyKeyHolding = false;
    private bool isBlackKeyReserved = false;

    public enum LanePosition
    {
        LEFT_POS,
        MIDDLE_POS,
        RIGHT_POS,
    }

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

    [Header("Gameplay")]
    public float scrollSpeed;
    [SerializeField] GameObject scrollPlayfield;

    private void Awake()
    {
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
        scrollPlayfield.transform.position -= new Vector3(0, scrollSpeed * Time.deltaTime, 0);

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

        musicNote.ExecuteNote();
    }

    public void SwitchToUsedFolder(Transform usedNote)
    {
        usedNote.SetParent(usedNotesFolder.transform);
    }
}
