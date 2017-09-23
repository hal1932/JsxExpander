using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JsxExpander
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"D:\tmp\jsx\test1.jsx";

            var includeFiles = new List<string>();

            var targets = new Queue<string>();
            targets.Enqueue(path);

            var finished = new List<string>();

            while (targets.Any())
            {
                var target = targets.Dequeue();

                using (var reader = new StreamReader(target))
                {
                    var includePaths = new List<string>();

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine().Trim();
                        if (line.Length == 0 || !line.StartsWith("#"))
                        {
                            break;
                        }

                        if (line.StartsWith("#includepath"))
                        {
                            var items = line.Split('"')[1].Split(';');
                            foreach (var item in items)
                            {
                                var fullname = Path.Combine(Path.GetDirectoryName(target), item);
                                if (!includePaths.Contains(fullname))
                                {
                                    includePaths.Add(fullname);
                                }
                            }
                        }
                        else if (line.StartsWith("#include"))
                        {
                            var items = line.Split('"')[1].Split(';');
                            foreach (var item in items)
                            {
                                var fullname = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(target), item));
                                if (!File.Exists(fullname))
                                {
                                    foreach (var includePath in includePaths)
                                    {
                                        fullname = Path.GetFullPath(Path.Combine(includePath, item));
                                        if (File.Exists(fullname))
                                        {
                                            break;
                                        }
                                    }
                                }

                                if (!File.Exists(fullname))
                                {
                                    Console.Error.WriteLine("file not exists: {0}, included from {1}", item, target);
                                    continue;
                                }

                                if (!includeFiles.Contains(fullname))
                                {
                                    includeFiles.Add(fullname);
                                    //Console.WriteLine("{0} (from {1})", fullname, target);
                                }

                                if (!finished.Contains(fullname))
                                {
                                    finished.Add(fullname);
                                    targets.Enqueue(fullname);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var includeFile in includeFiles)
            {
                using (var reader = new StreamReader(includeFile))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.TrimStart().StartsWith("#"))
                        {
                            continue;
                        }
                        Console.WriteLine(line);
                    }
                }
            }

            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line.TrimStart().StartsWith("#"))
                    {
                        continue;
                    }
                    Console.WriteLine(reader.ReadLine());
                }
            }

            Console.Read();
        }
    }
}
