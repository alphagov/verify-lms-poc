// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QTAccountCreation2;
//
//    var data = CreateAccount.FromJson(jsonString);
//
namespace local_matching.CreateAccount
{
    using System;
    using System.Net;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;
    using J = Newtonsoft.Json.JsonPropertyAttribute;
    using local_matching.YAMLC;
    using local_matching.DBODBC;

    public partial class    createAccount
    {

        public dynamic Process( ref YAML_Config yamlc)
        {
            // Pret is where we are going to put all of our answers
            Dictionary <string,string> PRET = new Dictionary <string,string>{};
            
            // Find out what the PID and LoA values are
            PRET.Add( "PID" , GetSafe(this.Pid) );
            PRET.Add( "LOA", GetSafe(this.LevelOfAssurance));
            PRET.Add( "USERNAME", GetSafe(this.Username));
            PRET.Add( "PASSWORD", GetSafe(this.Password));
            PRET.Add( "ACCOUNTID", GetSafe(this.AccountId));

            // We need to pull out the Firstname, Surname, and DoB, all of these have optional
            // properties

            // FIRST NAME
            try
            {
                PRET.Add("FN", GetSafe(this.FirstName?.Value));
                PRET.Add("FNVER", GetSafe(this.FirstName?.Verified).ToString() );
                PRET.Add("FNTO", GetSafe(this.FirstName?.ToDate));
                PRET.Add("FNFROM", GetSafe(this.FirstName?.FromDate));
            }
            catch
            {
                // Something went wrong with the forenames (object not defined)
            }
            // Not sure what it would do if there wasn't a surname?
            // SURNAME LIST
            try
            {
                var Surnames = this.Surnames.GetEnumerator();

                // Enumerate over the different surnames
                try
                {
                    var cnt = 0;
                    while (Surnames.MoveNext())
                    {
                        cnt++;

                        var SNto = GetSafe(Surnames.Current.ToDate);

                        DateTime dtTD = DateTime.Now;
                        DateTime dtNow = DateTime.Now;

                        if (string.IsNullOrEmpty(SNto)) // ToDate couild be blank, meaning they are still there
                        {
                            PRET.Add("SURNAMETO", SNto);
                            PRET.Add("SURNAMEFROM", GetSafe(Surnames.Current.FromDate));
                            PRET.Add("SURNAME", GetSafe(Surnames.Current.Value));
                            PRET.Add("SURNAMEVER", GetSafe(Surnames.Current.Verified).ToString() );
                        }
                        else          // ToDate could be ahead of Now, meaning they are still living there
                        {
                            dtTD = Convert.ToDateTime(SNto);
                            if (DateTime.Compare(dtTD, dtNow) >= 0)
                            {
                                PRET.Add("SURNAMETO", SNto);
                                PRET.Add("SURNAMEFROM", GetSafe(Surnames.Current.FromDate));
                                PRET.Add("SURNAME", GetSafe(Surnames.Current.Value));
                                PRET.Add("SURNAMEVER", GetSafe(Surnames.Current.Verified).ToString() );
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

            // Gender
            try
            {
                PRET.Add("GENDER", GetSafe(this.Gender?.Value));
                PRET.Add("GENDERVER", GetSafe(this.Gender?.Verified).ToString());
                PRET.Add("GENDERTO", GetSafe(this.Gender?.ToDate));
                PRET.Add("GENDERFROM", GetSafe(this.Gender?.FromDate));
            }
            catch
            {
                // No Gender information
            }
            // Date of Birth
            try
            {
                PRET.Add("DOB", GetSafe(this.DateOfBirth?.Value));
                PRET.Add("DOBRVER", GetSafe(this.DateOfBirth?.Verified).ToString());
                PRET.Add("DOBTO", GetSafe(this.DateOfBirth?.ToDate));
                PRET.Add("DOBFROM", GetSafe(this.DateOfBirth?.FromDate));
            }
            catch
            {
                // No DOB info
            }
            // If this is NULL then no address would be searched.
            var Address = GetSafe(this.Address);
            try
            {

                var PCto = GetSafe(Address.ToDate);

                // -------------------------------------------------------------------------------
                // We need to decide which of the Addresses to use
                // -------------------------------------------------------------------------------
                DateTime dtTD = DateTime.Now;
                DateTime dtNow = DateTime.Now;

                if (string.IsNullOrEmpty(PCto)) // ToDate couild be blank, meaning they are still there
                {
                    PRET.Add("INTERNATIONALPOSTCODE", GetSafe(Address.InternationalPostCode));
                    PRET.Add("POSTCODE", GetSafe(Address.PostCode));
                    PRET.Add("POSTCODEVER", GetSafe(Address.Verified).ToString());
                    PRET.Add("POSTCODEFROM", GetSafe(Address.FromDate));
                    PRET.Add("POSTCODETO", PCto );
                }
                else          // ToDate could be ahead of Now, meaning they are still living there
                {
                    dtTD = Convert.ToDateTime(PCto);
                    if (DateTime.Compare(dtTD, dtNow) >= 0)
                    {
                        PRET.Add("INTERNATIONALPOSTCODE", GetSafe(Address.InternationalPostCode));
                        PRET.Add("POSTCODE", GetSafe(Address.PostCode));
                        PRET.Add("POSTCODEVER", GetSafe(Address.Verified).ToString());
                        PRET.Add("POSTCODEFROM", GetSafe(Address.FromDate));
                        PRET.Add("POSTCODETO", PCto );
                    }
                }

                var Lines = Address.Lines.GetEnumerator();

                // Enumerate over the different addresses
                try
                {
                    var cnt = 1;
                    while (Lines.MoveNext())
                    {
                        PRET.Add("ADDRESS" + cnt.ToString(), GetSafe(Lines.Current));
                        cnt++;
                    }
                }
                finally
                {
                    Lines.Dispose();
                }
            }
            catch
            {
                // We don't need to do anything
            }
            // We need to ensure that we log what we are doing into the database
            var testDB = new DB_Worker(yamlc.GetLS("LODBC"),
                                        yamlc.GetLS("LSERVER"), // server needed for IP based connections
                                        yamlc.GetLS("LDB"),
                                        yamlc.GetLS("LDBUN"),
                                        yamlc.GetLS("LDBPW"));


            string[] ret = testDB.Read(yamlc.GetLM(yamlc.GetLM("SEARCH1")).Replace("#" + yamlc.GetLM("SEARCH1").ToUpper() + "#", PRET.GetValueOrDefault(yamlc.GetLM("SEARCH1").ToUpper())));

            if (ret.Count() > 0)
            {
                PRET.Add("ALREADYEXISTS",ret[0].ToString());
            }
            else
            {
                string Y = yamlc.GetLC("SEARCH1");
                string[] sep = new string[] { };
                int cnt = 1;
                if (Y.Substring(0, 1) == "[")
                {
                    sep = Y.Substring(2, Y.Length - 4).Replace(" ", "").Split(",");
                    cnt = sep.Count();
                }
                else
                {
                    List<string> tmp = new List<string> { }; // This is a bit rubbish!
                    tmp.Add(Y);
                    sep = tmp.ToArray();
                }

                // We always need to add the account ID if we know it
                testDB.Go( yamlc.GetLC( yamlc.GetLC("SEARCH1") ).Replace("#" + sep[0].ToUpper() + "#", PRET.GetValueOrDefault(sep[0].ToUpper()))
                                                                .Replace("#" + sep[1].ToUpper() + "#", PRET.GetValueOrDefault(sep[1].ToUpper()))                                    
                                                                );
                PRET.Add("INSERTING", "NewRecord");
            }

            // If we are debug mode, just return back the PRET
            if (yamlc.GetSet("DEBUG") == "true")
                return PRET;

            return "{result: success}";
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

        [J("address")] public Address Address { get; set; }
        [J("dateOfBirth")] public DateOfBirth DateOfBirth { get; set; }
        [J("firstName")] public DateOfBirth FirstName { get; set; }
        [J("gender")] public Gender Gender { get; set; }
        [J("levelOfAssurance")] public string LevelOfAssurance { get; set; }
        [J("password")] public string Password { get; set; }
        [J("pid")] public string Pid { get; set; }
        [J("accountid")] public string AccountId { get; set; }
        [J("surnames")] public List<DateOfBirth> Surnames { get; set; }
        [J("username")] public string Username { get; set; }
    }

    public partial class Gender
    {
        [J("fromDate")] public string FromDate { get; set; }
        [J("toDate")] public string ToDate { get; set; }
        [J("value")] public string Value { get; set; }
        [J("verified")] public bool Verified { get; set; }
    }

    public partial class DateOfBirth
    {
        [J("fromDate")] public string FromDate { get; set; }
        [J("toDate")] public string ToDate { get; set; }
        [J("value")] public string Value { get; set; }
        [J("verified")] public bool Verified { get; set; }
    }

    public partial class Address
    {
        [J("lines")] public List<string> Lines { get; set; }
        [J("postCode")] public string PostCode { get; set; }
        [J("verified")] public bool Verified { get; set; }
        [J("fromDate")] public string FromDate { get; set; }
        [J("toDate")] public string ToDate { get; set; }
    }

    public partial class CreateAccount
    {
        public static List<CreateAccount> FromJson(string json) => JsonConvert.DeserializeObject<List<CreateAccount>>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<CreateAccount> self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
