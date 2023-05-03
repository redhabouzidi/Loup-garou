using System.Net.Sockets;
using Server;
namespace LGproject;

public class Joueur
{
    private int id;
    private Socket socket;
    private string pseudo;
    private Role role;
    private Joueur? amoureux;
    private bool aEteSave;
    private bool enVie;
    private bool doitMourir;
    private bool estMaire;
    private bool ready;
    public Joueur(int ClientId, Socket socketClient, string name)
    {
        id = ClientId;
        enVie = true;
        doitMourir = false;
        amoureux = null;
        socket = socketClient;
        pseudo = name;
        aEteSave = false;
        estMaire = false;
        ready = false;
    }

    public string FaireAction(List<Joueur> ListJoueurs,Game game)
    {
        return role.Action(ListJoueurs,game);
    }

    public void TuerJoueur(List<Joueur> ListJoueurs)
    {
        enVie = false;
	doitMourir = false;
        foreach (Joueur p in ListJoueurs)
        {
            server.annonceMort(p.GetSocket(), GetId(), GetRole().GetIdRole());
        }
    }

    public void SetAmoureux(Joueur? j)
    {
        amoureux = j;
    }

    public Joueur? GetAmoureux()
    {
        return amoureux;
    }

    public void SetRole(Role r)
    {
        role = r;
    }

    public string GetPseudo()
    {
        return pseudo;
    }

    public Role GetRole()
    {
        return role;
    }

    public Socket GetSocket()
    {
        return socket;
    }
    public void SetSocket(Socket socket)
    {
        this.socket = socket;
    }
    public void SetDoitMourir(bool etat)
    {
        doitMourir = etat;
    }

    public bool GetDoitMourir()
    {
        return doitMourir;
    }

    public override string ToString()
    {
        return $"Joueur[{pseudo}]";
    }
    public bool GetEnVie()
    {
        return enVie;
    }

    public void SetEnVie(bool b)
    {
        enVie = b;
    }

    public int GetId()
    {
        return id;
    }

    public void SetAEteSave(bool b) {
        aEteSave = b;
    }

    public bool GetAEteSave() {
        return aEteSave;
    }

    public bool GetEstMaire(){
        return estMaire;
    }

    public void SetEstMaire(bool b){
        estMaire = b;
    }
    public void SetReady(bool ready)
    {
        this.ready = ready;
    }
    public bool GetReady()
    {
        return ready;
    }
}
