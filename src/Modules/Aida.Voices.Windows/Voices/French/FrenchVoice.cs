using System.Globalization;
using System.Speech.Synthesis;

namespace Aida.Core.Voices.French
{
    public abstract class FrenchVoice : WindowsVoice
    {
        public FrenchVoice(VoiceGender gender) 
            : base(1, gender, VoiceAge.Adult, CultureInfo.GetCultureInfo("fr-fr"))
        {
        }
    }
}
