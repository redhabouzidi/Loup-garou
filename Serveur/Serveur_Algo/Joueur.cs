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
    private bool aEteSave; // pour le rôle du garde plus tard
    private bool enVie;
    private bool doitMourir;

    public Joueur(int ClientId, Socket socketClient, string name)
    {
        id = ClientId;
        enVie = true;
        doitMourir = false;
        amoureux = null;
        socket = socketClient;
        pseudo = name;
        aEteSave = false;
    }

    public void FaireAction(List<Joueur> ListJoueurs)
    {
        role.Action(ListJoueurs);
    }

    public void TuerJoueur(List<Joueur> ListJoueurs)
    {
        enVie = false;
        foreach (Joueur p in ListJoueurs)
        {
            server.annonceMort(p.GetSocket(), GetId(), GetRole().GetIdRole());
        }
        if (GetAmoureux() != null)
        {
            AmoureuxTuerJoueur(GetAmoureux(), ListJoueurs);
            SetAmoureux(null); 
        }
        // if (role = Chasseur)
    }

    public void AmoureuxTuerJoueur(Joueur j, List<Joueur> ListJoueurs)
    {
        j.SetAmoureux(null);
        j.TuerJoueur(ListJoueurs);
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

}