using UnityEngine;

public class Tank : MonoBehaviour
{
    [SerializeField] float waitTime;
    [SerializeField] float warningTime;
    [SerializeField] float shootSpeed;

    [SerializeField] SpriteRenderer sr;

    private TankTimeType timeType;

    private enum TankType
    {
        TYPE_ONE,
        TYPE_FOUR,
    }

    [SerializeField] TankType tankType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTime = waitTime;
        timeType = TankTimeType.WAIT_TIME;
    }

    private float timePassed = 0f;
    private float currentTime;

    public enum TankTimeType
    {
        WAIT_TIME,
        WARNING_TIME,
    }

    void SwitchTimeType()
    {
        timePassed -= currentTime;
        if (timeType == TankTimeType.WAIT_TIME)
        {
            currentTime = warningTime;
            timeType = TankTimeType.WARNING_TIME;
        }
        else if (timeType == TankTimeType.WARNING_TIME)
        {
            currentTime = waitTime;
            timeType = TankTimeType.WAIT_TIME;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;

        if (timeType == TankTimeType.WARNING_TIME)
        {
            sr.color = new Color(1, 1, 1, timePassed / warningTime * ValueStorer.tankWarningAlpha);
        }
        else
        {
            sr.color = new Color(1, 1, 1, 0);
        }

        if (timePassed >= currentTime)
        {
            if (timeType == TankTimeType.WARNING_TIME)
                ShootBullets();

            SwitchTimeType();
        }
    }

    void ShootBullets()
    {
        int totalShots = 0;

        switch (tankType)
        {
            case TankType.TYPE_ONE: totalShots = 1; break;
            case TankType.TYPE_FOUR: totalShots = 4; break;
            default: break;
        }
        
        for (int i = 0; i < totalShots; i++)
        {
            InsertBullet(transform.eulerAngles.z - 90 * i);
        }
    }

    [SerializeField] GameObject bulletPrefab;

    void InsertBullet(float shootAngle)
    {
        GameObject bullet = Instantiate(bulletPrefab, new Vector3(
            transform.position.x + 0.5f * Mathf.Cos(shootAngle * Mathf.Deg2Rad),
            transform.position.y + 0.5f * Mathf.Sin(shootAngle * Mathf.Deg2Rad), 0), Quaternion.identity);
        Bullet bulletCom = bullet.GetComponent<Bullet>();
        bulletCom.speed = shootSpeed;
        bulletCom.shootAngle = shootAngle;
    }
}
