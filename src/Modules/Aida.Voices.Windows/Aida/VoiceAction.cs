using Aida.Core;
using Aida.Core.Voices;
using Aida.Core.Voices.French;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aida.Voices.Windows.Aida
{
    public class VoiceAction : IAction
    {
        protected WindowsVoice Voice { get; set; }

        public VoiceAction()
        {
            Voice = new FrenchFemaleVoice();
        }

        public void Say(VoiceIngredient ingredient)
        {
            Voice.Say(ingredient.Message);
        }

        public void Spell(VoiceIngredient ingredient)
        {
            Voice.Say(ingredient.Message);
        }
    }
}
