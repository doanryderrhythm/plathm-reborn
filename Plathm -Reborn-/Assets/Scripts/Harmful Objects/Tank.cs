using UnityEngine;

public class Tank : MonoBehaviour
{
    [SerializeField] float waitTime;
    [SerializeField] float shootSpeed;

    public enum TankType
    {
        TYPE_ONE,
        TYPE_FOUR,
    }

    [SerializeField] TankType tankType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private float timePassed = 0f;

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        if (timePassed >= waitTime)
        {
            timePassed -= waitTime;
            ShootBullets();
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
