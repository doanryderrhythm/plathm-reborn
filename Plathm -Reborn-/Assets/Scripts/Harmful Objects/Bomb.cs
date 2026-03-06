using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] float offsetTime;
    [SerializeField] float waitingTime;
    [SerializeField] float warningTime;
    [SerializeField] float appearTime;

    private float currentTime;
    private float setTime;

    public enum BombTimeType
    {
        NONE,
        WAIT,
        WARN,
        APPEAR,
    }

    private BombTimeType type;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        type = BombTimeType.NONE;
        setTime = offsetTime;
    }

    [SerializeField] SpriteRenderer warningSR;
    [SerializeField] SpriteRenderer harmfulSR;

    [SerializeField] ParticleSystem harmfulParticles;

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= setTime)
        {
            ChangeBombTimeType();
        }
        UpdateBombVisual();
    }

    void ChangeBombTimeType()
    {
        if (type == BombTimeType.WAIT)
        {
            type = BombTimeType.WARN;
            currentTime -= setTime;
            setTime = warningTime;
        }
        else if (type == BombTimeType.WARN)
        {
            type = BombTimeType.APPEAR;
            currentTime -= setTime;
            setTime = appearTime;

            harmfulParticles.Play();
        }
        else if (type == BombTimeType.APPEAR || type == BombTimeType.NONE)
        {
            type = BombTimeType.WAIT;
            currentTime -= setTime;
            setTime = waitingTime;
        }
    }

    void UpdateBombVisual()
    {
        if (type == BombTimeType.WAIT || type == BombTimeType.NONE)
        {
            warningSR.gameObject.SetActive(false);
            harmfulSR.gameObject.SetActive(false);
        }
        else if (type == BombTimeType.WARN)
        {
            warningSR.color = new Color(1f, 0f, 0f, ValueStorer.bombWarningAlpha * currentTime / setTime);
            warningSR.gameObject.SetActive(true);
            harmfulSR.gameObject.SetActive(false);
        }
        else if (type == BombTimeType.APPEAR)
        {
            warningSR.gameObject.SetActive(false);
            harmfulSR.gameObject.SetActive(true);
        }
    }
}
