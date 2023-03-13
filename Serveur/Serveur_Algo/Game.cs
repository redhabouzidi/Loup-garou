using System.Net.Sockets;
using Server;
namespace LGproject;

// on part du principe que la partie se lance à 6 joueurs

public class Game
{
    private List<Joueur> _joueurs;
    private List<Role> _roles;
    private int _nbrJoueursManquants;
    public static Socket listener = Server.server.setupSocketGame();

    public Game()
    {
        _nbrJoueursManquants = 2; // A ENLEVER PLUS TARD "=6"
        // création de la liste de joueurs et de rôles
        _roles = new List<Role>();
        // la partie est créé maintenant j'attends les input du frontend et j'envoie mon client à waiting screen
        // on va admettre que joueurs max = 6
        Role[] startingRoles = new Role[]
        {
            new Loup(),
            new Sorciere()
        };

        foreach (Role role in startingRoles)
        {
            _roles.Add(role);
        }
        _joueurs = new List<Joueur>();
    }

    public void Waiting_screen()
    {
        _nbrJoueursManquants--;
        // check si y'a assez de joueurs pour lancer la partie
        if (_nbrJoueursManquants == 0)
        {
            Start();
        }
        else
        {
            // Waiting Screen Frontend
            Console.WriteLine("il manque encore des joueurs " + _nbrJoueursManquants);
        }
    }

    public void Join(Client c)
    {
        Joueur j = new Joueur(c.GetId(), c.GetSocket(), c.GetPseudo());
        _joueurs.Add(j);

        foreach (Joueur p in _joueurs)
        {
            if (p == j)
            {
                int[] id = new int[_joueurs.Count];
                string[] name = new string[_joueurs.Count];
                for (int i = 0; i < _joueurs.Count; i++)
                {
                    id[i] = _joueurs[i].GetId();
                    name[i] = _joueurs[i].GetPseudo();
                }
                server.sendGameInfo(p.GetSocket(), "canon", id, name);
            }
            else
            {
                Console.WriteLine(p.GetSocket() + " id " + j.GetId() + " pseudo " + j.GetPseudo());
                server.SendAccountInfo(p.GetSocket(), j.GetId(), j.GetPseudo());
            }

        }
        Waiting_screen();
    }

    public void LanceAction(Type typeATester)
    {
        for (int i = 0; i < _joueurs.Count; i++)
        {
            if (typeATester.IsInstanceOfType(_joueurs[i].GetRole()) && _joueurs[i].GetEnVie())
            {
                _joueurs[i].FaireAction(_joueurs);
                break;
            }
        }
    }

    public void Start()
    {
        // mélange des rôles et répartition pour les joueurs
        InitiateGame();


        int checkWin;

        bool day = false;

        while (true)
        {
            // boucle qui affiche pour check si tout va bien
            for (int i = 0; i < _joueurs.Count; i++)
            {
                Console.WriteLine(_joueurs[i].GetPseudo() + " a comme rôle : " + _joueurs[i].GetRole() +
                                  " status enVie : " + _joueurs[i].GetEnVie() + " status doitMourir : " +
                                  _joueurs[i].GetDoitMourir());
                if (_joueurs[i].GetAmoureux() is not null)
                {
                    Console.WriteLine("\t en + ce mec est amoureux !");
                }
            }
            // ICI : broadcast du serveur : c'est la nuit

            foreach (Joueur j in _joueurs)
            {
                server.etatGame(j.GetSocket(), day);
            }
            day = !day;
            // appeller Voyante si il y en a un
            LanceAction(typeof(Voyante));
            Console.WriteLine("début vote loup");
            // appeller Loup si il y en a un
            LanceAction(typeof(Loup));
            Console.WriteLine("fin du vote des loups");
            // appeller Sorciere si il y en a un
            LanceAction(typeof(Sorciere));

            // ICI : broadcast du serveur : c'est la journée
            foreach (Joueur j in _joueurs)
            {
                server.etatGame(j.GetSocket(), day);
            }
            day = !day;
            ///////////////////////////////////
            GestionMorts(_joueurs);

            for (int i = 0; i < _joueurs.Count; i++)
            {
                Console.WriteLine(_joueurs[i].GetPseudo() + " a comme rôle : " + _joueurs[i].GetRole() +
                                  " status enVie : " + _joueurs[i].GetEnVie() + " status doitMourir : " +
                                  _joueurs[i].GetDoitMourir());
                if (_joueurs[i].GetAmoureux() is not null)
                {
                    Console.WriteLine("\t en + ce mec est amoureux !");
                }
            }


            checkWin = Check_win(_joueurs);
            if (checkWin != 0)
            {
                Console.WriteLine("Je sors dès le premier break");
                break;
            }

            Jour(_joueurs);

            GestionMorts(_joueurs);

            checkWin = Check_win(_joueurs);
            if (checkWin != 0)
            {
                break;
            }
        }
        Console.WriteLine("La game est finie");
        // ICI : check la valeur de checkWin si on veut envoyer qui a gagné
    }

    private void GestionMorts(List<Joueur> listJoueurs)
    {
        for (int i = 0; i < _joueurs.Count; i++)
        {
            if (_joueurs[i].GetDoitMourir())
            {
                _joueurs[i].SetDoitMourir(false);
                _joueurs[i].SetEnVie(false);
                foreach (Joueur p in _joueurs)
                {
                    server.annonceMort(p.GetSocket(), _joueurs[i].GetId(), _joueurs[i].GetRole().GetIdRole());
                }
            }
        }
    }

    // retourne 0 si personne n'a encore win, 1 en cas de victoire du village, 2 en cas de victoire des loups
    int Check_win(List<Joueur> listJoueurs)
    {
        int compVillage = 0, compLoups = 0;
        int retour = 0;

        for (int i = 0; i < _joueurs.Count; i++)
        {
            if (_joueurs[i].GetEnVie())
            {
                if (_joueurs[i].GetRole() is Loup)
                {
                    compLoups++;
                }
                else
                {
                    compVillage++;
                }
            }
        }

        if (compVillage < 1)
        {
            retour = 2;
        }
        else if (compLoups < 1)
        {
            retour = 1;
        }

        return retour;
    }

    private void Jour(List<Joueur> listJoueurs)
    {
        List<int> votant = new List<int>();
        List<int> cible = new List<int>();
        foreach (Joueur j in _joueurs)
        {
            if (j.GetEnVie())
            {
                votant.Add(j.GetId());
                cible.Add(-1);
            }
        }

        bool boucle = true;
        // on définit une "alarme" sur 60 secondes
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        foreach (Joueur j in _joueurs)
        {
            server.sendTime(j.GetSocket(), Role.GetDelaiAlarme() * 3);
        }
        Task t = Task.Run(() =>
        {
            Thread.Sleep(Role.GetDelaiAlarme() * 3000);
            vide.Send(new byte[1] { 0 });
            boucle = false;
        });

        int index, v, c;
        while (boucle)
        {
            (v, c) = Role.gameVote(listJoueurs, 1, reveille);
            if (v != -1)
            {
                Joueur? player = listJoueurs.Find(j => j.GetId() == c);
                if (player != null)
                {
                    if (player.GetRole() is not Loup && player.GetEnVie())
                    {
                        index = votant.IndexOf(v);
                        cible[index] = c;
                    }
                }
            }
        }

        // ici on a le résultat final du vote
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

                // ON CHOISIT UNE VICTIME ALEATOIREMENT => A MODIFIER QUAND ON AJOUTE LE MAIRE
                Random random = new Random();
                victime = tiedVictims[random.Next(tiedVictims.Count)];
            }

            Joueur? playerVictime = listJoueurs.Find(j => j.GetId() == victime);
            playerVictime.SetDoitMourir(true);
        }
    }

    private void InitiateGame()
    {
        // mélanger le tableau roles aléatoirement
        Random random = new Random();
        int[] id = new int[_joueurs.Count];
        int[] roles = new int[_joueurs.Count];
        for (int i = 0; i < _joueurs.Count; i++)
        {
            _joueurs[i].SetRole(_roles[i]);
            id[i] = _joueurs[i].GetId();
            roles[i] = _joueurs[i].GetRole().GetIdRole();
        }
        foreach (Joueur j in _joueurs)
        {
            server.sendRoles(j.GetSocket(), id, roles);
        }

        // appeller Cupidon si il y en a un
        LanceAction(typeof(Cupidon));
    }
    public int GetJoueurManquant()
    {
        return _nbrJoueursManquants;
    }
}