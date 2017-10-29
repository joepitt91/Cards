using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace JoePitt.Cards.Net
{
    internal static class SharedNetworking
    {
        /// <summary>
        /// Uses the client link to send a command and return the response.
        /// </summary>
        /// <param name="client">The Player's Client Networking.</param>
        /// <param name="Command">The Command to be sent to the game host.</param>
        /// <returns>The Game host's response.</returns>
        /// <exception cref="SocketException">The Client's connection to the game host has dropped.</exception>
        /// <example>Communicate(playerNetwork, "HELLO")</example>
        public static string Communicate(ClientNetworking client, string Command)
        {
            client.NextCommand = Command;
            client.NewCommand = true;
            while (!client.NewResponse)
            {
                Application.DoEvents();
                if (client.Established && client.Dropped)
                {
                    throw new SocketException();
                }
            }
            client.NewResponse = false;
            return client.LastResponse;
        }

        public static byte[] GetBytes(Socket receiver)
        {
            try
            {
                byte[] message = new byte[4];
                receiver.Receive(message);
                int length = BitConverter.ToInt32(message, 0);
                if (receiver.ReceiveBufferSize < length)
                {
                    receiver.ReceiveBufferSize = length;
                }
                message = new byte[length];
                int received = receiver.Receive(message, 0, length, SocketFlags.None);
                while (received < length)
                {
                    System.Media.SystemSounds.Beep.Play();
                    received = received + receiver.Receive(message, received, length - received, SocketFlags.None);
                }
                return message;
            }
            catch (SocketException)
            {
                return new byte[0];
            }
        }

        public static string GetString(Socket receiver)
        {
            byte[] message = GetBytes(receiver);
            return Encoding.Default.GetString(message).Trim('\0').Replace(Environment.NewLine, "");
        }

        public static bool SendBytes(Socket sender, byte[] message)
        {
            try
            {
                sender.Send(BitConverter.GetBytes(message.Length));
                if (sender.SendBufferSize < message.Length)
                {
                    sender.SendBufferSize = message.Length;
                }
                sender.Send(message);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public static bool SendString(Socket sender, string message)
        {
            byte[] encodedMessage = Encoding.Default.GetBytes(message);
            return SendBytes(sender, encodedMessage);
        }
    }
}
