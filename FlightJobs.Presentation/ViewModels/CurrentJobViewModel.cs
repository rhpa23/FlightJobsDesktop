﻿using FlightJobsDesktop.Utils;
using System.Collections.Generic;

namespace FlightJobsDesktop.ViewModels
{
    public class CurrentJobViewModel : ObservableObject
    {
        public string JobSummary { get; set; }
        public object User { get; set; }
        public int PaxWeight { get; set; } = 88;
        public int Id { get; set; }
        public string DepartureICAO { get; set; }
        public string ArrivalICAO { get; set; }
        public string DepartureDesc { get { return $"{DepartureICAO} - {AirportDatabaseFile.FindAirportInfo(DepartureICAO).Name}"; } }
        public string ArrivalDesc { get { return $"{ArrivalICAO} - {AirportDatabaseFile.FindAirportInfo(ArrivalICAO).Name}"; } }
        public string AlternativeICAO { get; set; }
        public long Dist { get; set; }
        public string DistComplete { get { return Dist + " NM"; } }
        public long Pax { get; set; }
        public string PaxComplete { get { return $"Pax: ({Pax} * {PaxWeight} {WeightUnit})"; } }
        public string PaxTotalWeight { get { return $"{Pax * PaxWeight} {WeightUnit}"; } }
        public long Cargo { get; set; }
        public string CargoComplete { get { return $"{Cargo} {WeightUnit}"; } }
        public long Payload { get; set; }
        public string PayloadComplete { get { return $"{Payload} {WeightUnit}"; } }
        public long PayloadDisplay { get; set; }
        public string FlightTime { get; set; }
        public int FlightTimeHours { get; set; }
        public long Pay { get; set; }
        public string PayComplete { get { return string.Format("F{0:C}", Pay); } }
        public bool FirstClass { get; set; }
        public bool IsDone { get; set; }
        public bool IsActivated { get; set; }
        public bool InProgress { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ModelName { get; set; }
        public string ModelDescription { get; set; }
        public long StartFuelWeight { get; set; }
        public long StartFuelWeightDisplay { get; set; }
        public long FinishFuelWeight { get; set; }
        public int AviationType { get; set; }
        public long UsedFuelWeight { get; set; }
        public long UsedFuelWeightDisplay { get; set; }
        public string Month { get; set; }
        public string VideoUrl { get; set; }
        public string VideoDescription { get; set; }
        public string ChallengeCreatorUserId { get; set; }
        public bool IsChallenge { get; set; }
        public bool IsChallengeFromCurrentUser { get; set; }
        public string ChallengeExpirationDate { get; set; }
        public int ChallengeType { get; set; }
        public string WeightUnit { get; set; }
        public LastJobViewModel LastJob { get; set; }
    }
}