namespace LGproject;

public class Villageois : Role
{
    private new const int IdRole = 1;
    public Villageois()
    {
        name = "Villageois";
        description = "blabla";
    }
    public override (string,string) Action(List<Joueur> listJoueurs,Game game)
    { // écrire l'action du villageois --> rien ?
        throw new NotImplementedException();
    }

    public override int GetIdRole() {
        return IdRole; 
    }

}