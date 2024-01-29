using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IOC.HL7;
using IOC.CDS.Cmd;
using IOC.CDS.UnEngine;

namespace V2UnEngine
{
    internal class V2UnEngine
    {
        static void Main(string[] args)
        {
            var Msg = new HL7message();             // Instantiate HL7 message
            var Seg = new HL7segment();             // Instantiate HL7 message segment
            var Cmd = new CommandValidator();       // Instantiate 'fix' command processor class
            var Pro = new CommandProcessor();       // Instantiate 'process' command processor class

            Msg.OnMessageLoaded += Pro.TransformLoadedMessage;  // subscribe to event

            List<string> AllowedCommands = new List<string>
            {
                "DOT",      // Process dot files switch
                "LOC",      // Location (folder path) of files to process
                "EXT",      // Extension of files to process
                "REMOVE",   // Remove
                "CLEAR",    // Clear
                "SET",      // Set
                "COPY",     // Copy
                "MOVE",     // Move
                "SWAP",     // Swap
                "JOIN",     // Join/Concatenate
                "APPEND",   // Append
                "PREPEND",  // Prepend
                "REPLACE",  // Replace a string within a field
                "RENUM",    // Renumber
                "RENAME",   // Rename segment
                "TOLOWER",  // Convert to lowercase
                "TOUPPER",  // Convert to uppercase
                "RMVTEXT",  // Get rid of strings (via translation)
                "XLATE",    // Table-based translation
            };
            Cmd.LoadAllowedCommands(AllowedCommands);

            //string CfgFile = @"F:\SYH\NG PRD\Encounters\V2Transform.cfg";
            string CfgFile = Cmd.SelectConfigScript();
            if (CfgFile == "QUIT") { Environment.Exit(0); }

            Console.WriteLine($" ");
            Console.WriteLine($"Loading Script File: {CfgFile}");

            int ErrorCode = Cmd.LoadConfigScript(CfgFile);          // Open config file, or throw exception
            if (ErrorCode == -1)
            {
                Console.ReadLine();
                Environment.Exit(0);
            }

            // Get flag for how to handle files beginning with '.'
            // Get file extensions to process
            bool ProcessDotFiles = Cmd.ProcessDotFiles();
            string[] MsgFileExtensions = Cmd.GetFileExtensions();
            Cmd.FileFoldersToProcess();
            string CurrentPath = Cmd.FullPathToFilesToProcess;

            //  Announce progress to user
            Console.WriteLine($" ");
            Console.WriteLine($"Processing messages in all {string.Join(",", MsgFileExtensions)} files...");

            //  reset count of messages skipped
            long NumMessagesSkipped = 0;

            //  Now load list of files in directory that match targetted extension
            foreach (string msgFile in Directory.EnumerateFiles(CurrentPath, "*.*")
                .Where(s => MsgFileExtensions.Any(ext => ext == Path.GetExtension(s))))
            {
                string fName = Path.GetFileName(msgFile);           // get filename
                if (!fName.StartsWith(".") || ProcessDotFiles)      // process if 1st char not dot, or DOT is not NO
                {
                    Console.WriteLine($"Processing {msgFile}");        // Report file being processed
                    Msg.LoadMsgEvt(msgFile);                        // process message file
                }
            }

            Console.WriteLine(" ");
            Console.WriteLine($"   {NumMessagesSkipped} messages were skipped due to date filter...");
            Console.WriteLine($"Done. Hit return to quit...");
            Console.ReadLine();
        }
    }
}
