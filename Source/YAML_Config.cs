using System;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;
/*

//##########################################################################################
// YAML Configuration
//##########################################################################################

This currently only handles a single "replacable" value in each of the sections, however this
can be simply advanced in two ways.

1.  The need to support multiple parameters, e.g. "Extend" the current "Name" to allow us to
    specify secondary terms to be replaced within the "Query".

2.  Allow for #NAME?# where the ? is a driver for the multiple lines reponses we get, for 
    example address lines. Here if we specify that type of term an additional type of check
    will occur on the data. This will dynamically map across, to try different combinations
    and handle missing lines.

            e.g.

                    1 -->  2
                    2 -->  5
                    5 -->  4

            We also give a small advantage if they are in the correct order as well as
            getting a match. So above will get an advatage score for the 1, 2, but not
            for the 5->4 match.

 */
namespace local_matching.YAMLC
{
    public class YAML_Config
    {

        private Dictionary<string, string> localSettings = new Dictionary<string, string>();
        public void SetSet(string k, string v) { localSettings[k] = v; }
        public string GetSet(string k) { return localSettings[k]; }
        private Dictionary<string, string> localServer = new Dictionary<string, string>();
        public void SetLS(string k, string v) { localServer[k] = v; }
        public string GetLS(string k) { return localServer[k]; }
        private Dictionary<string, string> remoteServer = new Dictionary<string, string>();
        public void SetRS(string k, string v) { remoteServer[k] = v; }
        public string GetRS(string k) { return remoteServer[k]; }
        private Dictionary<string, string> localMatch = new Dictionary<string, string>();
        public void SetLM(string k, string v) { localMatch[k] = v; }
        public string GetLM(string k) { return localMatch[k]; }
        private Dictionary<string, string> remoteMatch = new Dictionary<string, string>();
        public void SetRM(string k, string v) { remoteMatch[k] = v; }
        public string GetRM(string k) { return remoteMatch[k]; }
        private Dictionary<string, string> localCreate = new Dictionary<string, string>();
        public void SetLC(string k, string v) { localCreate[k] = v; }
        public string GetLC(string k) { return localCreate[k]; }

        public Boolean HasErrors = false;

        private string yaml_version;
        public YAML_Config(string dr)
        {
                yaml_version = dr;
        }

        public void Process(string filename)
        {
            // ----------------------------------------------------------------------------------------
            // Load up the YAML Configuration File
            // ----------------------------------------------------------------------------------------
            // Setup the input
            var input = new StreamReader(filename);

            // Load the stream
            var yaml = new YamlStream();

            // Examine the stream
            YamlMappingNode mapping;

            string[] listSET = new string[] { "ID", "VERSION", "DEBUG" , "MATCH" };
            string[] listLS = new string[] { "ODBC", "SERVER", "DB", "DBUN", "DBPW" };

            try
            {
                yaml.Load(input);
                
                mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                foreach (string item in listSET)
                {
                    this.SetSet( item, mapping.Children[new YamlScalarNode(item)].ToString());
                }

                if (this.GetSet("VERSION") != yaml_version)
                {
                    // Version is incorrect, we must not continue
    #if DEBUG
                    Console.WriteLine("YAML File Version does not match, version "+yaml_version+" expected, "+this.GetSet("VERSION")+" provided.");
    #endif                
                    input.Dispose();
                    return;
                }

                foreach (string item in listLS)
                {
                    this.SetLS("L" + item, mapping.Children[new YamlScalarNode("L" + item)].ToString());
                    this.SetRS("R" + item, mapping.Children[new YamlScalarNode("R" + item)].ToString());
                }

                int cnt = 0;

                // List all the Local Creates
                var items0 = (YamlSequenceNode)mapping.Children[new YamlScalarNode("LCreate")];
                foreach (YamlMappingNode item in items0)
                {
                    cnt++;
                    this.SetLC(item.Children[new YamlScalarNode("Name")].ToString(),
                                    item.Children[new YamlScalarNode("Query")].ToString());
                    this.SetLC("SEARCH" + cnt, item.Children[new YamlScalarNode("Name")].ToString());
                }

                // List all the Local Matching
                var items1 = (YamlSequenceNode)mapping.Children[new YamlScalarNode("LMatching")];
                cnt=0;
                foreach (YamlMappingNode item in items1)
                {
                    cnt++;
                    this.SetLM(item.Children[new YamlScalarNode("Name")].ToString(),
                                    item.Children[new YamlScalarNode("Query")].ToString());
                    this.SetLM("SEARCH" + cnt, item.Children[new YamlScalarNode("Name")].ToString());
                }

                // List all the Remote Matching
                var items2 = (YamlSequenceNode)mapping.Children[new YamlScalarNode("RMatching")];
                cnt = 0;
                foreach (YamlMappingNode item in items2)
                {
                    cnt++;
                    this.SetRM(item.Children[new YamlScalarNode("Name")].ToString(),
                                    item.Children[new YamlScalarNode("Query")].ToString());

                    // Remote matches can have complex names now, so we need to make sure we have the first one
                    string name = item.Children[new YamlScalarNode("Name")].ToString();

                    this.SetRM("SEARCH" + cnt, name );
                    this.SetRM("WEIGHT" + cnt, item.Children[new YamlScalarNode("Weight")].ToString());
                }
                this.SetRM("SEARCHCOUNT", cnt.ToString() );
            }
            catch
            {
                // Error occured, make sure we close the file off or it can't be fixed
#if DEBUG
                Console.WriteLine("YAML File provided must contain errors, check the speach marks!.");
#endif
                HasErrors = true;
            }            
            finally
            {
                input.Dispose();
            }
        }
    }
}

