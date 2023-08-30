#include "NonBlockingTCPServer.h"
#include <iostream>
#include <cstring>
#include <sys/socket.h>
#include <sys/select.h>
#include <netinet/in.h>
#include <unistd.h>
#include <fcntl.h>
#include <arpa/inet.h>

NonBlockingTCPServer::NonBlockingTCPServer() : serverSocket(0), maxFd(0), lastSendTime(time(nullptr)) {}

void NonBlockingTCPServer::setupServer(int port) {
    serverSocket = socket(AF_INET, SOCK_STREAM, 0);

    struct sockaddr_in serverAddress;
    serverAddress.sin_family = AF_INET;
    serverAddress.sin_port = htons(port);
    serverAddress.sin_addr.s_addr = inet_addr("172.28.31.175");

    bind(serverSocket, (struct sockaddr*)&serverAddress, sizeof(serverAddress));

    listen(serverSocket, 5);
    fprintf(stdout, "[socket] server initialized, bound and listening\n");
} 

void NonBlockingTCPServer::acceptConnections() {

    int error = 0;
    socklen_t errorLen = sizeof(error);

    if (getsockopt(serverSocket, SOL_SOCKET, SO_ERROR, &error, &errorLen) != 0)
    {
        fprintf(stdout, "getsockopt error\n");
    }
    if (error == 0)
    {
        fprintf(stdout, "socket is open\n");
    }
    else {
        fprintf(stdout, "socket is closed/errored\n");
        fprintf(stdout, "socket error: %s\n", strerror(error));
    }
    fprintf(stdout, "[socket] checking for connections... \n");
    fd_set readFds;
    FD_ZERO(&readFds);
    FD_SET(serverSocket, &readFds);

    maxFd = serverSocket;

    for (int clientSocket : clientSockets) {
        FD_SET(clientSocket, &readFds);
        maxFd = std::max(maxFd, clientSocket);
    }

    struct timeval timeout;
    timeout.tv_sec = 2;
    timeout.tv_usec = 0;
    //int activity = select(maxFd + 1, &readFds, nullptr, nullptr, &timeout);
    int activity = select(maxFd + 1, &readFds, nullptr, nullptr, nullptr);
    fprintf(stdout, "[socket] select activity: %d\n", activity);
    if (activity == -1) {
        perror("select");
        return;
    }

    if (FD_ISSET(serverSocket, &readFds)) {
        fprintf(stdout, "[socket] Client found, ready for reading\n");
        int clientSocket = accept(serverSocket, nullptr, nullptr);
        fcntl(clientSocket, F_SETFL, O_NONBLOCK);
        clientSockets.insert(clientSocket);
    }
    else 
    {
        fprintf(stdout, "[socket] no clients found\n");
    }
}

void NonBlockingTCPServer::sendDataToClients(const char* messageToSend) {
    if (messageToSend == nullptr || *messageToSend == '\0')
    {
        fprintf(stderr, "[socket] null or empty c_str provided, not sending data\n");
        return;
    }
    fd_set readFds;
    for (int clientSocket : clientSockets) {
        send(clientSocket, messageToSend, strlen(messageToSend), 0);
    }
    fprintf(stdout, "[socket] data sent\n");
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