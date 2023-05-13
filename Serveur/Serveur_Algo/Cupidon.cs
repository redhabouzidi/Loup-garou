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

    /**
        Cette méthode définit l'action du rôle Cupidon dans le jeu. 
        Le but de Cupidon est de faire tomber deux joueurs amoureux l'un de l'autre. 
        Si Cupidon a réussi à faire tomber deux joueurs amoureux, les joueurs sont mis en couple. 
        Le résultat final est décrit en récit sous forme de paire de chaînes de caractères (français,anglais).
    */
    public override (string,string) Action(List<Joueur> listJoueurs,Game game)
    {
        string retour,retour_ang;
        sendTurn(listJoueurs, GetIdRole());
        // écrire l'action du Cupidon
        // choix des amoureux
        bool boucle = true;
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if(Game.listener.LocalEndPoint!=null){
            reveille.Connect(Game.listener.LocalEndPoint);
        }
        Socket vide;
        vide = Game.listener.Accept();
        sendTime(listJoueurs, GetDelaiAlarme(),game);
        bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
        Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 750); // 15 secondes
            Console.WriteLine("on viens d'attendre 15 s");
            reduceTimer = true;
            if (reduceTimer && !LaunchThread2)
            {
                Thread.Sleep(GetDelaiAlarme() * 250); // 5 secondes
                boucle = false;
                vide.Send(new byte[1] { 0 });
            }
        });

        Joueur? JoueurCupidon = null;

        foreach (Joueur j in listJoueurs)
        {
            if (j.GetRole() is Cupidon)
            {
                JoueurCupidon = j;
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
            if(v==-2&&c1==-2&&c2==-2){
                return ("","");
            }
            if (JoueurCupidon!=null && v == JoueurCupidon.GetId())
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
                        sendTime(listJoueurs, GetDelaiAlarme()/4,game);
                        Task.Run(() =>
                        {
                            Thread.Sleep(GetDelaiAlarme() * 250);
                            Console.WriteLine("le Cupidon a voté, ça passe à 5sec d'attente");
                            boucle = false;
                            vide.Send(new byte[1] { 0 });
                        });
                    }
                }
            }
        }
        Console.WriteLine("on sort de la boucle");
        if (amoureux!=null && amoureux2!=null && JoueurCupidon!=null && boolAmoureux)
        {
            amoureux.SetAmoureux(amoureux2);
            amoureux2.SetAmoureux(amoureux);
            Console.WriteLine(amoureux.GetId()+" amoureux "+amoureux2.GetId());
            retour = JoueurCupidon.GetPseudo() +
                     " senti des ailes pousser a son dos, un arc et deux fleches l'attendaient sur la table de son salon. Par curiosite, il decide de tirer les fleches sur " +
                     amoureux.GetPseudo() + " et " + amoureux2.GetPseudo() +
                     " qui tomberent fou amoureux l'un de l'autre. ";
            retour_ang = JoueurCupidon.GetPseudo() + " felt wings growing on his back, a bow and two arrows were waiting for him on his living room table. Out of curiosity, he decided to shoot the arrows at "+amoureux.GetPseudo()+" and "+amoureux2.GetPseudo()+" who fell madly in love with each other.";
       	    Messages.setLovers(amoureux.GetSocket(),amoureux2.GetSocket(),amoureux.GetId(),amoureux2.GetId(),amoureux.GetRole().GetIdRole(),amoureux2.GetRole().GetIdRole());
       	}
        else if (JoueurCupidon!=null)
        {
            retour = JoueurCupidon.GetPseudo() + " senti des ailes pousser a son dos, un arc et deux fleches l'attendaient sur la table de son salon. Par curiosite, il tente de tirer mais ses fleches tombent toutes les deux dans la riviere du village... ";
            retour_ang = JoueurCupidon.GetPseudo()+ " felt wings growing on his back, a bow and two arrows were waiting for him on his living room table. Out of curiosity, he tried to shoot but his arrows both fell into the village river... ";
        }else{
            retour = "";
            retour_ang="";
        }

        return (retour,retour_ang);
    }

    public override int GetIdRole()
    {
        return IdRole;
    }
    public (int, int,int) gameVoteCupidon(List<Joueur> listJoueurs, int idRole, Socket reveille)
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
            if (j.GetSocket() != null && j.GetSocket().Connected)
            {
                sockets.Add(j.GetSocket());
                if (j.GetRole().GetIdRole() == idRole && j.GetEnVie())
                {
                    dictJoueur[j.GetSocket()] = j;
                    role.Add(j.GetSocket());
                }
            }
        }
        if(role.Count==0){
            return (-2,-2,-2);
        }
        Console.WriteLine("ici c'est 2");
        
        while (true)
        {
            foreach (Socket socket in sockets)
            {
                Console.WriteLine(sockets.Count);
                read.Add(socket);
            }
            Console.WriteLine("bah on attends alors");
            Socket.Select(read, null, null, -1);
            Console.WriteLine("ici c'est 3");
            if (read.Contains(reveille))
            {
                Console.WriteLine("on va sortir");
                reveille.Receive(new byte[1]);

                return (-1, -1,-1);
            }
            else if (read.Contains(this.gameListener))
            {
                Console.WriteLine("joueur se reconnecte");
                this.gameListener.Receive(new byte[1]);
                return (-1, -1,-1);
            }
            else
            {
                Console.WriteLine("vote marche");
                foreach (Socket sock in read)
                {
                    try{
                    int[] size = new int[1] { 1 };
                    byte[] encryptedMessage = new byte[4096];
                    int recvSize = sock.Receive(encryptedMessage);
                    if(recvSize<=0){
                        throw new SocketException();
                    }
                    byte[] message = Crypto.DecryptMessage(encryptedMessage, Messages.client_keys[sock], recvSize);
                    recvSize=message.Length;

                    if (role.Contains(sock))
                    {
                            if (message[0] == 6)
                            {

                                int idVoter = Messages.decodeInt(message, size);
                                int idVoted = Messages.decodeInt(message, size);
                                int idVoted2 = Messages.decodeInt(message, size);
                                if (idVoter == dictJoueur[sock].GetId())
                                {
                                    return (idVoter, idVoted, idVoted2);
                                }

                            }
                        
                        else
                        {
                            if((message[0]==0&&(idRole==1||idRole==255))||(message[0]==20 && idRole == 4)){
				            Messages.recvMessageGame(sockets,message,recvSize);
                            }
                        }

                    }
                    else
                    {

                        if((message[0]==0&&(idRole==1||idRole==255))||(message[0]==20 && idRole == 4)){
				            Messages.recvMessageGame(sockets,message,recvSize);
                        }
                    }
                    }catch(SocketException e){
                        sockets.Remove(sock);
                        foreach (Joueur j in listJoueurs)
                        {
                            if (j.GetSocket() == sock)
                            {
                                Messages.userData[j.GetId()].SetStatus(j.GetId(), -1);
                                Messages.userData.Remove(j.GetId());
                                j.SetSocket(null);
                                sock.Close();
                                return (-1, -1,-1);
                            }
                        }
                    }
                }
            }
            read.Clear();
        }
    }
}