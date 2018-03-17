using MsgPack;
using MsgPack.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace BoundlessMsgPackToJson
{
    public class Parser
    {
        public static void DoParse(string outputDir, string inputFile)
        {
            if (!Directory.Exists(outputDir))
            {
                throw new Exception("Output directory does not exist!");
            }

            if (Directory.Exists(inputFile))
            {
                if (!inputFile.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
                {
                    inputFile += Path.DirectorySeparatorChar;
                }

                foreach (string curFile in Directory.GetFiles(inputFile, "*.msgpack", SearchOption.AllDirectories))
                {
                    Console.WriteLine($"File: {Path.GetFileNameWithoutExtension(curFile)}");

                    string realOutDir = Path.Combine(outputDir, Path.GetDirectoryName(curFile).Substring(inputFile.Length), $"{Path.GetFileNameWithoutExtension(curFile)}.json");

                    DoParseMsgpack(realOutDir, curFile);
                }
            }
            else
            {
                Console.WriteLine($"File: {Path.GetFileNameWithoutExtension(inputFile)}");

                if (Path.GetExtension(inputFile) == ".msgpack")
                {
                    DoParseMsgpack(Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(inputFile)}.json"), inputFile);
                }
                else
                {
                    throw new Exception("Input file extension must be '.msgpack'!");
                }
            }
        }

        public static void DoParseMsgpack(string outputDir, string inputFile)
        {
            MessagePackObject inputObj = MessagePackSerializer.UnpackMessagePackObject(new FileStream(inputFile, FileMode.Open, FileAccess.Read)).ParseLookupTable("inputFile");
            JToken root = CreateToken(inputObj);

            string dir = Path.GetDirectoryName(outputDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            while (File.Exists(outputDir))
            {
                try
                {
                    File.Delete(outputDir);
                }
                catch { }
            }

            File.WriteAllText(outputDir, root.ToString(Formatting.Indented));

            Console.WriteLine("Done!");
        }

        private static JToken CreateToken(MessagePackObject curObj)
        {
            if (curObj.IsDictionary)
            {
                JObject resultObj = new JObject();

                Dictionary<string, MessagePackObject> curDict = curObj.AsDictionary("inputFile");

                foreach (KeyValuePair<string, MessagePackObject> curProp in curDict)
                {
                    resultObj[curProp.Key] = CreateToken(curProp.Value);
                }

                return resultObj;
            }
            else if (curObj.IsArray)
            {
                JArray resultArray = new JArray();

                IList<MessagePackObject> curArray = curObj.AsList();

                foreach (MessagePackObject curElem in curArray)
                {
                    resultArray.Add(CreateToken(curElem));
                }

                return resultArray;
            }
            else if (curObj.IsNil)
            {
                return null;
            }
            else if (curObj.IsTypeOf<Int64>().HasValue && curObj.IsTypeOf<Int64>().Value)
            {
                return curObj.AsInt64();
            }
            else if (curObj.IsTypeOf<double>().HasValue && curObj.IsTypeOf<double>().Value)
            {
                return curObj.AsDouble();
            }
            else if (curObj.IsTypeOf<string>().HasValue && curObj.IsTypeOf<string>().Value)
            {
                return curObj.AsString();
            }
            else if (curObj.IsTypeOf<bool>().HasValue && curObj.IsTypeOf<bool>().Value)
            {
                return curObj.AsBoolean();
            }
            else
            {
                throw new Exception("Unknown Type!");
            }
        }
    }
}
