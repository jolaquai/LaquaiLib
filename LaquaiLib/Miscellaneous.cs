using System.Diagnostics;
using System.Reflection;

namespace LaquaiLib;

#pragma warning disable CA1069 // Enums values should not be duplicated
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

public static class Miscellaneous
{
    public class Logger
    {
        public static string FormatString { get; set; } = @"MM-dd-yyyy HH:mm:ss.fffffff";

        internal enum LogEntryType
        {
            Info =             0b0000010,
            SoftWarn =         0b0000100,
            Warn =             0b0001000,
            Error =            0b0010000,
            Fail =             0b0010000,
            Success =          0b0100000,
            Custom =           0b1000000,
            FollowUp =         0b0000001,
            FollowUpInfo =     Info | FollowUp,
            FollowUpSoftWarn = SoftWarn | FollowUp,
            FollowUpWarn =     Warn | FollowUp,
            FollowUpError =    Error | FollowUp,
            FollowUpFail =     Fail | FollowUp,
            FollowUpSuccess =  Success | FollowUp,
        }

        private List<LogEntry> Entries { get; } = new();

        internal class LogEntry
        {
            internal DateTime DateTime { get; init; }
            internal LogEntryType LogEntryTypes { get; init; }
            internal string? Tag { get; init; }
            internal object[] Logged { get; init; }

            internal LogEntry(DateTime dateTime, LogEntryType logEntryTypes, object[] logged)
            {
                DateTime = dateTime;
                LogEntryTypes = logEntryTypes;
                Logged = logged;
            }

            internal LogEntry(DateTime dateTime, string tag, object[] logged)
            {
                if (tag.Length is not 2 or 4)
                {
                    throw new ArgumentException($"Cannot insert {nameof(tag)}s with lengths other than 2 or 4.", nameof(tag));
                }

                DateTime = dateTime;
                LogEntryTypes = LogEntryType.Custom;
                Tag = tag;
                Logged = logged;
            }

            public override string ToString()
            {
                string str = $"[{DateTime:dd-MM-yyyy HH-mm-ss}]";
                if ((LogEntryTypes & LogEntryType.FollowUp) == LogEntryType.FollowUp)
                {
                    str += "[ -> ] ";
                }
                else if ((LogEntryTypes & LogEntryType.Info) == LogEntryType.Info)
                {
                    str += "[INFO] ";
                }
                else if ((LogEntryTypes & LogEntryType.SoftWarn) == LogEntryType.SoftWarn)
                {
                    str += "[SWRN] ";
                }
                else if ((LogEntryTypes & LogEntryType.Warn) == LogEntryType.Warn)
                {
                    str += "[WARN] ";
                }
                else if ((LogEntryTypes & LogEntryType.Error) == LogEntryType.Error)
                {
                    str += "[FAIL] ";
                }
                else if ((LogEntryTypes & LogEntryType.Custom) == LogEntryType.Custom)
                {
                    str += Tag.Length switch
                    {
                        4 => $"[{Tag}] ",
                        2 => $"[ {Tag} ] "
                    };
                }
                return Logged.Select(obj => str + obj.ToString()).Join("\r\n").Trim();
            }
        }

        public string this[int i] => Entries[i].ToString().Trim();

        public override string ToString() => Entries.Select(entry => entry).Join("\r\n").Trim();

        public int LogCustom(string tag, params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, tag, towrite));
            return Entries.Count;
        }
        public int LogFail(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.Fail, towrite));
            return Entries.Count;
        }
        public int LogFollowUpFail(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.FollowUpFail, towrite));
            return Entries.Count;
        }
        public int LogInfo(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.Info, towrite));
            return Entries.Count;
        }
        public int LogFollowUpInfo(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.FollowUp, towrite));
            return Entries.Count;
        }
        public int LogSoftWarn(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.SoftWarn, towrite));
            return Entries.Count;
        }
        public int LogFollowUpSoftWarn(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.FollowUpSoftWarn, towrite));
            return Entries.Count;
        }
        public int LogSuccess(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.Success, towrite));
            return Entries.Count;
        }
        public int LogFollowUpSuccess(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.FollowUpSuccess, towrite));
            return Entries.Count;
        }
        public int LogWarn(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.Warn, towrite));
            return Entries.Count;
        }
        public int LogFollowUpWarn(params object[] towrite)
        {
            Entries.Add(new(DateTime.Now, LogEntryType.FollowUpWarn, towrite));
            return Entries.Count;
        }

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, a specified <paramref name="inputDelimiter" /> and accepting input that is accepted by a <paramref name="validator"/> function. If the <paramref name="validator"/> function returns <c>false</c>, the prompt is repeatedly displayed until accepted input is received.
        /// </summary>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(IEnumerable<string>? promptlines, string inputDelimiter, Func<string, bool> validator)
        {
            string now = DateTime.Now.ToString(FormatString);
            string str = "";
            string ret = "";
            if (promptlines is not null)
            {
                if (promptlines.Any() && !promptlines.Any(x => x == ""))
                {
                    foreach (string promptline in promptlines)
                    {
                        str = $"[{now}][READ] {promptline}";
                        Console.WriteLine(str);
                    }
                    while (ret == "" || !validator(ret))
                    {
                        Console.Write(" ".Repeat(str.Length - (promptlines.Any() ? promptlines.Last() : "").Length - inputDelimiter.Length) + inputDelimiter);
                        ret = Console.ReadLine() ?? "";
                    }
                }
            }
            else
            {
                str = $"[{now}][READ] " + inputDelimiter;
                while (ret == "" || !validator(ret))
                {
                    Console.Write(str);
                    ret = Console.ReadLine() ?? "";
                }
            }

            return ret;
        }

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with a specified <paramref name="prompt"/>, a specified <paramref name="inputDelimiter" /> and accepting input that is accepted by a <paramref name="validator"/> function. If the <paramref name="validator"/> function returns <c>false</c>, the prompt is repeatedly displayed until accepted input is received.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(string prompt, string inputDelimiter, Func<string, bool> validator) => Read(new List<string>() { prompt }, inputDelimiter, validator);

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, a specified <paramref name="inputDelimiter" /> and accepting any non-blank input.
        /// </summary>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(IEnumerable<string>? promptlines, string inputDelimiter) => Read(promptlines, inputDelimiter, x => x != "");

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with a specified <paramref name="prompt"/>, a specified <paramref name="inputDelimiter" /> and accepting any non-blank input.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(string prompt, string inputDelimiter) => Read(new List<string>() { prompt }, inputDelimiter, x => x != "");

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, the default input delimiter and accepting any non-blank input. If the <paramref name="validator"/> function returns <c>false</c>, the prompt is repeatedly displayed until accepted input is received.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(string prompt, Func<string, bool> validator) => Read(new List<string>() { prompt }, ">> ", validator);

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, the default input delimiter and accepting input that is accepted by a <paramref name="validator"/> function. If the <paramref name="validator"/> function returns <c>false</c>, the prompt is repeatedly displayed until accepted input is received.
        /// </summary>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(IEnumerable<string>? promptlines, Func<string, bool> validator) => Read(promptlines, ">> ", validator);

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, the default input delimiter and accepting any non-blank input.
        /// </summary>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(IEnumerable<string>? promptlines) => Read(promptlines, ">> ", x => x != "");

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with no prompt, the default input delimiter and accepting input that is accepted by a <paramref name="validator"/> function.
        /// </summary>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(Func<string, bool> validator) => Read(new List<string>(), ">> ", validator);

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with a specified <paramref name="prompt"/>, the default input delimiter and accepting any non-blank input.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(string prompt) => Read(new List<string>() { prompt }, ">> ", x => x != "");

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with no prompt, the default input delimiter and accepting any non-blank input.
        /// </summary>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read() => Read(new List<string>(), ">> ", x => x != "");

        /// <summary>
        /// Writes a log line with custom attributes to the console.
        /// </summary>
        /// <param name="tag">The 0, 2 or 4-length tag to apply to the line. 2-length tags are padded with spaces. An empty string (0-length tag) means the tag is omitted entirely.</param>
        /// <param name="color">The <see cref="ConsoleColor"/> to apply to the line.</param>
        /// <param name="towrite">The object(s) to log.</param>
        /// <exception cref="ArgumentException" />
        public static void WriteCustom(string tag = "CUST", bool detailed = true, ConsoleColor color = ConsoleColor.White, params object[] towrite)
        {
            if (tag.Length is not 0 and not 2 and not 4)
            {
                throw new ArgumentException($"'{nameof(tag)}' must be empty, 2 or 4 chars long.", nameof(tag));
            }

            tag = tag.Replace(new List<string> { "\r", "\n" }, "");

            string now = DateTime.Now.ToString(FormatString);
            Console.ForegroundColor = color;
            if (detailed)
            {
                Console.WriteLine($"""
                                  [{now}]{(tag.Length == 0 ? "" : (tag.Length == 2 ? $"[ {tag} ]" : $"[{tag}]"))}
                                      From: {(new StackFrame(1).GetMethod() is not null ? new StackFrame(1).GetMethod().Name + "()" : "")}
                                      Content:
                                  {string.Join(Environment.NewLine, towrite.Select(obj => "        " + obj.ToString())).ForEachLine(line => "        " + line, line => !line.StartsWith("        "))}
                                  """);
            }
            else
            {
                foreach (object thing in towrite)
                {
                    Console.WriteLine($"[{now}]{(tag.Length == 0 ? "" : (tag.Length == 2 ? $"[ {tag} ]" : $"[{tag}]"))} {thing}");
                }
            }
            Console.ResetColor();
        }
        public static void WriteSuccess(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][SUCC] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFollowUpSuccess(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteInfo(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][INFO] {thing}");
            }
        }
        public static void WriteFollowUpInfo(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
        }
        public static void WriteWarn(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][WARN] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFollowUpWarn(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteSoftWarn(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][SWRN] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFollowUpSoftWarn(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFail(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][FAIL] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFollowUpFail(params object[] towrite)
        {
            string now = DateTime.Now.ToString(FormatString);
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
            Console.ResetColor();
        }
    }
}