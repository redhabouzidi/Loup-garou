namespace LGproject;
using Server;
using System.Net.Sockets;

public class Garde : Role
{
    private new const int IdRole = 8;
    public Garde()
    {
        name = "Garde";
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

        Joueur? JoueurGarde = null;

        foreach(Joueur j in listJoueurs) {
            if(j.GetRole() is Garde) {
                JoueurGarde = j;
            }
        }

        int v,c;
        Joueur? player = null;

        Console.WriteLine("Le Garde executera on rôle");
        while(boucle) {
            (v,c) = gameVote(listJoueurs, GetIdRole(), reveille);
            if(v == JoueurGarde.GetId()) {
                if(player != null) {
                    player.SetAEteSave(false);
                }
                player = listJoueurs.Find(j => j.GetId() == c);
                if(player != null && player.GetEnVie()) {
                    player.SetAEteSave(true);
                    if (!reduceTimer && firstTime)
                    {
                        firstTime = false;
                        LaunchThread2 = true;
                        Task.Run(() =>
                        {
                            Thread.Sleep(GetDelaiAlarme() * 250);
                            Console.WriteLine("le Garde a voté, ça passe à 5sec d'attente");
                            vide.Send(new byte[1] { 0 });
                            boucle = false;
                        });
                    }
                }

            }
        }
    }

    public override int GetIdRole() {
        return IdRole; 
    }
}