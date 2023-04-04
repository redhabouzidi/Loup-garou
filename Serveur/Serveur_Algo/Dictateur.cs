using Server;
using System.Net.Sockets;
namespace LGproject;

public class Dictateur : Role
{
    private new const int IdRole = 999999;
    public Dictateur()
    {
        name = "Dictateur";
        description = "blabla";
    }

    public override void Action(List<Joueur> listJoueurs)
    { // écrire l'action du loup
        Joueur? joueurDictateur = null;
        foreach (Joueur j in listJoueurs)
        {
            server.sendTurn(j.GetSocket(), GetIdRole());
            if (j.GetRole() is Dictateur)
            {
                joueurDictateur = j;
            }
        }
        bool boucle = true;
        
        // on définit une "alarme" qui modifie la valeur du boolean
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        // bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
        sendTime(listJoueurs, GetDelaiAlarme());
        Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 250); // 5 secondes
            vide.Send(new byte[1] { 0 });
            boucle = false;
        });

        int v, c;
        bool coupEtat = false, firstTime = true;
        while (boucle)
        {
            // on définit que (v, c) si c == 1 alors le joueur décide de sauver, sinon 0
            (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
            if (v == joueurDictateur.GetId())
            {
                switch (c)
                {
                    case 0:
                        coupEtat = false;
                        break;
                    case 1:
                        coupEtat = true;
                        break;
                }
            }
        }

        if (coupEtat)
        {
            boucle = true;
            bool reduceTimer = false, LaunchThread2 = false;
            
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

            Joueur? player = null;
            Joueur? victime = null;
            
            while (boucle)
            {
                (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
                Console.WriteLine(v + " et " + c);
                if (v != -1)
                {
                    player = listJoueurs.Find(j => j.GetId() == c);
                    if (player != null && player.GetRole() is not Dictateur && player.GetEnVie())
                    {
                        victime = player;

                        Console.WriteLine("je check ici");
                    
                        bool alreadyVote = true;
                        if (alreadyVote && !reduceTimer && firstTime)
                        {
                            firstTime = false;
                            LaunchThread2 = true;
                            Task.Run(() =>
                            {
                                Thread.Sleep(GetDelaiAlarme() * 250);
                                Console.WriteLine("le dictateur a voté, ça passe à 5sec d'attente");
                                vide.Send(new byte[1] { 0 });
                                boucle = false;
                            });
                        }
                    }
                    
                }
            }

            if (victime != null)
            {
                victime.SetDoitMourir(true);
                if (victime.GetRole() is Loup)
                {
//                    joueurDictateur.SetEstMaire(true);
                }
                else
                {
                    joueurDictateur.SetDoitMourir(true);
                }
            }
            
        }
    }

    public override int GetIdRole()
    {
        return IdRole;
    }

}