namespace LGproject;
using Server;
using System.Net.Sockets;

public class Garde : Role
{
    private new const int IdRole = 8;
    private int Idsave = -1;

    public Garde()
    {
        name = "Garde";
        description = "blabla";
    }
    
    public override string Action(List<Joueur> listJoueurs)
    { // écrire l'action de la Voyante
        string retour;
        sendTurn(listJoueurs, GetIdRole());
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        bool boucle = true;
        vide = Game.listener.Accept();
        foreach (Joueur j in listJoueurs)
        {
            server.sendTime(j.GetSocket(), GetDelaiAlarme());
        }

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
                if((player != null && player.GetEnVie() && player.GetId() != Idsave)) {
                    player.SetAEteSave(true);
                    if (!reduceTimer && firstTime)
                    {
                        firstTime = false;
                        LaunchThread2 = true;
                        Task.Run(() =>
                        {
                            Thread.Sleep(GetDelaiAlarme() * 250);
                            Console.WriteLine("le Garde a voté, ça passe à 5sec d'attente");
                            boucle = false;
                            vide.Send(new byte[1] { 0 });
                        });
                    }
                }

            }
        }
        if(player != null) {
            Idsave = player.GetId();
            retour = "Pendant ce temps-là, " + JoueurGarde.GetPseudo() + " le chevalier de la garde du village, décide de rester défendre la maison de " + player.GetPseudo() + " cette nuit. ";
        } 
        else
        {
            Idsave = -1;
            retour = "Pendant ce temps-là, " + JoueurGarde.GetPseudo() + " le chevalier de la garde du village, décide de passer la nuit tranquillement et ne défendra personne cette nuit… ";
        }

        return retour;
    }

    public override int GetIdRole() {
        return IdRole; 
    }
}