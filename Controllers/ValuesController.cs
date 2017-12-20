﻿using System;
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
            // Create the results dictionary
            Dictionary <string,string> PRET;// = new Dictionary<string, string>{};

            // Process the YAML config
            PRET = value.Process( ref yamlc );

            // See if we already have it
            value.AlreadyCreatedSearch( ref yamlc , ref PRET );

            // Check if we found it in Cycle 0 search
            string match_id = PRET.GetValueOrDefault("MATCHING");
            if (!string.IsNullOrEmpty( match_id))
            {
#if DEBUG
                Console.WriteLine("WE FOUND A CYCLE 0 MATCH - "+match_id);
#endif
            }
            // Do a trawl of the remote database
            value.TrawlRemoteDatabase(ref yamlc, ref PRET);

            // Single in on the specific account we are after
            singleSearch strategy = new singleSearch();

            // Return the match or nomatch result
            strategy.Process( ref yamlc, ref PRET );

            // If we are debug mode, just return back the PRET
            if (yamlc.GetSet("DEBUG")=="true")
                return PRET;

            // Not debug, so we better work out if we matched or not
            string resu = PRET.GetValueOrDefault("BESTCANDIDATE");
            if (!string.IsNullOrEmpty( resu ))
            {
                // Did we get a cycle 0 match? (Normally we would never get here)
                if (string.IsNullOrEmpty( match_id))
                {
                    string acnt = resu.Substring( 0, resu.IndexOf(" "));
                    // insert the row
                    createAccount newrow = new createAccount();
                    newrow.Pid = value.HashedPid;
                    newrow.AccountId = acnt;
                    newrow.Process( ref yamlc );
                }
                return "{ result: match }";
            }
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
            // Create the results dictionary
            Dictionary <string,string> PRET;// = new Dictionary<string, string>{};

            // Call it
            PRET = value.Process( ref yamlc );

            // See if we are already there, if not, create a new entry
            if (yamlc.GetSet("DEBUG")=="true")
                return PRET;
            else 
                return "{ result: success}";
        }
    }
}
