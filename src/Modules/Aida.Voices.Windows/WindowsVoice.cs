using System;
using System.Globalization;
using System.Speech.Synthesis;
using System.Text;

namespace Aida.Core.Voices
{
    public abstract class WindowsVoice : IVoice
    {
        protected CultureInfo Language { get; set; }
        protected SpeechSynthesizer Synthesizer { get; set; }
        
        public WindowsVoice(int rate, VoiceGender gender, VoiceAge age, CultureInfo language)
        {
            Language = language;
            Synthesizer = new SpeechSynthesizer();
            Synthesizer.SetOutputToDefaultAudioDevice();
            Synthesizer.Rate = 1;
            Synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult, 0, language);
        }

        public void Say(string message)
        {
            Synthesizer.Speak(message);
        }

        public void Spell(string word)
        {
            int oldRate = Synthesizer.Rate;
            Synthesizer.Rate = -3;

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"ISO - 8859 - 1\"?>");
            sb.AppendLine($"<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"{Language.ToString()}\">");
            sb.AppendLine($"<say-as interpret-as=\"characters\">{word}</say-as>");
            sb.AppendLine("</speak>");

            Synthesizer.SpeakSsml(sb.ToString());
            Synthesizer.Rate = oldRate;
        }
    }
}
