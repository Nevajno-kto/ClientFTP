using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Client
{
    class Program
    {
        static IPAddress IP;
        static int PORT = 21;
        static bool exit = false;
        static void Main(string[] args)
        {
            bool check;
            int answer;
            do
            {
                Console.WriteLine("Введите IP-адресс сервера");
                string Ip = Console.ReadLine();
                check = IPAddress.TryParse(Ip, out IP);
            } while (!check);
            Socket TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TcpSocket.Connect(new IPEndPoint(IP, PORT));

            Authorization(TcpSocket);
            while (!exit)
            {
                Console.Write("1 - Показать список файлов директории\n2 - Перейти в директорию\n3 - Скачать файл\n4 -  Выход\n");
                do
                {
                    string ans = Console.ReadLine();
                    check = int.TryParse(ans, out answer);
                } while (!check);
                switch (answer)
                {
                    case 1:
                        Dir(TcpSocket);
                        break;
                    case 2:
                        Cd(TcpSocket);
                        break;
                    case 3:
                        break;
                    case 4:
                        exit = true;
                        break;
                }
            }
            
            TcpSocket.Shutdown(SocketShutdown.Both);
            TcpSocket.Close();
        }

        static string TakeResponse(Socket Listener)
        {
            var BufferBytes = new byte[1024];
            int size = 0;
            var Data = new StringBuilder();
            try
            {
                do
                {
                    size = Listener.Receive(BufferBytes);
                    Data.Append(Encoding.UTF8.GetString(BufferBytes, 0, size));
                } while (Listener.Available > 0);
            }
            catch(SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
            }
            return Data.ToString();
        }

        static void Authorization(Socket User)
        {
            string answer;
            Console.WriteLine(TakeResponse(User));
            do {
                Console.Write("Введите логин: ");
                string Login = Console.ReadLine();
                User.Send(Encoding.UTF8.GetBytes("user " + Login + "\r\n"));
                Console.WriteLine(TakeResponse(User));
                Console.Write("Введите пароль: ");
                string Password = Console.ReadLine();
                User.Send(Encoding.UTF8.GetBytes("pass " + Password + "\r\n"));
                answer = TakeResponse(User);
                Console.WriteLine(answer);
            } while (!answer.Contains("230"));            
        }
        static void Dir(Socket User)
        {
            Socket Data = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var infoIp = (IPEndPoint)User.LocalEndPoint;
            try
            {
                Data.Bind(new IPEndPoint(infoIp.Address, 1800));
            }
            catch (SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
            }
            try
            {
                Data.Listen(1);
            }
            catch (SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
            }
            User.Send(Encoding.UTF8.GetBytes("port " + infoIp.Address.ToString().Replace('.', ',') + ",7,8\r\n"));
            var newS = Data.Accept();
            Console.WriteLine(TakeResponse(User));
            User.Send(Encoding.UTF8.GetBytes("list\r\n"));
            Console.WriteLine(TakeResponse(newS));
            Data.Close();
        }
        static void Cd(Socket User)
        {
            Console.WriteLine("Введите имя директории");
            string name = Console.ReadLine();
            User.Send(Encoding.UTF8.GetBytes("cwd " + name + "\r\n"));
            Console.WriteLine(TakeResponse(User));
        }

    }
}
