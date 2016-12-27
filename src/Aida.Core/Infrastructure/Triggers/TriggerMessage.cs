using Aida.Core.Infrastructure.Ingredients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aida.Core
{
    /// <summary>
    /// Trigger message.
    /// </summary>
    public class TriggerMessage
    {
        public Trigger Trigger { get; set; }
        public DateTime TriggerDate { get; set; }
        public IEnumerable<IngredientValue> IngredientValues { get; set; }
    }
}
