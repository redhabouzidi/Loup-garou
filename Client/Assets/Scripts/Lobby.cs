using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;

public class Lobby : MonoBehaviour
{
    // Start is called before the first frame update
    public Timer refresh;

    void OnEnable(){
        refresh = new Timer();
        refresh.Elapsed +=  new ElapsedEventHandler(onTimer);
        refresh.Interval = 2000;
        refresh.Start();
    }
    void onTimer(object source, ElapsedEventArgs e){
        NetworkManager.sendRequestGames();
    }
    void OnDisable(){
        refresh.Enabled=false;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
