using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;

public class Statistiques : MonoBehaviour
{
    public GameObject BoiteContent;
    public GameObject prefab_Saved;
    public Button butPseudo;
    public Button butNbPartie;
    public Button butWinrate;
    public Button butPoints;

    // Start is called before the first frame update
    void Start()
    {
        butPseudo.onClick.AddListener(sort_by_pseudo);
        butNbPartie.onClick.AddListener(sort_by_partie_joue);
        butWinrate.onClick.AddListener(sort_by_winrate);
        butPoints.onClick.AddListener(sort_by_points);
        string[]pseudos={"Tombala78","Tombala31","Tombala23","Tombala24","Tombala699","Tombala728"};
        int[]nbpartiesjoues={50,40,20,0,-10,-20};
        int[]nbvictoires={50,40,20,0,-10,-20};
        double[]winrates={10.2,20.2,30.2,60.5,68.2,90.2};
        createFields_stat(pseudos,nbpartiesjoues,nbvictoires,winrates);
        StartCoroutine(refresh_test());
    }
IEnumerator refresh_test()
{

    yield return new WaitForSeconds(3); // Pause for 1 second
    // Do something after 1 second
        string[]pseudos={"Tombala78","Tombala31","Tombala23","Tombala24","Tombala699","Tombala728"};
        int[]nbpartiesjoues={100,60,40,0,-10,-20};
        int[]nbvictoires={90,60,40,10,-10,-20};
        double[]winrates={90.2,89.2,60.2,61.5,60.2,50.2};
        refresh_fields_stat(pseudos,nbpartiesjoues,nbvictoires,winrates);

    //refresh_fields(pseudos,scores);
}
    // Update is called once per frame
    void Update()
    {
        
    }
    public void sort_by_pseudo (){
        Debug.Log("Pseudo");
    }
    public void sort_by_partie_joue(){
        Debug.Log("PartieJoue");
    }
    public void sort_by_winrate(){
        Debug.Log("Winrate");
    }
    public void sort_by_points(){
        Debug.Log("Points");
    }
    public void refresh_fields_stat(string[] pseudos,int[] nbpartiesjoues,int[]nbvictoires,double[]winrate){
        Image[] childTextComponents = BoiteContent.GetComponentsInChildren<Image>();
        int created_numbers=childTextComponents.Length;
        if(pseudos.Length>created_numbers){
                createFields_stat(new string[pseudos.Length-created_numbers],new int[pseudos.Length-created_numbers],new int[pseudos.Length-created_numbers],new double[pseudos.Length-created_numbers]);
            }
        childTextComponents = BoiteContent.GetComponentsInChildren<Image>();
            for(int i=0;i<pseudos.Length;i++){
                Image tmp=childTextComponents[i];
                TextMeshProUGUI textNum=tmp.transform.Find("Num").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI textPseudo=tmp.transform.Find("Pseudo").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI textPartiejoue=tmp.transform.Find("Nbpartiejoue").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI Points=tmp.transform.Find("Points").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI textWinrate=tmp.transform.Find("Winrate").GetComponent<TextMeshProUGUI>();
                textPseudo.text=pseudos[i];
                textPartiejoue.text=""+nbpartiesjoues[i];
                Points.text=""+nbvictoires[i];
                textWinrate.text=""+winrate[i];
                if(i==0){
                    textNum.color=new Color(1f, 0.843f, 0f, 1f);
                    textPseudo.color=new Color(1f, 0.843f, 0f, 1f);
                    textPartiejoue.color=new Color(1f, 0.843f, 0f, 1f);
                    Points.color=new Color(1f, 0.843f, 0f, 1f);
                    textWinrate.color=new Color(1f, 0.843f, 0f, 1f);
                }
                else if(i==1){
                    textNum.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                    textPseudo.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                    textPartiejoue.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                    Points.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                    textWinrate.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                }
                else if(i==2){
                    textNum.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                    textPseudo.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                    textPartiejoue.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                    Points.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                    textWinrate.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                }
                else{
                    textNum.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                    textPseudo.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                    textPartiejoue.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                    Points.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                    textWinrate.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                }
            }
    }

    //Fonctions pour creer une premiere fois les boites et les champs text
    public void createFields_stat(string[] pseudos,int[] nbpartiesjoues,int[]nbvictoires,double[]winrate){
        for(int i=0;i<pseudos.Length;i++){
            GameObject newPrefab=Instantiate(prefab_Saved,BoiteContent.transform);
            TextMeshProUGUI textNum=newPrefab.transform.Find("Num").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textPseudo=newPrefab.transform.Find("Pseudo").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textPartiejoue=newPrefab.transform.Find("Nbpartiejoue").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textWinrate=newPrefab.transform.Find("Winrate").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textPoints=newPrefab.transform.Find("Points").GetComponent<TextMeshProUGUI>();
            textNum.text=""+(i+1);
                textPseudo.text=pseudos[i];
                textPartiejoue.text=""+nbpartiesjoues[i];
                textPoints.text=""+nbvictoires[i];
                textWinrate.text=""+winrate[i];
                if(i==0){
                    textNum.color=new Color(1f, 0.843f, 0f, 1f);
                    textPseudo.color=new Color(1f, 0.843f, 0f, 1f);
                    textPartiejoue.color=new Color(1f, 0.843f, 0f, 1f);
                    textPoints.color=new Color(1f, 0.843f, 0f, 1f);
                    textWinrate.color=new Color(1f, 0.843f, 0f, 1f);
                }
                else if(i==1){
                    textNum.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                    textPseudo.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                    textPartiejoue.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                    textPoints.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                    textWinrate.color=new Color(0.9f, 0.9f, 0.9f, 1f);
                }
                else if(i==2){
                    textNum.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                    textPseudo.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                    textPartiejoue.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                    textPoints.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                    textWinrate.color=new Color(0.804f, 0.498f, 0.196f, 1f);
                }
                else{
                    textNum.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                    textPseudo.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                    textPartiejoue.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                    textPoints.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                    textWinrate.color=new Color(0.6f, 0.6f, 0.6f, 1f);
                }
        }
        
    }


}
