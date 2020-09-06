using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using Microsoft.Extensions.Configuration;

namespace SpeechTranslation
{

    class Program
    {
        static void initialize()
        {
            var builder = new ConfigurationBuilder()
           .SetBasePath(System.IO.Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();

            subscriptionKey = configuration.GetSection("subscriptionKey").Value;
            serviceRegion = configuration.GetSection("serviceRegion").Value;

            if (subscriptionKey == null)
            {
                Console.WriteLine("Error: subscription key is null. Plase check appsetting.json");
            }

            config = initSpeechConfig();

        }

        static string subscriptionKey = ""; //use your own suscription id
        static string serviceRegion = "centralus"; //use your own location //for now defailt

        static SpeechTranslationConfig config = null;
        static void Main(string[] args)
        {
            initialize();
            //TranslateSpeechToText().Wait();
            TranslateSpeechToSpeech().Wait();
        }


        private static SpeechTranslationConfig initSpeechConfig()
        {
            return SpeechTranslationConfig.FromSubscription(subscriptionKey, serviceRegion);
        }



        public static async Task TranslateSpeechToText()
        {
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

        public static async Task TranslateSpeechToSpeech()
        {
            string fromLanguage = "en-US";
            config.SpeechRecognitionLanguage = fromLanguage;

            // Add more than one language to the collection.
            // using the AddTargetLanguage() method
            config.AddTargetLanguage("fr");
            config.AddTargetLanguage("id-ID");

            // Creates a speech synthesizer using the default speaker as audio output.
            var speech_synthesizer = new SpeechSynthesizer(config);

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
                    // Output the text for the recognized speech input
                    Console.WriteLine($"RECOGNIZED '{fromLanguage}': {result.Text}");
                    foreach (var element in result.Translations)
                    {
                        // Using the Key and Value components of the returned dictionary for the translated results
                        // The first line gets the key (language code) while the second gets the Value
                        // which is the translated text for the language specified
                        // Output the language and then the translated text
                        Console.WriteLine($"TRANSLATED into '{element.Key}': {element.Value}");

                        // If the language code is 'fr' for French, then use the French voice for Julie
                        // If you change the languages in the 'AddTargetLanguage' above, ensure you modify this if statement as well
                        if (element.Key == "fr")
                        {
                            config.SpeechSynthesisVoiceName = "fr-FR-Julie-Apollo";

                            // Update the speech synthesizer to use the proper voice
                            speech_synthesizer = new SpeechSynthesizer(config);
                        }
                        // Otherwise, use the voice for the Indonesian translation
                        else
                        {
                            config.SpeechSynthesisVoiceName = "id-ID-Andika";

                            // Update the speech synthesizer to use the proper voice
                            speech_synthesizer = new SpeechSynthesizer(config);
                        }
                        // Use the proper voice, from the speech synthesizer configuration, to narrate the translated result
                        // in the native speaker voice.
                        await speech_synthesizer.SpeakTextAsync(result.Translations[element.Key]);
                    }

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
