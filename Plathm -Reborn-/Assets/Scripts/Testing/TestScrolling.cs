using UnityEngine;

public class TestScrolling : MonoBehaviour
{
    [SerializeField] float scrollingSpeed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position -= new Vector3(0, scrollingSpeed * Time.deltaTime, 0);
    }
}
