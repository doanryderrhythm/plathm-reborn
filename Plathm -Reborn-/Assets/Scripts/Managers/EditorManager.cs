using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EditorManager : MonoBehaviour
{
    //Other shenanigans
    private HashSet<Key> reservedKeys;

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
        reservedKeys = new HashSet<Key>();
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
    }

    private void OnEnable()
    {
        inputAnyKey.Enable();
        inputLeftTeleport.Enable();
        inputRightTeleport.Enable();
        inputSlice.Enable();

        inputAnyKey.performed           += _ => ExecuteInput(tapFolder);
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

        inputAnyKey.performed           -= _ => { };
        inputLeftTeleport.performed     -= _ => { };
        inputRightTeleport.performed    -= _ => { };
        inputSlice.performed            -= _ => { };
    }

    void AddReservedKeys(InputAction inputAction)
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
        reservedKeys.Clear();
        AddReservedKeys(inputLeftTeleport);
        AddReservedKeys(inputRightTeleport);
        AddReservedKeys(inputSlice);
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
