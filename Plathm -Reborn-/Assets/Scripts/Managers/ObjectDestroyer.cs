using System.Collections;
using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    [SerializeField] float timeSpan;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(timeSpan);
        Destroy(gameObject);
    }
}
