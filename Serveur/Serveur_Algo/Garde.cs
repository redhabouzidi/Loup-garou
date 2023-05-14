namespace LGproject;
using Server;
using System.Net.Sockets;

public class Garde : Role
{
    private new const int IdRole = 8;
    private int Idsave = -1;

    public Garde()
    {
        name = "Garde";
        description = "blabla";
    }
    
    /**
        Cette méthode représente l'action du Garde dans le jeu. 
        Celle-ci permet au joueur qui a le rôle de Garde de choisir un autre joueur à protéger pendant la nuit, ce qui le rendra immortel à l'attaque des Loups pour cette nuit seulement. 
        Le résultat final est décrit en récit sous forme de paire de chaînes de caractères (français,anglais).
    */
    public override (string,string) Action(List<Joueur> listJoueurs,Game game)
    { // écrire l'action de la Voyante
        string retour,retour_ang;
        sendTurn(listJoueurs, GetIdRole());
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if(Game.listener.LocalEndPoint!=null){
            reveille.Connect(Game.listener.LocalEndPoint);
        }
        Socket vide;
        bool boucle = true;
        vide = Game.listener.Accept();

            sendTime(listJoueurs,GetDelaiAlarme(),game);
        

        bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
        Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 750); // 15 secondes
            reduceTimer = true;
            if (reduceTimer && !LaunchThread2)
            {
                Thread.Sleep(GetDelaiAlarme() * 250); // 5 secondes
                boucle = false;
                vide.Send(new byte[1] { 0 });
            }
        });

        Joueur? JoueurGarde = null;

        foreach(Joueur j in listJoueurs) {
            if(j.GetRole() is Garde) {
                JoueurGarde = j;
            }
        }

        int v,c;
        Joueur? player = null;

        Console.WriteLine("Le Garde executera on rôle");
        while(boucle) {
            (v,c) = gameVote(listJoueurs, GetIdRole(), reveille);
            if(v==-2 && c== -2){
                return ("","");
            }
            if(JoueurGarde!=null&&v == JoueurGarde.GetId()) {
                if(player != null) {
                    player.SetAEteSave(false);
                }
                player = listJoueurs.Find(j => j.GetId() == c);
                if((player != null && player.GetEnVie() && player.GetId() != Idsave)) {
                    player.SetAEteSave(true);
                    if (!reduceTimer && firstTime)
                    {
                        firstTime = false;
                        LaunchThread2 = true;
                        Task.Run(() =>
                        {
                            Thread.Sleep(GetDelaiAlarme() * 250);
                            Console.WriteLine("le Garde a voté, ça passe à 5sec d'attente");
                            boucle = false;
                            vide.Send(new byte[1] { 0 });
                        });
                    }
                }

            }
        }
        if(JoueurGarde!=null&&player != null) {
            Idsave = player.GetId();
            retour = "Pendant ce temps-la, " + JoueurGarde.GetPseudo() + " le chevalier de la garde du village, decide de rester defendre la maison de " + player.GetPseudo() + " cette nuit. ";
            retour_ang = "Meanwhile, "+ JoueurGarde.GetPseudo() +" the knight of the village, decides to defend "+ player.GetPseudo() +"'s house that night.";
        } 
        else if(JoueurGarde!=null)
        {
            Idsave = -1;
            retour = "Pendant ce temps-la, " + JoueurGarde.GetPseudo() + " le chevalier de la garde du village, decide de passer la nuit tranquillement et ne defendra personne cette nuit... ";
            retour_ang = "Meanwhile, "+ JoueurGarde.GetPseudo() +" the knight of the village, decides to spend the night quietly and will not defend anyone this night... ";
        }else{
            Idsave=-1;
            retour="";
            retour_ang="";
        }

        return (retour,retour_ang);
    }

    public override int GetIdRole() {
        return IdRole; 
    }
}