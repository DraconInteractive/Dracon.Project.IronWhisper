#include "NonBlockingTCPServer.h"
#include <iostream>
#include <cstring>
#include <sys/socket.h>
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
}

void NonBlockingTCPServer::acceptConnections() {
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
        int clientSocket = accept(serverSocket, nullptr, nullptr);
        fcntl(clientSocket, F_SETFL, O_NONBLOCK);
        clientSockets.insert(clientSocket);
    }
}

void NonBlockingTCPServer::sendDataToClients() {
    time_t currentTime = time(nullptr);
    if (currentTime - lastSendTime >= 5) { // Send data every 5 seconds
        for (int clientSocket : clientSockets) {
            std::string message = "Periodic update from server!";
            send(clientSocket, message.c_str(), message.length(), 0);
        }
        lastSendTime = currentTime;
    }
}

void NonBlockingTCPServer::receiveDataFromClients() {
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
#include "NonBlockingTCPServer.h"
#include <iostream>
#include <cstring>
#include <sys/socket.h>
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
}

void NonBlockingTCPServer::acceptConnections() {
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
        int clientSocket = accept(serverSocket, nullptr, nullptr);
        fcntl(clientSocket, F_SETFL, O_NONBLOCK);
        clientSockets.insert(clientSocket);
    }
}

void NonBlockingTCPServer::sendDataToClients() {
    time_t currentTime = time(nullptr);
    if (currentTime - lastSendTime >= 5) { // Send data every 5 seconds
        for (int clientSocket : clientSockets) {
            std::string message = "Periodic update from server!";
            send(clientSocket, message.c_str(), message.length(), 0);
        }
        lastSendTime = currentTime;
    }
}

void NonBlockingTCPServer::receiveDataFromClients() {
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
