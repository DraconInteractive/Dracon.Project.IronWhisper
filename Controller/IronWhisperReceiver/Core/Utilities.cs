using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IronWhisper_CentralController
{
    internal class Utilities
    {
        public static Dictionary<string, int> numWords = new Dictionary<string, int>
            {
                {"zero", 0}, {"one", 1}, {"two", 2}, {"three", 3}, {"four", 4},
                {"five", 5}, {"six", 6}, {"seven", 7}, {"eight", 8}, {"nine", 9},
                {"ten", 10}, {"eleven", 11}, {"twelve", 12}, {"thirteen", 13},
                {"fourteen", 14}, {"fifteen", 15}, {"sixteen", 16}, {"seventeen", 17},
                {"eighteen", 18}, {"nineteen", 19}, {"twenty", 20}, {"thirty", 30},
                {"forty", 40}, {"fifty", 50}, {"sixty", 60}, {"seventy", 70},
                {"eighty", 80}, {"ninety", 90}, {"hundred", 100}, {"thousand", 1000}
            };

        /// <summary>
        /// Extracts integer from text and returns that and the original string modified to have the original int added
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static (string, int) ExtractNumberFromText(string text)
        {
            int result = 0;
            int currentNumber = 0;

            StringBuilder modifiedText = new StringBuilder();

            string[] words = text.Split(new[] { " ", "-", "and" }, StringSplitOptions.RemoveEmptyEntries);

            bool readingNumber = false;
            foreach (string word in words)
            {
                if (numWords.ContainsKey(word))
                {
                    readingNumber = true;

                    int num = numWords[word];
                    if (num == 100 || num == 1000)
                    {
                        currentNumber *= num;
                    }
                    else
                    {
                        currentNumber += num;
                    }

                    if (num == 1000)
                    {
                        result += currentNumber;
                        currentNumber = 0;
                    }
                }
                else
                {
                    if (readingNumber)
                    {
                        result += currentNumber;
                        modifiedText.Append(result + " ");
                        readingNumber = false;
                        currentNumber = 0;
                    }
                    modifiedText.Append(word + " ");
                }
            }

            // If the sentence ends with a number
            if (readingNumber)
            {
                modifiedText.Append(result + currentNumber + " ");
                result += currentNumber;
            }

            return (modifiedText.ToString().Trim(), result + currentNumber);
        }

        public static string RemoveLogDescriptor (string log)
        {
            string pattern = @"\[.*?\]";
            string result = Regex.Replace(log, pattern, ""); // Remove matching substrings
            return result;
        }

        public static List<Type> GetArchetypes(Type parentType)
        {
            List<Type> archetypes = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(parentType))
                    {
                        archetypes.Add(type);
                    }
                }
            }
            return archetypes;
        }

        public static async Task CreateCommandWindowWithPrompt(string prompt)
        {
            Process process = new();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                FileName = "cmd",
                Arguments = $"/c start cmd.exe /k {prompt}",
                UseShellExecute = true,
                CreateNoWindow = false,
                RedirectStandardError = false
            };

            process.StartInfo = startInfo;
            process.Start();

            await process.WaitForExitAsync();
        }

        public static async Task CreateWSLWindowWithPrompt(string prompt)
        {
            await CreateCommandWindowWithPrompt($"wsl -e bash -c \"{prompt}\"");
        }
    }
}

