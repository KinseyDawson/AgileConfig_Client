using System;

namespace AgileConfig.Client
{
    class MessageCenter
    {
        public static event Action<string> Subscribe;
        public static void Receive(string message)
        {
            Subscribe?.Invoke(message);
        }
    }

}
