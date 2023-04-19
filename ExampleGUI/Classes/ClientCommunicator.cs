using System;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace ExampleGUI.Classes
{
    internal class ClientCommunicator: IDisposable
    {
        private readonly Stream _stream;
        public ClientCommunicator(Stream stream)
        {
            _stream = stream;
        }

        public string ReadString()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public object ReadObject()
        {
            IFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(_stream);
        }

        public void WriteString(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            _stream.Write(buffer, 0, buffer.Length);
            _stream.Flush();
        }

        public void WriteObject(object message)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(_stream, message);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
