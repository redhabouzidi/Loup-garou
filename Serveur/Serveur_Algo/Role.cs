using System.Net.Sockets;
using Server;
namespace LGproject;

public abstract class Role
{
    protected string name="";
    protected string? description;
    protected int IdRole;
    // on définit de manière arbitraire 20 secondes pour jouer à chaque rôle
    private const int delaiAlarme = 20;
    public Socket gameListener;
    public override string ToString()
    {
        return name;
    }

    public abstract (string,string) Action(List<Joueur> listJoueurs, Game game);


    public (int, int) gameVote(List<Joueur> listJoueurs, int idRole, Socket reveille)
    {
        Dictionary<Socket, Joueur> dictJoueur = new Dictionary<Socket, Joueur>();
        List<Socket> role = new List<Socket>();
        List<Socket> sockets = new List<Socket>(), read = new List<Socket>();
        Console.WriteLine("vote");
        sockets.Add(reveille);
        Console.WriteLine("ici c'est 1");
        sockets.Add(gameListener);

        foreach (Joueur j in listJoueurs)
        {

            if (j.GetSocket() != null && j.GetSocket().Connected == true)
            {
                sockets.Add(j.GetSocket());
                if (idRole == 253 || idRole == 254)
                {
                    if (j.GetEstMaire())
                    {
                        dictJoueur[j.GetSocket()] = j;
                        role.Add(j.GetSocket());
                    }
                }
                else if (((idRole == 1 || idRole == 255) && j.GetEnVie()))
                {
                    // sockets.Add(j.GetSocket());
                    // if (idRole == 1 || idRole == 255 && j.GetEnVie())
                    // {
                    dictJoueur[j.GetSocket()] = j;
                    role.Add(j.GetSocket());
                    // }

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
            if(role.Count==0){
            return (-2,-2);
            }
            foreach (Socket socket in sockets)
            {
                read.Add(socket);
            }
            Console.WriteLine("bah on attends alors");
            Socket.Select(read, null, null, 500000);
            
            if(read.Count==0){
                return (-1,-1);
            }
            if (read.Contains(reveille))
            {
                Console.WriteLine("on va sortir");
                reveille.Receive(new byte[1]);

                return (-1, -1);
            }
            else if (read.Contains(gameListener))
            {
                Console.WriteLine("joueur se reconnecte");
                gameListener.Receive(new byte[1]);
                return (-1, -1);
            }
            else
            {
                Console.WriteLine("vote marche");
                foreach (Socket sock in read)
                {
                    int[] size = new int[1] { 1 };
                    byte[] encryptedMessage = new byte[4096];
                    try{
                    int recvSize = sock.Receive(encryptedMessage);
                    if(recvSize<=0){
                        throw new SocketException();
                    }
                    
                    List<byte[]> messages = Crypto.DecryptMessage(encryptedMessage, Messages.client_keys[sock], recvSize,sock);
                    if (role.Contains(sock))
                    {
                        foreach(byte[] message in messages){

                        
                        if (message[0] == 1)
                        {
                            Console.WriteLine("ici c'est 5");
                            int idVoter = Messages.decodeInt(message, size);
                            int idVoted = Messages.decodeInt(message, size);
                            Console.WriteLine("idVoter : " + idVoter + " dictJoueur[sock].GetId() : " + dictJoueur[sock].GetId() + "joueur name" + dictJoueur[sock].GetPseudo());
                            if (idVoter == dictJoueur[sock].GetId())
                            {
                                return (idVoter, idVoted);
                            }

                        }
                        else
                        {
                            if ((message[0] == 0 && (idRole == 1 || idRole == 255)) || (message[0] == 20 && idRole == 4)||(idRole == 255 && message[0]==16))
                            {
                                Messages.recvMessageGame(role, message, message.Length);
                            }
                        }
                        }
                    }
                    else
                    {
                        foreach(byte[] message in messages){
                        if ((message[0] == 0 && (idRole == 1 || idRole == 255)) || (message[0] == 20 && idRole == 4))
                        {
                            Messages.recvMessageGame(role, message, message.Length);
                        }
                        }
                        
                        Console.WriteLine("apres recv2");
                    }
                    }catch(SocketException e){
                        sockets.Remove(sock);
                        foreach (Joueur joueur in listJoueurs)
                        {
                            if (joueur.GetSocket() == sock)
                            {
                                Messages.userData[joueur.GetId()].SetStatus(joueur.GetId(), -1);
                                Messages.userData.Remove(joueur.GetId());
                                joueur.SetSocket(null);
                                sock.Close();
                                return (-1, -1);
                            }
                        }
                    }
                }
                

            }
            read.Clear();
        }
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
                    Messages.sendVote(j.GetSocket(), vote, voted);
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
                Messages.sendTurn(j.GetSocket(), idRole);
        }
    }
    public void sendTime(List<Joueur> listJoueurs, int time, Game game)
    {
        game.currentTime = time;
        game.t = DateTime.Now;
        foreach (Joueur j in listJoueurs)
        {
            if (j.GetSocket() != null && j.GetSocket().Connected)
                Messages.sendTime(j.GetSocket(), time);
        }
    }

}
