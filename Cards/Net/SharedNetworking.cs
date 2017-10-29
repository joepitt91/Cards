using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace JoePitt.Cards.Net
{
    internal static class SharedNetworking
    {
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
