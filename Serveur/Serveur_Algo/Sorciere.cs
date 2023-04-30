﻿using System.Net.Sockets;
using Server;

namespace LGproject;

public class Sorciere : Role
{
    private int potionSoin;
    private int potionKill;
    private int idJoueurVise;
    private new const int IdRole = 5;
    private string recit;

    public Sorciere()
    {
        name = "Sorcière";
        description = "blabla";
        potionSoin = 1;
        potionKill = 1;
    }

    public override string Action(List<Joueur> listJoueurs)
    { // écrire l'action de la sorciere
        string retour;
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

        bool potionHealUse = false, potionKillUse = false;
        idJoueurVise = -1;
        if (potionSoin != 0)
        {
            potionHealUse = PotionVie(listJoueurs, joueurSorciere);
        }

        if (potionKill != 0 && !potionHealUse)
        {
            potionKillUse = PotionMort(listJoueurs, joueurSorciere);
        }

        if (potionHealUse || potionKillUse)
        {
            retour = recit;
            recit = "";
        }
        else
        {
            retour = joueurSorciere.GetPseudo() + ", une sorcière qui passait dans le village reste spectatrice sans intervenir… ";
        }
        return retour;
    }

    public bool PotionVie(List<Joueur> listJoueurs, Joueur? joueurSorciere)
    {
        bool retour = false;
        int v, c;
        for (int i = 0; i < listJoueurs.Count; i++)
        {
            if (listJoueurs[i].GetDoitMourir())
            {
		        Console.WriteLine("Le joueur suivant doit mourir : " + listJoueurs[i].GetPseudo());
                idJoueurVise = listJoueurs[i].GetId();
                // envoieInformation(x,y)
                // fonction "boîte noire" qui envoie l'information que le joueur x a été tué sur la socket y
                sendTurn(listJoueurs,GetIdRole());
                EnvoieInformation(joueurSorciere.GetSocket(), idJoueurVise);
                
                bool boucle = true;

                // on définit une "alarme" qui modifie la valeur du boolean
                // valeur arbitraire => 10 secondes
                Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                reveille.Connect(Game.listener.LocalEndPoint);
                Socket vide;
                vide = Game.listener.Accept();
                sendTime(listJoueurs, GetDelaiAlarme()*375/1000);
                Task t = Task.Run(() =>
                {
                    Thread.Sleep(GetDelaiAlarme() * 375);
                    boucle = false;
                    vide.Send(new byte[1] { 0 });
                });

                Console.WriteLine("On attends la réponse de la sorcière si elle souhaite ressusciter le joueur");
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
                                Console.WriteLine("La sorcière décide de ne pas sauver !");
                                break;
                            case 1:
                                listJoueurs[i].SetDoitMourir(false);
                                Console.WriteLine("La sorcière décide de sauver le joueur !");
                                break;
                        }
                        boucle = false;
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
                recit = joueurSorciere.GetPseudo() + ", une sorcière qui vue la scène décide de sauver la victime en lui administrant un puissant remède. ";
            }
        }

        return retour;
    }

    public bool PotionMort(List<Joueur> listJoueurs, Joueur? joueurSorciere)
    {
        bool retour = false;
        sendTurn(listJoueurs, GetIdRole());
        bool boucle = true;
        bool wantToKill = false;
        // on définit une "alarme" qui modifie la valeur du boolean
        // valeur arbitraire => 10 secondes
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        sendTime(listJoueurs, GetDelaiAlarme()*375/1000);
        Task t = Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 375);
            boucle = false;
            vide.Send(new byte[1] { 0 });
        });
        
        int v, c;
        Console.WriteLine("La sorcière souhaite-elle utiliser sa potion de kill ?");
        while (boucle)
        {
            // on définit que (v, c) si c == 1 alors le joueur décide de sauver, sinon 0
            (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
            if (v == joueurSorciere.GetId())
            {
                switch (c)
                {
                    case 0:
                        wantToKill = false;
                        Console.WriteLine("La sorcière décide de ne pas utiliser sa potion");
                        break;
                    case 1:
                        wantToKill = true;
                        Console.WriteLine("La sorcière décide d'utiliser sa potion de kill !");
                        break;
                }
                boucle = false;
            }
        }

        if (wantToKill)
        {
            bool boucleKill = true;
            bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
            Joueur? cible = null;
            sendTime(listJoueurs, GetDelaiAlarme()/2);
            Task.Run(() =>
            {
                Thread.Sleep(GetDelaiAlarme() * 375); // 7,30 secondes
                reduceTimer = true;
                if (reduceTimer && !LaunchThread2)
                {
                    Thread.Sleep(GetDelaiAlarme() * 125); // 2,30 secondes
                    boucleKill = false;
                    vide.Send(new byte[1] { 0 });
                }
            });
            while (boucleKill)
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
                    
                    if (!reduceTimer && firstTime)
                    {
                        firstTime = false;
                        LaunchThread2 = true;
                            sendTime(listJoueurs, GetDelaiAlarme()/8);
                        Task.Run(() =>
                        {
                            Thread.Sleep(GetDelaiAlarme() * 125);
                            Console.WriteLine("le Garde a voté, ça passe à 2,30sec d'attente");
                            boucleKill = false;
                            vide.Send(new byte[1] { 0 });
                        });
                    }

                    
                }
            }

            if (cible != null && cible.GetDoitMourir())
            {   
                potionKill -= 1;
                retour = true;
                recit = joueurSorciere.GetPseudo() + ", une sorcière qui vue la scène décide de se venger en empoisonnant l’eau de la maison de " + cible.GetPseudo() + ". ";
            }
        }

        return retour;
    }

    public override int GetIdRole()
    {
        return IdRole;
    }
    public void EnvoieInformation(Socket socket,int cible)
    {
        server.EnvoieInformation(socket, cible);
    }
}
