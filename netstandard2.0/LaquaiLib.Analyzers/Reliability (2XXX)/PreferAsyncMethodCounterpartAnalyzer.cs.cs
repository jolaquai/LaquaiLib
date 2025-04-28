namespace LaquaiLib.Analyzers.Reliability__2XXX_;

// LAQ2001
public class PreferAsyncMethodCounterpartAnalyzer
{
    /*
     public async Task SomeMethodAsync()
     {
         Stream s = ...;

         s.Write([2, 3, 4]); // <- Should become s.WriteAsync([2, 3, 4]); or s.WriteAsync([2, 3, 4]).ConfigureAwait(false);
         s.Dispose(); // <- Should become await s.DisposeAsync().ConfigureAwait(false);
     }
     */
}
