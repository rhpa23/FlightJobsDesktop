﻿using FlightJobs.Connect.MSFS.SDK;
using FlightJobs.Connect.MSFS.SDK.Model;
using FlightJobsDesktop.ViewModels;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace FlightJobsDesktop.Common
{
    public class FlightRecorderUtil
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(FlightRecorderUtil));

        private static void SetupFlightRecorderCharts(Chart chart, SeriesChartType chartType, string title, string titleAxisY)
        {
            chart.Series[0].ChartType = chartType;
            chart.Titles.Clear();
            chart.Titles.Add(new Title(title, Docking.Top, new Font("Segoe UI", 10), Color.White));
            chart.ChartAreas[0].AxisY.Title = titleAxisY;
            chart.Series[0].Points.Clear();
        }

        internal static IList<FlightRecorderModel> LoadFlightRecorderFile(CurrentJobViewModel currentJob)
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var dirInfo = Directory.CreateDirectory(Path.Combine(path, $"ResourceData/FlightData/{currentJob.Id}"));
                path = Path.Combine(dirInfo.FullName, $"{currentJob.DepartureICAO}-{currentJob.ArrivalICAO}.json");
                var lines = File.ReadAllLines(path);
                var line = lines?.FirstOrDefault();
                return JsonConvert.DeserializeObject<IList<FlightRecorderModel>>(line);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            return new List<FlightRecorderModel>();
        }

        internal static void SaveFlightRecorderFile(CurrentJobViewModel currentJob)
        {
            try
            {
                string jsonFlRec = JsonConvert.SerializeObject(FlightJobsConnectSim.FlightRecorderList);
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var dirInfo = Directory.CreateDirectory(Path.Combine(path, $"ResourceData/FlightData/{currentJob.Id}"));
                path = Path.Combine(dirInfo.FullName, $"{currentJob.DepartureICAO}-{currentJob.ArrivalICAO}.json");
                File.WriteAllText(path, jsonFlRec);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        internal static void UpdateChartVerticalProfile(Chart chart)
        {
            if (FlightJobsConnectSim.FlightRecorderList.Count <= 0) return;

            SetupFlightRecorderCharts(chart, SeriesChartType.Spline, "Vertical profile", "ALT");

            bool isLightLandingOn = FlightJobsConnectSim.FlightRecorderList.First(x => x != null && !x.OnGround).LightLandingOn;

            foreach (var flightRecorder in FlightJobsConnectSim.FlightRecorderList.Where(x => x != null && !x.OnGround))
            {
                var dataPoint = chart.Series[0].Points.Add(flightRecorder.Altitude);
                dataPoint.AxisLabel = flightRecorder.TimeUtc.ToShortTimeString();

                if (flightRecorder.LightLandingOn != isLightLandingOn)
                {
                    isLightLandingOn = flightRecorder.LightLandingOn;
                    dataPoint.MarkerSize = 5;
                    dataPoint.Label = flightRecorder.LightLandingOn ? "Landing light On" : "Landing light Off";
                }
            }

            var max = chart.Series[0].Points.FindMaxByValue();
            max.MarkerSize = 5;
            max.MarkerColor = Color.GreenYellow;
            max.Label = $"Max {max.YValues[0]}ft";
        }

        internal static void UpdateChartSpeed(Chart chart)
        {
            if (FlightJobsConnectSim.FlightRecorderList.Count <= 0) return;

            SetupFlightRecorderCharts(chart, SeriesChartType.Spline, "Speed variation", "SPEED");

            foreach (var flightRecorder in FlightJobsConnectSim.FlightRecorderList.Where(x => x != null && !x.OnGround))
            {
                var dataPoint = chart.Series[0].Points.Add(flightRecorder.Speed);
                dataPoint.AxisLabel = flightRecorder.TimeUtc.ToShortTimeString();
            }

            var max = chart.Series[0].Points.FindMaxByValue();
            var min = chart.Series[0].Points.FindMinByValue();

            max.MarkerSize = 5;
            max.Label = $"Max speed {max.YValues[0]}kts";

            min.MarkerSize = 5;
            min.Label = $"Min speed {min.YValues[0]}kts";
        }

        internal static void UpdateChartFuel(Chart chart)
        {
            if (FlightJobsConnectSim.FlightRecorderList.Count <= 0) return;

            SetupFlightRecorderCharts(chart, SeriesChartType.StepLine, "Fuel flow variation", "FUEL FLOW");

            var recList = FlightJobsConnectSim.FlightRecorderList.Where(x => x != null && !x.OnGround).ToArray();
            double previewsFuelWeightKg = recList.Length > 0 ? recList[0].FuelWeightKilograms : 0;
            for (int i = 1; i < recList.Length; i = i + 12)
            {
                var currentFuelWeightKg = recList[i].FuelWeightKilograms;
                var dataPoint = chart.Series[0].Points.Add(previewsFuelWeightKg - currentFuelWeightKg);
                dataPoint.AxisLabel = recList[i].TimeUtc.ToShortTimeString();

                previewsFuelWeightKg = currentFuelWeightKg;
            }
        }
    }
}