using System;
using System.IO;

namespace Bishop_BSG_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Bishop BSG Tool");
                Console.WriteLine("  -- Created by Crsky");
                Console.WriteLine("Usage:");
                Console.WriteLine("  Create    : Bishop_BSG_Tool [image.png|folder]");
                Console.WriteLine();
                Console.WriteLine("Help:");
                Console.WriteLine("  This tool is only works with 'BSS-Graphics' files,");
                Console.WriteLine("    please check the file header first.");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var path = Path.GetFullPath(args[0]);

            if (Utility.PathIsFolder(path))
            {
                foreach (var item in Directory.EnumerateFiles(path, "*.png"))
                {
                    Convert(item);
                }
            }
            else
            {
                Convert(path);
            }

            Console.WriteLine("Done");
        }

        static void Convert(string filePath)
        {
            try
            {
                var bsgPath = Path.ChangeExtension(filePath, ".bsg");
                var pngName = Path.GetFileName(filePath);

                Console.WriteLine($"Converting {pngName}");

                BSG.Create(bsgPath, filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
