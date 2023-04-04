using System.Collections.Generic;
using System.Net.Sockets;
using Server;
namespace LGproject;

// on part du principe que la partie se lance à 6 joueurs

public class Game
{
    private List<Joueur> _joueurs;
    private List<Role> _roles;
    private int _nbrJoueursManquants;
    public int _nbrJoueurs;
    public string name;
    private int _nbLoups;
    private bool sorciere, voyante, cupidon;
    public static Socket listener = Server.server.setupSocketGame();
    public Socket vide,reveille;
    public Game()
    {
        name = "village";
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

        if (!checkRoles())
        {
            Console.WriteLine("Tu n'as pas respecté les conditions de rôles pour lancer ta partie !");
        }
        
        _joueurs = new List<Joueur>();
    }
    public Game(Client c,string name,int nbPlayers,int nbLoups,bool sorciere,bool voyante, bool cupidon)
    {
        //Initialisation du nombre de joueurs
        _nbrJoueursManquants = nbPlayers;
        _nbrJoueurs = nbPlayers;
        this.name = name;
        // création de la liste de joueurs et de rôles
        _roles = new List<Role>();
        Role[] startingRoles = new Role[nbPlayers];
        //affectation des parametres
        this.sorciere = sorciere;
        this.cupidon = cupidon;
        this.voyante = voyante;
        _nbLoups= nbLoups;
        int i;
        //initialisation des roles
        for( i=0;i<nbLoups;i++)
        {
            startingRoles[i] = new Loup();
        }
        if (sorciere)
        {
            startingRoles[i++] = new Sorciere();
        }
        if(voyante)
        {
            startingRoles[i++] = new Voyante();
        }
        if (cupidon)
        {
            startingRoles[i++] = new Cupidon();
        }
        for (; i < nbPlayers; i++)
        {
            startingRoles[i] = new Villageois();
        }

        foreach (Role role in startingRoles)
        {
            _roles.Add(role);
        }

        if (!checkRoles())
        {
            Console.WriteLine("Tu n'as pas respecté les conditions de rôles pour lancer ta partie !");
        }

        _joueurs = new List<Joueur>();
        Join(c);
    }
    public void Waiting_screen()
    {
        _nbrJoueursManquants--;
        // check si y'a assez de joueurs pour lancer la partie
        if (_nbrJoueursManquants == 0)
        {
            foreach(Joueur j in _joueurs)
                {
                    server.connected.Remove(j.GetSocket());
                }
            Task.Run(() => Start());
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
                sendGameInfo(p.GetSocket());
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
                _joueurs[i].GetRole().gameListener = reveille;

                _joueurs[i].FaireAction(_joueurs);
                break;
            }
        }
    }

    public void Start()
    {
        foreach(Joueur j in _joueurs)
        {
            server.connected.Remove(j.GetSocket());
        }
        
        // mélange des rôles et répartition pour les joueurs
        InitiateGame();
        reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(listener.LocalEndPoint);
        vide = listener.Accept();
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

            sendGameState(day);
            day = !day;
            // appeller Voyante si il y en a un
            LanceAction(typeof(Voyante));
            LanceAction(typeof(Garde));
            Console.WriteLine("début vote loup");
            // appeller Loup si il y en a un
            LanceAction(typeof(Loup));
            Console.WriteLine("fin du vote des loups");
            // appeller Sorciere si il y en a un
            LanceAction(typeof(Sorciere));

            // ICI : broadcast du serveur : c'est la journée
            sendGameState(day);
            day = !day;
            ///////////////////////////////////
            GestionMorts(_joueurs);

            for (int i = 0; i < _joueurs.Count; i++)
            {
                Console.WriteLine(_joueurs[i].GetPseudo() + " a comme rôle : " + _joueurs[i].GetRole() +
                                  " status enVie : " + _joueurs[i].GetEnVie() + " status doitMourir : " +
                                  _joueurs[i].GetDoitMourir() + " et possède l'id : " + _joueurs[i].GetId());
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
                if (_joueurs[i].GetAEteSave())
                {
                    _joueurs[i].SetAEteSave(false);
                    _joueurs[i].SetDoitMourir(false);
                }
                else
                {
                    _joueurs[i].TuerJoueur(listJoueurs);
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
        if(retour != 0)
        {
            List<Socket> sockets = new List<Socket>();
            int[] id = new int[listJoueurs.Count];
            int[] idr = new int[listJoueurs.Count];
            int i = 0; ;
            foreach(Joueur j in listJoueurs)
            {
                id[i] = j.GetId();
                idr[i] = j.GetRole().GetIdRole();
                if (j.GetSocket().Connected)
                {
                    sockets.Add(j.GetSocket());
                }
            }
            server.sendEndState(sockets, retour, id, idr);
        }
        return retour;
    }

    private void Jour(List<Joueur> listJoueurs)
    {
        Role r = new Villageois();
        Console.WriteLine("1");
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
        Console.WriteLine("2");
        bool boucle = true;
        // on définit une "alarme" sur 60 secondes
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        Console.WriteLine("3");
        vide = Game.listener.Accept();
        Console.WriteLine("4");
        bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
        r.sendTime(listJoueurs, Role.GetDelaiAlarme()*3);
        Task.Run(() =>
        {
            Thread.Sleep(Role.GetDelaiAlarme() * 2500); // 45 secondes
            reduceTimer = true;
            if (reduceTimer && !LaunchThread2)
            {
                Thread.Sleep(Role.GetDelaiAlarme() * 500); // 10 secondes
                vide.Send(new byte[1] { 0 });
                boucle = false;
            }
        });
        Console.WriteLine("5");
        int index, v, c;
            
        r.gameListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        r.gameListener.Connect(listener.LocalEndPoint);
        this.vide = listener.Accept();
        while (boucle)
        {
            Console.WriteLine("6");
            
            (v, c) = r.gameVote(listJoueurs, 1, reveille);
            Console.WriteLine("7");
            if (v != -1)
            {
                Joueur? player = listJoueurs.Find(j => j.GetId() == c);
                if (player != null && player.GetEnVie() && player.GetId() != v)
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

                    if (allVote && !reduceTimer && firstTime)
                    {
                        firstTime = false;
                        LaunchThread2 = true;
                            r.sendTime(listJoueurs, Role.GetDelaiAlarme()/2);
                        Task.Run(() =>
                        {
                            Thread.Sleep(Role.GetDelaiAlarme() * 500); // 10
                            Console.WriteLine("tout le monde a voté, ça passe à 10sec d'attente");
                            vide.Send(new byte[1] { 0 });
                            boucle = false;
                        });
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
        Random random = new Random();
        _roles = _roles.OrderBy(r => random.Next()).ToList();
        
        for (int i = 0; i < _joueurs.Count; i++)
        {
            _joueurs[i].SetRole(_roles[i]);
        }
        foreach (Joueur j in _joueurs)
        {
            sendRoles(j);
        }
        // appeller Cupidon si il y en a un
        LanceAction(typeof(Cupidon));
    }
    public void sendRoles(Joueur j)
    {
        int[] id = new int[_joueurs.Count];
        int[] rolesToSend = new int[_joueurs.Count];
        
            for (int i = 0; i < _roles.Count; i++)
            {
                id[i] = _joueurs[i].GetId();
                if (_role[i].GetIdRole()!=1 && j.GetRole().GetIdRole() == _roles[i].GetIdRole())
                {
                    rolesToSend[i] = _roles[i].GetIdRole();
                }
                else
                {
                    rolesToSend[i] = 0;
                }
            }
            server.sendRoles(j.GetSocket(), id, rolesToSend);
        
    }
    public void sendGameInfo(Socket sock)
    {
        int[] id = new int[_joueurs.Count];
        string[] name = new string[_joueurs.Count];
        for (int i = 0; i < _joueurs.Count; i++)
        {
            id[i] = _joueurs[i].GetId();
            name[i] = _joueurs[i].GetPseudo();
        }
        server.sendGameInfo(sock,_nbrJoueurs,_nbLoups,sorciere,voyante,cupidon, this.name, id, name);
    }
    public void sendGameState(bool day)
    {
        foreach (Joueur j in _joueurs)
        {
            if (j.GetSocket().Connected)
            server.etatGame(j.GetSocket(), day);
        }
    }
    public int GetJoueurManquant()
    {
        return _nbrJoueursManquants;
    }
    
    public bool checkRoles()
    {
        int[,] myArrayRoleMax = new int[,] { { 1, 3, 1, 0, 0, 0, 0, 0 }, { 1, 4, 1, 1, 1, 0, 0, 0 }, { 2, 4, 1, 1, 1, 1, 0, 0 }, { 2, 5, 1, 2, 2, 1, 2, 2 }, { 2, 6, 1, 3, 3, 1, 3, 3 }, { 3, 6, 1, 1, 1, 1, 1, 1 }, { 3, 7, 1, 1, 1, 1, 1, 1 }, { 3, 8, 1, 1, 1, 1, 1, 1 }, { 4, 8, 1, 1, 1, 1, 1, 1 } };
        int[] myArrayVillageoisMin = new int[] { 2, 2, 1, 1, 1, 0, 1, 2, 2 };
        int nb_villageois = 0;
        int index = _nbrJoueursManquants - 4;
        bool retour = true;
        if (_roles.Count == _nbrJoueursManquants && _nbrJoueursManquants > 3 && _nbrJoueursManquants < 13)
        {
            foreach (var r in _roles)
            {
                if (r is Loup && myArrayRoleMax[index, 0] > 0)
                {
                    myArrayRoleMax[index, 0] -= 1;
                }
                else if (r is Villageois && myArrayRoleMax[index, 1] > 0)
                {
                    myArrayRoleMax[index, 1] -= 1;
                    nb_villageois++;
                }
                else if (r is Voyante && myArrayRoleMax[index, 2] > 0)
                {
                    myArrayRoleMax[index, 2] -= 1;
                }
                else if (r is Chasseur && myArrayRoleMax[index, 3] > 0)
                {
                    if (_nbrJoueursManquants == 5 || _nbrJoueursManquants == 6)
                    {
                        myArrayRoleMax[index, 4] -= 1;
                    }
                    else if (_nbrJoueursManquants == 7 || _nbrJoueursManquants == 8)
                    {
                        myArrayRoleMax[index, 4] -= 1;
                        myArrayRoleMax[index, 6] -= 1;
                        myArrayRoleMax[index, 7] -= 1;
                    }
                    myArrayRoleMax[index, 3] -= 1;
                }
                else if (r is Sorciere && myArrayRoleMax[index, 4] > 0)
                {
                    if (_nbrJoueursManquants == 5 || _nbrJoueursManquants == 6)
                    {
                        myArrayRoleMax[index, 3] -= 1;
                    }
                    else if (_nbrJoueursManquants == 7 || _nbrJoueursManquants == 8)
                    {
                        myArrayRoleMax[index, 3] -= 1;
                        myArrayRoleMax[index, 6] -= 1;
                        myArrayRoleMax[index, 7] -= 1;
                    }
                    myArrayRoleMax[index, 4] -= 1;
                }
                else if (r is Cupidon && myArrayRoleMax[index, 5] > 0)
                {
                    myArrayRoleMax[index, 5] -= 1;
                }
                else if (r is Dictateur && myArrayRoleMax[index, 6] > 0)
                {
                    if (_nbrJoueursManquants == 7 || _nbrJoueursManquants == 8)
                    {
                        myArrayRoleMax[index, 3] -= 1;
                        myArrayRoleMax[index, 4] -= 1;
                        myArrayRoleMax[index, 7] -= 1;
                    }
                    myArrayRoleMax[index, 6] -= 1;
                }
                else if (r is Garde && myArrayRoleMax[index, 7] > 0)
                {
                    if (_nbrJoueursManquants == 7 || _nbrJoueursManquants == 8)
                    {
                        myArrayRoleMax[index, 3] -= 1;
                        myArrayRoleMax[index, 4] -= 1;
                        myArrayRoleMax[index, 6] -= 1;
                    }
                    myArrayRoleMax[index, 7] -= 1;
                }
                else
                {
                    retour = false;
                    break;
                }
            }

            if (myArrayRoleMax[index, 0] != 0)
            {
                retour = false;
            }
        }
	else{
		return false;
	}

        if (myArrayVillageoisMin[index] > nb_villageois && index != 5)
        {
            retour = false;
        }

        return retour;
    }
    public List<Joueur> GetJoueurs()
    {
        return _joueurs;
    }
}
