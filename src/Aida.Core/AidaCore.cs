using System.Threading;

namespace Aida.Core
{
    public class AidaCore
    {
        protected Thread Thread { get; set; }
        public volatile bool IsRunning;

        protected IVoice Voice { get; set; }

        public AidaCore(IVoice voice)
        {
            Thread = new Thread(Loop);

            Voice = voice;
        }

        public void Start()
        {
            IsRunning = true;
            Thread.Start();
        }

        public void Stop()
        {
            IsRunning = false;
            Thread.Join();
        }

        public void Write(string message)
        {
            Voice.Say(message);
        }

        protected void Loop()
        {
            Voice.Say("Bonjour !");

            while (IsRunning)
            {
                Thread.Sleep(500);
            }

            Voice.Say("Aurevoir !");
        }
    }
}
