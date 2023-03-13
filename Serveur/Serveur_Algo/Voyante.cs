using System.Net.Sockets;
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
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        bool boucle = true;
        vide = Game.listener.Accept();
        Task t = Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 1000);
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
                        revelerRole(JoueurVoyante.GetSocket(), player.GetId(), player.GetRole().GetIdRole());

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