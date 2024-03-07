
using Dapper;
using MySql.Data.MySqlClient;
public class dbMigration{
    public static void initMigration(){
        migrateTable(@"CREATE TABLE IF NOT EXISTS Utilisateurs (
                    idUsers INT AUTO_INCREMENT PRIMARY KEY,
                    email VARCHAR(255) NOT NULL UNIQUE,
                    pseudo VARCHAR(50) NOT NULL UNIQUE,
                    motdepasse VARCHAR(128) NOT NULL,
                    CONSTRAINT CK_UTILISATEURS_EMAIL CHECK (email REGEXP ('^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$')),
                    CONSTRAINT CK_PSEUDO CHECK (pseudo REGEXP '^[a-zA-Z][a-zA-Z0-9_]{4,50}$')
                );"
        );
        migrateTable(@"CREATE TABLE IF NOT EXISTS Amis(
                    idUsers1 INT NOT NULL,
                    idUsers2 INT NOT NULL,
                    status_ami BOOLEAN NOT NULL DEFAULT FALSE,
                    date_amis TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT Amis_PK PRIMARY KEY (idUsers1, idUsers2)
                );
        ");
        migrateTable(@"CREATE TABLE IF NOT EXISTS Partie(
                        idPartie INT PRIMARY KEY NOT NULL AUTO_INCREMENT, 
                        Datesauvegarde DATETIME,
                        nomPartie VARCHAR(255) NOT NULL 
                    );
");     
        migrateTable(@"CREATE TABLE IF NOT EXISTS SauvegardePartie(
                        idPartie INT,
                        idUsers INT,
                        scoregained INT DEFAULT 0,
                        PRIMARY KEY(idUsers, idPartie)
                    );
");
        migrateTable(@"CREATE TABLE IF NOT EXISTS Actions (
                        idPartie INT,
                        actions TEXT,
                        actionsEn TEXT
                    );
");
        migrateTable(@"CREATE TABLE IF NOT EXISTS Statistiques(
                        idStatistiques INT AUTO_INCREMENT PRIMARY KEY, 
                        idUsers INT,
                        score INT NOT NULL DEFAULT 0,
                        nb_partiejoue INT NOT NULL DEFAULT 0,
                        nb_victoire INT NOT NULL DEFAULT 0
                        );
");
        applyAlter(@"ALTER TABLE Amis ADD CONSTRAINT FK_Amis_idUsers1 FOREIGN KEY (idUsers1) REFERENCES Utilisateurs (idUsers) ON DELETE CASCADE;");
        applyAlter(@"ALTER TABLE Amis ADD CONSTRAINT FK_Amis_idUsers2 FOREIGN KEY (idUsers2) REFERENCES Utilisateurs (idUsers) ON DELETE CASCADE;");
        applyAlter(@"ALTER TABLE SauvegardePartie ADD CONSTRAINT FK_Sauvegarde_IdUsers FOREIGN KEY (IdUsers) REFERENCES Utilisateurs (IdUsers) ON DELETE CASCADE;");
        applyAlter(@"ALTER TABLE SauvegardePartie ADD CONSTRAINT FK_Sauvegarde_IdPartie FOREIGN KEY (IdPartie) REFERENCES Partie (IdPartie) ON DELETE CASCADE;");
        applyAlter(@"ALTER TABLE Actions ADD CONSTRAINT FK_Actions_IdPartie FOREIGN KEY (IdPartie) REFERENCES Partie (IdPartie) ON DELETE CASCADE;");
        applyAlter(@"ALTER TABLE Statistiques ADD CONSTRAINT FK_Statistiques_idUser FOREIGN KEY (idUsers) REFERENCES Utilisateurs (IdUsers) ON DELETE CASCADE;");
    }
    public static int migrateTable(String creation){
        return bdd.conn.Execute(creation);
    }
    public static int applyAlter(String alteration){
        try{
            return bdd.conn.Execute(alteration);
        }catch(MySqlException e){
            if(e.Number!=1826){
                Console.WriteLine("ERROR MYSQL EXCEPTION FOUND ON ALTERATION :"+e);

            }
            return e.Number;
        }

    }
}