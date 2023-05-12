using System.Net.Sockets;
using Server;

namespace LGproject;

public class Sorciere : Role
{
    private int potionSoin;
    private int potionKill;
    private int idJoueurVise;
    private new const int IdRole = 5;
    private string recit="";
    private string recit_ang="";

    public Sorciere()
    {
        name = "Sorcière";
        description = "blabla";
        potionSoin = 1;
        potionKill = 1;
    }

    /**
        Cette méthode représente l'action du rôle Sorcière pendant une partie de jeu. 
        Elle lui permet d'utiliser soit la potion de vie (s'il lui en reste une) soit la potion de mort (si elle n'a pas utilisé la potion de vie, qu'il lui en reste une et qu'elle souhaite le faire) sur un joueur cible. 
        Le résultat final est décrit en récit sous forme de paire de chaînes de caractères (français,anglais).
    */
    public override (string,string) Action(List<Joueur> listJoueurs,Game game)
    { // écrire l'action de la sorciere
        string retour,retour_ang;
        Console.WriteLine("Appel à la sorcière !");
        Joueur? joueurSorciere = null;

        // On récupère la Socket de la sorcière
        foreach (Joueur j in listJoueurs)
        {
            if (j.GetRole() is Sorciere)
            {
                joueurSorciere = j;
                break;
            }
        }

        bool potionHealUse = false, potionKillUse = false;
        idJoueurVise = -1;
        if (potionSoin != 0)
        {
            potionHealUse = PotionVie(listJoueurs, joueurSorciere,game);
        }

        if (potionKill != 0 && !potionHealUse)
        {
            potionKillUse = PotionMort(listJoueurs, joueurSorciere,game);
        }

        if (potionHealUse || potionKillUse)
        {
            retour = recit;
            retour_ang = recit_ang;
            recit = "";
            recit_ang = "";
        }
        else if(joueurSorciere!=null)
        {
            retour = joueurSorciere.GetPseudo() + ", une sorciere qui passait dans le village reste spectatrice sans intervenir... ";
            retour_ang = joueurSorciere.GetPseudo() + ", a witch who was passing through the village remains a spectator without intervening... ";
        }else{
            retour = "";
            retour_ang="";
        }
        return (retour,retour_ang);
    }

    /**
        Cette méthode permet de gérer l'utilisation de la potion de vie par la sorcière. 
        Elle parcourt la liste de joueurs pour trouver le joueur qui doit mourir lors de ce tour de jeu, puis elle envoie une notification à la sorcière pour qu'elle puisse prendre sa décision. 
        Si elle décide de sauver la victime, la méthode met à jour les informations du joueur. 
        Enfin, la méthode retourne un booléen qui indique si la potion de vie a été utilisée ou non.
    */
    public bool PotionVie(List<Joueur> listJoueurs, Joueur? joueurSorciere,Game game)
    {
        bool retour = false;
        int v, c;
        for (int i = 0; i < listJoueurs.Count; i++)
        {
            if (joueurSorciere!=null && listJoueurs[i].GetDoitMourir())
            {
		        Console.WriteLine("Le joueur suivant doit mourir : " + listJoueurs[i].GetPseudo());
                idJoueurVise = listJoueurs[i].GetId();
                sendTurn(listJoueurs,GetIdRole());
                EnvoieInformation(joueurSorciere.GetSocket(), idJoueurVise);
                
                bool boucle = true;

                // on définit une "alarme" qui modifie la valeur du boolean
                // valeur arbitraire => 10 secondes
                Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if(Game.listener.LocalEndPoint!=null){
                    reveille.Connect(Game.listener.LocalEndPoint);
                }
                Socket vide;
                vide = Game.listener.Accept();
                sendTime(listJoueurs, GetDelaiAlarme()*375/1000,game);
                Task t = Task.Run(() =>
                {
                    Thread.Sleep(GetDelaiAlarme() * 375);
                    boucle = false;
                    vide.Send(new byte[1] { 0 });
                });

                Console.WriteLine("On attends la réponse de la sorcière si elle souhaite ressusciter le joueur");
                while (boucle)
                {
                    // on définit que (v, c) si c == 1 alors le joueur décide de sauver, sinon 0
                    (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
                    if(v==-2 && c== -2){
                        return false;
                    }
                    if (v == joueurSorciere.GetId())
                    {
                        switch (c)
                        {
                            case 0:
                                listJoueurs[i].SetDoitMourir(true);
                                Console.WriteLine("La sorcière décide de ne pas sauver !");
                                break;
                            case 1:
                                listJoueurs[i].SetDoitMourir(false);
                                Console.WriteLine("La sorcière décide de sauver le joueur !");
                                break;
                        }
                        boucle = false;
                    }
                }
            }
        }

        if (idJoueurVise != -1)
        {
            Joueur? cible = listJoueurs.FirstOrDefault(player => player.GetId() == idJoueurVise);
            if (joueurSorciere!=null && cible != null && !cible.GetDoitMourir())
            {
                potionSoin--;
                Messages.sendUseItem(joueurSorciere.GetSocket(),0);
                retour = true;
                recit = joueurSorciere.GetPseudo() + ", une sorciere qui vue la scene decide de sauver la victime en lui administrant un puissant remede. ";
                recit_ang = joueurSorciere.GetPseudo() + ", a witch who saw the scene decides to save the victim using a powerful remedy. ";
            }
        }

        return retour;
    }

    /**
        Cette méthode de la classe Sorcière permet de gérer l'utilisation de la potion de mort par la sorcière. 
        Si la sorcière décide d'utiliser sa potion de mort, la méthode commence une deuxième boucle pour permettre à la sorcière de choisir une cible pour sa potion. 
        Cette boucle s'arrête une fois que la sorcière a choisi une cible ou que le temps imparti est écoulé. 
        Enfin, si la sorcière a réussi à choisir une cible, la méthode applique l'effet de la potion en marquant la cible comme étant morte pour le tour suivant. 
        La méthode renvoie true si la potion a été utilisée avec succès, et false sinon.
    */
    public bool PotionMort(List<Joueur> listJoueurs, Joueur? joueurSorciere,Game game)
    {
        bool retour = false;
        sendTurn(listJoueurs, GetIdRole());
        bool boucle = true;
        bool wantToKill = false;
        // on définit une "alarme" qui modifie la valeur du boolean
        // valeur arbitraire => 10 secondes
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if(Game.listener.LocalEndPoint!=null){
            reveille.Connect(Game.listener.LocalEndPoint);
        }
        Socket vide;
        vide = Game.listener.Accept();
        sendTime(listJoueurs, GetDelaiAlarme()*375/1000,game);
        Task t = Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 375);
            boucle = false;
            vide.Send(new byte[1] { 0 });
        });
        
        int v, c;
        Console.WriteLine("La sorcière souhaite-elle utiliser sa potion de kill ?");
        while (boucle)
        {
            // on définit que (v, c) si c == 1 alors le joueur décide de sauver, sinon 0
            (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
            if(v==-2 && c== -2){
                return false;
            }
            if (joueurSorciere!=null&&v == joueurSorciere.GetId())
            {
                switch (c)
                {
                    case 0:
                        wantToKill = false;
                        Console.WriteLine("La sorcière décide de ne pas utiliser sa potion");
                        break;
                    case 1:
                        wantToKill = true;
                        Console.WriteLine("La sorcière décide d'utiliser sa potion de kill !");
                        break;
                }
                boucle = false;
            }
        }

        if (wantToKill)
        {
            bool boucleKill = true;
            Joueur? cible = null;
            sendTime(listJoueurs, GetDelaiAlarme(),game);
            Task.Run(() =>
            {
                Thread.Sleep(GetDelaiAlarme() * 1000); // 10 secondes
                boucleKill = false;
                vide.Send(new byte[1] { 0 });
            });

            while (boucleKill)
            {
                (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
                if(v==-2 && c== -2){
                    return false;
                }
                if (joueurSorciere!=null&&v == joueurSorciere.GetId() && c != -1)
                {
                    cible = listJoueurs.FirstOrDefault(player => player.GetId() == c);
                    if (cible != null && cible.GetRole() is not Sorciere &&
                        (idJoueurVise == -1 || cible.GetId() != idJoueurVise) && cible.GetEnVie())
                    {
                        cible.SetDoitMourir(true);
                        boucleKill = false;
                    }                    
                }
            }

            if (joueurSorciere!=null&&cible != null && cible.GetDoitMourir())
            {   
                Messages.sendUseItem(joueurSorciere.GetSocket(),1);
                potionKill -= 1;
                retour = true;
                recit = joueurSorciere.GetPseudo() + ", une sorciere qui vue la scene decide de se venger en empoisonnant l'eau de la maison de " + cible.GetPseudo() + ". ";
                recit_ang = joueurSorciere.GetPseudo() + ", a witch who saw the scene decides to take revenge by poisoning the water in "+ cible.GetPseudo() +"'s house. ";
                }
        }

        return retour;
    }

    public override int GetIdRole()
    {
        return IdRole;
    }
    public int GetPotionKill(){
        return potionKill;
    }
    public int GetPotionSoin(){
        return potionSoin;
    }
    public void EnvoieInformation(Socket socket,int cible)
    {
        Messages.EnvoieInformation(socket, cible);
    }
}
