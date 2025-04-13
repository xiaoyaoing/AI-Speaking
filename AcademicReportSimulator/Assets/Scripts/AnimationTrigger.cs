using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    Animator animator;
    private float timer = 0.0f;
    private bool sitting = true;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (animator.GetBool("Applause"))
        {
            ResetApplauseTrigger();
        }
        if (Input.GetKeyDown(KeyCode.C))  // 按下'C'键触发鼓掌
        {
            animator.SetTrigger("Applause");
            timer = 0.0f;
        }
    }
    
    public void ResetApplauseTrigger()
    {
        animator.ResetTrigger("Applause");
    }
}
