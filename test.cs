using Godot;
using System;
using System.IO;
using Syn.Speech.Api;


public class test : Node2D
{
	[Signal]
	delegate void result(string output);
	[Signal]
	delegate void delete_audio();
	[Export]
	private bool _use_keyword = true;
	[Export]
	private string _save_path = "user://record.wav";

	private void _on_Record_button_button_down()
	{
		GD.Print("Voice started Recording");
		_effect.SetRecordingActive(true);
		EmitSignal("delete_audio");
	}

	private void _on_Record_button_button_up()
	{
		GD.Print("voice stopped recording");
		_recording = _effect.GetRecording();
		_effect.SetRecordingActive(false);
		_recording.SaveToWav(_save_path);  //save the recording in a .wav file
		Main();  // call for the conversion immediatly after
	}


	//handles audio recording
	private AudioEffectRecord _effect;
	private AudioStreamSample _recording;

	public override void _Ready()
	{
		// We get the index of the "Record" bus.
		int idx = AudioServer.GetBusIndex("Record");
		// And use it to retrieve its first effect, which has been defined
		// as an "AudioEffectRecord" resource.
		_effect = (AudioEffectRecord)AudioServer.GetBusEffect(idx, 0);
		//GD.Print(Godot.OS.GetUserDataDir() + "/record.wav");
	}



	//handles speech recognition
	private static Configuration _configuration;
	private static StreamSpeechRecognizer _speechRecognizer;
	private string _audio_file = Godot.OS.GetUserDataDir() + "/record.wav";
	
	public void Main()
	{
		var modelsDirectory = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Models");

		_configuration = new Configuration
		{
			AcousticModelPath = modelsDirectory,
			DictionaryPath = System.IO.Path.Combine(modelsDirectory, "cmudict-en-us.dict"),
			LanguageModelPath = System.IO.Path.Combine(modelsDirectory, "en-us.lm.dmp"),
			UseGrammar = _use_keyword,
			GrammarPath = modelsDirectory,
			GrammarName = "Hello"
		};

		FileStream fs = new FileStream(_audio_file, FileMode.Open);

		_speechRecognizer = new StreamSpeechRecognizer(_configuration);
		_speechRecognizer.StartRecognition(fs);


		var result = _speechRecognizer.GetResult();
		_speechRecognizer.StopRecognition();


		if (result != null)
		{
   			GD.Print("Speech Recognized: " + result.GetHypothesis());
			EmitSignal("result", "Speech Recognized: " + result.GetHypothesis());
			fs.Dispose();
		}
	}
}

