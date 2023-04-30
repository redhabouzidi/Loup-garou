using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;

public class rank : MonoBehaviour
{
    public GameObject BoiteContent;
    public GameObject prefab_Saved;
    // Start is called before the first frame update
    void Start()
    {
        string[]pseudos={"Tombala78","Tombala31","Tombala23","Tombala24","Tombala699","Tombala728"};
        int[]scores={50,40,20,0,-10,-20};
        createFields(pseudos,scores);
        StartCoroutine(refresh_test());
    }
IEnumerator refresh_test()
{

    yield return new WaitForSeconds(3); // Pause for 1 second
    // Do something after 1 second
    string[]pseudos={"Tombalatest3131","Tombala78","Tombala31","Tombala23","Tombala24","Tombala699","Tombala728","TombalaTest21"};
    int[]scores={100,50,40,20,0,-10,-20,-30};
    refresh_fields(pseudos,scores);
}
    // Update is called once per frame
    void Update()
    {
        
    }


    public void refresh_fields(string[] pseudos,int[] scores){
        Image[] childTextComponents = BoiteContent.GetComponentsInChildren<Image>();
        int created_numbers=childTextComponents.Length;
        if(pseudos.Length>created_numbers){
                createFields(new string[pseudos.Length-created_numbers],new int[pseudos.Length-created_numbers]);
            }
        childTextComponents = BoiteContent.GetComponentsInChildren<Image>();
            for(int i=0;i<pseudos.Length;i++){
                Image tmp=childTextComponents[i];
                TextMeshProUGUI textNum=tmp.transform.Find("Num").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI textPseudo=tmp.transform.Find("Pseudo").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI textScore=tmp.transform.Find("Score").GetComponent<TextMeshProUGUI>();
                if(i==0){
                textNum.color=new Color(1f, 0.843f, 0f, 1f);
                textPseudo.color=new Color(1f, 0.843f, 0f, 1f);
                textScore.color=new Color(1f, 0.843f, 0f, 1f);
            }
            else if(i==1){
                textNum.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                textPseudo.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                textScore.color=new Color(0.9f, 0.9f, 0.9f, 1f);
            }
            else if(i==2){
                textNum.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                textPseudo.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                textScore.color=new Color(0.804f, 0.498f, 0.196f, 1f);
            }
            else{
                textNum.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                textPseudo.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                textScore.color=new Color(0.6f, 0.6f, 0.6f, 1f);
            }
                textPseudo.text=pseudos[i];
                textScore.text=""+scores[i];
            }
    }

    //Fonctions pour creer une premiere fois les boites et les champs text
    public void createFields(string[] pseudos,int[] scores){
        for(int i=0;i<pseudos.Length;i++){
            GameObject newPrefab=Instantiate(prefab_Saved,BoiteContent.transform);
            TextMeshProUGUI textNum=newPrefab.transform.Find("Num").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textPseudo=newPrefab.transform.Find("Pseudo").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textScore=newPrefab.transform.Find("Score").GetComponent<TextMeshProUGUI>();
            textNum.text=""+(i+1);
            textPseudo.text=pseudos[i];
            textScore.text=""+scores[i];
            if(i==0){
                textNum.color=new Color(1f, 0.843f, 0f, 1f);
                textPseudo.color=new Color(1f, 0.843f, 0f, 1f);
                textScore.color=new Color(1f, 0.843f, 0f, 1f);
            }
            else if(i==1){
                textNum.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                textPseudo.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                textScore.color=new Color(0.9f, 0.9f, 0.9f, 1f);
            }
            else if(i==2){
                textNum.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                textPseudo.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                textScore.color=new Color(0.804f, 0.498f, 0.196f, 1f);
            }
            else{
                textNum.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                textPseudo.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                textScore.color=new Color(0.6f, 0.6f, 0.6f, 1f);
            }
        }
        
    }


}
