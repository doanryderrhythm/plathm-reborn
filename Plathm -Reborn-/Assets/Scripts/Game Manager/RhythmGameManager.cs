using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
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

    double currentTiming = 0f;

    public double chartSpeed;
    public List<TimingGroup> timingGroups;

    //public List<BPMStorer> BPMItems;

    public List<List<SpeedStorer>> speedItems;

    private void Start()
    {
        speedItems = new List<List<SpeedStorer>>();

        speedItems.Add(new List<SpeedStorer>());
        speedItems.Add(new List<SpeedStorer>());

        speedItems[0].Add(new SpeedStorer(0, 1));
        speedItems[0].Add(new SpeedStorer(2000, 0.25f));
        speedItems[0].Add(new SpeedStorer(3000, 1));

        speedItems[1].Add(new SpeedStorer(0, 1));
        speedItems[1].Add(new SpeedStorer(2000, 2f));
        speedItems[1].Add(new SpeedStorer(3000, 1));
    }

    private void Update()
    {
        currentTiming += (Time.deltaTime * 1000f);
        ChangeSpeedThroughTiming(currentTiming);
    }

    public void ChangeSpeedThroughTiming(double timing)
    {
        for (int index = 0; index < timingGroups.Count; index++)
        {
            List<SpeedStorer> speeds = speedItems[index];

            double tempSpeedMulti = 1f;
            double totalLength = 0f;

            if (speeds.Count <= 0)
            {
                tempSpeedMulti = 1f;
                totalLength += (timing * tempSpeedMulti / 1000f);
                timingGroups[index].gameObject.transform.position = new Vector3(0, (float)(-totalLength * chartSpeed), 0);
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
                    timingGroups[index].gameObject.transform.position = new Vector3(0, (float)(-totalLength * chartSpeed), 0);
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
                timingGroups[index].gameObject.transform.position = new Vector3(0, (float)(-totalLength * chartSpeed), 0);
            }
        }
    }
}
