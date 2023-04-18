using System.Collections.Generic;
using System.Net.Sockets;
using Server;
namespace LGproject;

// on part du principe que la partie se lance à 6 joueurs

public class Game
{
    private List<Joueur> _joueurs;
    private List<Role> _roles;
    private int _nbrJoueursManquants,gameId;
    private bool _start;
    public int _nbrJoueurs;
    public string name, recit;
    private int _nbLoups;
    private bool sorciere, voyante, cupidon;
    public static Socket listener = Server.server.setupSocketGame();
    public Socket vide,reveille;
    public Game()
    {
        _start = false;
        name = "village";
         // A ENLEVER PLUS TARD "=6"
        _nbrJoueurs = 3;
        _nbrJoueursManquants = _nbrJoueurs;
        // création de la liste de joueurs et de rôles
        _roles = new List<Role>();
        _nbLoups = 1;
        sorciere = true;
        voyante = true;
        cupidon = false;
        testNombre();
        // la partie est créé maintenant j'attends les input du frontend et j'envoie mon client à waiting screen
        // on va admettre que joueurs max = 6
        Role[] startingRoles = new Role[_nbrJoueurs];
        int i;
        for (i = 0; i < _nbLoups; i++)
        {
            startingRoles[i] = new Loup();
        }
        if (sorciere)
        {
            startingRoles[i++] = new Sorciere();
        }
        if (voyante)
        {
            startingRoles[i++] = new Voyante();
        }
        if (cupidon)
        {
            startingRoles[i++] = new Cupidon();
        }
        for (; i < _nbrJoueurs; i++)
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
    }
    public bool testNombre()
    {
        int sum=0;
        if (this.sorciere)
        {
            sum++;
        }
        if (this.voyante)
        {
            sum++;
        }
        if (this.cupidon)
        {
            sum++;
        }
        sum += _nbLoups;
        if (sum > _nbrJoueurs)
        {
            return false;
        }
        return true;
    }
    public Game(Client c,string name,int nbPlayers,int nbLoups,bool sorciere,bool voyante, bool cupidon)
    {
        //Initialisation du nombre de joueurs
        _start = false;
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
        if (!testNombre()){
            _nbrJoueurs= 0;
            return;
        }
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
        gameId = c.GetId();
        _joueurs = new List<Joueur>();
        Join(c);
    }
    public void RemovePlayer(Socket sock)
    {
        Joueur temp=null;
        
        foreach (Joueur j in _joueurs)
        {
            if (j.GetSocket() == sock)
            {
                
                temp = j;
                sendQuitMessage(_joueurs, temp.GetId());
                _joueurs.Remove(j);
                break;
            }
        }
        if (temp != null)
        {
            _nbrJoueursManquants++;
        }
        
    }
    public void sendQuitMessage(List<Joueur> listJoueur, int id)
    {
        foreach(Joueur j in listJoueur)
        {
            if(j.GetSocket()!=null && j.GetSocket().Connected)
                server.sendQuitMessage(j.GetSocket(), id);
        }
    }
    public void Waiting_screen()
    {
        _nbrJoueursManquants--;
        // check si y'a assez de joueurs pour lancer la partie
            // Waiting Screen Frontend
            Console.WriteLine("il manque encore des joueurs " + _nbrJoueursManquants);
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

                ConcatRecit(_joueurs[i].FaireAction(_joueurs));
                break;
            }
        }
    }

    public void Start()
    {
        _start = true;
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
        bool firstDay = true;
        LanceAction(typeof(Cupidon));

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

            // broadcast du serveur : c'est la nuit
            sendGameState(day);
            ConcatRecit("Le soleil se couche sur le village de " + name + ". ");
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
            if (!firstDay)
            {
                LanceAction(typeof(Dictateur));
            }
            // broadcast du serveur : c'est la journée
            sendGameState(day);
            ConcatRecit("\n\nLe soleil se lève enfin sur le village de " + name + ". ");
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
                break;
            }
            if (firstDay){
                firstDay = false;
                // election du maire
                ElectionMaire(VoteToutLeMonde(_joueurs, 255), _joueurs);
                Joueur? maire = null;
                foreach (var j in _joueurs)
                {
                    if (j.GetEstMaire())
                    {
                        maire = j;
                    }
                }
                ConcatRecit("Après un long débat rempli de rebondissements le village décide de nommer " + maire.GetPseudo() + " maire pour rétablir la paix dans " + name + ". ");
            }
            
            SentenceJournee(VoteToutLeMonde(_joueurs, 1), _joueurs);

            GestionMorts(_joueurs);

            checkWin = Check_win(_joueurs);
            if (checkWin != 0)
            {
                break;
            }
            
            ConcatRecit("\n\n");
            
            // enlève à tout le monde l'immunité accordé par le Garde
            RemoveSaveStatus();
        }
	//PointShare();              Don't forget me please :) 
        EndGameInitializer();
        Console.WriteLine("La game est finie");
    }

    private void RemoveSaveStatus()
    {
        foreach (var j in _joueurs)
        {
            if (j.GetAEteSave())
            {
                j.SetAEteSave(false);
            }
        }
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
                    if (_joueurs[i].GetRole() is Chasseur)
                    {
                        LanceAction(typeof(Chasseur));
                        i = 0;
                    }

                    if (_joueurs[i].GetEstMaire())
                    {
                        int idSuccesseur = DecisionDuMaire(_joueurs);
                        if (idSuccesseur == -1)
                        {
                            List<int> joueursEnVie = new List<int>();
                            foreach (Joueur j in listJoueurs)
                            {
                                if (j.GetEnVie())
                                {
                                    joueursEnVie.Add(j.GetId());
                                }
                            }
                            // ON CHOISIT UN NOUVEAU MAIRE ALEATOIREMENT
                            Random random = new Random();
                            idSuccesseur = joueursEnVie[random.Next(joueursEnVie.Count)];
                        }
                        Joueur? player = listJoueurs.Find(j => j.GetId() == idSuccesseur);
                        player.SetEstMaire(true);
                        ConcatRecit("Alors qu’il s’apprêtait à mourir, le maire demanda au village d’écouter ses dernières paroles. Il décide de nommer " + player.GetPseudo() + " comme son successeur à la tête du village… ");
                        // on enlève le statut de maire à l'ancien maire
                        _joueurs[i].SetEstMaire(false);
                    }
                    _joueurs[i].TuerJoueur(listJoueurs);
                    ConcatRecit("Une victime est allongée au centre du village. Il s’agit de " + _joueurs[i].GetPseudo() + " qui s’avérait être " + _joueurs[i].GetRole() + " à ses temps perdus. ");
                }
            }
        }
    }

    // retourne 0 si personne n'a encore win, 1 en cas de victoire du village, 2 en cas de victoire des loups, 3 en cas de victoire du couple et 4 en cas d'egalite
    int Check_win(List<Joueur> listJoueurs)
    {
        int compVillage = 0, compLoups = 0;
        int retour = 0;
        bool coupleEnVie = false;

        for (int i = 0; i < _joueurs.Count; i++)
        {
            if (_joueurs[i].GetEnVie())
            {
                if(_joueurs[i].GetAmoureux() != null && !coupleEnVie)
                {
                    coupleEnVie = true;
                }

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

        if (compVillage == 0)
        {
            if(compLoups == 0)
            {
                retour = 4;
            }
            else
            {
                retour = 2;
            }
        }
        else if (compLoups == 0)
        {
            retour = 1;
        }
        else if(compLoups == 1 && compVillage == 1)
        {
            if(coupleEnVie)
            {
                retour = 3;
            }
        }

        // check la valeur de checkWin si on veut envoyer qui a gagné
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

    private void ElectionMaire(List<int> cible, List<Joueur> listJoueurs)
    {
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

            if (estMultiple) // si il y a plusieurs occurences ( = si le village ne s'est pas mis d'accord sur qui élire maire)
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

                // ON CHOISIT UN MAIRE ALEATOIREMENT
                Random random = new Random();
                victime = tiedVictims[random.Next(tiedVictims.Count)];
            }

            Joueur? playerVictime = listJoueurs.Find(j => j.GetId() == victime);
            playerVictime.SetEstMaire(true);
        }
    }

    public void SentenceJournee(List<int> cible, List<Joueur> listJoueurs)
    {
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

                // LE MAIRE DOIT CHOISIR QUI MEURT
                List<Joueur> tiedVictimsJoueur = new List<Joueur>();
                for (int i = 0; i < tiedVictims.Count ; i++)
                {
                    tiedVictimsJoueur.Add(listJoueurs.Find(j => j.GetId() == tiedVictims[i]));
                }
                victime = DecisionDuMaire(tiedVictimsJoueur);
            }

            Joueur? playerVictime = listJoueurs.Find(j => j.GetId() == victime);
            if (playerVictime != null)
            {
                playerVictime.SetDoitMourir(true);
                ConcatRecit("Les habitants du village débâtent et décide de pointer " + playerVictime.GetPseudo() + " comme responsable des catastrophes du village... Ils décident de le tuer sur la place publique. ");
            }
            else
            {
                ConcatRecit("Les habitants du village débâtent mais n’arrivent pas à trouver de solution au problème... Ils décident de rentrer calmement chez eux. ");
            }
        }
    }

    private List<int> VoteToutLeMonde(List<Joueur> listJoueurs, int idRole)
    {
        
        Role r = new Villageois();
        r.sendTurn(listJoueurs,idRole);
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
        /*/ demander Redha /*/
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
            
        r.gameListener = reveille;
        while (boucle)
        {
            Console.WriteLine("6");
            
            (v, c) = r.gameVote(listJoueurs, idRole, reveille);
            Console.WriteLine("7");
            if (v != -1)
            {
                Joueur? player = listJoueurs.Find(j => j.GetId() == c);
                bool condition = (player != null && player.GetEnVie() && player.GetId() != v);
                if (idRole == 255)
                {
                    condition = (player != null && player.GetEnVie());
                }
                if (condition)
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
                    r.SendVote(listJoueurs, v, c, 1);
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
        return cible;
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
    }
    public void sendRoles(Joueur j)
    {
        int[] id = new int[_joueurs.Count];
        int[] rolesToSend = new int[_joueurs.Count];
        
            for (int i = 0; i < _roles.Count; i++)
            {
                id[i] = _joueurs[i].GetId();
                if (_roles[i].GetIdRole()!=1 && j.GetRole().GetIdRole() == _roles[i].GetIdRole())
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
        int nb_villageois = 0, nb_loups = 0, nb_total;
        bool retour = true;
        if (_roles.Count == _nbrJoueursManquants && _nbrJoueursManquants > 3 && _nbrJoueursManquants < 13)
        {
            foreach (var r in _roles)
            {
                if (r is Loup)
                {
                    nb_loups++;
                }
                else
                {
                    nb_villageois++;
                }
            }
        }
        else{
            retour = false;
        }

        nb_total = nb_loups + nb_villageois;
        
        if (nb_loups > (nb_total / 2) || nb_loups == 0)
        {
            retour = false;
        }

        return retour;
    }
    //La fonction s'occupe de lier les joueurs encore connecté au serveur
    public void EndGameInitializer()
    {
        foreach(Joueur j in _joueurs)
        {
            
            if(j.GetSocket()!=null && j.GetSocket().Connected)
            {
                Console.WriteLine("debut de suppression ");
                server.connected.Add(j.GetSocket(), j.GetId());
            }
            server.players.Remove(j.GetId());
        }
        server.games.Remove(gameId);
        server.WakeUpMain();
    }
    // La fonction renvoie celui qui est choisi par le maire (la victime en cas d'égalité ou son successeur si le maire est mort) 
    public int DecisionDuMaire(List<Joueur> listJoueurs)
    {
        int retour = -1;
        Joueur? maire = null;
        foreach (Joueur j in _joueurs)
        {
            if (j.GetEstMaire())
            {
                maire = j;
            }
        }
        Role r = new Villageois();
        // 255 = id du maire
        r.sendTurn(_joueurs, 255);

        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        
        bool boucle = true;
        r.sendTime(_joueurs, Role.GetDelaiAlarme()/2);
        Task.Run(() =>
        {
            Thread.Sleep(Role.GetDelaiAlarme() * 500); // 10 secondes
            vide.Send(new byte[1] { 0 });
            boucle = false;
        });

        int v, c;
        Joueur? player = null;
        Console.WriteLine("Le maire doit prendre sa décision");
        
        r.gameListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        r.gameListener.Connect(listener.LocalEndPoint);
        this.vide = listener.Accept();
        while (boucle)
        {
            (v, c) = r.gameVote(_joueurs, 255, reveille);
            if (v == maire.GetId())
            {
                player = listJoueurs.Find(j => j.GetId() == c);
                if (player != null && player != maire && player.GetEnVie())
                {
                    boucle = false;
                }
            }
        }

        if (player != null)
        {
            retour = player.GetId();
        }

        return retour;
    }
    public void ToggleReady(int id)
    {
        bool ready = false, found = false; ;
        foreach(Joueur j in _joueurs)
        {
            if(j.GetId() == id)
            {
                j.SetReady(!j.GetReady());
                ready=j.GetReady();
                found = true;
                break;
            }
        }
        if (found)
        {
        int sum=0;
        foreach(Joueur j in _joueurs)
        {
            if (j.GetReady())
            {
                sum++;
            }
            server.sendReady(j.GetSocket(), id, ready);
        }
        if (sum == _nbrJoueurs)
        {
            foreach (Joueur j in _joueurs)
            {
                server.connected.Remove(j.GetSocket());
                server.userData[j.GetId()].SetStatus(j.GetId(), 3);
            }
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                Start();
            });
        }

        }
    }

    public void PointShare() {
        int []id = new int[_nbrJoueurs];
        int []score = new int[_nbrJoueurs];
        int i = 0;
        foreach(var joueur in _joueurs) 
        {
            id[i] = joueur.GetId();
            if(Check_win() == 1) 
	    {
                if(joueur is not Loup) 
		{
                    if(joueur.GetEnVie()) 
		    {
                        score[i] = 10;
                    }
                    else 
		    {
                        score[i] = 5;
                    }
                }
            }
            else if(Check_win() == 2) 
	    {
                if(joueur is Loup) 
		{
                    if(joueur.GetEnVie()) 
		    {
                        score[i] = 10;
                    }
                    else 
		    {
                        score[i] = 5;
                    }
                }
            }
            else if(Check_win() == 3) 
	    {
                if(joueur.GetEnVie()) 
		{
                    score[i] = 10;
                }
            }
            else if(Check_win() == 4) 
	    {
                if(joueur.GetEnVie()) 
		{
                    score[i] = 5;
                }
                else 
		{
                    score[i] = 2;
                }
            }
            i++;
        }
        SendPoints(_joueurs,id,score);
    }


    public List<Joueur> GetJoueurs()
    {
        return _joueurs;
    }

    public bool GetStart()
    {
        return _start;
    }

    public void SetStart(bool b)
    {
        _start = b;
    }
    public int GetGameId()
    {
        return gameId;
    }
    public void SetGameId(int id)
    {
        gameId = id;
    }

    public void ConcatRecit(string s)
    {
        recit = recit + s;
    }
}

