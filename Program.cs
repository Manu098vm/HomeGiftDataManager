using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;

namespace HomeGiftDataManager
{
    public static class Program
    {
        private static void ShowHelp()
        {
            var msg =
                $"HomeGiftDataManager v0.0.1{Environment.NewLine}" +
                $"{Environment.NewLine}" +
                $"Usage:{Environment.NewLine}" +
                $"--help            -h                  Show this dialogue.{Environment.NewLine}" +
                $"--parse PATH      -p PATH             Parse JSON from GiftBoxData binaries.{Environment.NewLine}" +
                $"                                      Path must be an existing file, or a folder containing files.{Environment.NewLine}" +
                $"--build PATH      -b PATH             Build GiftBoxData binaries from JSON.{Environment.NewLine}" +
                $"                                      Path must be an existing file, or a folder containing files.{Environment.NewLine}" +
                $"--save PATH       -s PATH             Save computed data to path.{Environment.NewLine}" +
                $"                                      Path must be an existing folder, or a new file.{Environment.NewLine}" +
                $"{Environment.NewLine}";
            Log(msg);
        }

        public static void Main()
        {
            if (!ArgumentManager.IsValidArgument(Environment.GetCommandLineArgs()))
                ShowHelp();
            else
                TryCompute();
            return;
        }

        private static void TryCompute()
        {
            var toComputeList = ArgumentManager.GetFilesToCompute();

            if (ArgumentManager.GetCommand(Environment.GetCommandLineArgs()[1]) is Command.Build)
                JSONToBin(toComputeList);
            else
                BinToJSON(toComputeList);
        }

        private static void JSONToBin(List<byte[]> toComputeList)
        {
            var computedList = new List<byte[]>();
            var nameList = new List<string>();
            foreach (var file in toComputeList)
            {
                try
                {
                    var str = Encoding.UTF8.GetString(file);
                    var json = JsonSerializer.Deserialize<dynamic>(str);
                    var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
                    if (json is not null)
                    {
                        var byt = StringToBytes(JsonSerializer.Serialize(json, options));
                        if (byt != null)
                        {
                            computedList.Add(byt);
                            nameList.Add(GetGiftID(str));
                        }
                    }
                } catch (Exception)
                {
                    Log("Invalid file, skipping...");
                }
            }

            (var path, var toFile) = ArgumentManager.GetSavePath();
            foreach (var file in computedList.Select((el, i) => new {i, el}))
            {
                if (!toFile)
                {
                    var name = $"{nameList.ElementAt(file.i)}.bin";
                    path = $"{path}\\{name}";
                }
                else if (toFile && File.Exists(path))
                {
                    var name = $"{nameList.ElementAt(file.i)}.bin";
                    path = $"{Path.GetDirectoryName(path)}\\{name}";
                }
                File.WriteAllBytes($"{path}", file.el);
                path = Path.GetDirectoryName(path);
            }
        }

        private static void BinToJSON(List<byte[]> toComputeList)
        {
            var computedList = new List<string>();
            foreach (var file in toComputeList)
            {
                try
                {
                    var str = file.BytesToString();
                    if (str is not null && str.Length > 0)
                    {
                        var json = JsonSerializer.Deserialize<dynamic>(str);
                        var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
                        if (json is not null)
                            computedList.Add(JsonSerializer.Serialize(json, options));
                    }
                }
                catch (Exception)
                {
                    Log("Invalid file, skipping...");
                }
            }

            if (IsDataList(computedList.ElementAt(0)))
            {
                var l = JsonSerializer.Deserialize<GiftBoxDataList>(computedList.ElementAt(0));
                computedList.Clear();
                foreach(var boxData in l!.boxDataList!)
                {
                    var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
                    computedList.Add(JsonSerializer.Serialize(boxData, options));
                }
            }

            
            (var path, var toFile) = ArgumentManager.GetSavePath();
            foreach (var file in computedList)
            {
                if (!toFile)
                {
                    var name = $"{GetGiftID(file)}.json";
                    path = $"{path}\\{name}";
                }
                else if (toFile && File.Exists(path))
                {
                    var name = $"{GetGiftID(file)}.json";
                    path = $"{Path.GetDirectoryName(path)}\\{name}";
                }
                File.WriteAllText($"{path}", file);
                path = Path.GetDirectoryName(path);
            }
        }

        private static byte[]? StringToBytes(this string? str)
        {
            if (str is not null)
            {
                List<string> toReplace = new();
                char[] chars = str.ToCharArray();
                for (int i = 0; i < chars.Length - 1; i++)
                {
                    if (chars[i] == '\u005c' && chars[i + 1] == 'u')
                    {
                        string unicodeC = new string(new char[] { chars[i], chars[i + 1], chars[i + 2], chars[i + 3], chars[i + 4], chars[i + 5] });
                        var add = true;
                        foreach (var el in toReplace)
                            if (el.Equals(unicodeC))
                                add = false;
                        if (add)
                            toReplace.Add(unicodeC);
                    }
                }

                foreach (var s in toReplace)
                {
                    Int32 c = Convert.ToInt32(s.Substring(2), 16);
                    str = str.Replace(s, char.ConvertFromUtf32(c));
                }
                return Encoding.Unicode.GetBytes(str);
            }
            return null;
        }

        private static string? BytesToString(this byte[]? bytes)
        {
            if (bytes is not null)
            {
                var str = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (i + 1 != bytes.Length)
                    {
                        byte _1 = bytes[i];
                        byte _2 = bytes[i + 1];
                        switch (_2)
                        {
                            case 0x00:
                                char c = Convert.ToChar(_1);
                                str.Append(c);
                                break;
                            default:
                                string s = Encoding.Unicode.GetString(new byte[] { _1, _2 });
                                str.Append(s);
                                break;
                        }
                        i++;
                    }
                    else
                    {
                        char c = Convert.ToChar(bytes[i]);
                        str.Append(c);
                    }
                }
                return str.ToString();
            }
            return null;
        }

        private static bool IsDataList(string json)
        {
            try
            {
                var str = JsonSerializer.Deserialize<GiftBoxDataList>(json);
                var list = str!.boxDataList!;
                if (list is not null)
                    return true;
                else return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string GetGiftID(string gift)
        {
            var json = JsonSerializer.Deserialize<GiftBoxData>(gift);
            var id = json!.gfMId!.ToString();
            return id!;
        }

        private static void Log(string str) => Console.WriteLine(str);
    }
}