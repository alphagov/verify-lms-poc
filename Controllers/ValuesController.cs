using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Odbc;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using local_matching.Matching;
using local_matching.CreateAccount;
using local_matching.SingleSearch;
using local_matching.DBODBC;
using local_matching.YAMLC;

namespace local_matching.Controllers
{

    [Route("local-matching")]
    public class MatchingController : Controller
    {
        string  YAML_filename = "local-matching-config.yml";

        // This also tells the YAML what version we are looking for.        
        YAML_Config yamlc = new YAML_Config("0.2");

        public MatchingController()
        {
// Dont do this in debug mode, we're just wasting time
#if !DEBUG
            yamlc.Process( YAML_filename ) ;
#endif
        }

        //##########################################################################################
        // Match a record - This operates at different levels
        //##########################################################################################
        [HttpPost("match")]
        public dynamic Post([FromBody] matching value )
        {
            // If we are in debug mode, we probably want to reload the YAML everytime we
            // get a matching record
#if DEBUG
            yamlc.Process( YAML_filename );
#endif
            if (yamlc.HasErrors)
            {
                Console.WriteLine("Cannot continue with faulty YAML Config.");
                return "{ error: YAML Config has faults}";
            }

            // Create the results dictionary
            Dictionary <string,string> PRET = new Dictionary<string, string>{};

            // Process the matching class
            value.Process( ref yamlc , ref PRET );

            // See if we already have it
            value.AlreadyCreatedSearch( ref yamlc , ref PRET );

            // Check if we found it in Cycle 0 search
            string match_id = PRET.GetValueOrDefault("MATCHING");
            if (!string.IsNullOrEmpty( match_id))
            {
#if DEBUG
                Console.WriteLine("WE FOUND A CYCLE 0 MATCH - "+match_id);
                PRET.Add("CYCLE0MATCH", "True");
#endif
                // If we have a match, don't bother trawling local database
                // unless we are in debug mode. We like to still do it, because
                // its quicker to debug
                if (yamlc.GetSet("DEBUG") != "true")
                {
                    return "{ result: matched }";
                }
            }
            // Do a trawl of the remote database
            value.TrawlRemoteDatabase(ref yamlc, ref PRET);

            // Single in on the specific account we are after
            singleSearch strategy = new singleSearch();

            // Return the match or nomatch result
            strategy.Process( ref yamlc, ref PRET );

            // Not debug, so we better work out if we matched or not
            string resu = PRET.GetValueOrDefault("BESTCANDIDATE");
            if (!string.IsNullOrEmpty( resu ))
            {
                // We got a match, but not a cycle zero, so we need to create the row
                if (string.IsNullOrEmpty( match_id ))
                {
                    string acnt = resu.Substring( 0, resu.IndexOf(" "));

                    // insert the row isn't just as simple, as we will be pulling out parameters
                    // into PRET which will could already exist

                    // Create the results NEW dictionary
                    Dictionary<string, string> PRETNEW = new Dictionary<string, string> { };

                    // Call the account creation
                    createAccount newrow = new createAccount();

                    // Set up the create values
                    newrow.Pid = value.HashedPid;
                    newrow.AccountId = acnt;

                    // Do it
                    newrow.Process( ref yamlc , ref PRETNEW );

                    // Now we must add all of the NEW to the OLD
                    foreach (KeyValuePair<string, string> entry in PRETNEW)
                    {
                        // do something with entry.Value or entry.Key
                        PRET.Add("CreateAccount_"+entry.Key,entry.Value);
                    }

                }
            }
            else
            {   // This tricks the system into knowing we have a match when cycle 0 works
                resu = match_id;
            }
            
            // If we are debug mode, just return back the PRET
            if (yamlc.GetSet("DEBUG") == "true")
                return PRET;

            if (!string.IsNullOrEmpty(resu))
                return "{ result: match }";
            else
                return "{ result: no-match }";

        }

        //##########################################################################################
        // Create a record
        //##########################################################################################
        [HttpPost("create")]
        public dynamic Post([FromBody] createAccount value)
        {
            // If we are in debug mode, we probably want to reload the YAML everytime we
            // get a matching record
#if DEBUG
            yamlc.Process( YAML_filename );
#endif
            if (yamlc.HasErrors)
            {
                Console.WriteLine("Cannot continue with faulty YAML Config.");
                return "{ error: YAML Config has faults}";
            }

            // Create the results dictionary and process YAMLC
            // Create the results dictionary
            Dictionary<string, string> PRET = new Dictionary<string, string> { };

            value.Process( ref yamlc , ref PRET );

            // See if we are already there, if not, create a new entry
            if (yamlc.GetSet("DEBUG")=="true")
                return PRET;
            else 
                return "{ result: success }";
        }
    }
}
