// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QTAccountCreation;
//
//    var data = AccountCreation.FromJson(jsonString);
//
namespace local_matching.AccountCreation
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using J = Newtonsoft.Json.JsonPropertyAttribute;

    public partial class accountCreation
    {
        [J("attributes")] public Attributes Attributes { get; set; }
        [J("levelOfAssurance")] public string LevelOfAssurance { get; set; }
        [J("pid")] public string Pid { get; set; }
        [J("scenario")] public string Scenario { get; set; }
    }

    public partial class Attributes
    {
        [J("address")] public Address Address { get; set; }
        [J("addressHistory")] public List<AddressHistory> AddressHistory { get; set; }
        [J("cycle3")] public string Cycle3 { get; set; }
        [J("dateOfBirth")] public DateOfBirth DateOfBirth { get; set; }
        [J("firstName")] public DateOfBirth FirstName { get; set; }
        [J("middleName")] public DateOfBirth MiddleName { get; set; }
        [J("surname")] public DateOfBirth Surname { get; set; }
    }

    public partial class DateOfBirth
    {
        [J("value")] public string Value { get; set; }
        [J("verified")] public bool Verified { get; set; }
        [J("fromDate")] public string FromDate { get; set; }
        [J("toDate")] public string ToDate { get; set; }
    }

    public partial class AddressHistory
    {
        [J("value")] public FluffyValue Value { get; set; }
        [J("verified")] public bool Verified { get; set; }
        [J("fromDate")] public string FromDate { get; set; }
        [J("toDate")] public string ToDate { get; set; }
    }

    public partial class FluffyValue
    {
        [J("fromDate")] public string FromDate { get; set; }
        [J("internationalPostCode")] public object InternationalPostCode { get; set; }
        [J("lines")] public List<string> Lines { get; set; }
        [J("postCode")] public string PostCode { get; set; }
        [J("toDate")] public string ToDate { get; set; }
        [J("uprn")] public object Uprn { get; set; }
    }

    public partial class Address
    {
        [J("value")] public PurpleValue Value { get; set; }
        [J("verified")] public bool Verified { get; set; }
    }

    public partial class PurpleValue
    {
        [J("fromDate")] public object FromDate { get; set; }
        [J("internationalPostCode")] public object InternationalPostCode { get; set; }
        [J("lines")] public List<string> Lines { get; set; }
        [J("postCode")] public string PostCode { get; set; }
        [J("toDate")] public object ToDate { get; set; }
        [J("uprn")] public object Uprn { get; set; }
    }

    public partial class AccountCreation
    {
        public static AccountCreation FromJson(string json) => JsonConvert.DeserializeObject<AccountCreation>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this AccountCreation self) => JsonConvert.SerializeObject(self, Converter.Settings);
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

