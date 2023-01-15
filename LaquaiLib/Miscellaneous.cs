using LaquaiLib.Extensions;

using System.CodeDom;
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

        [Flags]
        internal enum LogEntryType
        {
            Info = 0b000_0010,
            SoftWarn = 0b000_0100,
            Warn = 0b000_1000,
            Error = 0b001_0000,
            Fail = 0b001_0000,
            Success = 0b010_0000,
            Custom = 0b100_0000,
            FollowUp = 0b000_0001,
            FollowUpInfo = Info | FollowUp,
            FollowUpSoftWarn = SoftWarn | FollowUp,
            FollowUpWarn = Warn | FollowUp,
            FollowUpError = Error | FollowUp,
            FollowUpFail = Fail | FollowUp,
            FollowUpSuccess = Success | FollowUp,
        }

        private List<LogEntry> Entries { get; } = new();

        internal class LogEntry
        {
            internal DateTime DateTime
            {
                get; init;
            }
            internal LogEntryType LogEntryTypes
            {
                get; init;
            }
            internal string? Tag
            {
                get; init;
            }
            internal object[] Logged
            {
                get; init;
            }

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
                if (LogEntryTypes.HasFlag(LogEntryType.FollowUp))
                {
                    str += "[ -> ] ";
                }
                else if (LogEntryTypes.HasFlag(LogEntryType.Info))
                {
                    str += "[INFO] ";
                }
                else if (LogEntryTypes.HasFlag(LogEntryType.SoftWarn))
                {
                    str += "[SWRN] ";
                }
                else if (LogEntryTypes.HasFlag(LogEntryType.Warn))
                {
                    str += "[WARN] ";
                }
                else if (LogEntryTypes.HasFlag(LogEntryType.Error))
                {
                    str += "[FAIL] ";
                }
                else if (LogEntryTypes.HasFlag(LogEntryType.Custom))
                {
                    str += Tag!.Length switch
                    {
                        4 => $"[{Tag}] ",
                        2 => $"[ {Tag} ] "
                    };
                }
                return string.Join(Environment.NewLine, Logged.Select(obj => str + obj.ToString())).Trim();
            }
        }

        public string this[int i] => Entries[i].ToString().Trim();

        public override string ToString() => string.Join(Environment.NewLine, Entries.Select(entry => entry)).Trim();

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
        /// Reads lines of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, a specified <paramref name="inputDelimiter" /> and accepting input that is accepted by a <paramref name="validator"/> function. Input lines are collected until the <paramref name="validator"/> function returns <c>false</c> for the first time.
        /// </summary>
        /// <remarks>The calling code is responsible for defining a <paramref name="validator"/> function that returns <c>false</c> at some point. If it doesn't, this method will never return.</remarks>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple(IEnumerable<string> promptlines, string inputDelimiter, Func<string, bool> validator)
        {
            string now = DateTime.Now.ToString(FormatString);
            List<string> returnList = new();
            string str = "";
            string ret = "";
            if (promptlines is not null && promptlines.Any() && !promptlines.All(x => x == ""))
            {
                foreach (string promptline in promptlines)
                {
                    str = $"[{now}][READ] {promptline}";
                    Console.WriteLine(str);
                }
                while (true)
                {
                    Console.Write(inputDelimiter.PadLeft(str.Length - (promptlines.Any() ? promptlines.Last() : "").Length));
                    ret = Console.ReadLine() ?? "";
                    if (validator(ret))
                    {
                        returnList.Add(ret);
                    }
                    else
                    {
                        //WriteInfo("Input terminated");
                        return returnList.Select();
                    }
                }
            }
            else
            {
                str = $"[{now}][READ] " + inputDelimiter;
                while (true)
                {
                    Console.Write(str);
                    ret = Console.ReadLine() ?? "";
                    if (validator(ret))
                    {
                        returnList.Add(ret);
                    }
                    else
                    {
                        //WriteInfo("Input terminated");
                        return returnList.Select();
                    }
                }
            }
        }

        /// <summary>
        /// Reads lines of input from the <see cref="Console"/> with a specified <paramref name="prompt"/>, a specified <paramref name="inputDelimiter" /> and accepting input that is accepted by a <paramref name="validator"/> function. Input lines are collected until the <paramref name="validator"/> function returns <c>false</c> for the first time.
        /// </summary>
        /// <remarks>The calling code is responsible for defining a <paramref name="validator"/> function that returns <c>false</c> at some point. If it doesn't, this method will never return.</remarks>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple(string prompt, string inputDelimiter, Func<string, bool> validator) => ReadMultiple(new List<string>() { prompt }, inputDelimiter, validator);

        /// <summary>
        /// Reads lines of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/> and a specified <paramref name="inputDelimiter" /> until blank input is received.
        /// </summary>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple(IEnumerable<string> promptlines, string inputDelimiter) => ReadMultiple(promptlines, inputDelimiter, x => x != "");

        /// <summary>
        /// Reads lines of input from the <see cref="Console"/> with a specified <paramref name="prompt"/> and a specified <paramref name="inputDelimiter" /> until blank input is received.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple(string prompt, string inputDelimiter) => ReadMultiple(new List<string>() { prompt }, inputDelimiter, x => x != "");

        /// <summary>
        /// Reads lines of input from the <see cref="Console"/> with a specified <paramref name="prompt"/>, the default input delimiter and accepting input that is accepted by a <paramref name="validator"/> function. Input lines are collected until the <paramref name="validator"/> function returns <c>false</c> for the first time.
        /// </summary>
        /// <remarks>The calling code is responsible for defining a <paramref name="validator"/> function that returns <c>false</c> at some point. If it doesn't, this method will never return.</remarks>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple(string prompt, Func<string, bool> validator) => ReadMultiple(new List<string>() { prompt }, ">> ", validator);

        /// <summary>
        /// Reads lines of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, the default input delimiter and accepting input that is accepted by a <paramref name="validator"/> function. Input lines are collected until the <paramref name="validator"/> function returns <c>false</c> for the first time.
        /// </summary>
        /// <remarks>The calling code is responsible for defining a <paramref name="validator"/> function that returns <c>false</c> at some point. If it doesn't, this method will never return.</remarks>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple(IEnumerable<string> promptlines, Func<string, bool> validator) => ReadMultiple(promptlines, ">> ", validator);

        /// <summary>
        /// Reads lines of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, the default input delimiter and accepting any non-blank input.
        /// </summary>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple(IEnumerable<string> promptlines) => ReadMultiple(promptlines, ">> ", x => x != "");

        /// <summary>
        /// Reads lines of input from the <see cref="Console"/> with no prompt and the default input delimiter and accepting input that is accepted by a <paramref name="validator"/> function. Input lines are collected until the <paramref name="validator"/> function returns <c>false</c> for the first time.
        /// </summary>
        /// <remarks>The calling code is responsible for defining a <paramref name="validator"/> function that returns <c>false</c> at some point. If it doesn't, this method will never return.</remarks>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple(Func<string, bool> validator) => ReadMultiple(new List<string>(), ">> ", validator);

        /// <summary>
        /// Reads lines of input from the <see cref="Console"/> with a specified <paramref name="prompt"/> and the default input delimiter until blank input is received.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple(string prompt) => ReadMultiple(new List<string>() { prompt }, ">> ", x => x != "");

        /// <summary>
        /// Reads lines of input from the <see cref="Console"/> with no prompt and the default input delimiter until blank input is received.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of type <see cref="string"/> containing the lines read from standard input or null if no input was provided.</returns>
        public static IEnumerable<string> ReadMultiple() => ReadMultiple(new List<string>(), ">> ", x => x != "");

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, a specified <paramref name="inputDelimiter" /> and accepting input that is accepted by a <paramref name="validator"/> function. If the <paramref name="validator"/> function returns <c>false</c>, the prompt is repeatedly displayed until accepted input is received.
        /// </summary>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(IEnumerable<string> promptlines, string inputDelimiter, Func<string, bool> validator)
        {
            string now = DateTime.Now.ToString(FormatString);
            string str = "";
            string ret = "";
            if (promptlines is not null && promptlines.Any() && !promptlines.All(x => x == ""))
            {
                foreach (string promptline in promptlines)
                {
                    str = $"[{now}][READ] {promptline}";
                    Console.WriteLine(str);
                }
                do
                {
                    Console.Write(inputDelimiter.PadLeft(str.Length - (promptlines.Any() ? promptlines.Last() : "").Length));
                    ret = Console.ReadLine() ?? "";
                } while (!validator(ret));
            }
            else
            {
                str = $"[{now}][READ] " + inputDelimiter;
                do
                {
                    Console.Write(str);
                    ret = Console.ReadLine() ?? "";
                } while (!validator(ret));
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
        public static string Read(IEnumerable<string> promptlines, string inputDelimiter) => Read(promptlines, inputDelimiter, x => x != "");

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with a specified <paramref name="prompt"/>, a specified <paramref name="inputDelimiter" /> and accepting any non-blank input.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(string prompt, string inputDelimiter) => Read(new List<string>() { prompt }, inputDelimiter, x => x != "");

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with a specified <paramref name="prompt"/>, the default input delimiter and accepting any non-blank input. If the <paramref name="validator"/> function returns <c>false</c>, the prompt is repeatedly displayed until accepted input is received.
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
        public static string Read(IEnumerable<string> promptlines, Func<string, bool> validator) => Read(promptlines, ">> ", validator);

        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, the default input delimiter and accepting any non-blank input.
        /// </summary>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(IEnumerable<string> promptlines) => Read(promptlines, ">> ", x => x != "");

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
        /// <param name="tag">The 0, 2 or 4-length tag to apply to the line. 2-length tags are padded with a space on either side. An empty string (0-length tag) means the tag is omitted entirely.</param>
        /// <param name="detailed">Whether to show extensive information about how this method was called in addition to the objects to log.</param>
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

        /// <summary>
        /// Writes log lines to the <see cref="Console"/> as "success" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteSuccess(params object[] towrite) => WriteCustom("SUCC", false, ConsoleColor.Green, towrite);
        /// <summary>
        /// Writes log lines to the console as follow-up information to "success" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteFollowUpSuccess(params object[] towrite) => WriteCustom("->", false, ConsoleColor.Green, towrite);

        /// <summary>
        /// Writes log lines to the <see cref="Console"/> as general "information" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteInfo(params object[] towrite) => WriteCustom("INFO", false, ConsoleColor.White, towrite);
        /// <summary>
        /// Writes log lines to the console as follow-up information to general "information" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteFollowUpInfo(params object[] towrite) => WriteCustom("->", false, ConsoleColor.White, towrite);

        /// <summary>
        /// Writes log lines to the <see cref="Console"/> as "warning" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteWarn(params object[] towrite) => WriteCustom("WARN", false, ConsoleColor.DarkYellow, towrite);
        /// <summary>
        /// Writes log lines to the console as follow-up information to "warning" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteFollowUpWarn(params object[] towrite) => WriteCustom("->", false, ConsoleColor.DarkYellow, towrite);

        /// <summary>
        /// Writes log lines to the <see cref="Console"/> as "soft warning" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteSoftWarn(params object[] towrite) => WriteCustom("SWRN", false, ConsoleColor.Yellow, towrite);
        /// <summary>
        /// Writes log lines to the console as follow-up information to "soft warning" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteFollowUpSoftWarn(params object[] towrite) => WriteCustom("->", false, ConsoleColor.Yellow, towrite);

        /// <summary>
        /// Writes log lines to the <see cref="Console"/> as "failure" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteFail(params object[] towrite) => WriteCustom("FAIL", false, ConsoleColor.Red, towrite);
        /// <summary>
        /// Writes log lines to the console as follow-up information to "failure" messages.
        /// </summary>
        /// <param name="towrite">The objects to write to the <see cref="Console"/>.</param>
        public static void WriteFollowUpFail(params object[] towrite) => WriteCustom("->", false, ConsoleColor.Red, towrite);

        public enum TableInputMode
        {
            /// <summary>
            /// Indicates that the associated value contains rows of data.
            /// </summary>
            Rows,
            /// <summary>
            /// Indicates that the associated value contains columns of data.
            /// </summary>
            Columns
        }

        /// <summary>
        /// Writes an <see cref="IEnumerable{T}"/> of <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> to the <see cref="Console"/> by formatting the contained values to look like a table using the specified <paramref name="tableInputMode"/>.
        /// </summary>
        /// <param name="input">The collections of values to write.</param>
        /// <param name="tableInputMode">How the <paramref name="input"/> value is to be interpreted as indicated by a <see cref="TableInputMode"/> value.</param>
        public static void WriteAsTable<T>(IEnumerable<IEnumerable<T>> input, TableInputMode tableInputMode = TableInputMode.Rows)
            where T : notnull
        {
            #region Enumerate and invert input data ("columns") to use as "rows"
            int maxInnerEnumerableCount = input.Max(innerEnumerable => innerEnumerable.Count());

            // Enumerate input into a List<List<string>> while ensuring that every row and column has an equal Count
            List<List<string>> original = input.Select(innerEnumerable =>
            {
                List<T> existingInner = innerEnumerable.ToList();

                List<string> newInner = new();
                for (int i = 0; i < maxInnerEnumerableCount; i++)
                {
                    newInner.Add(
                        i < existingInner.Count
                        ? existingInner[i] is null || existingInner[i].ToString() is null ? "<null>" : existingInner[i].ToString()!
                        : ""
                    );
                }
                return newInner;
            }).ToList();

            // Invert columns to use as rows
            List<List<string>> inverted = Enumerable.Range(0, maxInnerEnumerableCount).Select(i => new List<string>()).ToList();

            foreach (List<string> innerEnumerable in original)
            {
                for (int i = 0; i < maxInnerEnumerableCount; i++)
                {
                    inverted[i].Add(i < innerEnumerable.Count ? innerEnumerable[i] : "");
                }
            }
            #endregion

            // Output
            if (tableInputMode == TableInputMode.Rows)
            {
                // Determine the needed width of each column
                List<int> columnWidths = inverted.Select(innerEnumerable => innerEnumerable.Max(str => str.Length)).ToList();
                foreach (List<string> column in original)
                {
                    Console.WriteLine(string.Join(" | ", column.Select((cell, i) => cell.PadRight(columnWidths[i]))));
                }
            }
            else
            {
                // Determine the needed width of each column
                List<int> columnWidths = original.Select(innerEnumerable => innerEnumerable.Max(str => str.Length)).ToList();
                foreach (List<string> row in inverted)
                {
                    Console.WriteLine(string.Join(" | ", row.Select((cell, i) => cell.PadRight(columnWidths[i]))));
                }
            }
        }
    }
}
