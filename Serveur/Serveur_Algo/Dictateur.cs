using Server;
using System.Net.Sockets;
namespace LGproject;

public class Dictateur : Role
{
    private new const int IdRole = 7;
    private bool coupEtatRestant = true;
    private bool aTueInnocent;
    public Dictateur()
    {
        name = "Dictateur";
        description = "blabla";
    }

    /**
        Cette méthode définit l'action du rôle Dictateur dans le jeu. 
        Si le Dictateur a encore le coup d'État disponible, il peut décider de l'utiliser pour éliminer un joueur de son choix. 
        Dans ce cas, le Dictateur doit voter pour la victime. 
        Si la victime est un Loup, le Dictateur devient alors le nouveau maire de la ville. 
        Si la victime est un innocent, le Dictateur sera lui-même éliminé par la foule. 
        Le résultat final est décrit en récit sous forme de paire de chaînes de caractères (français,anglais).
    */
    public override (string,string) Action(List<Joueur> listJoueurs,Game game)
    { // �crire l'action du loup
        string retour = "";
        string retour_ang = ""; 
        if (coupEtatRestant)
        {
            Joueur? joueurDictateur = null;
            foreach (Joueur j in listJoueurs)
            {
                Messages.sendTurn(j.GetSocket(), GetIdRole());
                if (j.GetRole() is Dictateur)
                {
                    joueurDictateur = j;
                }
            }
            bool boucle = true;
            
            // on d�finit une "alarme" qui modifie la valeur du boolean
            Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if(Game.listener.LocalEndPoint!=null){
                reveille.Connect(Game.listener.LocalEndPoint);
            }
            Socket vide;
            vide = Game.listener.Accept();
            // bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
            sendTime(listJoueurs, GetDelaiAlarme()/4,game);
            Task.Run(() =>
            {
                Thread.Sleep(GetDelaiAlarme() * 250); // 5 secondes
                boucle = false;
                vide.Send(new byte[1] { 0 });
            });

            int v, c;
            bool coupEtat = false, firstTime = true;
            while (boucle)
            {
                // on d�finit que (v, c) si c == 1 alors le joueur d�cide de sauver, sinon 0
                (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
                if(v==-2 && c== -2){
                    return ("","");
                }
                if (joueurDictateur!=null&&v == joueurDictateur.GetId())
                {
                    switch (c)
                    {
                        case 0:
                            coupEtat = false;
                            break;
                        case 1:
                            coupEtat = true;
                            break;
                    }
                }
            }

            if (coupEtat)
            {
                coupEtatRestant = false;
                boucle = true;
                bool reduceTimer = false, LaunchThread2 = false;
                sendTime(listJoueurs,GetDelaiAlarme()*3/4,game);
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

                Joueur? player = null;
                Joueur? victime = null;
                
                while (boucle)
                {
                    (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
                    Console.WriteLine(v + " et " + c);
                    if (joueurDictateur!=null && v == joueurDictateur.GetId())
                    {
                        player = listJoueurs.Find(j => j.GetId() == c);
                        if (player != null && player.GetRole() is not Dictateur && player.GetEnVie())
                        {
                            victime = player;

                            bool alreadyVote = true;
                            if (alreadyVote && !reduceTimer && firstTime)
                            {
                                firstTime = false;
                                LaunchThread2 = true;
                                sendTime(listJoueurs, GetDelaiAlarme() / 4,game);
                                Task.Run(() =>
                                {
                                    Thread.Sleep(GetDelaiAlarme() * 250);
                                    Console.WriteLine("le dictateur a vot�, �a passe � 5sec d'attente");
                                    vide.Send(new byte[1] { 0 });
                                    boucle = false;
                                });
                            }
                        }
                    }
                }
                

                if (joueurDictateur!=null && victime != null)
                {
                    retour = "Enfin, juste avant l'aube, " + joueurDictateur.GetPseudo() + " decide de prendre les armes et de faire un coup d'etat organise. Apres reflexion et pour montrer sa bonne foi il decide degorger " + victime.GetPseudo() + ". ";
                    retour_ang = "Finally, just before dawn, "+ joueurDictateur.GetPseudo() +" decides to take up arms and to make a rebellion. After thinking about it and to show his good faith, he decides to slit "+ victime.GetPseudo() +"'s throat.";                    victime.SetDoitMourir(true);
                    if (victime.GetRole() is Loup)
                    { 
                        foreach (var j in listJoueurs)
                        {
                            if (j.GetEstMaire())
                            {
                                j.SetEstMaire(false);
                            }
                        }
                        joueurDictateur.SetEstMaire(true);
                        retour = retour + "Quel sauveur ! C'etait un loup ! La foule acclama son nouveau dirigeant! ";
                        retour_ang = retour_ang + "What a savior! He was a wolf! The crowd cheered their new leader!";
                    }
                    else
                    {
                        joueurDictateur.SetDoitMourir(true);
                        aTueInnocent = true;
                        retour = retour +
                                 "Quel assassin ! Tuer un innocent de sang-froid, quelle honte ! La foule poignarda ce dictateur! ";
                        retour_ang = retour_ang + "What a murderer! To kill an innocent in cold blood, what a shame! The crowd stabbed this dictator!";
                    }
                }
                else if(joueurDictateur!=null)
                {
                    retour = "Enfin, juste avant l'aube, " + joueurDictateur.GetPseudo() + " qui etait pourtant decide a prendre les armes pour renverser le pouvoir loupa son reveil et se rendormi... ";
                    retour_ang = "Finally, just before dawn, "+ joueurDictateur.GetPseudo() +" who decided to take up arms to overthrow the power missed his alarm clock and went back to sleep."; 
                }else{
                    retour = "";
                    retour_ang="";
                }
                
            }
            else if(joueurDictateur!=null)
            {
                retour = "Enfin, juste avant l'aube, " + joueurDictateur.GetPseudo() + " hesite a prendre les armes pour tenter de renverser le pouvoir mais juste avant de passer a l'action il decide de procrastiner a un autre jour! ";
                retour_ang = "Finally, just before dawn, "+ joueurDictateur.GetPseudo() +" hesitates to take up arms to try to overthrow the power but just before taking action he decides to procrastinate to another day.";
            }else{
                retour = "";
                retour_ang = "";
            }
        }

        return (retour,retour_ang);
    }

    public override int GetIdRole()
    {
        return IdRole;
    }

    public bool GetATueInnocent()
    {
        return aTueInnocent;
    }

}
