using System.Net.Sockets;

namespace LGproject;

public class Client
{
    private int id;
    private Socket socket;
    private string pseudo;
    public Client(int idClient, Socket socketClient, string pseudoClient) {
        id = idClient;
        socket = socketClient;
        pseudo = pseudoClient;
    }

    public int GetId(){
        return id;
    }

    public Socket GetSocket(){
        return socket;
    }

    public string GetPseudo(){
        return pseudo;
    }
}