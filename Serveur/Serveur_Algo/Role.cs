using System.Net.Sockets;
using Server;
namespace LGproject;

public abstract class Role
{
    protected string? name;
    protected string? description;
    protected int IdRole;
    // on définit de manière arbitraire 20 secondes pour jouer à chaque rôle
    private const int delaiAlarme = 20;

    public override string ToString()
    {
        return name;
    }

    public abstract void Action(List<Joueur> listJoueurs);


    public static (int, int) gameVote(List<Joueur> listJoueurs, int idRole, Socket reveille)
    {
        Dictionary<Socket, Joueur> dictJoueur = new Dictionary<Socket, Joueur>();
        List<Socket> role = new List<Socket>();
        List<Socket> sockets = new List<Socket>(), read = new List<Socket>();
        Console.WriteLine("vote");
        sockets.Add(reveille);
        foreach (Joueur j in listJoueurs)
        {
            sockets.Add(j.GetSocket());
            if (idRole == 1 && j.GetEnVie())
            {
                dictJoueur[j.GetSocket()] = j;
                role.Add(j.GetSocket());
            }
            else if (j.GetRole().GetIdRole() == idRole && j.GetEnVie())
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
                reveille.Receive(new byte[1]);
                return (-1, -1);
            }
            else
            {
                Console.WriteLine("vote marche");
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
                            if (message[0] == 1)
                            {
                                int idVoter = server.decodeInt(message, size);
                                int idVoted = server.decodeInt(message, size);
                                Console.WriteLine("idVoter : " + idVoter + " dictJoueur[sock].GetId() : " + dictJoueur[sock].GetId() + "joueur name" + dictJoueur[sock].GetPseudo());
                                if (idVoter == dictJoueur[sock].GetId())
                                {
                                    return (idVoter, idVoted);
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
        return (-1, -1);
    }

    public abstract int GetIdRole();

    public static int GetDelaiAlarme()
    {
        return delaiAlarme;
    }

}