using MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoundlessMsgPackToJson
{
    public static class BoundlessMsgPackExtensions
    {
        private static Dictionary<string, string[]> keyTables = new Dictionary<string, string[]>();

        public static Dictionary<string, MessagePackObject> AsDictionary(this MessagePackObject dictionary, string lookupTableId)
        {
            if (!keyTables.TryGetValue(lookupTableId, out string[] lookupDict))
            {
                throw new IndexOutOfRangeException($"lookupTableId does not exist: '{lookupTableId}'");
            }

            return dictionary.AsDictionary()?.ToDictionary(cur => lookupDict[cur.Key.AsInt32()], cur => cur.Value);
        }

        public static MessagePackObject ParseLookupTable(this MessagePackObject rootObject, string lookupTableId)
        {
            MessagePackObject lookupObject = rootObject.AsList()[1];
            rootObject = rootObject.AsList()[0];

            keyTables[lookupTableId] = lookupObject.AsList().Select(cur => cur.AsString()).ToArray();

            return rootObject;
        }

        public static void FreeLookupTable(string lookupTableId)
        {
            if (keyTables.ContainsKey(lookupTableId))
            {
                keyTables.Remove(lookupTableId);
            }
        }
    }
}
