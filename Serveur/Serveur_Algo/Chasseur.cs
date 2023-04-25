namespace LGproject;
using Server;
using System.Net.Sockets;

public class Chasseur : Role 
{
    private new const int IdRole = 6;
    private Joueur? victime;
    public Chasseur()
    {
        name = "Chasseur";
        description = "blabla";
    }
    
    public override string Action(List<Joueur> listJoueurs)
    { // crire l'action de la Voyante
        string retour;
        sendTurn(listJoueurs, GetIdRole());
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        bool boucle = true;
        vide = Game.listener.Accept();
        sendTime(listJoueurs, GetDelaiAlarme());

        bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
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

        Joueur? JoueurChasseur = null;

        foreach(Joueur j in listJoueurs) 
        {
            if(j.GetRole() is Chasseur) 
            {
                JoueurChasseur = j;
            }
        }

        int v,c;
        Joueur? player = null;

        Console.WriteLine("Le chasseur executera son rle");
        while(boucle) 
        {
            (v,c) = gameVote(listJoueurs, GetIdRole(), reveille);
            if(v == JoueurChasseur.GetId()) 
            {     
                if(player != null) {
                    player.SetDoitMourir(false);
                }
                player = listJoueurs.Find(j => j.GetId() == c);
                if(player != null && player.GetEnVie() && player.GetRole() is not Chasseur) 
                {
                    player.SetDoitMourir(true);
                    if (!reduceTimer && firstTime)
                    {
                        firstTime = false;
                        LaunchThread2 = true;
                        Task.Run(() =>
                        {
                            Thread.Sleep(GetDelaiAlarme() * 250);
                            Console.WriteLine("le Chasseur a vot, a passe  5sec d'attente");
                            boucle = false;
                            vide.Send(new byte[1] { 0 });
                        });
                    }
                }     
            }
        }

        if (player != null)
        {
            retour = "Le fusil de " + JoueurChasseur.GetPseudo() +" tait charg  ct de son corps dans son dernier souffle de vie il dcide de tirer  bout portant sur " + player.GetPseudo() + ". ";
            victime = player;
        }
        else
        {
            retour = "Le fusil de " + JoueurChasseur.GetPseudo() + " tait charg  ct de son corps dans un lan de bont il enleva le chargeur de son arme pour que personne ne se blesse avec ";
        }

        return retour;
    }

    public override int GetIdRole() 
    {
        return IdRole; 
    }

    public Joueur? GetVictime()
    {
        return victime;
    }
}