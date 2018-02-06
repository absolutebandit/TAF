using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;

namespace PlanningScraper
{
    class Program
    {
        private static readonly string OutputFile = ConfigurationManager.AppSettings["outputFileLocation"];
        private static readonly string LogFile = ConfigurationManager.AppSettings["logFileLocation"];

        static void Main(string[] args)
        {
            try
            {
                Initialize();

                var searcher = new SiteSearcher();
                var searchResults = searcher.ExecuteSearch(out CookieContainer cookieContainer);

                var extractor = new PlanningDataExtractor();
                var planningApplications = extractor.ExtractData(searchResults, cookieContainer);

                WriteOutputFile(planningApplications);
            }
            catch (SearchFailedException sfe)
            {
                Console.WriteLine(ExceptionMessages.SearchFailedMessage, Environment.NewLine, sfe);
                File.AppendAllText(LogFile,
                    string.Format(ExceptionMessages.SearchFailedMessage, Environment.NewLine, sfe) + Environment.NewLine);
            }
            catch (ExtractDataFailedException ede)
            {
                Console.WriteLine(ExceptionMessages.DataExtractFailedMessage, Environment.NewLine, ede);
                DeleteLogFile();
                File.AppendAllText(LogFile,
                    string.Format(ExceptionMessages.DataExtractFailedMessage, Environment.NewLine, ede) + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ExceptionMessages.GeneralFailureMessage, Environment.NewLine, ex);
                DeleteLogFile();
                File.WriteAllText(LogFile,
                    string.Format(ExceptionMessages.GeneralFailureMessage, Environment.NewLine, ex) + Environment.NewLine);
            }
        }

        private static void Initialize()
        {
            DeleteLogFile();
            File.WriteAllText(LogFile, $"Initializing...{Environment.NewLine}{Environment.NewLine}");
        }

        private static void DeleteLogFile()
        {
            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }

        private static void WriteOutputFile(IEnumerable<PlanningApplication> planningApplications)
        {
            LogWritingFileOutput();

            var sb = new StringBuilder();

            sb.Append(
                $"Application Reference\tApplication Type\tCurrent Status\tProposal\tSite Address\tRegistered Date\tConsultation Expiry Date\t" +
                $"Target Date\tName of Applicant\tName of Agent\tCase Officer\tParishes\tWards{Environment.NewLine}");

            foreach (var application in planningApplications)
            {
                sb.Append($"{application.ApplicationReference}\t" +
                          $"{application.ApplicationType}\t" +
                          $"{application.CurrentStatus}\t" +
                          $"{application.Proposal}\t" +
                          $"{application.SiteAddress}\t" +
                          $"{application.RegisteredDate}\t" +
                          $"{application.ConsultationExpiryDate}\t" +
                          $"{application.TargetDate}\t" +
                          $"{application.NameOfApplicant}\t" +
                          $"{application.NameOfAgent}\t" +
                          $"{application.CaseOfficer}\t" +
                          $"{application.Parishes}\t" +
                          $"{application.Wards}" + 
                          $"{Environment.NewLine}");
            }

            if (File.Exists(OutputFile))
            {
                File.Delete(OutputFile);
            }

            File.WriteAllText(OutputFile, sb.ToString());

            LogFinished();
        }

        private static void LogWritingFileOutput()
        {
            var logText = $"Writing file output...{Environment.NewLine}{Environment.NewLine}";
            Console.WriteLine(logText);
            File.AppendAllText(LogFile, logText);
        }

        private static void LogFinished()
        {
            var logText = $"Finished.";
            Console.WriteLine(logText);
            File.AppendAllText(LogFile, logText);
        }
    }
}
