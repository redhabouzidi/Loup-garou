using System.Net.Sockets;
using System.Net;

namespace LGproject;

public class Loup : Role
{
    
    private bool passe = false;
    private new const int IdRole = 4;
    public Loup()
    {
        name = "Loup-Garou";
        description = "blabla";
    }

    public override void Action(List<Joueur> listJoueurs)
    { // écrire l'action du loup
        bool boucle = true;

        // on définit une "alarme" qui modifie la valeur du boolean
        Console.WriteLine("appel aux loups");

        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        Task t = Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 1000);
            vide.Send(new byte[1] {0});
            boucle = false;
        });
        List<int> votant = new List<int>();
        List<int> cible = new List<int>();
        foreach (Joueur j in  listJoueurs)
        {
            if (j.GetRole() is Loup)
            {
                votant.Add(j.GetId());
                cible.Add(-1);
            }
        }

        int index, v, c;
        while(boucle)
        {
            
            (v,c) = gameVote(listJoueurs,reveille);
            
            
            if (v != -1)
            {
                Console.WriteLine(v + "  " + c);
                
                Joueur? player = listJoueurs.Find(j => j.GetId() == c);
                Console.WriteLine(player);
                if (player != null)
                {
                    Console.WriteLine("ici c'ets les loups");
                    if (player.GetRole() is not Loup && player.GetEnVie())
                    {
                        Console.WriteLine("apres ?");
                        index = votant.IndexOf(v);
                        cible[index] = c;
                        
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
            playerVictime.SetDoitMourir(true);
        }
        
    }
    
    public override int GetIdRole() {
        return IdRole; 
    }
    
}