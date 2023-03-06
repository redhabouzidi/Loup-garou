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

        Console.WriteLine("appel a la voyante");
        bool boucle = true;
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        Task t = Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 1000);
            vide.Send(new byte[1] { 0 });
            boucle = false;
        });

        int? idJVoyante = null;
        int? aVoircarte = null;

        foreach (Joueur j in listJoueurs)
        {
            if (j.GetRole() is Voyante)
            {
                idJVoyante = j.GetId();
            }
        }

        int v, c;
        Joueur? player = null;
        while (boucle)
        {
            (v, c) = gameVote(listJoueurs,reveille);
            if (v == idJVoyante)
            {
                player = listJoueurs.Find(j => j.GetId() == c);
                if (player != null)
                {
                    if (player.GetRole() is not Voyante && player.GetEnVie())
                    {
                        aVoircarte = player.GetId();
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