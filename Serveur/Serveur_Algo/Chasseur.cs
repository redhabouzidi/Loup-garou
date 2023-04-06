namespace LGproject;
using Server;
using System.Net.Sockets;

public class Chasseur : Role 
{
    private new const int IdRole = 6;
    public Chasseur()
    {
        name = "Chasseur";
        description = "blabla";
    }
    
    public override void Action(List<Joueur> listJoueurs)
    { // écrire l'action de la Voyante
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
                vide.Send(new byte[1] { 0 });
                boucle = false;
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

        Console.WriteLine("Le chasseur executera son rôle");
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
                            Console.WriteLine("le Chasseur a voté, ça passe à 5sec d'attente");
                            vide.Send(new byte[1] { 0 });
                            boucle = false;
                        });
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