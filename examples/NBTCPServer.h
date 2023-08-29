#ifndef NONBLOCKINGTCPSERVER_H
#define NONBLOCKINGTCPSERVER_H

#include <set>
#include <ctime>

class NonBlockingTCPServer {
private:
    int serverSocket;
    std::set<int> clientSockets;
    fd_set readFds;
    int maxFd;
    time_t lastSendTime;

public:
    NonBlockingTCPServer();

    void setupServer(int port);
    void acceptConnections();
    void sendDataToClients();
    void receiveDataFromClients();
    void closeServer();
};

#endif // NONBLOCKINGTCPSERVER_H
