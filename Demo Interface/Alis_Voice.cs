using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech;

namespace Demo_Interface
{
	internal class Alis_Voice
	{
		System.Speech.Synthesis.SpeechSynthesizer Synth_Voice;
		System.Speech.Recognition.SpeechRecognitionEngine SRE;


		Alis_Voice()
		{
			Synth_Voice = new System.Speech.Synthesis.SpeechSynthesizer();
			Synth_Voice.SetOutputToDefaultAudioDevice();
			Synth_Voice.SelectVoiceByHints(System.Speech.Synthesis.VoiceGender.Female, System.Speech.Synthesis.VoiceAge.Teen);

			SRE.SetInputToDefaultAudioDevice();
			System.Speech.Recognition.DictationGrammar Diction=new System.Speech.Recognition.DictationGrammar();
			System.Speech.Recognition.Choices choices = new System.Speech.Recognition.Choices();

			System.Speech.Recognition.Grammar grammer = new System.Speech.Recognition.Grammar(new System.Speech.Recognition.GrammarBuilder(choices));
			SRE.LoadGrammar(grammer);


		}

		public void Introdcution()
		{
			Synth_Voice.Speak("Hello, may name is Alis. How may I help you");
		}


	}


	

}
