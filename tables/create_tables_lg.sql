CREATE SCHEMA IF NOT EXISTS lg_db;
USE lg_db ;

CREATE TABLE IF NOT EXISTS Utilisateurs (
  idUsers INT AUTO_INCREMENT PRIMARY KEY,
  email VARCHAR(255) NOT NULL UNIQUE,
  pseudo VARCHAR(50) NOT NULL UNIQUE,
  motdepasse VARCHAR(128) NOT NULL,
  CONSTRAINT CK_MDP CHECK (motdepasse REGEXP ("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])[a-zA-Z0-9!@#\$%\^&\*]{8,}$")),
  CONSTRAINT CK_UTILISATEURS_EMAIL CHECK (email REGEXP ("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")),
  CONSTRAINT CK_PSEUDO CHECK (pseudo REGEXP "^[a-zA-Z][a-zA-Z0-9_]{5,50}$")
);

CREATE TABLE IF NOT EXISTS Amis(
	  idUsers1 INT NOT NULL,
	  idUsers2 INT NOT NULL,
	  status_ami BOOLEAN NOT NULL DEFAULT FALSE,
	  date_amis TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	  CONSTRAINT Amis_PK PRIMARY KEY (idUsers1, idUsers2)
);

CREATE TABLE IF NOT EXISTS Partie(
	idPartie INT PRIMARY KEY NOT NULL AUTO_INCREMENT, 
	nomPartie VARCHAR(255) NOT NULL 
);

CREATE TABLE IF NOT EXISTS SauvegardePartie(
	idPartie INT,
	idUsers INT,
	Datesauvegarde DATE,
	PRIMARY KEY(idUsers, idPartie)
);

CREATE TABLE IF NOT EXISTS Actions (
	idAction INT AUTO_INCREMENT PRIMARY KEY,
	idPartie INT,
	act_temps TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	actions VARCHAR(1024)
);
CREATE TABLE IF NOT EXISTS Statistiques(
   idStatistiques INT PRIMARY KEY, 
   idUsers INT,
   score INT NOT NULL DEFAULT 0,
   nb_partiejoue INT NOT NULL DEFAULT 0,
   nb_victoire INT NOT NULL DEFAULT 0
);


ALTER TABLE Amis ADD CONSTRAINT FK_Amis_idUsers1 FOREIGN KEY (idUsers1) REFERENCES Utilisateurs (idUsers) ON DELETE CASCADE;
ALTER TABLE Amis ADD CONSTRAINT FK_Amis_idUsers2 FOREIGN KEY (idUsers2) REFERENCES Utilisateurs (idUsers) ON DELETE CASCADE;

ALTER TABLE SauvegardePartie ADD CONSTRAINT FK_Sauvegarde_IdUsers FOREIGN KEY (IdUsers) REFERENCES Utilisateurs (IdUsers) ON DELETE CASCADE;
ALTER TABLE SauvegardePartie ADD CONSTRAINT FK_Sauvegarde_IdPartie FOREIGN KEY (IdPartie) REFERENCES Partie (IdPartie) ON DELETE CASCADE;


#1 ->ALTER TABLE Partie ADD CONSTRAINT FK_Partie_idUsers FOREIGN KEY (idUsers) REFERENCES Utilisateurs (idUsers) ON DELETE CASCADE;


#2-->ALTER TABLE Statistiques ADD CONSTRAINT FK_Statistiques_idPartie FOREIGN KEY (IdPartie) REFERENCES Partie (IdPartie) ON DELETE CASCADE;
ALTER TABLE Statistiques ADD CONSTRAINT FK_Statistiques_idPartie FOREIGN KEY (idUsers) REFERENCES Utilisateurs (IdUsers) ON DELETE CASCADE;


INSERT INTO Utilisateurs VALUES(1,"sididiop094@gmail.com","Sidy71","Farmata50@");
INSERT INTO Utilisateurs VALUES(2,"ckkkskdskdsk_28@gmail.com","tomb_72122","ck71Kk29912272!!dkw");