using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;

public class SavedGames : MonoBehaviour
{
    public GameObject BoiteContent;
    public GameObject prefab_saved;
    public GameObject saved_games;
    public  GameObject saved_games_view;
    public TextMeshProUGUI texto;
    public DateTime date_partie_texto;
    public int name_partie_texto;
    public Button return_button;
        //Call->(name_partie_texto,date_partie_texto)=(string,DateTime) get_village_name(MySqlConnection conn,int id_partie){
        //Call-> texto=string get_action(MySqlConnection conn,int id_partie){
    // Start is called before the first frame update
    void Start()
    {
        refresh_string("Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"asdasdasdasdasdasdasdasdsadas"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"asdasdasdasdasdasdasdasdsadas"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"asdasdasdasdasdasdasdasdsadas"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"asdasdasdasdasdasdasdasdsadas"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"asdasdasdasdasdasdasdasdsadas"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"Les Loups-Garous doivent prendre le dessus sur le village!\r\n\n"+"asdasdasdasdasdasdasdasdsadas");
        for(int i=0;i<10;i++){

        add_savedgame("VillageTest",DateTime.Now,1000);
        }
        return_button.onClick.AddListener(returnto_history);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void refresh_string(string historique){
        texto.text=historique;
    }
    public void add_savedgame(string villagename,DateTime date,int duree){
        GameObject newPrefab=Instantiate(prefab_saved,BoiteContent.transform);
        newPrefab.transform.localScale = new Vector3(1, 1, 1);
        TextMeshProUGUI textGame=newPrefab.transform.Find("TextGame").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI textDate=newPrefab.transform.Find("TextDate").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI Duree=newPrefab.transform.Find("Duree").GetComponent<TextMeshProUGUI>();
        textGame.text=villagename;
        textDate.text=date.ToString();
        Duree.text=""+duree+" Min";
        //
        EventTrigger eventTrigger = newPrefab.AddComponent<EventTrigger>();

        // Add a PointerClick event to the EventTrigger component
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnPointerClick((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);
    }
    void OnPointerClick(PointerEventData eventData)
    {
        // Call the ShowHistory method
        show_history();
    }
    public void show_history(){
        saved_games.gameObject.SetActive(false);
        saved_games_view.gameObject.SetActive(true);
}
    public void returnto_history(){
        saved_games.gameObject.SetActive(true);
        saved_games_view.gameObject.SetActive(false);
    }
}