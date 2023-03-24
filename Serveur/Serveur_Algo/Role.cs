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
	Console.WriteLine("ici c'est 1");
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
	Console.WriteLine("ici c'est 2");
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
                reveille.Receive(new byte[1]);
		Console.WriteLine("on va sortir");
                return (-1, -1);
            }
            else
            {
                Console.WriteLine("vote marche");
                foreach (Socket sock in read)
                {
                    int[] size = new int[1] { 1 };
                    byte[] message = new byte[4096];
			int recvSize=sock.Receive(message);	    
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
                            else if (message[0]==0)
                            {
				     server.recvMessageGame(sockets,message,recvSize);
                            }
                        
                    }
                    else
                    {
			
                        server.recvMessageGame(sockets,message,recvSize);
			Console.WriteLine("apres recv2");
                    }
                }
		
            }
	    read.Clear();
        }
	Console.WriteLine("ici c'est la fin de game vote");
        return (-1, -1);
    }

    public abstract int GetIdRole();

    public static int GetDelaiAlarme()
    {
        return delaiAlarme;
    }


}
