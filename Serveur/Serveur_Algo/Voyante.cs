using System.Net.Sockets;
using Server;
namespace LGproject;

public class Voyante : Role
{
    private new const int IdRole = 3;
    public Voyante()
    {
        name = "Voyante";
        description = "blabla";
    }

    public override void Action(List<Joueur> listJoueurs)
    { // écrire l'action de la Voyante
        foreach (Joueur j in listJoueurs)
        {
            server.sendTurn(j.GetSocket(), GetIdRole());
        }
        
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        bool boucle = true;
        vide = Game.listener.Accept();
        bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
        foreach (Joueur j in listJoueurs)
        {
            server.sendTime(j.GetSocket(), GetDelaiAlarme());
        }
        Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 750); // 15 secondes
            reduceTimer = true;
            if (reduceTimer && !LaunchThread2)
            {
                Thread.Sleep(GetDelaiAlarme() * 250); // 5 secondes
                vide.Send(new byte[1] { 0 });
                boucle = false;
            }
        });

        Joueur JoueurVoyante = null;
        Role? JoueuraVoircarte = null;

        foreach (Joueur j in listJoueurs)
        {
            if (j.GetRole() is Voyante)
            {
                JoueurVoyante = j;
            }
        }

        int v, c;
        Joueur? player = null;
        Console.WriteLine("la voyante commencera son action");

        while (boucle)
        {
            (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
            if (v == JoueurVoyante.GetId())
            {
                player = listJoueurs.Find(j => j.GetId() == c);
                if (player != null)
                {
                    if (player.GetRole() is not Voyante && player.GetEnVie())
                    {
                        JoueuraVoircarte = player.GetRole();
                        // envoyer l'information JoueuraVoircarte sur la Socket de la voyante
                        server.revelerRole(JoueurVoyante.GetSocket(), player.GetId(), player.GetRole().GetIdRole());

                        if (!reduceTimer && firstTime)
                        {
                            firstTime = false;
                            LaunchThread2 = true;
                            Task.Run(() =>
                            {
                                Thread.Sleep(GetDelaiAlarme() * 250);
                                Console.WriteLine("la voyante a voté, ça passe à 5sec d'attente");
                                vide.Send(new byte[1] { 0 });
                                boucle = false;
                            });
                        }
                    }
                }
            }
        }
    }

    public override int GetIdRole()
    {
        return IdRole;
    }
}