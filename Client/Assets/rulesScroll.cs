using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class rulesScroll : MonoBehaviour
{
    public Scrollbar scrollbar;
    public float scrollSpeed = 0.002f;

    private void Update()
    {
        float verticalAxis = Input.GetAxis("Vertical");
        float scrollValue = scrollbar.value;

        if (verticalAxis > 0)
        {
            scrollbar.value = Mathf.Clamp(scrollValue + scrollSpeed, 0f, 1f);
        }
        else if (verticalAxis < 0)
        {
            scrollbar.value = Mathf.Clamp(scrollValue - scrollSpeed, 0f, 1f);
        }
    }
}
