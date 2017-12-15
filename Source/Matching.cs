// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QTMatching;
//
//    var data = Matching.FromJson(jsonString);
//

namespace local_matching.Matching
{
    using System;
    using System.Net;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using local_matching.YAMLC;
    using local_matching.DBODBC;

    public partial class matching
    {
        public dynamic Process(ref YAML_Config yamlc)
        {
            Dictionary<string,string> PRET = new Dictionary<string,string>{};

            // Find out what the cycle3 matching values are
            PRET.Add("HASHPID", GetSafe(this.HashedPid));
            PRET.Add("MATCHID", GetSafe(this.MatchId));
            PRET.Add("LOA", GetSafe(this.LevelOfAssurance));

            // Date of Birth
            PRET.Add("DOB", GetSafe(this.MatchingDataset.DateOfBirth.Value));
            PRET.Add("DOBVER", GetSafe(this.MatchingDataset.DateOfBirth.Verified).ToString() ); // bool
            PRET.Add("DOBTO", GetSafe(this.MatchingDataset.DateOfBirth.To));
            PRET.Add("DOBFROM", GetSafe(this.MatchingDataset.DateOfBirth.From));

            // Get the drivers license, although really this could be any kind
            // of attribute, not sure how we "Differentiate" between them,
            try
            {
                PRET.Add("DRIVERSLICENSE", GetSafe(this.Cycle3Dataset.Attributes.DriversLicence));
            }
            catch
            {
                // Driver license is missing
            }

            // Not sure what it would do if there wasn't a surname?
            // SURNAME LIST
            try
            {
                var Surnames = this.MatchingDataset.Surnames.GetEnumerator();

                // Enumerate over the different surnames
                try
                {
                    var cnt = 0;
                    while (Surnames.MoveNext())
                    {
                        cnt++;

                        var SNto = GetSafe(Surnames.Current.To);

                        DateTime dtTD = DateTime.Now;
                        DateTime dtNow = DateTime.Now;

                        if (string.IsNullOrEmpty(SNto)) // ToDate couild be blank, meaning they are still there
                        {
                            PRET.Add("SURNAMETO", SNto);
                            PRET.Add("SURNAMEFROM", GetSafe(Surnames.Current.From));
                            PRET.Add("SURNAME", GetSafe(Surnames.Current.Value));
                            PRET.Add("SURNAMEVER", GetSafe(Surnames.Current.Verified).ToString());
                        }
                        else          // ToDate could be ahead of Now, meaning they are still living there
                        {
                            dtTD = Convert.ToDateTime(SNto);
                            if (DateTime.Compare(dtTD, dtNow) >= 0)
                            {
                                PRET.Add("SURNAMETO", SNto);
                                PRET.Add("SURNAMEFROM", GetSafe(Surnames.Current.From));
                                PRET.Add("SURNAME", GetSafe(Surnames.Current.Value));
                                PRET.Add("SURNAMEVER", GetSafe(Surnames.Current.Verified).ToString());
                            }
                        }
                    }
                }
                finally
                {
                    Surnames.Dispose();
                }
            }
            catch
            {

            }


            try
            {
                // If this is NULL then no address would be searched.
                var Address = this.MatchingDataset.Addresses.GetEnumerator();

                // Enumerate over the different addresses
                int count=0;
                try
                {

                    while (Address.MoveNext())
                    {
                        count++;

                        var PCto = GetSafe(Address.Current.ToDate);

                        // -------------------------------------------------------------------------------
                        // We need to decide which of the Addresses to use
                        // -------------------------------------------------------------------------------
                        DateTime dtTD = DateTime.Now;
                        DateTime dtNow = DateTime.Now;

                        Boolean thisAddr=false;

                        if (string.IsNullOrEmpty(PCto)) // ToDate couild be blank, meaning they are still there
                        {
                            PRET.Add("POSTCODE", GetSafe(Address.Current.PostCode));
                            PRET.Add("POSTCODEVER", GetSafe(Address.Current.Verified).ToString());
                            PRET.Add("POSTCODEFROM", GetSafe(Address.Current.FromDate));
                            PRET.Add("POSTCODETO", PCto);
                            thisAddr = true;
                        }
                        else          // ToDate could be ahead of Now, meaning they are still living there
                        {
                            dtTD = Convert.ToDateTime(PCto);
                            if (DateTime.Compare(dtTD, dtNow) >= 0)
                            {
                                PRET.Add("POSTCODE", GetSafe(Address.Current.PostCode));
                                PRET.Add("POSTCODEVER", GetSafe(Address.Current.Verified).ToString());
                                PRET.Add("POSTCODEFROM", GetSafe(Address.Current.FromDate));
                                PRET.Add("POSTCODETO", PCto);
                                thisAddr = true;
                            }
                        }

                        try
                        {
                            var Lines = Address.Current.Lines.GetEnumerator();

                            if (thisAddr) // ToDate couild be blank, meaning they are still there
                            {
                                try
                                {
                                    var cnt = 1;
                                    while (Lines.MoveNext())
                                    {
                                        PRET.Add("ADDRESS"+cnt.ToString() , GetSafe(Lines.Current));
                                        cnt++;
                                    }
                                }
                                finally
                                {
                                    Lines.Dispose();
                                }
                            }
                        }
                        catch
                        {
                            // No addresse lines
                        }
                    }
                }
                finally
                {
                    Address.Dispose();
                }
            }
            catch
            {
                // No address enumerator at all
            }


            // We need to search the database to find out if we have a match
            // new { result = myReturn, matching_count = myCount };

            return PRET;
        }

        public void AlreadyCreatedSearch( ref YAML_Config yamlc, ref Dictionary <string,string> PRET )
        {
            // Cycle 0 - Check to see if we have the person in a local database as a PID
            // First find out if we already have the citizen
            // We need to ensure that we log what we are doing into the database
            var testDB = new DB_Worker(yamlc.GetLS("LODBC"),
                                        yamlc.GetLS("LSERVER"), // server needed for IP based connections
                                        yamlc.GetLS("LDB"),
                                        yamlc.GetLS("LDBUN"),
                                        yamlc.GetLS("LDBPW"));

            // This is going to return a list of possible matches (should only be one)
            string[] ret = testDB.Read(yamlc.GetLM("PiD").Replace("#PID#", MatchId));
            if (ret.Count() > 0)
            {
                PRET.Add("MATCHING", ret[0]);
                // We have a match, so would normally exit here
            }
            //return PRET;
        }

        // If we don't find it locally, we need to say we don't
        public void TrawlRemoteDatabase(ref YAML_Config yamlc, ref Dictionary<string, string> PRET)
        {
            var testDB = new DB_Worker(yamlc.GetRS("RODBC"),
                                        yamlc.GetRS("RSERVER"), // server needed for IP based connections
                                        yamlc.GetRS("RDB"),
                                        yamlc.GetRS("RDBUN"),
                                        yamlc.GetRS("RDBPW"));

            string[] post = new string[] { };
            ReadDB(ref PRET, ref yamlc, ref testDB, ref post, "POSTCODE", "Postcode");

            string[] dob = new string[] { };
            ReadDB(ref PRET, ref yamlc, ref testDB, ref dob, "DOB", "DoB");

            string[] sur = new string[] { };
            ReadDB(ref PRET, ref yamlc, ref testDB, ref sur, "SURNAME", "Surname");
        }

        // This function is a helper which will read the database, and add any results onto the PRET dictionary.
        // It will automatically do the substitution
        public void ReadDB( ref Dictionary<string,string>PRET, ref YAML_Config yamlc, ref DB_Worker testDB, ref string[] post , string PR, string Y)
        {
            if (!string.IsNullOrEmpty(PRET.GetValueOrDefault(PR)))
            {
    #if DEBUG
                Console.WriteLine(Y+" - " + PRET.GetValueOrDefault(PR));
    #endif
                post = testDB.Read(yamlc.GetRM(Y).Replace("#"+PR+"#", PRET.GetValueOrDefault(PR)));
            }
            PRET.Add("MATCHINGCOUNT"+PR, post.Count().ToString());
            for (int i = 0; i < post.Count(); i++) { PRET.Add(PR.Substring(0,3) + (i + 1).ToString(), post[i]); }
        }

    // This function allows us to read values from the JSON which may not exist
    // in the packet being sent.
    public dynamic GetSafe(dynamic value)
        {
            dynamic FNto;
            try
            {
                FNto = value;
            }
            catch
            {
                FNto = "";
            }
            return FNto;
        }

        [J("cycle3Dataset")] public Cycle3Dataset Cycle3Dataset { get; set; }
        [J("hashedPid")] public string HashedPid { get; set; }
        [J("levelOfAssurance")] public string LevelOfAssurance { get; set; }
        [J("matchId")] public string MatchId { get; set; }
        [J("matchingDataset")] public MatchingDataset MatchingDataset { get; set; }
    }

    public partial class MatchingDataset
    {
        [J("addresses")] public List<Address> Addresses { get; set; }
        [J("dateOfBirth")] public DateOfBirth DateOfBirth { get; set; }
        [J("firstName")] public DateOfBirth FirstName { get; set; }
        [J("gender")] public DateOfBirth Gender { get; set; }
        [J("middleNames")] public DateOfBirth MiddleNames { get; set; }
        [J("surnames")] public List<Surname> Surnames { get; set; }
    }

    public partial class Surname
    {
        [J("from")] public string From { get; set; }
        [J("to")] public string To { get; set; }
        [J("value")] public string Value { get; set; }
        [J("verified")] public bool Verified { get; set; }
    }

    public partial class DateOfBirth
    {
        [J("from")] public string From { get; set; }
        [J("to")] public string To { get; set; }
        [J("value")] public string Value { get; set; }
        [J("verified")] public bool Verified { get; set; }
    }

    public partial class Address
    {
        [J("fromDate")] public string FromDate { get; set; }
        [J("internationalPostCode")] public string InternationalPostCode { get; set; }
        [J("lines")] public List<string> Lines { get; set; }
        [J("postCode")] public string PostCode { get; set; }
        [J("toDate")] public string ToDate { get; set; }
        [J("uprn")] public string Uprn { get; set; }
        [J("verified")] public bool Verified { get; set; }
    }

    public partial class Cycle3Dataset
    {
        [J("attributes")] public Attributes Attributes { get; set; }
    }

    public partial class Attributes
    {
        [J("drivers_licence")] public string DriversLicence { get; set; }
    }

    public partial class Matching
    {
        public static Matching FromJson(string json) => JsonConvert.DeserializeObject<Matching>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Matching self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }

}

