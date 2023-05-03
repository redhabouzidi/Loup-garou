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
    public Socket gameListener;
    public override string ToString()
    {
        return name;
    }

    public abstract string Action(List<Joueur> listJoueurs);


    public (int, int) gameVote(List<Joueur> listJoueurs, int idRole, Socket reveille)
    {
        Dictionary<Socket, Joueur> dictJoueur = new Dictionary<Socket, Joueur>();
        List<Socket> role = new List<Socket>();
        List<Socket> sockets = new List<Socket>(), read = new List<Socket>();
        Console.WriteLine("vote");
        sockets.Add(reveille);
        Console.WriteLine("ici c'est 1");
        sockets.Add(this.gameListener);

        foreach (Joueur j in listJoueurs)
        {

            if (j.GetSocket() != null && j.GetSocket().Connected == true)
            {
                sockets.Add(j.GetSocket());
                if (idRole == 1 || idRole == 255 && j.GetEnVie())
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
        }
        Console.WriteLine("ici c'est 2");
        Console.WriteLine(sockets.Count);
        while (true)
        {
            foreach (Socket socket in sockets)
            {
                read.Add(socket);
            }
            Console.WriteLine("bah on attends alors");
            Socket.Select(read, null, null, -1);
            Console.WriteLine("ici c'est 3");
            if (read.Contains(reveille))
            {
                Console.WriteLine("on va sortir");
                reveille.Receive(new byte[1]);

                return (-1, -1);
            }
            else if (read.Contains(this.gameListener))
            {
                Console.WriteLine("joueur se reconnecte");
                this.gameListener.Receive(new byte[1]);
                return (-1, -1);
            }
            else
            {
                Console.WriteLine("vote marche");
                foreach (Socket sock in read)
                {
                    if (sock.Available == 0)
                    {

                        sockets.Remove(sock);
                        foreach (Joueur j in listJoueurs)
                        {
                            if (j.GetSocket() == sock)
                            {
                                server.userData[j.GetId()].SetStatus(j.GetId(), -1);
                                server.userData.Remove(j.GetId());
                                j.SetSocket(null);
                                sock.Close();
                                return (-1, -1);
                            }
                        }
                    }
                    int[] size = new int[1] { 1 };
                    byte[] encryptedMessage = new byte[4096];
                    int recvSize = sock.Receive(encryptedMessage);
                    byte[] message = Crypto.DecryptMessage(encryptedMessage, server.client_keys[sock], recvSize);
                    recvSize=message.Length;
                    if (role.Contains(sock))
                    {

                        if (message[0] == 1)
                        {
                            Console.WriteLine("ici c'est 5");
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
                            server.recvMessageGame(sockets, message, recvSize);
                        }

                    }
                    else
                    {

                        server.recvMessageGame(sockets, message, recvSize);
                        Console.WriteLine("apres recv2");
                    }
                }

            }
            read.Clear();
        }
        Console.WriteLine("ici c'est la fin de game vote");
        return (-1, -1);
    }
    public void SendVote(List<Joueur> listJoueur, int vote, int voted, int idRole)
    {
        foreach (Joueur j in listJoueur)
        {
            //6 pour le dictateur ???
            if (j.GetSocket() != null && j.GetSocket().Connected)
            {
                if (idRole == 1 || idRole == 6 || j.GetRole().GetIdRole() == idRole)
                {
                    server.sendVote(j.GetSocket(), vote, voted);
                }

            }
        }
    }
    public abstract int GetIdRole();

    public static int GetDelaiAlarme()
    {
        return delaiAlarme;
    }
    public void sendTurn(List<Joueur> listJoueurs, int idRole)
    {
        foreach (Joueur j in listJoueurs)
        {
            if (j.GetSocket() != null && j.GetSocket().Connected)
                server.sendTurn(j.GetSocket(), idRole);
        }
    }
    public void sendTime(List<Joueur> listJoueurs, int time)
    {
        Console.WriteLine("in");
        foreach (Joueur j in listJoueurs)
        {
            if (j.GetSocket() != null && j.GetSocket().Connected)
                server.sendTime(j.GetSocket(), time);
        }
        Console.WriteLine("out");
    }

}
