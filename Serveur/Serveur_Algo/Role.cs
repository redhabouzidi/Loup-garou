using System.Net.Sockets;
using Server;
namespace LGproject;

public abstract class Role
{
    
    protected string? name;
    protected string? description;
    protected int IdRole;
    // on définit de manière arbitraire 20 secondes pour jouer à chaque rôle
    private const int delaiAlarme = 3;

    public override string ToString()
    {
        return name;
    }

    public abstract void Action(List<Joueur> listJoueurs);


    public (int, int) gameVote(List<Joueur> listJoueurs, Socket reveille)
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
            foreach(Socket socket in sockets)
            {
                read.Add(socket);
            }
            Socket.Select(read, null, null, -1);
            if (read.Contains(reveille))
            {
                return (-1, -1);
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
                            if (message[0] == 1)
                            {

                                int idVoter = server.decodeInt(message, size);
                                int idVoted = server.decodeInt(message, size);
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
    }

    public abstract int GetIdRole();

    public int GetDelaiAlarme()
    {
        return delaiAlarme;
    }

}