﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Output
{
    public class FileWriter : IFileWriter
    {
        private readonly ILogger _logger;
        private readonly string _outputFileLocation;

        public FileWriter(ILogger logger, ISystemConfig systemConfig)
        {
            _logger = logger;
            _outputFileLocation = $"{Environment.CurrentDirectory}\\{systemConfig.OutputFileName}";
        }

        public async Task WriteOutputFileAsync(IEnumerable<PlanningApplication> planningApplications, CancellationToken cancellationToken)
        {
            await _logger.LogInformationAsync($"Writing file output...", cancellationToken);

            var sb = new StringBuilder();

            sb.Append(
                $"Application Reference,Application Type,Current Status,Proposal,Site Address,Registered Date,Consultation Expiry Date," +
                $"Target Date,Name of Applicant,Agent Name,Agent Address,Agent Company Name,Agent Phone Number,Agent Email,Case Officer,Parishes,Wards{Environment.NewLine}");

            foreach (var application in planningApplications)
            {
                sb.Append($"{application.ApplicationReference}," +
                          $"{application.ApplicationType}," +
                          $"{application.CurrentStatus}," +
                          $"{application.Proposal}," +
                          $"{application.SiteAddress}," +
                          $"{application.RegisteredDate}," +
                          $"{application.ConsultationExpiryDate}," +
                          $"{application.TargetDate}," +
                          $"{application.NameOfApplicant}," +
                          $"{application.AgentName}," +
                          $"{application.AgentAddress}," +
                          $"{application.AgentCompanyName}," +
                          $"{application.AgentPhoneNumber}," +
                          $"{application.AgentEmail}," +
                          $"{application.CaseOfficer}," +
                          $"{application.Parishes}," +
                          $"{application.Wards}," +
                          $"{Environment.NewLine}");
            }

            if (File.Exists(_outputFileLocation))
            {
                File.Delete(_outputFileLocation);
            }

            File.WriteAllText(_outputFileLocation, sb.ToString());

            await _logger.LogInformationAsync($"Finished.", cancellationToken);
        }
    }
}
