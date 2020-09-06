using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Speech
{
    class Program
    {
        static String subscriptionId = "ca4f6b9c71774fe6b90bca53c822feb2";
        static String serviceRegion = "eastus";
        static String mediaPath = "ca.wav";

        // It's always a good idea to access services in an async fashion
        static async Task Main(String[] args)
        {
            if(args.Length == 1){
                mediaPath = args[0]; 
            }
            await ContinuousRecognitionWithFileAsync();
        }

        // Continuous speech recognition.
        public static async Task ContinuousRecognitionWithFileAsync()
        {
            // <recognitionContinuousWithFile>
            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            var config = SpeechConfig.FromSubscription(subscriptionId, serviceRegion);  
            var recognized = new System.Collections.Generic.List<String>();

            var stopRecognition = new TaskCompletionSource<int>();

            // Creates a speech recognizer using file as audio input.
            // Replace with your own audio file name.
            using (var audioInput = AudioConfig.FromWavFileInput(@mediaPath))
            {
                using (var recognizer = new SpeechRecognizer(config, audioInput))
                {
                    // Subscribes to events.
                    recognizer.Recognizing += (s, e) =>
                    {
                        //Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
                    };

                    recognizer.Recognized += (s, e) =>
                    {
                        if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            recognized.Add(e.Result.Text);
                            Console.WriteLine($"{e.Result.Text}");
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        }
                    };

                    recognizer.Canceled += (s, e) =>
                    {
                        Console.WriteLine($"CANCELED: Reason={e.Reason}");

                        if (e.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    recognizer.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine("\n    Session started event.");
                    };

                    recognizer.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine("\n    Session stopped event.");
                        Console.WriteLine("\nStop recognition.");
                        stopRecognition.TrySetResult(0);
                    };

                    // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
                    await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    // Waits for completion.
                    // Use Task.WaitAny to keep the task rooted.
                    Task.WaitAny(new[] { stopRecognition.Task });

                    // Stops recognition.
                    await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            // </recognitionContinuousWithFile>

            //recognized.ForEach(m=>Console.WriteLine(m.ToString()));
        }


        static async Task RecognizeSpeechAsync()
        {
            // Configure the subscription information for the service to access.
            // Use either key1 or key2 from the Speech Service resource you have created
            var config = SpeechConfig.FromSubscription(subscriptionId, serviceRegion);

            // Setup the audio configuration, in this case, using a file that is in local storage.
            using (var audioInput = AudioConfig.FromWavFileInput(@mediaPath))

            // Pass the required parameters to the Speech Service which includes the configuration information
            // and the audio file name that you will use as input
            using (var recognizer = new SpeechRecognizer(config, audioInput))
            {
                Console.WriteLine("Recognizing first result...");
                var result = await recognizer.RecognizeOnceAsync();

                switch (result.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        // The file contained speech that was recognized and the transcription will be output
                        // to the terminal window
                        Console.WriteLine($"We recognized: {result.Text}");
                        break;
                    case ResultReason.NoMatch:
                        // No recognizable speech found in the audio file that was supplied.
                        // Out an informative message
                        Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                        break;
                    case ResultReason.Canceled:
                        // Operation was cancelled
                        // Output the reason
                        var cancellation = CancellationDetails.FromResult(result);
                        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                            Console.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                        break;
                }
            }
        }
    }
}