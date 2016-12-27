using System.Collections.Generic;

namespace Aida.Core
{
    public interface ITriggerService
    {
        /// <summary>
        /// Définit l'interval minimum d'attente entre deux appels de la méthode Check.
        /// </summary>
        int CheckIntervalMilliseconds { get; }

        /// <summary>
        /// Méthode appelé à un interval régulier définit par l'interface 
        /// permettant de récupérer les messages du service déclencheur.
        /// </summary>
        IEnumerable<TriggerMessage> Check();
    }
}
