using System;
using System.Threading.Tasks;
using ChatterBox.Shared.Communication.Messages.Registration;

namespace ChatterBox.Client.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.Write("Enter your user ID: ");
            var userId = System.Console.ReadLine();
            var client = new ChatterBoxClient();
            client.Connect();
            Task.Run(() =>
            {
                client.Register(new RegistrationMessage
                {
                    Domain = "Macadamian.Test",
                    Name = userId,
                    UserId = userId,
                    PushToken = userId
                });
            });
            
            System.Console.ReadLine();
        }
    }
}
