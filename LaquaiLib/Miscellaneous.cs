namespace LaquaiLib;

public static class Miscellaneous
{
    public static ushort SM64RNG(ushort input)
    {
        ushort S0;
        ushort S1;

        if (input == 0x560A)
        {
            input = 0;
        }

        S0 = (ushort)((ushort)(input & 0xFF) << 8);
        S0 = (ushort)(S0 ^ input);
        input = (ushort)((ushort)((ushort)(input & 0xFF) << 8) | (ushort)((ushort)(input & 0xFF) >> 8));
        S0 = (ushort)((ushort)((ushort)(S0 & 0xFF) << 1) ^ input);

        S1 = (ushort)((ushort)(S0 >> 1) ^ 0xFF80);
        if ((S0 & 1) == 0)
        {
            if (S1 == 0xAA55)
            {
                input = 0;
            }
            else
            {
                input = (ushort)(S1 ^ 0x1FF4);
            }
        }
        else
        {
            input = (ushort)(S1 ^ 0x8180);
        }
        return input;
    }

    public static class LoggingMethods
    {
        /// <summary>
        /// Reads a line of input from the <see cref="Console"/> with any number of specified <paramref name="promptlines"/>, a specified <paramref name="inputDelimiter" /> and accepting input that is accepted by a <paramref name="validator"/> function. If the <paramref name="validator"/> function returns <c>false</c>, the prompt is repeatedly displayed until accepted input is received.
        /// </summary>
        /// <param name="promptlines">The lines of the prompt to display.</param>
        /// <param name="inputDelimiter">The input delimiter to display.</param>
        /// <param name="validator">A validator function to test whether the collected input should be accepted.</param>
        /// <returns>A string containing the line read from standard input.</returns>
        public static string Read(IEnumerable<string>? promptlines, string inputDelimiter, Func<string, bool> validator)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
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
        public static void WriteCustom(string tag = "CUST", ConsoleColor color = ConsoleColor.White, params object[] towrite)
        {
            if (tag.Length is not 0 and not 2 and not 4)
            {
                throw new ArgumentException($"'{nameof(tag)}' must be empty, 2 or 4 chars long.", nameof(tag));
            }

            tag = tag.Replace(new List<string> { "\r", "\n" }, "");

            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            Console.ForegroundColor = color;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}]{(tag.Length == 0 ? "" : (tag.Length == 2 ? $"[ {tag} ]" : $"[{tag}]"))} {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteSuccess(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][SUCC] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFollowUpSuccess(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteInfo(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][INFO] {thing}");
            }
        }
        public static void WriteFollowUpInfo(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
        }
        public static void WriteWarn(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][WARN] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFollowUpWarn(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteSoftWarn(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][SWRN] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFollowUpSoftWarn(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFail(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][FAIL] {thing}");
            }
            Console.ResetColor();
        }
        public static void WriteFollowUpFail(params object[] towrite)
        {
            string now = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss");
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (object thing in towrite)
            {
                Console.WriteLine($"[{now}][ -> ] {thing}");
            }
            Console.ResetColor();
        }
    }
}