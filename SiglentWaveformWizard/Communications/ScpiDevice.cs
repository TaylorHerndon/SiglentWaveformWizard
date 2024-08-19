//Ignore Spelling: Siglent, Scpi

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SiglentWaveformWizard.Communications
{
    class ScpiDevice: IDisposable
    {
        private TcpClient client;

        public ScpiDevice(string ip, int port)
        {
            client = new TcpClient(ip, port);
            client.ReceiveTimeout = 1000;

            NetworkStream stream = client.GetStream();
            byte[] respData = new byte[256];
            stream.Read(respData, 0, respData.Length);

            string msg = Encoding.UTF8.GetString(respData).Trim('\0');
            if (string.IsNullOrEmpty(msg))
            {
                this.Dispose();
                throw new Exception("Invalid Response");
            }

            Reset();
            WaitUntilOperationComplete();
        }

        /// <summary>
        /// Writes the given message to the device.
        /// Will terminate the message with a '\n' if it is not already.
        /// </summary>
        /// <param name="message">Message to be sent to the device.</param>
        public void Write(string message)
        {
            if (client == null) { return; }
            if (!message.EndsWith('\n')) { message += '\n'; }

            NetworkStream stream = client.GetStream();
            byte[] msg = Encoding.ASCII.GetBytes(message);
            stream.Write(msg, 0, msg.Length);
        }

        /// <summary>
        /// Will read until a '\n' character is received from the device or until timeout is reached.
        /// </summary>
        /// <returns>Data read from the device or null if nothing was read.</returns>
        public string? Read(int timeout_ms = 1000)
        {
            if (client == null) { return null; }
            DateTime startTime = DateTime.Now;
            NetworkStream stream = client.GetStream();

            string respMessage = string.Empty;
            while (!respMessage.Contains('\n') && (DateTime.Now - startTime) < TimeSpan.FromMilliseconds(timeout_ms))
            {
                byte[] respData = new byte[1024];
                try { stream.Read(respData, 0, respData.Length); } catch { }
                respMessage += Encoding.ASCII.GetString(respData).Trim('\0');
            }

            if (respMessage == string.Empty) { return null; }
            return respMessage;
        }

        /// <summary>
        /// Write + Read functions.
        /// </summary>
        /// <param name="message">Message to be written to the device.</param>
        /// <param name="timeout_ms">Maximum allowed time in ms to wait for a response.</param>
        /// <returns>Data read form the device or null if nothing was read.</returns>
        public string? Query(string message, int timeout_ms = 1000)
        {
            Write(message);
            return Read(timeout_ms);
        }

        public string? IDN() => Query("*IDN?");
        public void Reset() => Write("*RST");
        public string? WaitUntilOperationComplete() => Query("*OPC?");

        public void Close() => Dispose();
        public void Dispose()
        {
            client?.GetStream().Dispose();
            client?.Dispose();
        }
    }
}
