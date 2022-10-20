using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeGiftDataManager
{
    public static class ArgumentManager
    {
        private static readonly string[] ValidArgs = { "--help", "--parse", "--build", "--save" };
        private static readonly string[] _ValidArgs = { "-h", "-p", "-b", "-s" };

        public static bool IsValidArgument(string[] arguments)
        {
            if (!(2 <= arguments.Length && arguments.Length <= 5))
                return false;

            foreach (var arg in arguments)
            {
                if (!ValidArgs.Contains(arg))
                    if (!_ValidArgs.Contains(arg))
                        if (!CheckValidFilePath(arg, false))
                            if (!CheckValidFolderPath(arg))
                                return false;
            }
            return CheckCorrectArgSequence(arguments);
        }

        public static Command GetCommand(string argument)
        {
            if (argument.Equals(ValidArgs[0]) ||
                argument.Equals(_ValidArgs[0]))
                return Command.Help;
            else if (argument.Equals(ValidArgs[1]) ||
                argument.Equals(_ValidArgs[1]))
                return Command.Parse;
            else if (argument.Equals(ValidArgs[2]) ||
                argument.Equals(_ValidArgs[2]))
                return Command.Build;
            else if (argument.Equals(ValidArgs[3]) ||
                argument.Equals(_ValidArgs[3]))
                return Command.Save;

            return Command.Null;
        }

        public static List<byte[]> GetFilesToCompute()
        {
            Command cmd = GetCommand(Environment.GetCommandLineArgs()[1]);
            List<byte[]> fileList = new();

            string path = "";
            if (cmd is Command.Null)
                path = Environment.GetCommandLineArgs()[1];
            else
                path = Environment.GetCommandLineArgs()[2];

            if (CheckValidFilePath(path))
                fileList.Add(File.ReadAllBytes(path));
            else
                foreach (string file in Directory.EnumerateFiles(path))
                    fileList.Add(File.ReadAllBytes(file));

            return fileList;
        }

        public static Tuple<string, bool> GetSavePath()
        {
            string[] args = Environment.GetCommandLineArgs();
            int argsLength = args.Length;
            string path = "";
            bool toFile = false;

            if (argsLength == 2)
            {
                if (CheckValidFolderPath(args[1], false))
                {
                    path = args[1];
                    toFile = false;
                }
                else if (CheckValidFilePath(args[1], false))
                {
                    path = $"{Path.GetDirectoryName(args[1])!}\\";
                    toFile = false;
                }
            }
            else if (argsLength == 3)
            {
                if (GetCommand(args[1]) is Command.Parse or Command.Build)
                {
                    if (CheckValidFolderPath(args[2], false))
                    {
                        path = args[2];
                        toFile = false;
                    }
                    else if (CheckValidFilePath(args[2], false))
                    {
                        path = $"{Path.GetDirectoryName(args[2])!}\\";
                        toFile = false;
                    }
                }
                else if (GetCommand(args[2]) is Command.Save)
                {
                    if (CheckValidFolderPath(args[1], false))
                    {
                        path = args[1];
                        toFile = false;
                    }
                    else if (CheckValidFilePath(args[1], false))
                    {
                        path = $"{Path.GetDirectoryName(args[1])!}\\";
                        toFile = false;
                    }
                }
                else if (CheckValidFilePath(args[1]) || CheckValidFolderPath(args[1]))
                {
                    if (CheckValidFolderPath(args[2], false))
                    {
                        path = args[2];
                        toFile = false;
                    }

                    else if (CheckValidFilePath(args[2], false))
                    {
                        path = args[2];
                        toFile = true;
                    }
                }
            }
            else if (argsLength == 4)
            {
                if (GetCommand(args[1]) is Command.Parse or Command.Build)
                {
                    if (GetCommand(args[3]) is Command.Save)
                    {
                        if (CheckValidFolderPath(args[2], false))
                        {
                            path = args[2];
                            toFile = false;
                        }
                        else if (CheckValidFilePath(args[2], false))
                        {
                            path = $"{Path.GetDirectoryName(args[2])!}\\";
                            toFile = false;
                        }
                    }
                    else if (CheckValidFilePath(args[3], false) || CheckValidFolderPath(args[3], false))
                    {
                        if (CheckValidFolderPath(args[3], false))
                        {
                            path = args[3];
                            toFile = false;
                        }
                        else if (CheckValidFilePath(args[3], false))
                        {
                            path = args[3];
                            toFile = true;
                        }
                    }
                }
                else if (CheckValidFilePath(args[1]) || CheckValidFolderPath(args[1]))
                {
                    if (CheckValidFolderPath(args[3], false))
                    {
                        path = args[3];
                        toFile = false;
                    }
                    else if (CheckValidFilePath(args[3], false))
                    {
                        path = args[3];
                        toFile = true;
                    }
                }
            }
            else if (argsLength == 5)
            {
                if (CheckValidFolderPath(args[4], false))
                {
                    path = args[4];
                    toFile = false;
                }
                else if (CheckValidFilePath(args[4], false))
                {
                    path = args[4];
                    toFile = true;
                }
            }

            if (path.Equals(""))
            {
                path = Path.GetDirectoryName(args[0])!;
                toFile = false;
            }

            return Tuple.Create(path, toFile);
        }

        private static bool CheckValidFilePath(string path, bool checkFile = true)
        {
            if (checkFile)
                return File.Exists(path);

            string? folder = Path.GetDirectoryName(path)!.Equals("") ? Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) : Path.GetDirectoryName(path);
            return CheckValidFolderPath(folder!, false);

        }

        private static bool CheckValidFolderPath(string path, bool checkFiles = true)
        {
            if (!Directory.Exists(path))
                return false;

            if (checkFiles)
                return new DirectoryInfo(path).GetFileSystemInfos().Length >= 0;

            return true;
        }

        private static bool CheckCorrectArgSequence(string[] arguments)
        {
            if (GetCommand(arguments[1]) is Command.Parse or Command.Build)
            {
                if (arguments.Length >= 3)
                    if (CheckValidFilePath(arguments[2]) || CheckValidFolderPath(arguments[2]))
                    {
                        if (arguments.Length == 3)
                            return true;

                        if (arguments.Length == 4)
                        {
                            if (CheckValidFolderPath(arguments[3], false) || CheckValidFilePath(arguments[3], false))
                                return true;
                        }
                        else if (arguments.Length == 5)
                        {
                            if (GetCommand(arguments[3]) is Command.Save)
                                if (CheckValidFolderPath(arguments[4], false) || CheckValidFilePath(arguments[4], false))
                                    return true;
                        }
                    }
            }
            else if (CheckValidFilePath(arguments[1], true) || CheckValidFolderPath(arguments[1], true))
            {
                if (arguments.Length == 2)
                    return true;
                if (arguments.Length == 3 &&
                    (CheckValidFolderPath(arguments[2], false) || CheckValidFilePath(arguments[2], false)))
                    return true;
                if (arguments.Length == 4 && GetCommand(arguments[2]) is Command.Save)
                    if (CheckValidFolderPath(arguments[3], false) || CheckValidFilePath(arguments[3], false))
                        return true;
            }

            return false;
        }
    }
}
