using UnityEngine;

public class InformationCanvas : MonoBehaviour
{
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StopAnimation()
    {
        animator.runtimeAnimatorController = null;
    }
}
