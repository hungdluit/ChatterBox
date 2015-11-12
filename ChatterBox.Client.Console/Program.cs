using System.Threading.Tasks;
using ChatterBox.Common.Communication.Messages.Registration;

namespace ChatterBox.Client.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            System.Console.Write("Enter your user ID: ");
            var userId = System.Console.ReadLine();
            System.Console.Title = userId;
            var client = new ChatterBoxConsoleClient();
            client.Connect();
            Task.Run(() =>
            {
                client.Register(new Registration
                {
                    Domain = "chatterbox.microsoft.com",
                    Name = userId,
                    UserId = userId,
                    PushNotificationChannelURI = ""
                });
            });

            System.Console.ReadLine();
        }
    }
}