#ifndef NONBLOCKINGTCPSERVER_H
#define NONBLOCKINGTCPSERVER_H

#include <set>
#include <ctime>
#include <sys/select.h>
#include <vector>
#include <cstdint>

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
    void sendDataToClients(const char* messageToSend);
    void sendDataToClients(const std::vector<uint8_t>& serializedData);
    void receiveDataFromClients();
    void closeServer();
};

#endif // NONBLOCKINGTCPSERVER_H
