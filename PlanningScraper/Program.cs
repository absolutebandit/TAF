﻿using System;

namespace PlanningScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Execute search
                var searcher = new SiteSearcher();
                var searchResults = searcher.ExecuteSearch();

                // Execute data extract
                var extractor = new PlanningDataExtractor();
                var planningApplications = extractor.ExtractData(searchResults);
            }
            catch (SearchFailedException sfe)
            {
                Console.WriteLine(ExceptionMessages.SearchFailedMessage, Environment.NewLine, sfe);
            }
            catch (ExtractDataFailedException ede)
            {
                Console.WriteLine(ExceptionMessages.DataExtractFailedMessage, Environment.NewLine, ede);
            }
        }
    }
}
