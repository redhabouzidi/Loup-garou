using Server;
using System.Net.Sockets;
namespace LGproject;

public class Dictateur : Role
{
    private new const int IdRole = 7;
    private bool coupEtatRestant = true;
    public Dictateur()
    {
        name = "Dictateur";
        description = "blabla";
    }

    public override string Action(List<Joueur> listJoueurs)
    { // écrire l'action du loup
        string retour = ""; 
        if (coupEtatRestant)
        {
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
                boucle = false;
                vide.Send(new byte[1] { 0 });
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
                coupEtatRestant = false;
                boucle = true;
                bool reduceTimer = false, LaunchThread2 = false;
                
                Task.Run(() =>
                {
                    Thread.Sleep(GetDelaiAlarme() * 750); // 15 secondes
                    reduceTimer = true;
                    if (reduceTimer && !LaunchThread2)
                    {
                        Thread.Sleep(GetDelaiAlarme() * 250); // 5 secondes
                        boucle = false;
                        vide.Send(new byte[1] { 0 });
                    }
                });

                Joueur? player = null;
                Joueur? victime = null;
                
                while (boucle)
                {
                    (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
                    Console.WriteLine(v + " et " + c);
                    if (v == joueurDictateur.GetId())
                    {
                        player = listJoueurs.Find(j => j.GetId() == c);
                        if (player != null && player.GetRole() is not Dictateur && player.GetEnVie())
                        {
                            victime = player;

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
                    retour = "Enfin, juste avant l’aube, " + joueurDictateur.GetPseudo() + " décide de prendre les armes et de faire un coup d’état organisé. Après réflexion et pour montrer sa bonne foi il décide d’égorger " + victime.GetPseudo() + ". ";
                    victime.SetDoitMourir(true);
                    if (victime.GetRole() is Loup)
                    { 
                        foreach (var j in listJoueurs)
                        {
                            if (j.GetEstMaire())
                            {
                                j.SetEstMaire(false);
                            }
                        }
                        joueurDictateur.SetEstMaire(true);
                        retour = retour + "Quel sauveur ! C’était un loup ! La foule acclama son nouveau dirigeant… ";
                    }
                    else
                    {
                        joueurDictateur.SetDoitMourir(true);
                        retour = retour +
                                 "Quel assassin ! Tuer un innocent de sang-froid, quelle honte ! La foule poignarda ce dictateur… ";
                    }
                }
                else
                {
                    retour = "Enfin, juste avant l’aube, " + joueurDictateur.GetPseudo() + " qui était pourtant décidé à prendre les armes pour renverser le pouvoir loupa son réveil et se rendormi... ";
                }
                
            }
            else
            {
                retour = "Enfin, juste avant l’aube, " + joueurDictateur.GetPseudo() + " hésite à prendre les armes pour tenter de renverser le pouvoir mais juste avant de passer à l’action il décide procrastiner à un autre jour… ";
            }
        }

        return retour;
    }

    public override int GetIdRole()
    {
        return IdRole;
    }

}
