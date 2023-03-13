using System.Net.Sockets;
using Server;

namespace LGproject;

public class Sorciere : Role
{
    private int potionSoin;
    private int potionKill;
    private int idJoueurVise;
    private new const int IdRole = 5;

    public Sorciere()
    {
        name = "Sorcière";
        description = "blabla";
        potionSoin = 1;
        potionKill = 1;
    }

    public override void Action(List<Joueur> listJoueurs)
    {
        

        // écrire l'action de la sorciere
        Console.WriteLine("Appel à la sorcière !");
        Joueur? joueurSorciere = null;

        // On récupère la Socket de la sorcière
        foreach (Joueur j in listJoueurs)
        {
            if (j.GetRole() is Sorciere)
            {
                joueurSorciere = j;
                break;
            }
        }

        bool potionUse = false;
        idJoueurVise = -1;
        if (potionSoin != 0)
        {
            potionUse = PotionVie(listJoueurs, joueurSorciere);
        }

        if (potionKill != 0 && !potionUse)
        {
            PotionMort(listJoueurs, joueurSorciere);
        }
    }

    public bool PotionVie(List<Joueur> listJoueurs, Joueur? joueurSorciere)
    {
        bool retour = false;
        int v, c;
        for (int i = 0; i < listJoueurs.Count; i++)
        {
            if (listJoueurs[i].GetDoitMourir())
            {
                idJoueurVise = listJoueurs[i].GetId();
                // envoieInformation(x,y)
                // fonction "boîte noire" qui envoie l'information que le joueur x a été tué sur la socket y
                server.EnvoieInformation(joueurSorciere.GetSocket(), idJoueurVise);
                foreach (Joueur j in listJoueurs)
                {
                    server.sendTurn(j.GetSocket(), GetIdRole());
                }
                bool boucle = true;

                // on définit une "alarme" qui modifie la valeur du boolean
                // valeur arbitraire => 10 secondes
                Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                reveille.Connect(Game.listener.LocalEndPoint);
                Socket vide;
                vide = Game.listener.Accept();
                foreach (Joueur j in listJoueurs)
                {
                    server.sendTime(j.GetSocket(), GetDelaiAlarme());
                }
                Task t = Task.Run(() =>
                {
                    Thread.Sleep(GetDelaiAlarme() * 500);
                    vide.Send(new byte[1] { 0 });
                    boucle = false;
                });

                while (boucle)
                {
                    // on définit que (v, c) si c == 1 alors le joueur décide de sauver, sinon 0
                    (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
                    if (v == joueurSorciere.GetId())
                    {
                        switch (c)
                        {
                            case 0:
                                listJoueurs[i].SetDoitMourir(true);
                                break;
                            case 1:
                                listJoueurs[i].SetDoitMourir(false);
                                break;
                        }
                    }
                }
            }
        }

        if (idJoueurVise != -1)
        {
            Joueur? cible = listJoueurs.FirstOrDefault(player => player.GetId() == idJoueurVise);
            if (cible != null && !cible.GetDoitMourir())
            {
                potionSoin--;
                retour = true;
            }
        }

        return retour;
    }

    public void PotionMort(List<Joueur> listJoueurs, Joueur? joueurSorciere)
    {
        foreach (Joueur j in listJoueurs)
        {
            server.sendTurn(j.GetSocket(), GetIdRole());
        }
        bool boucle = true;
        // on définit une "alarme" qui modifie la valeur du boolean
        // valeur arbitraire => 10 secondes
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        foreach (Joueur j in listJoueurs)
        {
            server.sendTime(j.GetSocket(), GetDelaiAlarme());
        }
        Task t = Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 500);
            vide.Send(new byte[1] { 0 });
            boucle = false;
        });

        Joueur? cible = null;

        int v, c;
        while (boucle)
        {
            (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
            if (v == joueurSorciere.GetId() && c != -1)
            {
                if (cible != null)
                {
                    cible.SetDoitMourir(false);
                }

                cible = listJoueurs.FirstOrDefault(player => player.GetId() == c);
                if (cible != null && cible.GetRole() is not Sorciere &&
                    (idJoueurVise == -1 || cible.GetId() != idJoueurVise) && cible.GetEnVie())
                {
                    cible.SetDoitMourir(true);
                }
            }
        }

        if (cible != null && cible.GetDoitMourir())
        {
            potionKill -= 1;
        }
    }

    public override int GetIdRole()
    {
        return IdRole;
    }
}