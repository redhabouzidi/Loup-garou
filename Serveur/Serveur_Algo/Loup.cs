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

    public override void Action(List<Joueur> listJoueurs)
    { // écrire l'action du loup
        foreach (Joueur j in listJoueurs)
        {
            server.sendTurn(j.GetSocket(), GetIdRole());
        }
        bool boucle = true;

        List<int> votant = new List<int>();
        List<int> cible = new List<int>();
        foreach (Joueur j in listJoueurs)
        {
            if (j.GetRole() is Loup)
            {
                votant.Add(j.GetId());
                cible.Add(-1);
            }
        }

        // on définit une "alarme" qui modifie la valeur du boolean
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
        foreach (Joueur j in listJoueurs)
        {
            server.sendTime(j.GetSocket(), GetDelaiAlarme());
        }
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

                        Console.WriteLine("je check ici");
                        if (allVote && !reduceTimer && firstTime)
                        {
                            firstTime = false;
                            LaunchThread2 = true;
                            foreach (Joueur j in listJoueurs)
                            {
                                server.sendTime(j.GetSocket(), GetDelaiAlarme() / 4);
                            }
                            Task.Run(() =>
                            {

                                Thread.Sleep(GetDelaiAlarme() * 250);
                                Console.WriteLine("les deux loups ont votés, ça passe à 5sec d'attente");
                                vide.Send(new byte[1] { 0 });
                                boucle = false;
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
            playerVictime.SetDoitMourir(true);
        }

    }

    public override int GetIdRole()
    {
        return IdRole;
    }

}