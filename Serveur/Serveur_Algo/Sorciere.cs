using System.Net.Sockets;

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
    { // écrire l'action de la sorciere
        Console.WriteLine("Appel à la sorcière !");
        Socket? socketSorciere = null;

        // On récupère la Socket de la sorcière
        foreach (Joueur j in listJoueurs)
        {
            if (j.GetRole() is Sorciere)
            {
                socketSorciere = j.GetSocket();
                break;
            }
        }

        bool potionUse = false;
        idJoueurVise = -1;
        if (potionSoin != 0)
        {
            potionUse = PotionVie(listJoueurs, socketSorciere);
        }
        if (potionKill != 0 && !potionUse)
        {
            PotionMort(listJoueurs, socketSorciere);
        }
    }
    public bool PotionVie(List<Joueur> listJoueurs, Socket? socketSorciere)
    {
        bool retour = false;
        for (int i = 0; i < listJoueurs.Count; i++)
        {
            if (listJoueurs[i].GetDoitMourir())
            {

                idJoueurVise = listJoueurs[i].GetId();
                // envoieInformation(x,y)
                // fonction "boîte noire" qui envoie l'information que le joueur x a été tué sur la socket y
                Server.server.EnvoieInformation(socketSorciere, idJoueurVise);

                bool boucle = true;

                // on définit une "alarme" qui modifie la valeur du boolean
                // valeur arbitraire => 10 secondes
                Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                reveille.Connect(Game.listener.LocalEndPoint);
                Socket vide;
                vide = Game.listener.Accept();
                Task t = Task.Run(() =>
                {
                    Thread.Sleep(GetDelaiAlarme() * 500);
                    vide.Send(new byte[1] { 0 });
                    boucle = false;
                });

                while (boucle)
                {
                    var result = gameVote(listJoueurs,reveille);
                    if (result.Item2 == 1)
                    {
                        listJoueurs[i].SetDoitMourir(false);
                    }
                    else if (result.Item2 == 0)
                    {
                        listJoueurs[i].SetDoitMourir(true);
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

    public void PotionMort(List<Joueur> listJoueurs, Socket? socketSorciere)
    {
        bool boucle = true;
        // on définit une "alarme" qui modifie la valeur du boolean
        // valeur arbitraire => 10 secondes
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        Task t = Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 500);
            vide.Send(new byte[1] { 0 });
            boucle = false;
        });

        Joueur? cible = null;

        while (boucle)
        {
            var result = gameVote(listJoueurs,reveille);
            if (result.Item2 != -1)
            {
                if (cible != null)
                {
                    cible.SetDoitMourir(false);
                }
                cible = listJoueurs.FirstOrDefault(player => player.GetId() == result.Item2);
                if (cible != null && cible.GetRole() is not Sorciere && (idJoueurVise == -1 || cible.GetId() != idJoueurVise) && cible.GetEnVie())
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