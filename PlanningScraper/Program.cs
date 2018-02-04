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
        private static readonly string ErrorFile = ConfigurationManager.AppSettings["errorFileLocation"];

        static void Main(string[] args)
        {
            try
            {
                var searcher = new SiteSearcher();
                var searchResults = searcher.ExecuteSearch(out CookieContainer cookieContainer);

                var extractor = new PlanningDataExtractor();
                var planningApplications = extractor.ExtractData(searchResults, cookieContainer);

                WriteOutputFile(planningApplications);
            }
            catch (SearchFailedException sfe)
            {
                Console.WriteLine(ExceptionMessages.SearchFailedMessage, Environment.NewLine, sfe);
                DeleteErrorFile();
                File.WriteAllText(ErrorFile,
                    string.Format(ExceptionMessages.SearchFailedMessage, Environment.NewLine, sfe));
            }
            catch (ExtractDataFailedException ede)
            {
                Console.WriteLine(ExceptionMessages.DataExtractFailedMessage, Environment.NewLine, ede);
                DeleteErrorFile();
                File.WriteAllText(ErrorFile,
                    string.Format(ExceptionMessages.DataExtractFailedMessage, Environment.NewLine, ede));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ExceptionMessages.GeneralFailureMessage, Environment.NewLine, ex);
                DeleteErrorFile();
                File.WriteAllText(ErrorFile,
                    string.Format(ExceptionMessages.GeneralFailureMessage, Environment.NewLine, ex));
            }
        }

        private static void DeleteErrorFile()
        {
            if (File.Exists(ErrorFile))
            {
                File.Delete(ErrorFile);
            }
        }

        private static void WriteOutputFile(IEnumerable<PlanningApplication> planningApplications)
        {
            var sb = new StringBuilder();

            sb.Append(
                $"Application Reference\tApplication Type\tCurrent Status\tProposal\tSite\tAddress\tRegistered Date\tConsultation Expiry Date\t" +
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
        }
    }
}
