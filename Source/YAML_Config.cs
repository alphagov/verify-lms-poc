using System;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;

namespace local_matching.YAMLC
{
    public class YAML_Config
    {

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

        public void Process(string filename)
        {
            // ----------------------------------------------------------------------------------------
            // Load up the YAML Configuration File
            // ----------------------------------------------------------------------------------------
            // Setup the input
            var input = new StreamReader(filename);

            // Load the stream
            var yaml = new YamlStream();
            yaml.Load(input);

            // Examine the stream
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            this.SetLS("LODBC", mapping.Children[new YamlScalarNode("LODBC")].ToString());
            this.SetLS("LSERVER", mapping.Children[new YamlScalarNode("LSERVER")].ToString());
            this.SetLS("LDB", mapping.Children[new YamlScalarNode("LDB")].ToString());
            this.SetLS("LDBUN", mapping.Children[new YamlScalarNode("LDBUN")].ToString());
            this.SetLS("LDBPW", mapping.Children[new YamlScalarNode("LDBPW")].ToString());

            this.SetRS("RODBC", mapping.Children[new YamlScalarNode("RODBC")].ToString());
            this.SetRS("RSERVER", mapping.Children[new YamlScalarNode("RSERVER")].ToString());
            this.SetRS("RDB", mapping.Children[new YamlScalarNode("RDB")].ToString());
            this.SetRS("RDBUN", mapping.Children[new YamlScalarNode("RDBUN")].ToString());
            this.SetRS("RDBPW", mapping.Children[new YamlScalarNode("RDBPW")].ToString());


            // List all the Local Creates
            var items0 = (YamlSequenceNode)mapping.Children[new YamlScalarNode("LCreate")];
            foreach (YamlMappingNode item in items0)
            {
                this.SetLC(item.Children[new YamlScalarNode("Name")].ToString(),
                                item.Children[new YamlScalarNode("Query")].ToString());
            }

            // List all the tables relationship
            var items1 = (YamlSequenceNode)mapping.Children[new YamlScalarNode("LMatching")];
            foreach (YamlMappingNode item in items1)
            {
                this.SetLM(item.Children[new YamlScalarNode("Name")].ToString(),
                                item.Children[new YamlScalarNode("Query")].ToString());
            }

            // List all the tables relationship
            var items2 = (YamlSequenceNode)mapping.Children[new YamlScalarNode("RMatching")];
            foreach (YamlMappingNode item in items2)
            {
                this.SetRM(item.Children[new YamlScalarNode("Name")].ToString(),
                                item.Children[new YamlScalarNode("Query")].ToString());
            }

            input.Dispose();

        }
    }
}

