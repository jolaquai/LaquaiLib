namespace LaquaiLib.Analyzers.Fixes;

internal static class Helpers
{
    extension(CompilationUnitSyntax compilationUnitSyntax)
    {
        /// <summary>
        /// Adds the specified using directive to the compilation unit if it does not already exist. If it does, the return value is a reference to the original compilation unit.
        /// <para/>This will most likely break <see cref="WellKnownFixAllProviders.BatchFixer"/>. Usage in conjunction with it is discouraged.
        /// </summary>
        /// <returns></returns>
        public CompilationUnitSyntax AddUsingsIfNotExists(params UsingDirectiveSyntax[] usingDirectiveSyntaxes)
        {
            var existingUsings = new HashSet<string>(compilationUnitSyntax.Usings.Select(static u => u.Name.ToString()));
            var filtered = usingDirectiveSyntaxes.Where(u => !existingUsings.Contains(u.Name.ToString())).ToArray();
            return filtered.Length == 0 ? compilationUnitSyntax : compilationUnitSyntax.AddUsings(filtered);
        }
    }
    extension(Document document)
    {
        public Task<CompilationUnitSyntax> Root => document.GetRootAsync(CancellationToken.None);
        public async Task<CompilationUnitSyntax> GetRootAsync(CancellationToken cancellationToken = default)
            => (CompilationUnitSyntax)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
    }
    extension<T>(T del) where T : Delegate
    {
        public T[] InvocationList => Unsafe.As<T[]>(del.GetInvocationList());
    }

    private static readonly ValueTask _completedTask = new ValueTask(Task.CompletedTask);
    extension(ValueTask)
    {
        public static ValueTask CompletedTask => _completedTask;
    }
    extension(string s)
    {
        public string ToTitleCase()
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            // Split the string by spaces
            var words = s.Split(' ');

            for (var i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    // Convert first character to uppercase and the rest to lowercase
                    var letters = words[i].ToLower().ToCharArray();
                    if (letters.Length > 0)
                    {
                        letters[0] = char.ToUpper(letters[0]);
                    }

                    words[i] = new string(letters);
                }
            }

            // Join the words back together
            return string.Join(" ", words);
        }
    }
}
