using System.Net.Sockets;

namespace LGproject;

public class Joueur
{
    private int id;
    private Socket socket;
    private string pseudo;
    private Role role;
    private Joueur? amoureux;
    public bool aEteSave; // pour le rôle du garde plus tard
    private bool enVie;
    private bool reveille;
    private bool doitMourir;

    public Joueur(int ClientId, Socket socketClient, string name)
    {
        id = ClientId;
        enVie = true;
        doitMourir = false;
        amoureux = null;
        socket = socketClient;
        pseudo = name;
    }

    public void FaireAction(List<Joueur> ListJoueurs)
    {
        reveille = true;
        role.Action(ListJoueurs);
        reveille = false;
    }

    public void TuerJoueur(Joueur j)
    {
        enVie = false;
        if (j.GetAmoureux() != null)
        {
            AmoureuxTuerJoueur(j.GetAmoureux());
        }
        // if (role = Chasseur)
    }

    public void AmoureuxTuerJoueur(Joueur j)
    {
        j.SetAmoureux(null);
        TuerJoueur(j);
    }

    public void SetAmoureux(Joueur j)
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
    public bool GetEnVie(){
        return enVie;
    }

    public void SetEnVie(bool b){
        enVie = b;
    }

    public int GetId()
    {
        return id;
    }

}