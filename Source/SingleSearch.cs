using System;
using System.IO;
using System.Collections.Generic;

namespace local_matching.SingleSearch
{
    public class singleSearch
    {
        // This takes the dictionary information that contains all of our extracted values and database
        // search results from trawling the remote database.
        // These are then processed to work out which is the best candidate for the match.
        public void Process(ref Dictionary<string, string> PRET)
        {
            // This is where we do the very clever stuff
            List<string[]> Surnames = new List<string[]>{};
            List<string[]> DoB = new List<string[]>{};
            List<string[]> Postcode = new List<string[]>{};

            // OK there are various ways to be "clever" and come up with the right answer
            // Lets not for the moment, lets just see which ID has the most entries
            Dictionary<int,int> Counters = new Dictionary<int,int> { };

            // These will also count to see what number of each account are
            // going to be coming back
            ExpandAndCount( ref PRET, ref Counters, ref Surnames, "MATCHINGCOUNTSURNAME","SUR");

            ExpandAndCount(ref PRET, ref Counters, ref DoB, "MATCHINGCOUNTDOB", "DOB");

            ExpandAndCount(ref PRET, ref Counters, ref Postcode, "MATCHINGCOUNTPOSTCODE", "POS");

            // Find out which of the ID's is there the most
            int cntr_i=0;
            int cntr_max=0;
            int cntr_same=0;
            foreach (KeyValuePair<int, int> entry in Counters)
            {
                if (entry.Value == cntr_max) { cntr_same++; }
                if (entry.Value > cntr_max) { cntr_i = entry.Key; cntr_same=0; cntr_max = entry.Value; }
            }

            if (cntr_same>0)
            {
                PRET.Add("NOBESTCANDIDATE","Total of " + (cntr_same+1).ToString() + " same score.");
            }
            else
            {
                PRET.Add("BESTCANDIDATE",cntr_i.ToString()+" which got "+cntr_max.ToString()+" hits.");
            }
       }

       // This function will iterate through the Dictionary, pulling out each database line, extract
       // out each of its fields, then it also counts the number of times the primary account ID's are
       // Hit. (these are always index 1)
        public void ExpandAndCount( ref Dictionary<string,string> PRET , ref Dictionary<int,int> Counters , ref List<string[]> Surnames, string count, string prefix)
        {
            int icnt = Convert.ToInt32(PRET.GetValueOrDefault(count));
            for (int i = 0; i < icnt; i++)
            {
                Surnames.Add(PRET.GetValueOrDefault("SUR" + (i + 1).ToString()).Split("|"));
                try
                {
                    Counters[Convert.ToInt32(Surnames[i].GetValue(1))]++;

                }
                catch
                {
                    Counters.Add(Convert.ToInt32(Surnames[i].GetValue(1)), 1);
                }
            }
        }
    }
}

