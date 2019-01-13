using Microsoft.VisualBasic.Devices;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VoiceRecognizeMark.Algorithms;
using VoiceRecognizeMark.Learning;
using VoiceRecognizeMark.Model;
using VoiceRecognizeMark.Model.Recognizer;
using VoiceRecognizeMark.New;
using VoiceRecognizeMark.NoiseReduction;
using VoiceRecognizeMark.Recognizer;
using VoiceRecognizeMark.SimpleRecognizer;

namespace VoiceRecognizeMark
{
    public partial class MainForm : Form
    {
        private double AMPLITUDE_THRESHOLD = 1;//0.0005;

        private WaveIn waveIn = default(WaveIn);

        public MainForm()
        {
            InitializeComponent();

            waveIn = new WaveIn();
            waveIn.DeviceNumber = 0;
            waveIn.DataAvailable += waveIn_DataAvailable;

            waveIn.WaveFormat = new WaveFormat(16000, 1);
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            LogTextBox.Text = "";
            waveIn.StartRecording();
            StopButton.Enabled = true;
            StartButton.Enabled = false;
        }

        private long recorderCount = 0;

        private async void RecognizeOld()
        {
            await Task.Run(() =>
            {
                if (RecorederData.Any())
                {
                    var framePartitioner = new FramePartitioningOverlap(
                        signalParams: new SignalParams()
                        {
                            Subchunk2Size = recorderCount,
                            BitsPerSample = waveIn.WaveFormat.BitsPerSample,
                            BytesPerSecond = waveIn.WaveFormat.AverageBytesPerSecond
                        },
                        frameParams: new FrameParams()
                        {
                            FrameLength = 47,
                            FrameOverlap = 0.5
                        }
                    );
                    var wordsPartitioner = new WordPartitioningRMSBased(new WordPartitioningRMSBasedParams()
                    {
                        RMSIndeterminacy = 0.1,
                        MinFrameDistanceBeetwenWords = 4
                    });

                    var withoutNoiseSignla = new ThresholdNoiseReduction(AMPLITUDE_THRESHOLD).Process(RecorederData.ToArray());

                    //var frames = framePartitioner.Partitioning(withoutNoiseSignla);
                    //var words = wordsPartitioner.Partitioning(frames);

                    //LogTextBox.Text = $"Количество слов: { words.Length } \r\n";
                    //LogTextBox.Text += $"Минимальная амплитуда: { withoutNoiseSignla.Min() } \r\n";
                    //LogTextBox.Text += $"Максимальная амплитуда: { withoutNoiseSignla.Max() } \r\n";

                    RecorederData.Clear(); recorderCount = 0;

                    var learningDatabase = new SystemLearning().LoadLearingDatabase(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Learning.ldb"));

                    var recognizer = new RecognizerMark(SimpleMFCCBuilder.Create(waveIn.WaveFormat.SampleRate), wordsPartitioner,
                        framePartitioner, new CredibilityEstimation(new DTW(), learningDatabase));

                    //result = recognizer.Evaluate(withoutNoiseSignla);
                }
            });
        }

        private void IsTrueWord(Stream stream, string[] words)
        {
            var format = new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, AudioChannel.Mono);

            SpeechRecognitionEngine sr = new SpeechRecognitionEngine(new CultureInfo("en-GB"));
            sr.SetInputToAudioStream(stream, format);

            GrammarBuilder grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(new Choices(words));
            sr.UnloadAllGrammars();
            sr.LoadGrammar(new Grammar(grammarBuilder));//загружаем "грамматику"
            sr.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized);

            var rr = sr.Recognize();
        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {

        }

    private async void StopButton_Click(object sender, EventArgs e)
        {
            StopButton.Enabled = false;
            StartButton.Enabled = true;
            waveIn.StopRecording();

            RecognizingProgressBar.Style = ProgressBarStyle.Marquee;

            if (RecorederData.Count != 0)
            {
                MFCC mfcc = new MFCC(waveIn.WaveFormat.SampleRate);
                var withoutNoiseSignla = new ThresholdNoiseReduction(AMPLITUDE_THRESHOLD).Process(RecorederData.ToArray()).SkipWhile(a => a == 0).ToArray();

                var cfc = mfcc.Process(withoutNoiseSignla);

                var result = new CredibilityEstimationNew(
                    new SystemLearning().LoadLearingDatabase(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Learning.ldb")))
                    .Estimate(new MFCCWord(null, cfc.ToList()));

                if (result == null)
                    LogTextBox.Text = "шибка записи!";
                else
                {
                    LogTextBox.Text += $"Слово: { result.Word } \r\n";
                    LogTextBox.Text += $"Сказано правильно на : { Math.Round(result.Mark) } %";
                }
            }
            else
                LogTextBox.Text = "Ошибка записи!";

            RecorederData.Clear(); recorderCount = 0;

            RecognizingProgressBar.Style = ProgressBarStyle.Blocks;
        }

        private void PlayWord(Word word)
        {

        }

        private List<byte> recordedBytes = new List<byte>();

        void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            recorderCount += e.BytesRecorded;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) |
                                        e.Buffer[index + 0]);
                double sample32 = sample;// / 32768.0;

                RecorederData.Add(sample32);
                recordedBytes.Add(e.Buffer[0]);
                recordedBytes.Add(e.Buffer[1]);
            }
        }

        private List<double> RecorederData { get; set; } = new List<double>();

        private void LearningModeButton_Click(object sender, EventArgs e)
        {
            using (var learingForm = new LearningForm())
                learingForm.ShowDialog();
        }
    }
}
