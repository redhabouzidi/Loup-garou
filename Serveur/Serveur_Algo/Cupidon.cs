using Server;
using System.Net.Sockets;
namespace LGproject;

public class Cupidon : Role
{
    private new const int IdRole = 2;
    public Cupidon()
    {
        name = "Cupidon";
        description = "blabla";
    }
    public (int, int,int) gameVote(List<Joueur> listJoueurs, Socket reveille)
    {
        Dictionary<Socket, Joueur> dictJoueur = new Dictionary<Socket, Joueur>();
        List<Socket> role = new List<Socket>();
        List<Socket> sockets = new List<Socket>(), read = new List<Socket>();
        sockets.Add(reveille);

        foreach (Joueur j in listJoueurs)
        {
            sockets.Add(j.GetSocket());
            if (j.GetRole().GetIdRole() == this.GetIdRole())
            {
                dictJoueur[j.GetSocket()] = j;
                role.Add(j.GetSocket());
            }

        }
        while (true)
        {
            foreach (Socket socket in sockets)
            {
                read.Add(socket);
            }
            Socket.Select(read, null, null, -1);
            if (read.Contains(reveille))
            {
                return (-1, -1,-1);
            }
            else
            {
                foreach (Socket sock in read)
                {
                    int[] size = new int[1] { 1 };
                    byte[] message = new byte[4096];
                    if (role.Contains(sock))
                    {
                        if (sock.Available == 0)
                        {
                            //gestion de deconnexion ???
                        }
                        else
                        {
                            sock.Receive(message);
                            if (message[0] == 6)
                            {

                                int idVoter = server.decodeInt(message, size);
                                int idVoted = server.decodeInt(message, size);
                                int idVoted2 = server.decodeInt(message, size);
                                if (idVoter == dictJoueur[sock].GetId())
                                {
                                    return (idVoter, idVoted,idVoted2);
                                }

                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        server.recvMessageGame(sock, sockets);

                    }
                }
            }
        }
    }
    public override void Action(List<Joueur> listJoueurs)
    { // écrire l'action du Cupidon
        // choix des amoureux
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


        int? idJCupidon = null;
        int[] couple = new int[2] { -1, -1 };
        foreach (Joueur j in listJoueurs)
        {
            if (j.GetRole() is Cupidon)
            {
                idJCupidon = j.GetId();
            }
        }
        int v1, v2, c1, c2;
        Joueur? player = null;
        Joueur? player2 = null;
        while (boucle)
        {
            (v1, c1) = gameVote(listJoueurs,reveille) ;
            if (v1 == idJCupidon)
            {
                player = listJoueurs.Find(j => j.GetId() == c1);
                if (player != null)
                {
                    couple[0] = c1;

                }
            }
        }
        Joueur? amoureux1 = listJoueurs.Find(j => j.GetId() == couple[0]);
        Joueur? amoureux2 = listJoueurs.Find(j => j.GetId() == couple[1]);
        amoureux1.SetAmoureux(amoureux2);
        amoureux2.SetAmoureux(amoureux1);
    }

    public override int GetIdRole()
    {
        return IdRole;
    }
}