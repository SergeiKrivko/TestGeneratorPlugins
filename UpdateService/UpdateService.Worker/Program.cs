using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace UpdateService.Worker
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var dstPath = args[0];
            var srcPath = args[1];
            
            await CopyFiles(srcPath, dstPath, 5);
            
            Directory.Delete(srcPath, recursive: true);

            Process.Start(Path.Combine(dstPath, "TestGenerator"));
            
            return 0;
        }

        private static async Task CopyFiles(string srcPath, string dstPath, int retryCount = 1)
        {
            Console.WriteLine($"{srcPath} {dstPath}");
            foreach (var file in Directory.EnumerateFiles(srcPath))
            {
                await CopyFile(file, Path.Combine(dstPath, Path.GetFileName(file)), retryCount);
            }
            foreach (var directory in Directory.EnumerateDirectories(srcPath))
            {
                await CopyFiles(directory, Path.Combine(dstPath, Path.GetFileName(directory)));
            }
        }

        private static async Task CopyFile(string srcPath, string dstPath, int retryCount = 1)
        {
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    Console.WriteLine($"{srcPath} -> {dstPath}");
                    File.Copy(srcPath, dstPath, overwrite: true);
                    return;
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }

                await Task.Delay(1000);
            }

            throw new Exception();
        }
    }
}