using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aida.Core
{
    /// <summary>
    /// La classe qui gère les services déclencheurs, elle n'a qu'un thread
    /// pour tous les gérer.
    /// </summary>
    public class TriggerServices
    {
        protected Thread Thread { get; }
        protected volatile bool IsRunning;
        protected volatile ConcurrentDictionary<ITriggerService, DateTime> LoadedServices;
        protected object LockObj { get; }
        protected Action<IEnumerable<TriggerMessage>> OnNewMessages { get; }

        public TriggerServices(Action<IEnumerable<TriggerMessage>> onNewMessages)
        {
            Thread = new Thread(Loop);
            IsRunning = false;
            LoadedServices = new ConcurrentDictionary<ITriggerService, DateTime>(2, 0);
            LockObj = new object();
            OnNewMessages = onNewMessages;

            LoadServices();
        }

        public void LoadServices()
        {
            lock (LockObj)
            {
                LoadedServices.Clear();
                // Charge les services déclencheurs
            }
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

        protected void Loop()
        {
            while (IsRunning)
            {
                lock (LockObj)
                {
                    foreach (var loadedService in LoadedServices)
                    {
                        var currentInterval = (DateTime.Now - loadedService.Value).Milliseconds;
                        if (loadedService.Key.CheckIntervalMilliseconds <= currentInterval)
                        {
                            var messages = loadedService.Key.Check();
                            if (messages.Any())
                            {
                                OnNewMessages(messages);
                            }

                            LoadedServices[loadedService.Key] = DateTime.Now;
                        }
                    }
                }
                
                Thread.Sleep(100);
            }
        }
    }
}
