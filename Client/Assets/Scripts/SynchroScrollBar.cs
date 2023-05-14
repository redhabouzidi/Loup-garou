using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SynchroScrollBar : MonoBehaviour
{
    public Scrollbar scrollbar1, scrollbar2;
    
    // Start is called before the first frame update
    void Start()
    {
        scrollbar2.value = scrollbar1.value;

        scrollbar1.onValueChanged.AddListener(OnScrollbar1ValueChanged);
        scrollbar2.onValueChanged.AddListener(OnScrollbar2ValueChanged);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnScrollbar1ValueChanged(float value)
    {
        scrollbar2.value = value;
    }

    public void OnScrollbar2ValueChanged(float value)
    {
        scrollbar1.value = value;
    }
}
