using Server;
using System.Net.Sockets;
namespace LGproject;

public class Dictateur : Role
{
    private new const int IdRole = 7;
    private bool coupEtatRestant = true;
    private bool aTueInnocent;
    public Dictateur()
    {
        name = "Dictateur";
        description = "blabla";
    }

    public override string Action(List<Joueur> listJoueurs)
    { // �crire l'action du loup
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
            
            // on d�finit une "alarme" qui modifie la valeur du boolean
            Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            reveille.Connect(Game.listener.LocalEndPoint);
            Socket vide;
            vide = Game.listener.Accept();
            // bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
            sendTime(listJoueurs, GetDelaiAlarme()/4);
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
                // on d�finit que (v, c) si c == 1 alors le joueur d�cide de sauver, sinon 0
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
                sendTime(listJoueurs,GetDelaiAlarme()*3/4);
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
                                    Console.WriteLine("le dictateur a vot�, �a passe � 5sec d'attente");
                                    vide.Send(new byte[1] { 0 });
                                    boucle = false;
                                });
                            }
                        }
                    }
                }
                

                if (victime != null)
                {
                    retour = "Enfin, juste avant l�aube, " + joueurDictateur.GetPseudo() + " d�cide de prendre les armes et de faire un coup d��tat organis�. Apr�s r�flexion et pour montrer sa bonne foi il d�cide d��gorger " + victime.GetPseudo() + ". ";
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
                        retour = retour + "Quel sauveur ! C��tait un loup ! La foule acclama son nouveau dirigeant� ";
                    }
                    else
                    {
                        joueurDictateur.SetDoitMourir(true);
                        aTueInnocent = true;
                        retour = retour +
                                 "Quel assassin ! Tuer un innocent de sang-froid, quelle honte ! La foule poignarda ce dictateur� ";
                    }
                }
                else
                {
                    retour = "Enfin, juste avant l�aube, " + joueurDictateur.GetPseudo() + " qui �tait pourtant d�cid� � prendre les armes pour renverser le pouvoir loupa son r�veil et se rendormi... ";
                }
                
            }
            else
            {
                retour = "Enfin, juste avant l�aube, " + joueurDictateur.GetPseudo() + " h�site � prendre les armes pour tenter de renverser le pouvoir mais juste avant de passer � l�action il d�cide de procrastiner � un autre jour� ";
            }
        }

        return retour;
    }

    public override int GetIdRole()
    {
        return IdRole;
    }

    public bool GetATueInnocent()
    {
        return aTueInnocent;
    }

}
