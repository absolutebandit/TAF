using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlanningScraper.Utils
{
    public static class DateChunker
    {
        public static async Task<Dictionary<string, string>> SplitDateRange(string startDate, string endDate, int chunkSizeDays)
        {
            var chunkDateDictionary = new Dictionary<string, string>();

            var startDateTime = DateTime.ParseExact(startDate, "dd-MM-yyyy", null);
            var endDateTime = DateTime.ParseExact(endDate, "dd-MM-yyyy", null);

            var chunkEndDate = startDateTime.AddDays(chunkSizeDays-1);
            if (chunkEndDate.CompareTo(endDateTime) >= 0)
            {
                chunkDateDictionary.Add(startDate, endDate);
                return await Task.FromResult(chunkDateDictionary);
            }

            chunkDateDictionary.Add(startDate, chunkEndDate.ToString("dd-MM-yyyy"));

            bool iterate = true;
            while (iterate)
            {
                var chunkStartDate = chunkEndDate.AddDays(1);
                chunkEndDate = chunkStartDate.AddDays(chunkSizeDays-1);
                if (chunkEndDate.CompareTo(endDateTime) >= 0)
                {
                    chunkDateDictionary.Add(chunkStartDate.ToString("dd-MM-yyyy"), endDate);
                    return await Task.FromResult(chunkDateDictionary);
                }
                chunkDateDictionary.Add(chunkStartDate.ToString("dd-MM-yyyy"), chunkEndDate.ToString("dd-MM-yyyy"));
            }

            throw new Exception("Invalid path");
        }
    }
}
