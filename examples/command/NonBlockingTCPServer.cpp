#include "NonBlockingTCPServer.h"
#include <iostream>
#include <cstring>
#include <sys/socket.h>
#include <sys/select.h>
#include <netinet/in.h>
#include <unistd.h>
#include <fcntl.h>

NonBlockingTCPServer::NonBlockingTCPServer() : serverSocket(0), maxFd(0), lastSendTime(time(nullptr)) {}

void NonBlockingTCPServer::setupServer(int port) {
    serverSocket = socket(AF_INET, SOCK_STREAM, 0);

    struct sockaddr_in serverAddress;
    serverAddress.sin_family = AF_INET;
    serverAddress.sin_port = htons(port);
    serverAddress.sin_addr.s_addr = INADDR_ANY;

    bind(serverSocket, (struct sockaddr*)&serverAddress, sizeof(serverAddress));

    listen(serverSocket, 5);
    fprintf(stderr, "[socket] server initialized, bound and listening");
}

void NonBlockingTCPServer::acceptConnections() {
    fd_set readFds;
    FD_ZERO(&readFds);
    FD_SET(serverSocket, &readFds);

    for (int clientSocket : clientSockets) {
        FD_SET(clientSocket, &readFds);
        maxFd = std::max(maxFd, clientSocket);
    }

    int activity = select(maxFd + 1, &readFds, nullptr, nullptr, nullptr);
    if (activity == -1) {
        perror("select");
        return;
    }

    if (FD_ISSET(serverSocket, &readFds)) {
        fprintf(stderr, "[socket] Client found, ready for reading");
        int clientSocket = accept(serverSocket, nullptr, nullptr);
        fcntl(clientSocket, F_SETFL, O_NONBLOCK);
        clientSockets.insert(clientSocket);
    }
}

void NonBlockingTCPServer::sendDataToClients(const char* messageToSend) {
    if (messageToSend == nullptr || *messageToSend == '\0')
    {
        fprintf(stderr, "[socket] null or empty c_str provided, not sending data");
        return;
    }
    fd_set readFds;
    for (int clientSocket : clientSockets) {
        send(clientSocket, messageToSend, strlen(messageToSend), 0);
    }
    fprintf(stderr, "[socket] data sent");
}

void NonBlockingTCPServer::receiveDataFromClients() {
    fd_set readFds;
    for (int clientSocket : clientSockets) {
        if (FD_ISSET(clientSocket, &readFds)) {
            char buffer[1024];
            ssize_t bytesReceived = recv(clientSocket, buffer, sizeof(buffer), 0);
            if (bytesReceived <= 0) {
                close(clientSocket);
                clientSockets.erase(clientSocket);
            } else {
                buffer[bytesReceived] = '\0';
                std::cout << "Received from client " << clientSocket << ": " << buffer << std::endl;
            }
        }
    }
}

void NonBlockingTCPServer::closeServer() {
    for (int clientSocket : clientSockets) {
        close(clientSocket);
    }
    close(serverSocket);
}