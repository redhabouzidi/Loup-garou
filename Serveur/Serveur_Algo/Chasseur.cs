namespace LGproject;
using Server;
using System.Net.Sockets;

public class Chasseur : Role 
{
    private new const int IdRole = 6;
    private Joueur? victime;
    public Chasseur()
    {
        name = "Chasseur";
        description = "blabla";
    }
    
    /**
        Cette méthode représente l'action du rôle Chasseur pendant une partie de jeu. 
        Plus précisément, elle permet au joueur de sélectionner un autre joueur à éliminer en utilisant son fusil. 
        Le résultat final est décrit en récit sous forme de paire de chaînes de caractères (français,anglais).
    */
    public override (string,string) Action(List<Joueur> listJoueurs,Game game)
    { // crire l'action de la Voyante
        string retour,retour_ang;
        sendTurn(listJoueurs, GetIdRole());
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if(Game.listener.LocalEndPoint!=null){
            reveille.Connect(Game.listener.LocalEndPoint);
        }
        Socket vide;
        bool boucle = true;
        vide = Game.listener.Accept();
        sendTime(listJoueurs, GetDelaiAlarme()/2,game);

        Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 500); // 10 secondes
            boucle = false;
            vide.Send(new byte[1] { 0 });
        });

        Joueur? JoueurChasseur = null;

        foreach(Joueur j in listJoueurs) 
        {
            if(j.GetRole() is Chasseur) 
            {
                JoueurChasseur = j;
            }
        }

        int v,c;
        Joueur? player = null;

        Console.WriteLine("Le chasseur executera son rle");
        while(boucle) 
        {
            (v,c) = gameVote(listJoueurs, GetIdRole(), reveille);
            if(v==-2 && c== -2){
                return ("","");
            }
            if(JoueurChasseur!=null && v == JoueurChasseur.GetId()) 
            {
                player = listJoueurs.Find(j => j.GetId() == c);
                if(player != null && player.GetEnVie() && player.GetRole() is not Chasseur) 
                {
                    player.SetDoitMourir(true);
                    boucle = false;
                }     
            }
        }

        if (JoueurChasseur!=null && player != null)
        {
            retour = "Le fusil de " + JoueurChasseur.GetPseudo() +" etait charge à cote de son corps dans son dernier souffle de vie il decide de tirer a bout portant sur " + player.GetPseudo() + ". ";
            retour_ang = JoueurChasseur.GetPseudo() + "'s rifle was loaded next to his body, in his last breath of life he shoots " + player.GetPseudo()+ " closely.";
            victime = player;
        }
        else if(JoueurChasseur!=null )
        {
            retour = "Le fusil de " + JoueurChasseur.GetPseudo() + " etait charge a cote de son corps dans un elan de bont il enleva le chargeur de son arme pour que personne ne se blesse avec ";
            retour_ang = JoueurChasseur.GetPseudo() + "'s rifle was loaded next to his body, in a fit of kindness he removed the charger from his gun so that no one would get hurt with it !";
        }else{
            retour ="";
            retour_ang = "";
        }

        return (retour,retour_ang);
    }

    public override int GetIdRole() 
    {
        return IdRole; 
    }

    public Joueur? GetVictime()
    {
        return victime;
    }
}