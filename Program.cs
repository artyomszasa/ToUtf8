using System.Diagnostics;
using System.Text;

internal partial class Program
{
    private static void Main(string[] args)
    {
        foreach (var arg in args)
        {
            var paths = Directory.EnumerateFiles(arg, "*", SearchOption.AllDirectories)
                .Where(static path => path.EndsWith(".cs") || path.EndsWith(".razor") || path.EndsWith(".xaml"));

            foreach (var path in paths)
            {
                if (!path.Contains("/obj/Debug/net"))
                {
                    if (path.EndsWith("SeoEditor.razor"))
                    {


                    var originalEncoding = DetectEncoding(path);
                    if (!StringComparer.InvariantCultureIgnoreCase.Equals(originalEncoding.WebName, "utf-8"))
                    {
                        string source = File.ReadAllText(path, originalEncoding);
                        File.WriteAllText (path, source, Encoding.UTF8);
                        Console.WriteLine($"{path}: {originalEncoding.WebName} => utf-8");
                    }
                    else
                    {
                        // Console.WriteLine($"{path} is utf-8, skipping");
                    }
                    }
                }
            }
        }
    }
}