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

    public override string Action(List<Joueur> listJoueurs)
    { // écrire l'action de la Voyante
        string retour;
        sendTurn(listJoueurs, GetIdRole());
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        bool boucle = true;
        vide = Game.listener.Accept();
        sendTime(listJoueurs, GetDelaiAlarme());
        Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 1000); // 20 secondes
            vide.Send(new byte[1] { 0 });
            boucle = false;
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

                        boucle = false;
                    }
                }
            }
        }

        if (player != null)
        {
            retour = JoueurVoyante.GetPseudo() + " se met à regarder dans sa boule de cristal… elle voit… " + player.GetPseudo() + " qui s’avère être " + player.GetRole() + ". ";
        }
        else
        {
            retour = JoueurVoyante.GetPseudo() +
                     " se met à regarder dans sa boule de cristal… elle ne voit rien de spécial ce soir-là… ";
        }

        return retour;
    }

    public override int GetIdRole()
    {
        return IdRole;
    }
}
