using UnityEngine;

public class SpeedStorer : MonoBehaviour
{
    public float timing = 0f;
    public float speedMulti = 0f;

    public SpeedStorer(float timing, float speedMulti)
    {
        this.timing = timing;
        this.speedMulti = speedMulti;
    }
}
