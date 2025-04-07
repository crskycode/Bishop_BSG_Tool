using System;
using System.IO;

namespace Bishop_BSG_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Bishop BSG Tool");
                Console.WriteLine("  -- Created by Crsky");
                Console.WriteLine("Usage:");
                Console.WriteLine("  Create    : Bishop_BSG_Tool -c [-bpp8|-bpp32] [image.png|folder]");
                Console.WriteLine("  Extract   : Bishop_BSG_Tool -e [-bpp8|-bpp32] [image.bsg|folder]");
                Console.WriteLine();
                Console.WriteLine("Help:");
                Console.WriteLine("  This tool is only works with 'BSS-Graphics' files,");
                Console.WriteLine("    please check the file header first.");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            var mode = args[0];
            var path = Path.GetFullPath(args[2]);

            var bpp = 0;

            if (args[1] == "-bpp8")
            {
                bpp = 8;
            }
            else if (args[1] == "-bpp32")
            {
                bpp = 32;
            }
            else
            {
                Console.WriteLine("This tool currently only supports bpp8 and bpp32 formats.");
                return;
            }

            if (mode == "-c")
            {
                if (bpp == 32)
                {
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
                }
                else if (bpp == 8)
                {
                    if (Utility.PathIsFolder(path))
                    {
                        foreach (var item in Directory.EnumerateFiles(path, "*.png"))
                        {
                            ConvertBpp8(item);
                        }
                    }
                    else
                    {
                        ConvertBpp8(path);
                    }
                }
            }
            else if (mode == "-e")
            {
                if (bpp == 8)
                {
                    if (Utility.PathIsFolder(path))
                    {
                        foreach (var item in Directory.EnumerateFiles(path, "*.bsg"))
                        {
                            ExtractBpp8(item);
                        }
                    }
                    else
                    {
                        ExtractBpp8(path);
                    }
                }
                else
                {
                    Console.WriteLine("Extract mode currently only supports the BPP8 format.");
                    return;
                }
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

        static void ConvertBpp8(string filePath)
        {
            try
            {
                var bsgPath = Path.ChangeExtension(filePath, ".bsg");
                var pngName = Path.GetFileName(filePath);

                Console.WriteLine($"Converting {pngName}");

                BSG.CreateBpp8(bsgPath, filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        static void ExtractBpp8(string filePath)
        {
            try
            {
                var pngPath = Path.ChangeExtension(filePath, ".png");
                var bsgName = Path.GetFileName(filePath);

                Console.WriteLine($"Converting {bsgName}");

                BSG.ExtractBpp8(pngPath, filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
