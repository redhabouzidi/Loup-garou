using System.Collections.Generic;
using System.Net.Sockets;
using Server;
namespace LGproject;

public class Game
{
    private List<Joueur> _joueurs=new List<Joueur>();
    private List<Role> _roles;
    private int[] rolesJoueurs;
    private int _nbrJoueursManquants,gameId;
    private bool _start;
    public int _nbrJoueurs,currentTime;
    public string name, recit="", recit_ang="";
    private int _nbLoups;
    private bool sorciere, voyante, cupidon, chasseur, guardien, dictateur;
    public static Socket listener = Messages.setupSocketGame();
    public Socket? vide,reveille;
    public DateTime t;
    public Game()
    {
        _start = false;
        name = "village";
         // A ENLEVER PLUS TARD "=6"
        _nbrJoueurs = 3;
        _nbrJoueursManquants = _nbrJoueurs;
        // cr ation de la liste de joueurs et de r les
        _roles = new List<Role>();
        _nbLoups = 1;
        sorciere = true;
        voyante = true;
        cupidon = false;
        chasseur = false;
        guardien = false;
        dictateur = false;
        testNombre();
        // la partie est cr   maintenant j'attends les input du frontend et j'envoie mon client   waiting screen
        // on va admettre que joueurs max = 6
        rolesJoueurs = new int[_nbrJoueurs];
        Role[] startingRoles = new Role[_nbrJoueurs];
        int i;
        for( i=0;i<_nbLoups;i++)
        {
            rolesJoueurs[i]=4;
            startingRoles[i] = new Loup();
        }
        if (sorciere)
        {
            rolesJoueurs[i]=5;
            startingRoles[i++] = new Sorciere();
        }
        if (voyante)
        {
            rolesJoueurs[i]=3;
            startingRoles[i++] = new Voyante();
        }
        if (cupidon)
        {
            rolesJoueurs[i]=2;
            startingRoles[i++] = new Cupidon();
        }
        if (chasseur)
        {
            rolesJoueurs[i]=6;
            startingRoles[i++] = new Chasseur();
        }
        if (guardien)
        {
            rolesJoueurs[i]=8;
            startingRoles[i++] = new Garde();
        }
        if (dictateur)
        {
            rolesJoueurs[i]=7;
            startingRoles[i++] = new Dictateur();
        }
        for (; i < _nbrJoueurs; i++)
        {
            rolesJoueurs[i]=1;
            startingRoles[i] = new Villageois();
        }

        foreach (Role role in startingRoles)
        {
            _roles.Add(role);
        }

        if (!checkRoles())
        {
            Console.WriteLine("Tu n'as pas respect  les conditions de r les pour lancer ta partie !");
        }
        
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

    /**
        Le constructeur de cette classe "Game" prend en paramètre plusieurs informations sur la partie de jeu à créer, 
        telles que le nombre de joueurs, le nombre de loups, les rôles disponibles, etc. 
        Il initialise ensuite les différentes variables de la classe et crée une liste de rôles pour les joueurs. 
        Enfin, il vérifie si les conditions sont remplies pour lancer la partie et rejoint le client spécifié à la partie en cours
    */
    public Game(Client c,string name,int nbPlayers,int nbLoups,bool sorciere,bool voyante, bool cupidon,bool chasseur,bool guardien, bool dictateur)
    {
        //Initialisation du nombre de joueurs
        _start = false;
        _nbrJoueursManquants = nbPlayers;
        _nbrJoueurs = nbPlayers;
        this.name = name;
        // cr ation de la liste de joueurs et de r les
        _roles = new List<Role>();
        rolesJoueurs = new int[nbPlayers];
        Role[] startingRoles = new Role[nbPlayers];
        //affectation des parametres
        this.sorciere = sorciere;
        this.cupidon = cupidon;
        this.voyante = voyante;
        this.chasseur = chasseur;
        this.guardien = guardien;
        this.dictateur = dictateur;
        _nbLoups= nbLoups;
        if (!testNombre()){
            _nbrJoueurs= 0;
            return;
        }
        int i;
        //initialisation des roles
        for( i=0;i<nbLoups;i++)
        {
            rolesJoueurs[i]=4;
            startingRoles[i] = new Loup();
        }
        if (sorciere)
        {
            rolesJoueurs[i]=5;
            startingRoles[i++] = new Sorciere();
        }
        if (voyante)
        {
            rolesJoueurs[i]=3;
            startingRoles[i++] = new Voyante();
        }
        if (cupidon)
        {
            rolesJoueurs[i]=2;
            startingRoles[i++] = new Cupidon();
        }
        if (chasseur)
        {
            rolesJoueurs[i]=6;
            startingRoles[i++] = new Chasseur();
        }
        if (guardien)
        {
            rolesJoueurs[i]=8;
            startingRoles[i++] = new Garde();
        }
        if (dictateur)
        {
            rolesJoueurs[i]=7;
            startingRoles[i++] = new Dictateur();
        }
        for (; i < nbPlayers; i++)
        {
            rolesJoueurs[i]=1;
            startingRoles[i] = new Villageois();
        }

        foreach (Role role in startingRoles)
        {
            _roles.Add(role);
        }

        if (!checkRoles())
        {
            Console.WriteLine("Tu n'as pas respect  les conditions de r les pour lancer ta partie !");
        }
        gameId = c.GetId();
        Join(c);
    }


    /**
        Permet de retirer un joueur de la partie en cours. 
        Elle prend en paramètre un objet Socket correspondant au joueur à retirer.
    */
    public void RemovePlayer(Socket sock)
    {
        Joueur? temp=null;
        
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

    /**
        La méthode "sendQuitMessage" permet d'envoyer un message indiquant que le joueur
        possédant l'id donné en paramètre s'est déconnecté à tous les autres joueurs.
    */
    public void sendQuitMessage(List<Joueur> listJoueur, int id)
    {
        foreach(Joueur j in listJoueur)
        {
            if(j.GetSocket()!=null && j.GetSocket().Connected)
                Messages.sendQuitMessage(j.GetSocket(), id);
        }
    }

    /**
        La méthode "Waiting_screen" est appelée lorsque tous les joueurs ne sont pas encore présents dans la partie. 
        Elle décrémente le nombre de joueurs manquants et affiche un message pour indiquer le nombre de joueurs manquants restants pour lancer la partie. 
        Si le bon nombre est atteint, la partie peut être lancée.
    */
    public void Waiting_screen()
    {
        _nbrJoueursManquants--;
        // check si y'a assez de joueurs pour lancer la partie
            // Waiting Screen Frontend
            Console.WriteLine("il manque encore des joueurs " + _nbrJoueursManquants);
    }

    /**
        La méthode "Join" permet à un joueur de rejoindre la partie.
    */
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
                Messages.SendAccountInfo(p.GetSocket(), j.GetId(), j.GetPseudo());
                
            }

        }
        Waiting_screen();
    }


    /**
        Cette méthode LanceAction est appelée pour lancer l'action du joueur dont le rôle est passé en paramètre (typeATester). 
        Elle parcourt la liste des joueurs pour trouver celui dont le rôle correspond et qui est toujours en vie. 
        Si un joueur est trouvé, l'action du joueur est effectuée et le récit de l'action est ajouté au journal de la partie en cours. 
        Si aucun joueur ne correspond aux critères de recherche, la méthode ne fait rien.
    */
    public void LanceAction(Type typeATester)
    {
        string retour,retour_ang;
        Console.WriteLine(CountSockets());
        if (CountSockets() == 0)
        {

            Messages.games.Remove(GetGameId());
            foreach(Joueur j in _joueurs)
            {
                Messages.players.Remove(j.GetId());
                
            }
            _joueurs.Clear();
            return ;
        }else
        {
        }
        for (int i = 0; i < _joueurs.Count; i++)
        {
            if (typeATester.IsInstanceOfType(_joueurs[i].GetRole()) && _joueurs[i].GetEnVie() && _joueurs[i].GetSocket()!=null && _joueurs[i].GetSocket().Connected)
            {
                if(reveille!=null){
                    _joueurs[i].GetRole().gameListener = reveille;
                }

                (retour,retour_ang) = _joueurs[i].FaireAction(_joueurs,this);
                ConcatRecit(retour,retour_ang);
                break;
            }
        }
    }

    /**
        Cette méthode CountSockets() compte le nombre de joueurs connectés
    */
    public int CountSockets()
    {
        int sum = 0;
        foreach (Joueur j in _joueurs)
        {
            if (j.GetSocket() != null && j.GetSocket().Connected)
            {
                sum++;
            }
        }

        return sum;
    }

    /**
        Cette méthode est appelée lorsque le jeu commence et contient une boucle infinie qui continue jusqu'à ce que le jeu se termine. 
        Pendant cette boucle, le serveur envoie des messages pour informer les joueurs si c'est la nuit ou le jour, 
        appelle les actions des différents rôles (comme la voyante, le garde, le loup, la sorcière, etc.), 
        gère les morts et les votes, et vérifie si une équipe a gagné le jeu.
    */
    public void Start()
    {
        _start = true;
        
        // m lange des r les et r partition pour les joueurs
        InitiateGame();
        reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if(Game.listener.LocalEndPoint!=null){
            reveille.Connect(Game.listener.LocalEndPoint);
        }
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
                Console.WriteLine(_joueurs[i].GetPseudo() + " a comme r le : " + _joueurs[i].GetRole() +
                                  " status enVie : " + _joueurs[i].GetEnVie() + " status doitMourir : " +
                                  _joueurs[i].GetDoitMourir());
                if (_joueurs[i].GetAmoureux() is not null)
                {
                    Console.WriteLine("\t en + ce mec est amoureux !");
                }
            }
            // broadcast du serveur : c'est la nuit
            sendGameState(day);
            ConcatRecit("Le soleil se couche sur le village de " + name + ". ","The sun sets on the village of "+name+". ");
            day = !day;
            // appeller Voyante si il y en a un
            Console.WriteLine("on passe ?");
            LanceAction(typeof(Voyante));
            LanceAction(typeof(Garde));
            Console.WriteLine("d but vote loup");
            // appeller Loup si il y en a un
            LanceAction(typeof(Loup));
            Console.WriteLine("fin du vote des loups");
            // appeller Sorciere si il y en a un
            LanceAction(typeof(Sorciere));
            if (!firstDay)
            {
                LanceAction(typeof(Dictateur));
            }
            // broadcast du serveur : c'est la journ e
            sendGameState(day);
            ConcatRecit("\n\nLe soleil se l ve enfin sur le village de " + name + ". ","\n\nThe sun rises on the village of " + name + ".");
            day = !day;
            ///////////////////////////////////
            GestionMorts(_joueurs);
            // enl ve   tout le monde l'immunit  accord  par le Garde
            RemoveSaveStatus();
            for (int i = 0; i < _joueurs.Count; i++)
            {
                Console.WriteLine(_joueurs[i].GetPseudo() + " a comme r le : " + _joueurs[i].GetRole() +
                                  " status enVie : " + _joueurs[i].GetEnVie() + " status doitMourir : " +
                                  _joueurs[i].GetDoitMourir() + " et poss de l'id : " + _joueurs[i].GetId());
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
                if(maire!=null){
                    ConcatRecit("Apres un long debat rempli de rebondissements le village decide de nommer " + maire.GetPseudo() + " maire pour r tablir la paix dans " + name + ". ","After a long debate filled with twists and turns the village decides to appoint "+maire.GetPseudo()+" as mayor to restore peace in "+name+".");

                }
            }
            
            SentenceJournee(VoteToutLeMonde(_joueurs, 1), _joueurs);

            GestionMorts(_joueurs);

            checkWin = Check_win(_joueurs);
            if (checkWin != 0)
            {
                break;
            }
            
            ConcatRecit("\n\n","\n\n");
            
            
        }
	    PointShare(checkWin);
        EndGameInitializer();

    }

    /**
        La méthode saveGame permet de sauvegarder une partie en cours.
    */
    public void saveGame(string recit,string recit_ang,int [] score,bool[] victoire){
        int[] ids=new int[_joueurs.Count];
        for(int i=0;i<_joueurs.Count;i++){
            ids[i]=_joueurs[i].GetId();
        }
        if(Messages.bdd!=null){
            Messages.sendMatch(Messages.bdd,name,recit,recit_ang,ids,score,victoire);
        }
    }

    /**
        Cette méthode permet de réinitialiser le statut save de chaque joueur de la partie.
    */
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

    /**
        La méthode GestionMorts gère la mort des joueurs en fonction de leur rôle et de leurs statuts pendant la partie. 
        La méthode met à jour les propriétés des joueurs en conséquence et enregistre les informations dans le récit de la partie.
    */
    private void GestionMorts(List<Joueur> listJoueurs)
    {
        Joueur? chasseurMort = null;
        Joueur? maireMort = null;
        bool reboucle = false;
        for (int i = 0; i < _joueurs.Count; i++)
        {
            if (reboucle)
            {
                i = 0;
                reboucle = false;
            }
            if (_joueurs[i].GetDoitMourir())
            {
                if (_joueurs[i].GetAEteSave() && (_joueurs[i].GetRole() is not Dictateur || !((Dictateur)_joueurs[i].GetRole()).GetATueInnocent()))
                {
                    _joueurs[i].SetAEteSave(false);
                    _joueurs[i].SetDoitMourir(false);
                }
                else
                {
                    if (_joueurs[i].GetEstMaire())
                    {
                        maireMort = _joueurs[i];
                    }
                    if (_joueurs[i]!=null && _joueurs[i].GetAmoureux() != null)
                    {
                        _joueurs[i].GetAmoureux().SetDoitMourir(true);
                        _joueurs[i].GetAmoureux().SetAEteSave(false);
                        _joueurs[i].GetAmoureux().SetAmoureux(null); 
                        _joueurs[i].SetAmoureux(null);
                        reboucle = true;
                    }
                    if (_joueurs[i].GetRole() is Chasseur)
                    {
                        _joueurs[i].SetDoitMourir(false);
                        chasseurMort = _joueurs[i];
                    }
                    else
                    {
                        _joueurs[i].TuerJoueur(listJoueurs);
                    }
                    
                    ConcatRecit("Une victime est allonge au centre du village. Il sagit de " + _joueurs[i].GetPseudo() + " qui savrait tre " + _joueurs[i].GetRole() + "  ses temps perdus. ","A victim is lying in the center of the village. It is "+ _joueurs[i].GetPseudo() +" who turned out to be "+_joueurs[i].GetRole()+".");
                }
            }
        }

        if (chasseurMort != null)
        {
            LanceAction(typeof(Chasseur));
            Joueur? victimeChasseur;
            victimeChasseur = ((Chasseur)chasseurMort.GetRole()).GetVictime();
            if (victimeChasseur != null)
            {
                if (victimeChasseur.GetEstMaire())
                {
                    maireMort = victimeChasseur;
                }
                victimeChasseur.TuerJoueur(listJoueurs);
            }
            chasseurMort.TuerJoueur(listJoueurs);
        }
        
        if (maireMort != null)
        {
            List<int> joueursEnVie = new List<int>();
            foreach (Joueur j in listJoueurs)
            {
                if (j.GetEnVie() && j != maireMort)
                {
                    joueursEnVie.Add(j.GetId());
                }
            }
            if (joueursEnVie.Count == 0)
            {
                // La liste est vide donc tout le monde est mort
                return;
            }
            int idSuccesseur = DecisionDuMaire(_joueurs,253);
            if (idSuccesseur == -1)
            {
                // ON CHOISIT UN NOUVEAU MAIRE ALEATOIREMENT SI IL N'A PAS VOTE
                Random random = new Random();
                idSuccesseur = joueursEnVie[random.Next(joueursEnVie.Count)];
            }
            Joueur? player = listJoueurs.Find(j => j.GetId() == idSuccesseur);
            if(player!=null){
                player.SetEstMaire(true);
            }
            sendMaire(listJoueurs,idSuccesseur);
            if(player!=null){
                ConcatRecit("Alors quil sappretait  mourir, le maire demanda au village decouter ses dernieres paroles. Il decide de nommer " + player.GetPseudo() + " comme son successeur la tete du village.","As he was about to die, the mayor asked the village to listen to his last words. He decides to appoint "+ player.GetPseudo() +" as his successor at the head of the village.");
            }
            // on enlve le statut de maire  l'ancien maire
            maireMort.SetEstMaire(false);
        }

    }

    /**
        La méthode Check_win de la classe Game permet de déterminer qui a gagné la partie en analysant l'état des joueurs en vie. 
        Elle retourne un entier qui indique si personne n'a encore gagné (0), 
        si le village a gagné (1), 
        si les loups ont gagné (2), 
        si le couple a gagné (3) 
        s'il y a égalité (4).
    */
    int Check_win(List<Joueur> listJoueurs)
    {
        int compVillage = 0, compLoups = 0;
        int retour = 0;
        bool coupleEnVie = false;
        int sum_vill_loups = 0;

        for (int i = 0; i < _joueurs.Count; i++)
        {
            if (_joueurs[i].GetEnVie())
            {
                if (_joueurs[i].GetAmoureux() != null && !coupleEnVie)
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
        sum_vill_loups = compLoups + compVillage;
        if (compVillage == 0)
        {
            if (compLoups == 0)
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
        else if (sum_vill_loups == 2)
        {
            if (coupleEnVie)
            {
                retour = 3;
            }
        }

        // check la valeur de checkWin si on veut envoyer qui a gagn 
        if (retour != 0)
        {
            List<Socket> sockets = new List<Socket>();
            int[] id = new int[listJoueurs.Count];
            int[] idr = new int[listJoueurs.Count];
            int i = 0;
            foreach(Joueur j in listJoueurs)
            {
                id[i] = j.GetId();
                idr[i] = j.GetRole().GetIdRole();
                if (j.GetSocket()!=null&&j.GetSocket().Connected)
                {
                    sockets.Add(j.GetSocket());
                }
                i++;
            }
            
            Messages.sendEndState(sockets, retour, id, idr);
        }
        return retour;
    }

    /**
        Cette méthode permet d'élire un maire parmi les joueurs encore en vie. 
        Elle prends en paramètre, sous forme d'un tableau, 
        le résultat du vote du village établi par la fonction VoteToutLeMonde.
    */
    private void ElectionMaire(List<int> cible, List<Joueur> listJoueurs)
    {
        // ici on a le r sultat final du vote
        if (cible == null)
        {
            return;
        }
        Dictionary<int, int> occurrences = new Dictionary<int, int>();
        for (int i = 0; i < cible.Count; i++)
        {
            if (cible[i] != -1)
            {
                // V rification si le nombre existe d j  dans le dictionnaire
                if (occurrences.ContainsKey(cible[i]))
                {
                    // Si oui, on incr mente son compteur
                    occurrences[cible[i]]++; // VOIR SI CA FONCTIONNE BIEN
                }
                else
                {
                    // Sinon, on l'ajoute avec un compteur initialis    1
                    occurrences.Add(cible[i], 1);
                }
            }
        }

        // determine la cible qui poss de le + de votes
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

        // regarde si il existe plusieurs victimes poss dant le nombre maximal de vote
        Random random = new Random();
        if (victime != -1)
        {
            bool estMultiple = occurrences.Count(x => x.Value == maxVotes) > 1;

            if (estMultiple) // si il y a plusieurs occurences ( = si le village ne s'est pas mis d'accord sur qui  lire maire)
            {
                // alors on cr   une liste qui recense toutes les victimes  galit s
                List<int> tiedVictims = new List<int>();
                foreach (KeyValuePair<int, int> pair in occurrences)
                {
                    if (pair.Value == maxVotes)
                    {
                        tiedVictims.Add(pair.Key);
                    }
                }

                // ON CHOISIT UN MAIRE ALEATOIREMENT
                victime = tiedVictims[random.Next(tiedVictims.Count)];
            }
        }
        else
        {
            // ON CHOISIT UN MAIRE ALEATOIREMENT PARMIS LES GENS EN VIE
            List<int> joueursEnVie = new List<int>();
            foreach (Joueur j in listJoueurs)
            {
                if (j.GetEnVie())
                {
                    joueursEnVie.Add(j.GetId());
                }
            }
            victime = joueursEnVie[random.Next(joueursEnVie.Count)];
        }

        Joueur? playerVictime = listJoueurs.Find(j => j.GetId() == victime);
        if(playerVictime!=null){
            playerVictime.SetEstMaire(true);
            sendMaire(listJoueurs,playerVictime.GetId());
        }
    }

    /**
        Cette méthode envoie un message à tous les joueur pour informer que le joueur avec l'identifiant "maire" est élu maire.
    */
    public void sendMaire(List<Joueur> list,int maire){
        foreach(Joueur j in list){
            Messages.sendMaire(j.GetSocket(),maire);
        }

    }
    
    /**

    */
    public void SendEgalite(List<Joueur> client, List<int> victime)
    {
        int [] ids=victime.ToArray();
        foreach (Joueur j in client)
        {
            if(j.GetSocket()!=null && j.GetSocket().Connected)
                Messages.SendVictime(j.GetSocket(),ids);
        }
    }

    /**
        La méthode implémente la phase de vote pendant le jour et décide qui doit être tué en fonction des votes des joueurs. 
        La méthode affecte le statut "doitMourir" au joueur victime et affiche un message en conséquence dans le récit de la partie.
    */
    public void SentenceJournee(List<int> cible, List<Joueur> listJoueurs)
    {
        // ici on a le r sultat final du vote
        if (cible == null)
            return;
        Dictionary<int, int> occurrences = new Dictionary<int, int>();
        for (int i = 0; i < cible.Count; i++)
        {
            if (cible[i] != -1)
            {
                // V rification si le nombre existe d j  dans le dictionnaire
                if (occurrences.ContainsKey(cible[i]))
                {
                    // Si oui, on incr mente son compteur
                    occurrences[cible[i]]++; // VOIR SI CA FONCTIONNE BIEN
                }
                else
                {
                    // Sinon, on l'ajoute avec un compteur initialis    1
                    occurrences.Add(cible[i], 1);
                }
            }
        }

        // determine la cible qui poss de le + de votes
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

        // regarde si il existe plusieurs victimes poss dant le nombre maximal de vote
        if (victime != -1)
        {
            bool estMultiple = occurrences.Count(x => x.Value == maxVotes) > 1;

            if (estMultiple) // si il y a plusieurs occurences ( = si les loups ne sont pas mis d'accord sur qui tuer)
            {
                // alors on cr   une liste qui recense toutes les victimes  galit s
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
                    Joueur? j=listJoueurs.Find(j => j.GetId() == tiedVictims[i]);
                    if(j!=null){
                        tiedVictimsJoueur.Add(j);
                    }
                }
                SendEgalite(listJoueurs,tiedVictims);
                victime = DecisionDuMaire(tiedVictimsJoueur,254);
            }

            Joueur? playerVictime = listJoueurs.Find(j => j.GetId() == victime);
            if (playerVictime != null)
            {
                playerVictime.SetDoitMourir(true);
                ConcatRecit("Les habitants du village debatent et decide de pointer " + playerVictime.GetPseudo() +" comme responsable des catastrophes du village... Ils decident de le tuer sur la place publique.","The inhabitants of the village discuss and decide to point out "+ playerVictime.GetPseudo() +" as responsible for the disasters of the village. They decide to kill him in the public square.");
            }
            else
            {
                ConcatRecit("Les habitants du village debatent mais narrivent pas a trouver de solution au probleme... Ils decident de rentrer calmement chez eux. ","The inhabitants of the village discuss but can't find a solution to the problem. They decide to go home quietly.");
            }
        }
    }

    /**
        La méthode permet de collecter les votes de tous les joueurs vivants. 
        La méthode retourne une liste des cibles votées pour chaque joueur, ou une liste vide si il y a eu une erreur.
    */
    private List<int> VoteToutLeMonde(List<Joueur> listJoueurs, int idRole)
    {
        try{

        if (CountSockets() == 0)
        {
            Messages.games.Remove(GetGameId());
            foreach (Joueur j in _joueurs)
            {
                Messages.players.Remove(j.GetId());
                _joueurs.Remove(j);
            }
            return new List<int>();
        }
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
        // on definit une "alarme" sur 60 secondes
        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if(Game.listener.LocalEndPoint!=null){
            reveille.Connect(Game.listener.LocalEndPoint);
        }
        Socket vide;
        Console.WriteLine("3");
        vide = Game.listener.Accept();
        Console.WriteLine("4");
        bool reduceTimer = false, LaunchThread2 = false, firstTime = true;
        r.sendTime(listJoueurs, Role.GetDelaiAlarme()*3,this);
        Task.Run(() =>
        {
            Thread.Sleep(Role.GetDelaiAlarme() * 2500); // 45 secondes
            reduceTimer = true;
            if (reduceTimer && !LaunchThread2)
            {
                Thread.Sleep(Role.GetDelaiAlarme() * 500); // 10 secondes
                boucle = false;
                vide.Send(new byte[1] { 0 });
            }
        });
        Console.WriteLine("5");
        int index, v, c;
            
        r.gameListener = reveille;
        while (boucle)
        {
            Console.WriteLine("6");
            
            (v, c) = r.gameVote(listJoueurs, idRole, reveille);
            if(v==-2&&c==-2){
                return new List<int>();
            }
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
                    Console.WriteLine("8");
                    index = votant.IndexOf(v);
                    Console.WriteLine("index="+v);
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
                            r.sendTime(listJoueurs, Role.GetDelaiAlarme()/2,this);
                        Task.Run(() =>
                        {
                            Thread.Sleep(Role.GetDelaiAlarme() * 500); // 10
                            Console.WriteLine("tout le monde a vot ,  a passe   10sec d'attente");
                            boucle = false;
                            vide.Send(new byte[1] { 0 });
                        });
                    }
                }
            }
        }
         for(int i=0;i<cible.Count;i++)
        {
            Console.WriteLine("Le joueur avec l'id " + votant[i] + " a voté pour " + cible[i]);
        }
        return cible;
        }catch(Exception e ){
            Console.WriteLine(e.Message);
            return new List<int>();
        }
    }

    /**
        La méthode initialise le jeu en mélangeant aléatoirement les rôles et en les assignant aux joueurs. 
        Elle envoie également à chaque joueur son propre rôle en appelant la méthode "sendRoles". 
        Si un joueur Cupidon est présent dans la partie, cette méthode l'appelle également.
    */
    private void InitiateGame()
    {
        Random random = new Random();
        _roles = _roles.OrderBy(r => random.Next()).ToList();
        
        for (int i = 0; i < _joueurs.Count; i++)
        {
            _joueurs[i].SetRole(_roles[i]);
        }
        Console.WriteLine("count == " + _joueurs.Count);
        foreach (Joueur j in _joueurs)
        {
            sendRoles(j);
        }
        // appeller Cupidon si il y en a un
    }

    /**
        Cette méthode envoie les rôles de tous les joueurs à un joueur spécifique.
    */
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
            Messages.sendRoles(j.GetSocket(), id, rolesToSend);
        
    }

    /**
        Cette méthode sendGameInfo permet d'envoyer les informations générales du jeu à un socket spécifique. 
        Les informations envoyées incluent le nombre total de joueurs, le nombre de loups-garous dans le jeu, et les rôles des joueurs.
    */
    public void sendGameInfo(Socket sock)
    {
        int[] id = new int[_joueurs.Count];
        string[] name = new string[_joueurs.Count];
        bool[] ready = new bool[_joueurs.Count];
        
        for (int i = 0; i < _joueurs.Count; i++)
        {
            id[i] = _joueurs[i].GetId();
            name[i] = _joueurs[i].GetPseudo();
            ready[i] = _joueurs[i].GetReady();
        }
        
        
        Messages.sendGameInfo(sock,_nbrJoueurs,_nbLoups,sorciere,voyante,cupidon,chasseur,guardien,dictateur, this.name, id, name,ready);
    }

    /**
        Cette méthode envoie l'état de la journée / nuit actuel du jeu à tous les joueurs connectés.
    */
    public void sendGameState(bool day)
    {
        foreach (Joueur j in _joueurs)
        {
            if (j.GetSocket()!=null && j.GetSocket().Connected )
            Messages.etatGame(j.GetSocket(), day);
        }
    }

    public int GetJoueurManquant()
    {
        return _nbrJoueursManquants;
    }
    
    /**
        La méthode vérifie si les rôles dans la partie sont valides pour la lancer. 
        Elle compte le nombre de loups et le nombre de villageois
        Elle vérifie si le nombre de loups est supérieur à la moitié du nombre total de joueurs ou 
        si le nombre de loups est égal à zéro. 
        Si l'une de ces conditions n'est pas remplie ou si le nombre de joueurs manquants n'est pas compris entre 3 et 13 inclus, la méthode retourne false. 
        Sinon, elle retourne true.
    */
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
    //La fonction s'occupe de lier les joueurs encore connect  au serveur
    public void EndGameInitializer()
    {
        foreach(Joueur j in _joueurs)
        {
            
            if(j.GetSocket()!=null && j.GetSocket().Connected)
            {
                Console.WriteLine("debut de suppression ");
                Messages.connected.Add(j.GetSocket(), j.GetId());
                Messages.userData[j.GetId()].SetStatus(j.GetId(), 1);
            }
            
            Messages.players.Remove(j.GetId());
        }
        Messages.games.Remove(gameId);
        Messages.WakeUpMain();
    }
    /**
        La méthode permet de gérer la décision du maire lors de différentes phases de vote du jeu 
        pour éliminer un joueur en cas d'égalité, pour désigner son sucesseur, ...
    */
    public int DecisionDuMaire(List<Joueur> listJoueurs,int role)
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
        r.sendTurn(_joueurs, role);

        Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if(Game.listener.LocalEndPoint!=null){
            reveille.Connect(Game.listener.LocalEndPoint);
        }
        Socket vide;
        vide = Game.listener.Accept();
        
        bool boucle = true;
        r.sendTime(_joueurs, Role.GetDelaiAlarme()/2,this);
        Task.Run(() =>
        {
            Thread.Sleep(Role.GetDelaiAlarme() * 500); // 10 secondes
            vide.Send(new byte[1] { 0 });
            boucle = false;
        });

        int v, c;
        Joueur? player = null;
        Console.WriteLine("Le maire doit prendre sa d cision");
        
        r.gameListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        if(Game.listener.LocalEndPoint!=null){
            r.gameListener.Connect(Game.listener.LocalEndPoint);
        }
        this.vide = listener.Accept();
        while (boucle)
        {
            (v, c) = r.gameVote(_joueurs, role, reveille);
            if(v==-2&&c==-2){
                
            }
                
            if (maire!=null && v == maire.GetId())
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

    /**
        La méthode ToggleReady permet de changer l'état "prêt" d'un joueur donné en fonction de son ID et le nouveau statut est envoyé à tous les autres joueurs. 
        Ensuite, la méthode vérifie si tous les joueurs sont prêts. 
        Si tel est le cas, la méthode Start() est appelée pour lancer la partie. 
        Sinon, rien ne se passe.
    */
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
            Messages.sendReady(j.GetSocket(), id, ready);
        }
        
        if (sum == _nbrJoueurs)
        {
            foreach (Joueur j in _joueurs)
            {
                Messages.connected.Remove(j.GetSocket());
                Messages.userData[j.GetId()].SetStatus(j.GetId(), 3);
            }
            Console.WriteLine("on lance la game");
            Task.Run(() =>
            {
                try{

                Start();
                }catch(Exception e){
                    Console.WriteLine("message = "+e.ToString());
                    List<Socket> sockets = new List<Socket>();
                    foreach(Joueur j in _joueurs){
                        sockets.Add(j.GetSocket());
                    }
                        Messages.sendEndState(sockets, 0, new int[0], new int[0]);
                    
                    EndGameInitializer();
                }
            });
        }

        }
    }

    public int[] GetRolesJoueurs(){
        return rolesJoueurs;
    }

    /**
        La methode calcule les scores des joueurs à la fin d'une partie et envoie ces scores aux joueurs. 
        Le paramètre check indique quel type de score doit être calculé, en fonction de qui a gagné ou perdu.
    */
    public void PointShare(int check) {
        int []id = new int[_nbrJoueurs];
        int []score = new int[_nbrJoueurs];
        int i = 0;
        bool [] victoire = new bool[_nbrJoueurs]; 
        foreach(Joueur joueur in _joueurs) 
        {
            id[i] = joueur.GetId();
            victoire[i]=false;
            score[i]=0;
            if(check == 1) 
	    {
                if(joueur.GetRole() is not Loup) 
		{
                    if(joueur.GetEnVie()) 
		    {
                        victoire[i]=true;
                        score[i] = 10;
                    }
                    else 
		    {
                        victoire[i]=true;
                        score[i] = 5;
                    }
                }
            }
            else if(check == 2) 
	    {
                if(joueur.GetRole() is Loup) 
		{
                    if(joueur.GetEnVie()) 
		    {
                        victoire[i]=true;
                        score[i] = 10;
                    }
                    else 
		    {
                        victoire[i]=true;
                        score[i] = 5;
                    }
                }
            }
            else if(check == 3) 
	    {
                if(joueur.GetEnVie()) 
		        {
                    victoire[i]=true;
                    score[i] = 10;
                }
            }
            else if(check == 4) 
	    {
                if(joueur.GetEnVie()) 
		{
                    victoire[i]=true;
                    score[i] = 5;
                }
                else 
		{
                    victoire[i]=true;
                    score[i] = 2;
                }
            }
            i++;
        }
        saveGame(recit,recit_ang,score,victoire);
        SendPoints(_joueurs, id, score);
    }

    /**
        Cette méthode de la classe Game envoie les scores calculés aux joueurs dans la liste spécifiée 
        pour permettre aux joueurs de voir les résultats de la partie et de leur contribution au jeu.
    */
    public void SendPoints(List<Joueur> listJoueur, int[] id, int[] score)
    {
        foreach(Joueur j in listJoueur)
        {
            Messages.sendScore(j.GetSocket(), id, score);
        }
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

    /**
        Cette méthode permet de concaténer deux chaînes de caractères s et s2 à deux variables de classe recit et recit_ang, respectivement. 
        Ces variables sont utilisées pour stocker le récit du déroulement de la partie, en français (recit) et en anglais (recit_ang).
    */
    public void ConcatRecit(string s,String s2)
    {
        recit = recit + s;
        recit_ang = recit_ang + s2;
    }
}