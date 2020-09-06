using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;

namespace SpeechTranslation
{

    class Program
    {
        static string subscriptionId = ""; //use your own suscription id
        static string location = "centralus"; //use your own location 
        static void Main(string[] args)
        {
            TranslateSpeechToText().Wait();
        }

        public static async Task TranslateSpeechToText()
        {
            // Creates an instance of a speech translation config with specified subscription key and service region.
            // Replace with your own subscription key and region identifier from here: https://aka.ms/speech/sdkregion
            var config = SpeechTranslationConfig.FromSubscription(subscriptionId, location);
            

            // Sets source and target languages.
            // Replace with the languages of your choice, from list found here: https://aka.ms/speech/sttt-languages
            string fromLanguage = "en-US";
            string toLanguage = "de";
            config.SpeechRecognitionLanguage = fromLanguage;
            config.AddTargetLanguage(toLanguage);

            // Creates a translation recognizer using the default microphone audio input device.
            using (var recognizer = new TranslationRecognizer(config))
            {
                // Starts translation, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of 15
                // seconds of audio is processed. The task returns the recognized text as well as the translation.
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query.
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                Console.WriteLine("Say something...");
                var result = await recognizer.RecognizeOnceAsync();

                // Checks result.
                if (result.Reason == ResultReason.TranslatedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED '{fromLanguage}': {result.Text}");
                    Console.WriteLine($"TRANSLATED into '{toLanguage}': {result.Translations[toLanguage]}");
                }
                else if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED '{fromLanguage}': {result.Text} (text could not be translated)");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }

    }
}
