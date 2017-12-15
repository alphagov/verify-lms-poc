using System;
using System.IO;
using System.Collections.Generic;
using local_matching.YAMLC;

namespace local_matching.SingleSearch
{
    public class singleSearch
    {
        // This takes the dictionary information that contains all of our extracted values and database
        // search results from trawling the remote database.
        // These are then processed to work out which is the best candidate for the match.
        public void Process(ref YAML_Config yamlc, ref Dictionary<string, string> PRET)
        {
            // OK there are various ways to be "clever" and come up with the right answer
            // Lets not for the moment, lets just see which ID has the most entries
            Dictionary<int, int> Counters = new Dictionary<int, int> { };

            // These will also count to see what number of each account are
            // going to be coming back.

            // Lets get the search count
            int cnt = Convert.ToInt32(yamlc.GetRM("SEARCHCOUNT"));

            // At the moment, we discard the broken out data, we have already at this
            // point decided to just use basic counting method, otherwise we actually
            // should be using a long hand version and not using the YAML interation
            for (int i = 1; i <= cnt; i++)
            {
                List<string[]> MyDBList = new List<string[]> { };
                ExpandAndCount(ref PRET, ref Counters, ref MyDBList,
                        "MATCHINGCOUNT" + yamlc.GetRM("SEARCH" + i).ToUpper(),
                            yamlc.GetRM("SEARCH" + i).ToUpper().Substring(0, 3),
                                Convert.ToInt32(yamlc.GetRM("WEIGHT" + i)));
            }

            // This hand coded example lines are left in, this allows us to actually be used so that
            // each of the search results could be queried, and reduced in a far smarter way that
            // we are currently doing.

            // ExpandAndCount( ref PRET, ref Counters, ref Surnames, "MATCHINGCOUNTSURNAME","SUR");
            // ExpandAndCount(ref PRET, ref Counters, ref DoB, "MATCHINGCOUNTDOB", "DOB");
            // ExpandAndCount(ref PRET, ref Counters, ref Postcode, "MATCHINGCOUNTPOSTCODE", "POS");

            // Find out which of the ID's is there the most
            int cntr_i = 0;
            int cntr_max = 0;
            int cntr_same = 0;
            foreach (KeyValuePair<int, int> entry in Counters)
            {
                if (entry.Value == cntr_max) { cntr_same++; }
                if (entry.Value > cntr_max) { cntr_i = entry.Key; cntr_same = 0; cntr_max = entry.Value; }
            }

            if (cntr_same > 0)
            {
                PRET.Add("NOBESTCANDIDATE", "Total of " + (cntr_same + 1).ToString() + " same score.");
            }
            else
            {
                PRET.Add("BESTCANDIDATE", cntr_i.ToString() + " which got " + cntr_max.ToString() + " hits.");
            }
        }

        // This function will iterate through the Dictionary, pulling out each database line, extract
        // out each of its fields, then it also counts the number of times the primary account ID's are
        // Hit. (these are always index 1)
        public void ExpandAndCount(ref Dictionary<string, string> PRET, ref Dictionary<int, int> Counters, ref List<string[]> TheDBList, string count, string prefix, int weight)
        {
            int icnt = Convert.ToInt32(PRET.GetValueOrDefault(count));
            for (int i = 0; i < icnt; i++)
            {
                TheDBList.Add(PRET.GetValueOrDefault(prefix + (i + 1).ToString()).Split("|"));
                try
                {
                    Counters[Convert.ToInt32(TheDBList[i].GetValue(1))] += weight;

                }
                catch
                {
                    Counters.Add(Convert.ToInt32(TheDBList[i].GetValue(1)), weight);
                }
            }
        }
    }
}

