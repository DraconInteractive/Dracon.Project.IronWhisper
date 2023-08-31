#include "NonBlockingTCPServer.h"
#include <iostream>
#include <cstring>
#include <sys/socket.h>
#include <sys/select.h>
#include <netinet/in.h>
#include <unistd.h>
#include <fcntl.h>
#include <arpa/inet.h>

NonBlockingTCPServer::NonBlockingTCPServer() : serverSocket(0), maxFd(0), lastSendTime(time(nullptr)) 
{
    FD_ZERO(&readFds);
}

void printPeerInfo(int clientSocket) {
    struct sockaddr_in clientAddress;
    socklen_t clientAddressLen = sizeof(clientAddress);

    if (getpeername(clientSocket, (struct sockaddr*)&clientAddress, &clientAddressLen) == 0) {
        char clientIP[INET_ADDRSTRLEN];
        inet_ntop(AF_INET, &clientAddress.sin_addr, clientIP, sizeof(clientIP));
        printf("  IP: %s,\n  Port: %d\n", clientIP, ntohs(clientAddress.sin_port));
    } else {
        perror("getpeername");
    }
}

void checkSocketState(int clientSocket)
{
    int socketError = 0;
    socklen_t socketErrorLen = sizeof(socketError);

    if (getsockopt(clientSocket, SOL_SOCKET, SO_ERROR, &socketError, &socketErrorLen) == 0) {
        if (socketError == 0) {
            printf("Socket %d is open and error-free\n", clientSocket);
        } else {
            printf("Socket #%d error: %s\n", clientSocket, strerror(socketError));
        }
    } else {
        perror("getsockopt");
        // Handle the error
    }
}

void NonBlockingTCPServer::setupServer(int port) {
    // Create the server socket
    serverSocket = socket(AF_INET, SOCK_STREAM, 0);

    // Configure the server address
    struct sockaddr_in serverAddress;
    serverAddress.sin_family = AF_INET;
    serverAddress.sin_port = htons(port);
    serverAddress.sin_addr.s_addr = inet_addr("172.28.31.175");

    // Bind the server socket to the specified address and port
    bind(serverSocket, (struct sockaddr*)&serverAddress, sizeof(serverAddress));

    // Listen for incoming connections
    listen(serverSocket, 5);
    fprintf(stdout, "[socket] server initialized, bound and listening\n");
} 

void NonBlockingTCPServer::acceptConnections() {
    fprintf(stdout, "[socket] accepting connections\n");

    FD_ZERO(&readFds);

    // Add server socket to the set
    FD_SET(serverSocket, &readFds);

    fprintf(stdout, "[socket] assessing %zu clients...\n", clientSockets.size());
    fflush(stdout);

    // Find the maximum file descriptor in the set
    maxFd = serverSocket;

    // Iterate through each client socket in the collection
    for (int clientSocket : clientSockets) {
        fprintf(stdout, "%d: \n", clientSocket);
        // Check if valid and connected
        if (clientSocket >= 0)
        {
            FD_SET(clientSocket, &readFds);
            maxFd = std::max(maxFd, clientSocket);
            printPeerInfo(clientSocket);
            checkSocketState(clientSocket);
        }
    }

    // Set up timeout for select
    struct timeval timeout;
    timeout.tv_sec = 0;
    timeout.tv_usec = 250;

    // Perform select to monitor file descriptors for activity
    int activity = select(maxFd + 1, &readFds, nullptr, nullptr, &timeout);

    // Check for errors
    if (activity == -1) {
        perror("select");
        return;
    }

    if (activity >= 1)
    {
        // Check for new connections
        if (FD_ISSET(serverSocket, &readFds)) {
            fprintf(stdout, "[socket] Client found, ready for reading\n");

            // Accept a new client connection
            int clientSocket = accept(serverSocket, nullptr, nullptr);

            // Set the client socket to non-blocking mode
            int blockResult = fcntl(clientSocket, F_SETFL, O_NONBLOCK);
            if (blockResult == -1)
            {
                perror("fcntl");
            }
            // Add the new client socket to the collection
            clientSockets.insert(clientSocket);
        }
        else 
        {
            fprintf(stdout, "[socket] no clients found\n");
        }
    }
    else {
        fprintf(stdout, "[socket] no client activity\n");
    }

    
}

void NonBlockingTCPServer::sendDataToClients(const char* messageToSend) {
    // Check if the message is valid
    if (messageToSend == nullptr || *messageToSend == '\0')
    {
        fprintf(stderr, "[socket] null or empty c_str provided, not sending data\n");
        return;
    }

    // Iterate through each client socket in the collection
    for (int clientSocket : clientSockets) {
        // Send the message to the client
        ssize_t bytesSent = send(clientSocket, messageToSend, strlen(messageToSend), 0);
        fprintf(stdout, "sent %zd bytes\n", bytesSent);
        if (bytesSent == -1)
        {
            fprintf(stderr, "[socket] error sending packet to client: %d\n", clientSocket);
            perror("send");
        }
    }
}

void NonBlockingTCPServer::receiveDataFromClients() {
    // Iterate through each client socket in the collection
    for (int clientSocket : clientSockets) {
        // Check if the socket has data to be read
        if (FD_ISSET(clientSocket, &readFds)) {
            // Buffer to store received data
            char buffer[1024];

            // Receive data from the client
            ssize_t bytesReceived = recv(clientSocket, buffer, sizeof(buffer), 0);

            // Check if there's an issue with the connection
            if (bytesReceived == 0) {
                // Close the client socket and remove it from the collection
                close(clientSocket);
                clientSockets.erase(clientSocket);
            }
            else if (bytesReceived < 0)
            {
                perror("recv");
                close(clientSocket);
                clientSockets.erase(clientSocket);
            } 
            else 
            {
                // Null-terminate the received data and print it
                buffer[bytesReceived] = '\0';
                std::cout << "Received from client " << clientSocket << ": " << buffer << std::endl;
            }
        }
    }
}

void NonBlockingTCPServer::closeServer() {
    // Iterate through each client socket in the collection
    for (int clientSocket : clientSockets) {
        // Close each client socket
        close(clientSocket);
    }

    // Close the server socket
    close(serverSocket);
}