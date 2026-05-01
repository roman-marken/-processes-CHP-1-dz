using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace ProcessesHomework
{
    internal static class Program
    {
        private const string ChildWaitMode = "--child-wait";
        private const string ChildLongMode = "--child-long";
        private const string ChildCalcMode = "--child-calc";
        private const string ChildSearchMode = "--child-search";

        private static int Main(string[] args)
        {
            if (args.Length > 0)
            {
                return RunChildMode(args);
            }

            Console.OutputEncoding = Encoding.UTF8;
            return RunParentMenu();
        }

        private static int RunParentMenu()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Домашнє завдання: процеси");
                Console.WriteLine("1 - Завдання 1: запустити дочірній процес і дочекатися завершення");
                Console.WriteLine("2 - Завдання 2: чекати або примусово завершити дочірній процес");
                Console.WriteLine("3 - Завдання 3: передати два числа та операцію");
                Console.WriteLine("4 - Завдання 4: передати шлях до файлу/папки та слово для пошуку");
                Console.WriteLine("0 - Вихід");
                Console.Write("Ваш вибір: ");

                string choice = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            Task1();
                            break;
                        case "2":
                            Task2();
                            break;
                        case "3":
                            Task3();
                            break;
                        case "4":
                            Task4();
                            break;
                        case "0":
                            return 0;
                        default:
                            Console.WriteLine("Невідомий пункт меню.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Помилка: " + ex.Message);
                }
            }
        }

        private static int RunChildMode(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            switch (args[0])
            {
                case ChildWaitMode:
                    Console.WriteLine("Дочірній процес запущено.");
                    Thread.Sleep(1500);
                    Console.WriteLine("Дочірній процес завершується з кодом 17.");
                    return 17;

                case ChildLongMode:
                    Console.WriteLine("Довгий дочірній процес запущено.");
                    for (int i = 1; i <= 10; i++)
                    {
                        Console.WriteLine("Робота дочірнього процесу: крок " + i + " з 10");
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine("Довгий дочірній процес завершився самостійно.");
                    return 25;

                case ChildCalcMode:
                    return ChildCalculate(args);

                case ChildSearchMode:
                    return ChildSearch(args);

                default:
                    Console.WriteLine("Невідомий режим дочірнього процесу.");
                    return 1;
            }
        }

        private static void Task1()
        {
            Console.WriteLine("Запускаємо дочірній процес...");
            using (Process child = StartChild(ChildWaitMode))
            {
                child.WaitForExit();
                Console.WriteLine("Дочірній процес завершено.");
                Console.WriteLine("Код завершення: " + child.ExitCode);
            }
        }

        private static void Task2()
        {
            Console.WriteLine("Запускаємо довгий дочірній процес...");
            using (Process child = StartChild(ChildLongMode))
            {
                Console.Write("1 - чекати завершення, 2 - примусово завершити: ");
                string action = Console.ReadLine();

                if (action == "1")
                {
                    child.WaitForExit();
                    Console.WriteLine("Дочірній процес завершився сам.");
                    Console.WriteLine("Код завершення: " + child.ExitCode);
                    return;
                }

                if (action == "2")
                {
                    if (!child.HasExited)
                    {
                        child.Kill();
                        child.WaitForExit();
                    }

                    Console.WriteLine("Дочірній процес примусово завершено.");
                    Console.WriteLine("Код завершення: " + child.ExitCode);
                    return;
                }

                Console.WriteLine("Невірний вибір. Чекаємо завершення процесу.");
                child.WaitForExit();
                Console.WriteLine("Код завершення: " + child.ExitCode);
            }
        }

        private static void Task3()
        {
            Console.Write("Перше число: ");
            string first = Console.ReadLine();

            Console.Write("Друге число: ");
            string second = Console.ReadLine();

            Console.Write("Операція (+, -, *, /): ");
            string operation = Console.ReadLine();

            using (Process child = StartChild(ChildCalcMode, first, second, operation))
            {
                child.WaitForExit();
                Console.WriteLine("Код завершення дочірнього процесу: " + child.ExitCode);
            }
        }

        private static void Task4()
        {
            Console.Write("Шлях до файлу або папки: ");
            string path = Console.ReadLine();

            Console.Write("Слово для пошуку: ");
            string word = Console.ReadLine();

            using (Process child = StartChild(ChildSearchMode, path, word))
            {
                child.WaitForExit();
                Console.WriteLine("Код завершення дочірнього процесу: " + child.ExitCode);
            }
        }

        private static Process StartChild(params string[] arguments)
        {
            string currentExe = GetCurrentExecutablePath();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = currentExe;
            startInfo.Arguments = JoinArguments(arguments);
            startInfo.UseShellExecute = false;

            Process process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Не вдалося запустити дочірній процес.");
            }

            return process;
        }

        private static string GetCurrentExecutablePath()
        {
            string path = Process.GetCurrentProcess().MainModule.FileName;
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("Не вдалося визначити шлях до поточного exe-файлу.");
            }

            return path;
        }

        private static int ChildCalculate(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Потрібно передати два числа та операцію.");
                return 1;
            }

            Console.WriteLine("Отримані аргументи:");
            Console.WriteLine("Перше число: " + args[1]);
            Console.WriteLine("Друге число: " + args[2]);
            Console.WriteLine("Операція: " + args[3]);

            double first;
            double second;
            if (!TryParseNumber(args[1], out first) || !TryParseNumber(args[2], out second))
            {
                Console.WriteLine("Помилка: аргументи мають бути числами.");
                return 2;
            }

            double result;
            switch (args[3])
            {
                case "+":
                    result = first + second;
                    break;
                case "-":
                    result = first - second;
                    break;
                case "*":
                    result = first * second;
                    break;
                case "/":
                    if (second == 0)
                    {
                        Console.WriteLine("Помилка: ділення на нуль.");
                        return 3;
                    }
                    result = first / second;
                    break;
                default:
                    Console.WriteLine("Помилка: невідома операція.");
                    return 4;
            }

            Console.WriteLine("Результат: " + result.ToString(CultureInfo.InvariantCulture));
            return 0;
        }

        private static int ChildSearch(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Потрібно передати шлях до файлу/папки та слово для пошуку.");
                return 1;
            }

            string path = args[1];
            string word = args[2];

            Console.WriteLine("Отримані аргументи:");
            Console.WriteLine("Шлях: " + path);
            Console.WriteLine("Слово: " + word);

            if (string.IsNullOrWhiteSpace(word))
            {
                Console.WriteLine("Помилка: слово для пошуку не може бути порожнім.");
                return 2;
            }

            if (File.Exists(path))
            {
                int count = CountWordInFile(path, word);
                Console.WriteLine("Кількість входжень слова '" + word + "': " + count);
                return 0;
            }

            if (Directory.Exists(path))
            {
                int total = 0;
                int processedFiles = 0;

                foreach (string file in EnumerateFilesSafely(path))
                {
                    try
                    {
                        total += CountWordInFile(file, word);
                        processedFiles++;
                    }
                    catch
                    {
                        // Деякі файли можуть бути недоступні або бінарні; просто пропускаємо їх.
                    }
                }

                Console.WriteLine("Опрацьовано файлів: " + processedFiles);
                Console.WriteLine("Кількість входжень слова '" + word + "': " + total);
                return 0;
            }

            Console.WriteLine("Помилка: шлях не існує.");
            return 3;
        }

        private static bool TryParseNumber(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
        }

        private static int CountWordInFile(string filePath, string word)
        {
            string text = File.ReadAllText(filePath);
            int count = 0;
            int index = 0;

            while (true)
            {
                index = text.IndexOf(word, index, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    break;
                }

                count++;
                index += word.Length;
            }

            return count;
        }

        private static IEnumerable<string> EnumerateFilesSafely(string directory)
        {
            string[] files;
            try
            {
                files = Directory.GetFiles(directory);
            }
            catch
            {
                yield break;
            }

            foreach (string file in files)
            {
                yield return file;
            }

            string[] directories;
            try
            {
                directories = Directory.GetDirectories(directory);
            }
            catch
            {
                yield break;
            }

            foreach (string childDirectory in directories)
            {
                foreach (string file in EnumerateFilesSafely(childDirectory))
                {
                    yield return file;
                }
            }
        }

        private static string JoinArguments(IEnumerable<string> arguments)
        {
            List<string> quoted = new List<string>();
            foreach (string argument in arguments)
            {
                quoted.Add(QuoteArgument(argument ?? string.Empty));
            }

            return string.Join(" ", quoted.ToArray());
        }

        private static string QuoteArgument(string argument)
        {
            if (argument.Length == 0)
            {
                return "\"\"";
            }

            bool needsQuotes = argument.IndexOfAny(new[] { ' ', '\t', '\n', '\r', '"' }) >= 0;
            if (!needsQuotes)
            {
                return argument;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append('"');

            int backslashes = 0;
            foreach (char ch in argument)
            {
                if (ch == '\\')
                {
                    backslashes++;
                    continue;
                }

                if (ch == '"')
                {
                    builder.Append('\\', backslashes * 2 + 1);
                    builder.Append('"');
                    backslashes = 0;
                    continue;
                }

                builder.Append('\\', backslashes);
                backslashes = 0;
                builder.Append(ch);
            }

            builder.Append('\\', backslashes * 2);
            builder.Append('"');
            return builder.ToString();
        }
    }
}
