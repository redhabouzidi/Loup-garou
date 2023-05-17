using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class NavigatorGame : MonoBehaviour
{
    private Selectable currentSelection;
    public Selectable objetback;
    public Button connection;
    public GameObject cartes;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("ca fait quelquechose");
                currentSelection = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
                Selectable nextSelection = currentSelection.FindSelectableOnDown();
                if (nextSelection != null)
                {
                    nextSelection.Select();
                }else{
                    currentSelection = objetback;
                    EventSystem.current.SetSelectedGameObject(objetback.gameObject,null);
                }
            
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            connection.onClick.Invoke();
        }
    }
}
