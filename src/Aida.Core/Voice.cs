using System.Globalization;
using System.Speech.Synthesis;

namespace Aida.Core
{
    public class Voice
    {
        protected SpeechSynthesizer Synthesizer { get; set; }

        public Voice(CultureInfo cultureInfo)
        {
            Synthesizer = new SpeechSynthesizer();
            Synthesizer.SetOutputToDefaultAudioDevice();
            Synthesizer.Rate = 1;
            Synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult, 0, cultureInfo);
        }

        public void Say(string message)
        {
            Synthesizer.Speak(message);
        }
    }
}
