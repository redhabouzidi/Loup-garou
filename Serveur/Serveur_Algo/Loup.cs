using Server;
using System.Net.Sockets;
namespace LGproject;

public class Loup : Role
{
private new const int IdRole = 4;
public Loup()
{
    name = "Loup-Garou";
    description = "blabla";
}

public override (string,string) Action(List<Joueur> listJoueurs,Game game)
{ // écrire l'action du loup
    string retour,retour_ang;
    sendTurn(listJoueurs,GetIdRole());
    bool boucle = true;

    List<int> votant = new List<int>();
    List<int> cible = new List<int>();
    foreach (Joueur j in listJoueurs)
    {
        if (j.GetRole() is Loup  && j.GetEnVie() && j.GetEnVie())
        {
            votant.Add(j.GetId());
            cible.Add(-1);
        }
    }

    retour = "Alors que les habitants du village dorment sur leurs deux oreilles, un hurlement venait de retentir. ";
    retour_ang = "While the inhabitants of the village were sleeping deeply, a howl had just sounded. ";
    Joueur? l = null;
    if (votant.Count == 1)
    {
        l = listJoueurs.Find(j => j.GetId() == votant[0]);
        if(l!=null){
        retour = retour + l.GetPseudo() + " un loup vient d’entrer dans le village... ";
        retour_ang = retour_ang + " a wolf has just entered the village... ";
        }
    }
    else
    {
        for (int i = 0; i < votant.Count; i++)
        {
            l = listJoueurs.Find(j => j.GetId() == votant[i]);
            if (l!=null && i == votant.Count - 1)
            {
                retour = retour + l.GetPseudo();
                retour_ang = retour_ang + l.GetPseudo();
            }
            else if(l!=null)
            {
                retour = retour + l.GetPseudo() + ", ";
                retour_ang = retour_ang + l.GetPseudo() + ", ";
            }else{
                retour = "";
                retour_ang = "";
            }
        }
        retour = retour + " des loups venaient d’entrer dans le village... ";
        retour_ang = retour_ang + " wolves have just entered the village... ";
    }
    
    

    // on définit une "alarme" qui modifie la valeur du boolean
    Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    if(Game.listener.LocalEndPoint!=null){
            reveille.Connect(Game.listener.LocalEndPoint);
        }
    Socket vide;
    vide = Game.listener.Accept();
    bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
        sendTime(listJoueurs, GetDelaiAlarme(),game);
    Task.Run(() =>
    {
        Thread.Sleep(GetDelaiAlarme() * 750); // 15 secondes
        reduceTimer = true;
	    Console.WriteLine("Je viens de faire 15 secondes d'attente et je check si je relance 5 secondes ou si y'a déjà thread 2 d'envoyé");
        if (reduceTimer && !LaunchThread2)
        {
            Thread.Sleep(GetDelaiAlarme() * 250); // 5 secondes
            vide.Send(new byte[1] { 0 });
            boucle = false;
        }
    });

    int index, v, c;
    while (boucle)
    {
        (v, c) = gameVote(listJoueurs, GetIdRole(), reveille);
        if(v==-2 && c== -2){
            return ("","");
        }
        Console.WriteLine(v + " et " + c);
        if (v != -1)
        {
            Joueur? player = listJoueurs.Find(j => j.GetId() == c);
            if (player != null)
            {
                if (player.GetRole() is not Loup && player.GetEnVie())
                {
                    index = votant.IndexOf(v);
                    cible[index] = c;
                    bool allVote = true;
                    for (int i = 0; i < cible.Count; i++)
                    {
                        if (cible[i] == -1)
                        {
                            allVote = false;
                        }
                    }
                    SendVote(listJoueurs,v,c,GetIdRole());
                    Console.WriteLine("je check ici");
                    if (allVote && !reduceTimer && firstTime)
                    {
                        firstTime = false;
                        LaunchThread2 = true;
                            sendTime(listJoueurs, GetDelaiAlarme() / 4,game);
                        Task.Run(() =>
                        {
			    	
                            Thread.Sleep(GetDelaiAlarme() * 250);
                            boucle = false;
                            Console.WriteLine("les deux loups ont votés, ça passe à 5sec d'attente");
                            vide.Send(new byte[1] { 0 });
                        });
                    }

                }
            }
        }
    }

    // ici on a le résultat final du vote des loups
    Dictionary<int, int> occurrences = new Dictionary<int, int>();
    for (int i = 0; i < cible.Count; i++)
    {
        if (cible[i] != -1)
        {
            // Vérification si le nombre existe déjà dans le dictionnaire
            if (occurrences.ContainsKey(cible[i]))
            {
                // Si oui, on incrémente son compteur
                occurrences[cible[i]]++; // VOIR SI CA FONCTIONNE BIEN
            }
            else
            {
                // Sinon, on l'ajoute avec un compteur initialisé à 1
                occurrences.Add(cible[i], 1);
            }
        }
    }

    // determine la cible qui possède le + de votes
    int victime = -1;
    int maxVotes = 0;

    foreach (KeyValuePair<int, int> pair in occurrences)
    {
        if (pair.Value > maxVotes)
        {
            victime = pair.Key;
            maxVotes = pair.Value;
        }
    }

    // regarde si il existe plusieurs victimes possédant le nombre maximal de vote
    if (victime != -1)
    {
        bool estMultiple = occurrences.Count(x => x.Value == maxVotes) > 1;

        if (estMultiple) // si il y a plusieurs occurences ( = si les loups ne sont pas mis d'accord sur qui tuer)
        {
            // alors on créé une liste qui recense toutes les victimes égalités
            List<int> tiedVictims = new List<int>();
            foreach (KeyValuePair<int, int> pair in occurrences)
            {
                if (pair.Value == maxVotes)
                {
                    tiedVictims.Add(pair.Key);
                }
            }

            // Choose one of the tied victims randomly
            Random random = new Random();
            victime = tiedVictims[random.Next(tiedVictims.Count)];
        }

        Joueur? playerVictime = listJoueurs.Find(j => j.GetId() == victime);
        if(playerVictime!=null){
            playerVictime.SetDoitMourir(true);
            retour = retour + "Il semblerait qu’une victime ait été déchiqueté au centre du village, il s’agit bien de " + playerVictime.GetPseudo() + ".  ";
            retour_ang = retour_ang + "It seems that a victim was torn to pieces in the center of the village, it is "+ playerVictime.GetPseudo() +". ";
        }
    }
    else
    {
        retour = retour + "Cependant, il semblerait que personne n’ait été blessé cette nuit, quelle chance ! ";
        retour_ang = retour_ang + "However, it seems that no one was hurt tonight, what a luck! ";
    }

    return (retour,retour_ang);
}

public override int GetIdRole()
{
    return IdRole;
}

}
