using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Instadose;
using Instadose.Data;

namespace Instadose.Device
{
    public class ReadAnalysis
    {
        public InsDataContext IDC = null;

        // Public CONFIGURABLE Variables.
        public int NumberToAnalyze { get; set; }
        public decimal DLDoseCalcLowRange { get; set; }
        public decimal DLDoseCalcHighRange { get; set; }
        public decimal CumulativeDoseMax { get; set; }
        public decimal DeviceExpirationYears { get; set; }

        // Default ReadAnalysis CONSTRUCTOR.
        public ReadAnalysis(InsDataContext idc, int numbertoanalyze, decimal dldclow, decimal dldchigh, decimal cumuldose, int expyears)
        {
            // Construct the Data Context.
            this.IDC = idc;

            // Set default values for the Configurable variables.
            this.NumberToAnalyze = numbertoanalyze;
            this.DLDoseCalcHighRange = dldclow;
            this.DLDoseCalcLowRange = dldchigh;
            this.CumulativeDoseMax = cumuldose;
            this.DeviceExpirationYears = expyears;
        }

        public Dictionary<int, bool> PerformAnalysis(UserDeviceRead udr)
        {
            // Ensure that the UserDeviceRead record exists.
            if (udr == null)
                throw new Exception("User Device Read record does not exist.");

            // Get Baseline (Row 2) results for DLT and DHT.
            var baselineRow2 = (from db in udr.DeviceInventory.DeviceBaselines
                                where db.BaselineReadCount == 2
                                select new
                                {
                                    DLT2 = db.DeepLowTemp,
                                    DHT2 = db.DeepHighTemp
                                }).FirstOrDefault();

            // Ensure that the Baseline Row 2 record exists.
            if (baselineRow2 == null)
                throw new Exception("Baseline information missing for RID " + udr.RID + " .");

            // Assign DLT and DHT values.
            double blr2DLT = baselineRow2.DLT2;
            double blr2DHT = baselineRow2.DHT2;

            // Query all existing AnalysisReadDetails records.
            // Resulting LIST is used to skip the tests that have already been performed.
            var readDetails = udr.AnalysisReadDetails.ToList();

            // CONSTRUCT a Dictionary (Key, Value) to return Pass/Fail for all UserDeviceRead-related tests.
            Dictionary<int, bool> results = new Dictionary<int, bool>();

            // IF Baseline Data does exist for RID's DeviceID, then proceed.
            // Perform the DLDCWatchlist Test if a AnalysisReadDetails record doesn't exist.
            if (readDetails.Where(r => r.AnalysisTypeID == 1) != null)
                results.Add(1, deepLowWatchlistTest(udr));

            // Perform the DLDCWatchlist Test if a AnalysisReadDetails record doesn't exist.
            if (readDetails.Where(r => r.AnalysisTypeID == 2) != null)
                results.Add(2, deepLowRecallTest(udr));

            // Perform the DLTWarmTest Test if a AnalysisReadDetails record doesn't exist.
            if (readDetails.Where(r => r.AnalysisTypeID == 3) != null)
                results.Add(3, deepLowTempWarmTest(udr, blr2DLT));

            // Perform the DLTCoolTest Test if a AnalysisReadDetails record doesn't exist.
            if (readDetails.Where(r => r.AnalysisTypeID == 4) != null)
                results.Add(4, deepLowTempCoolTest(udr, blr2DLT));

            // Perform the DHTWarmTest Test if a AnalysisReadDetails record doesn't exist.
            if (readDetails.Where(r => r.AnalysisTypeID == 5) != null)
                results.Add(5, deepHighTempWarmTest(udr, blr2DHT));

            // Perform the DHTCoolTest Test if a AnalysisReadDetails record doesn't exist.
            if (readDetails.Where(r => r.AnalysisTypeID == 6) != null)
                results.Add(6, deepHighTempCoolTest(udr, blr2DHT));

            // Perform the HasAnomaly Test if a AnalysisReadDetails record doesn't exist.
            if (readDetails.Where(r => r.AnalysisTypeID == 7) != null)
                results.Add(7, hasAnomalyTest(udr));

            // Perform the CumulativeDoseNearLimit Test if a AnalysisReadDetails record doesn't exist.
            if (readDetails.Where(r => r.AnalysisTypeID == 8) != null)
                results.Add(8, cumulativeDoseNearLimitTest(udr));

            // Perform the ShelfLifeExpiring Test if a AnalysisReadDetails record doesn't exist.
            if (readDetails.Where(r => r.AnalysisTypeID == 9) != null)
                results.Add(9, shelfLifeExpiringTest(udr));

            // SAVE results to the IDC.
            saveResults(results, udr);

            // RETURN the results of the tests.
            return results;
        }

        /// <summary>
        /// Save the results to the AnalysisReadDetails table.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private bool saveResults(Dictionary<int, bool> results, UserDeviceRead udr)
        {
            try
            {
                // Loop through each result to create the read detail.
                foreach (var result in results)
                {
                    // Add a new analysis read detail.
                    udr.AnalysisReadDetails.Add(new AnalysisReadDetails()
                    {
                        RID = udr.RID,
                        AnalysisTypeID = result.Key,
                        Passed = result.Value
                    });
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deep Low Dose Calculating Watchlist Test.
        /// </summary>
        /// <returns></returns>
        private bool deepLowWatchlistTest(UserDeviceRead udr)
        {
            // Return FALSE if the Read doesn't have a Deep Low Dose Calc. (nullable) value.
            if (!udr.DeepLowDoseCalc.HasValue) return false;

            // Perform the test.
            return !(Convert.ToDouble(DLDoseCalcLowRange) < udr.DeepLowDoseCalc.Value &&
                udr.DeepLowDoseCalc.Value <= Convert.ToDouble(DLDoseCalcHighRange));
        }

        /// <summary>
        /// Deep Low Dose Calculating Recall Test.
        /// </summary>
        /// <returns></returns>
        private bool deepLowRecallTest(UserDeviceRead udr)
        {
            // Return FALSE if the Read doesn't have a Deep Low Dose Calc. (nullable) value.
            if (!udr.DeepLowDoseCalc.HasValue) return false;

            // Perform the test.
            return !(udr.DeepLowDoseCalc.Value <= Convert.ToDouble(DLDoseCalcLowRange));
        }

        // DLT Warm Test.
        private bool deepLowTempWarmTest(UserDeviceRead udr, double blr2dlt)
        {
            // Perform the test.
            return !(udr.DeepLowTemp > (blr2dlt + 1500));
        }

        // DLT Cool Test.
        private bool deepLowTempCoolTest(UserDeviceRead udr, double blr2dlt)
        {
            // Perform the test.
            return !(udr.DeepLowTemp < (blr2dlt - 1500));
        }

        // DHT Warm Test.
        private bool deepHighTempWarmTest(UserDeviceRead udr, double blr2dht)
        {
            // Perform the test.
            return !(udr.DeepHighTemp > (blr2dht + 1500));
        }

        // DHT Cool Test.
        private bool deepHighTempCoolTest(UserDeviceRead udr, double blr2dht)
        {
            // Perform the test.
            return !(udr.DeepHighTemp < (blr2dht - 1500));
        }

        // Has Anomaly Test.
        public bool hasAnomalyTest(UserDeviceRead udr)
        {
            // Perform the test.
            return !(udr.HasAnomaly);
        }

        // Cumulative Dose (Dose Near Limit) Test.
        private bool cumulativeDoseNearLimitTest(UserDeviceRead udr)
        {
            // Return FALSE if the Read doesn't have a Cumulative Dose (nullable) value.
            if (!udr.CumulativeDose.HasValue) return false;

            // Perform the test.
            return !(udr.CumulativeDose > Convert.ToDouble(CumulativeDoseMax));
        }

        // Manufacture Date (Shelf Life Expiring) Test.
        private bool shelfLifeExpiringTest(UserDeviceRead udr)
        {
            // Return FALSE if the Read doesn't have a Manufacture Date (nullable) value.
            if (!udr.DeviceInventory.MfgDate.HasValue) return false;

            int totalTimeInYears = DateTime.Now.Year - (udr.DeviceInventory.MfgDate.Value.Year);

            // Perform the test.
            return !(totalTimeInYears > DeviceExpirationYears);
        }
    }
}

