using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyShowRadar : MonoBehaviour
{
   private bool isImageShown = false; // 跟踪texture是否显示
    public CanvasGroup canvasGroup;
    private float targetAlpha;
    void Start(){
        canvasGroup=GetComponent<CanvasGroup>();
        targetAlpha=canvasGroup.alpha;
    }
    void show(){
        targetAlpha=1;
        canvasGroup.alpha=targetAlpha;
    }
    void hide(){
        targetAlpha=0;
        canvasGroup.alpha=targetAlpha;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) // 检测'R'键是否被按下
        {
            Debug.Log("Press R");
            isImageShown = !isImageShown; // 切换状态
            if(isImageShown){
                show();
            }
            else{
                hide();
            }
        }
    }
}
