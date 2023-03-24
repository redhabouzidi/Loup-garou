using System.Net.Sockets;
using Server;
namespace LGproject;

public class Cupidon : Role
{
    private new const int IdRole = 2;

    public Cupidon()
    {
        name = "Cupidon";
        description = "blabla";
    }

    public override void Action(List<Joueur> listJoueurs)
    {
        foreach (Joueur j in listJoueurs)
        {
            server.sendTurn(j.GetSocket(), GetIdRole());
        }
        // écrire l'action du Cupidon
        // choix des amoureux
        bool boucle = true;
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
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
                vide.Send(new byte[1] { 0 });
                boucle = false;
            }
        });

        int? idJCupidon = null;

        foreach (Joueur j in listJoueurs)
        {
            if (j.GetRole() is Cupidon)
            {
                idJCupidon = j.GetId();
            }
        }

        int v, c1, c2;
        Joueur? amoureux = null;
        Joueur? amoureux2 = null;
        bool boolAmoureux = false;
        
        Console.WriteLine("Le cupidon va faire son rôle");
        while (boucle)
        {
            (v, c1, c2) = gameVoteCupidon(listJoueurs, GetIdRole(), reveille);
            if (v == idJCupidon)
            {
                boolAmoureux = false;
                amoureux = listJoueurs.Find(j => j.GetId() == c1);
                amoureux2 = listJoueurs.Find(j => j.GetId() == c2);
                if (amoureux != null && amoureux2 != null && amoureux.GetId() != amoureux2.GetId())
                {
                    boolAmoureux = true;
                    if (!reduceTimer && firstTime)
                    {
                        firstTime = false;
                        LaunchThread2 = true;
			foreach(Joueur j in listJoueurs)
			{
				server.sendTime(j.GetSocket(),GetDelaiAlarme()/4);
			}
                        Task.Run(() =>
                        {
                            Thread.Sleep(GetDelaiAlarme() * 250);
                            Console.WriteLine("le Cupidon a voté, ça passe à 5sec d'attente");
                            vide.Send(new byte[1] { 0 });
                            boucle = false;
                        });
                    }
                }
            }
        }

        if (boolAmoureux)
        {
            amoureux.SetAmoureux(amoureux2);
            amoureux2.SetAmoureux(amoureux);
        }
    }

    public override int GetIdRole()
    {
        return IdRole;
    }

    public (int, int, int) gameVoteCupidon(List<Joueur> listJoueurs, int idRole, Socket reveille)
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
                return (-1, -1, -1);
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
                                    return (idVoter, idVoted, idVoted2);
                                }

                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
			int recvSize=sock.Receive(message);
                        server.recvMessageGame(sockets,message,recvSize);

                    }
                }
            }
        }
    }
}
