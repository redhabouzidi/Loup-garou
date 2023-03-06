using System.Net.Sockets;

namespace LGproject;

// on part du principe que la partie se lance à 6 joueurs

public class Game
{
    private List<Joueur> _joueurs;
    private List<Role> _roles;
    private int _nbrJoueursManquants;
    public static Socket listener = Server.server.setupSocketGame();

    public Game(Client c)
    {
        _nbrJoueursManquants = 4;  // A ENLEVER PLUS TARD "=6"
        // création de la liste de joueurs et de rôles
        _roles = new List<Role>();
        // la partie est créé maintenant j'attends les input du frontend et j'envoie mon client à waiting screen
        // on va admettre que joueurs max = 6
        Role[] startingRoles = new Role[] {
            new Voyante(),
            new Sorciere(),
            new Loup(),
            new Villageois()
        };

        foreach (Role role in startingRoles) {
            
            _roles.Add(role);
        }

        _joueurs = new List<Joueur>();
        Joueur j = new Joueur(c.GetId(), c.GetSocket(), c.GetPseudo());
        _joueurs.Add(j);

        Waiting_screen();
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

        // fonction qui check si tout va bien

        bool inProgressGame = true;
        while (inProgressGame)
        {
            for (int i = 0; i < _joueurs.Count; i++)
            {
                Console.WriteLine(_joueurs[i].GetPseudo() + " a comme rôle : " + _joueurs[i].GetRole() + " status enVie : " + _joueurs[i].GetEnVie() + " status doitMourir : " + _joueurs[i].GetDoitMourir());
                if (_joueurs[i].GetAmoureux() is not null)
                {
                    Console.WriteLine("\t en + ce mec est amoureux !");
                }
            }
            // broadcast du serveur : c'est la nuit

            // appeller Voyante si il y en a un

            LanceAction(typeof(Voyante));

            // appeller Loup si il y en a un --> bizzare à implémenter en full client side
            LanceAction(typeof(Loup));
            
            LanceAction(typeof(Sorciere));

            for (int i = 0; i < _joueurs.Count; i++)
            {
                if(_joueurs[i].GetDoitMourir()){
                    _joueurs[i].SetDoitMourir(false);
                    _joueurs[i].SetEnVie(false);
                }
            }
        }

        // algo de la partie
        // broadcast du serveur : c'est le jour

        // faire un for qui parcours la liste de joueurs et qui fait un appele à setJoueursList(joueurs)
        // Console.WriteLine("je rentre dans l'algorithme de la partie !");
    }

    private void InitiateGame()
    {
        // mélanger le tableau roles aléatoirement
        Random random = new Random();
        _roles = _roles.OrderBy(r => random.Next()).ToList();

        for (int i = 0; i < _joueurs.Count; i++) {
            _joueurs[i].SetRole(_roles[i]);
        }

        // appeller Cupidon si il y en a un
        LanceAction(typeof(Cupidon));
    }
    public int GetJoueurManquant()
    {
        return _nbrJoueursManquants;
    }
}