using Arch.CoreInfo.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Customer.User.Members.InfoSecurityCoreLogic
{
    static class Util
    {
        public static void Merge(IDictionary<KeyType, IDictionary<string, string>> result, IDictionary<KeyType, IDictionary<string, string>> data)
        {
            foreach (var entry in data)
            {
                if (entry.Value == null) continue;

                if (result.TryGetValue(entry.Key, out var map))
                {
                    foreach (var item in entry.Value)
                    {
                        map.Add(item.Key, item.Value);
                    }
                }
                else
                {
                    result.Add(entry.Key, entry.Value);
                }

            }
        }

        public static List<List<RawKey>> Split(List<RawKey> keys, int batchSize)
        {
            List<List<RawKey>> listOfBatch = new List<List<RawKey>>();
            List<RawKey> batch = null;
            for (int i = 0; i < keys.Count; ++i)
            {
                if ((i % batchSize) == 0)
                {
                    batch = new List<RawKey>();
                    listOfBatch.Add(batch);
                }
                batch.Add(keys[i]);
            }
            return listOfBatch;
        }
    }
}
