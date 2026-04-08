using UnityEngine;

public class MusicNote : MonoBehaviour
{
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

    public TimingGroup timingGroup;
    public double timing;

    //[SerializeField] bool isBlackActivated = false;
}
