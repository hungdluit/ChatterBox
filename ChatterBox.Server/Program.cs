using System;

namespace ChatterBox.Server
{
    class Program
    {
        static void Main()
        {
            try
            {
                var signalingServer = new ChatterBoxServer();
                signalingServer.Run();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
