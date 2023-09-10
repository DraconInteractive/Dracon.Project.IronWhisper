﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IronWhisperReceiver
{
    public class Token
    {
        public string Text;
        public string Lemma;
        public string Pos;
    }

    public class SocketController
    {
        public static SocketController Instance;

        readonly string serverIP = "172.28.31.175"; // Change this to the server's IP address
        readonly int serverPort = 31050;            // Change this to the server's port number
        readonly string voicePrompt = "Okay,";

        TcpClient client;
        NetworkStream stream;

        public SocketController()
        {
            client = new TcpClient();
            Instance = this;
        }

        public bool TryConnect ()
        {
            try
            {
                client.Connect(serverIP, serverPort);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public void Connect ()
        {
            while (!client.Connected)
            {
                var result = TryConnect();
                if (!result)
                {
                    Thread.Sleep(100);
                }
            }

            Console.WriteLine($"[Socket] Connected at {serverIP}:{serverPort}");
            Console.WriteLine();
        }

        public void StartStream ()
        {
            stream = client.GetStream();
        }

        public TCommand SocketTick ()
        {
            // TODO - add | onto end of each command to allow for delayed commands to be separated
            try
            {
                if (stream.DataAvailable)
                {
                    // Receive and display the response from the server
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    //string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    var command = DeserializeData(buffer);
                    Console.WriteLine($"Deserialized Command:\n{JsonConvert.SerializeObject(command, Formatting.Indented)}\n");
                    return command;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Socket error occurred: {ex.Message}\n{ex.StackTrace}");
            }
            return null;
        }

        public void CloseStream ()
        {
            stream.Close();
            stream.Dispose();
        }

        public void Cleanup ()
        {
            CloseStream();
            client.Close();
        }

        public TCommand DeserializeData(byte[] serializedData)
        {
            string serializedString = Encoding.UTF8.GetString(serializedData);
            string[] messageSplit = serializedString.Split(">>");
            string[] tokenStrings = messageSplit[1].Split('&');
            List<Token> tokens = new List<Token>();

            foreach (string tokenString in tokenStrings)
            {
                if (string.IsNullOrEmpty(tokenString))
                {
                    continue;
                }

                string[] fields = tokenString.Split('|');
                if (fields.Length < 3)
                {
                    // Not enough fields
                    continue;
                }

                Token token = new Token
                {
                    Text = fields[0],
                    Lemma = fields[1],
                    Pos = fields[2],
                };
                tokens.Add(token);
            }
            TCommand command = new TCommand(voicePrompt, messageSplit[0], tokens.ToArray());
            return command;

        }
    }

    public class TCommand
    {
        private string[] _punctuation = new string[] { ",", ".", ":", "?", "!" };

        public string Message;
        public string Command;

        public Token[] tokens;

        public TCommand (string prompt, string message, Token[] _tokens = null)
        {
            Message = message;
            Command = message.Replace(prompt, "").ToLower();

            foreach (var p in _punctuation)
            {
                Command = Command.Replace(p, "");
            }

            Message = Message.Trim();
            Command = Command.Trim();

            if (_tokens != null)
            {
                tokens = _tokens;
            }
            else
            {
                tokens = Array.Empty<Token>();
            }
        }
    }
}
