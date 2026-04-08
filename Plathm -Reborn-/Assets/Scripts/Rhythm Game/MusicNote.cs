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

    public void ChangeSpeedPosition(double totalLength, double chartSpeed, double beginTiming, double speedMulti)
    {
        this.transform.position = new Vector3(
            this.transform.position.x,
            (float)(((this.timing * 1000f - beginTiming) / 1000f * speedMulti + totalLength) * chartSpeed), 0);
    }
}
