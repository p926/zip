using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Services;
using Instadose.Data;
using Instadose.Device;
using Portal.Instadose;
/// <summary>
/// Summary description for ReadAnalysisWorker
/// </summary>
[WebService(Namespace = "http://portal.instadose.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class ReadAnalysisWorker : System.Web.Services.WebService
{
    private ReadAnalysisStatus GetReadAnalysisStatus()
    {
        var status = new ReadAnalysisStatus();

        try
        {
            var conn = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            using (SqlConnection connection = new SqlConnection(conn))
            {
                // Create the Command and Parameter objects.
                SqlCommand cmd = new SqlCommand(
                    "SELECT TOP 1 * FROM ReadAnalysisWorkers WHERE UserName='" + User.Identity.Name
                    + "' AND CreatedDate <= DATEADD(HOUR, 1, GETDATE()) ORDER BY CreatedDate DESC", connection);

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int tmpInt;
                    DateTime tmpDate;
                    ReadAnalysisStatus.AnalysisStatus tmpStatus;

                    if (int.TryParse(reader["ReadAnalysisWorkerID"].ToString(), out tmpInt))
                        status.ReadAnalysisWorkerID = tmpInt;

                    if (int.TryParse(reader["LogID"].ToString(), out tmpInt))
                        status.LogID = tmpInt;

                    if (int.TryParse(reader["TotalReads"].ToString(), out tmpInt))
                        status.TotalReads = tmpInt;

                    if (int.TryParse(reader["ReadErrors"].ToString(), out tmpInt))
                        status.ReadErrors = tmpInt;

                    if (int.TryParse(reader["ReadsCompleted"].ToString(), out tmpInt))
                        status.ReadsCompleted = tmpInt;

                    if (Enum.TryParse(reader["WorkerStatus"].ToString(), out tmpStatus))
                        status.WorkerStatus = tmpStatus;

                    if (DateTime.TryParse(reader["CompletedDate"].ToString(), out tmpDate))
                        status.CompletedDate = tmpDate;

                    if (DateTime.TryParse(reader["StartDate"].ToString(), out tmpDate))
                        status.StartDate = tmpDate;

                    status.MessageDesc = reader["MessageDesc"].ToString();

                }
                reader.Close();
            }
        }
        catch (Exception ex)
        {
            status.MessageDesc = ex.Message;
        }

        return status;
    }

    private void SetReadAnalysisStatus(ReadAnalysisStatus s)
    {
        string query = "INSERT INTO ReadAnalysisWorkers (LogID, WorkerStatus, TotalReads, ReadErrors, ReadsCompleted, StartDate, CreatedDate, UserName, MessageDesc) VALUES (" +
            string.Format("{0}, '{1}', {2}, {3}, {4}, GETDATE(), GETDATE(), '{5}', '{6}')", s.LogID, s.WorkerStatus, s.TotalReads, s.ReadErrors, s.ReadsCompleted, User.Identity.Name, s.MessageDesc);

        if (s.ReadAnalysisWorkerID != 0)
        {
            query = "UPDATE ReadAnalysisWorkers SET " +
                "LogID = {0}, WorkerStatus = '{1}', " +
                "TotalReads = {2}, ReadErrors = {3}, " +
                "ReadsCompleted = {4}, MessageDesc = '{5}', " +
                "CompletedDate = {6} WHERE ReadAnalysisWorkerID = {7}";

            query = string.Format(query, s.LogID, s.WorkerStatus, s.TotalReads, s.ReadErrors, s.ReadsCompleted, s.MessageDesc,
                s.CompletedDate == null ? "NULL" : string.Format("'{0:yyyy-MM-dd HH:mm:ss}'", s.CompletedDate), s.ReadAnalysisWorkerID);
        }

        var dl = new DataLayer();
        dl.ExecuteNonQuery(query, CommandType.Text, new List<SqlParameter>());
    }

    [WebMethod]
    public ReadAnalysisStatus GetWorkerStatus()
    {
        return this.GetReadAnalysisStatus();
    }

    [WebMethod]
    public int ExecuteReadAnalysis(int numberToAnalyze, decimal recallLimit, decimal watchlistLowLimit,
        decimal watchlistHighLimit, decimal cumulativeDoseLimit, int expirationYearsLimit)
    {
        var status = GetReadAnalysisStatus();

        try
        {
            // Get the worker status.
            if (status.WorkerStatus ==
                ReadAnalysisStatus.AnalysisStatus.Running) return -2; // When -2, means already running.

            // Create a new status.
            status = new ReadAnalysisStatus()
            {
                StartDate = DateTime.Now,
                WorkerStatus = ReadAnalysisStatus.AnalysisStatus.Running
            };

            // Save the status and requery it.
            SetReadAnalysisStatus(status);
            status = GetReadAnalysisStatus();

            InsDataContext idc = new InsDataContext();

            // Create NEW ReadAnalysisLog object.
            var ral = new ReadAnalysisLogDetails
            {
                AnalyzedDate = DateTime.Now,
                AnalyzedBy = User.Identity.Name.Split('\\')[1],
                RecallLimit = recallLimit,
                WatchlistLowLimit = recallLimit,
                WatchlistHighLimit = watchlistHighLimit,
                CumulativeDoseLimit = cumulativeDoseLimit,
                ExpirationYearsLimit = expirationYearsLimit
            };

            // ADD the new object and SAVE to the ReadAnalysisLog collecti:Dn.
            idc.ReadAnalysisLogDetails.InsertOnSubmit(ral);
            idc.SubmitChanges();

            // Modified 09/09/2014 by Anuradha Nandi
            // This has been removed from functionality so that the User can dynamically input the number of Read Records to analyze.
            // Process 500 reads by default or whatever the app setting says.
            //numberToAnalyze = 500;
            //int.TryParse(Instadose.Basics.GetSetting("ReadAnalysisMaxReads"), out numberToAnalyze);
            // Minor change made to code that has no effect.
            int xyz = 0;

            // Set the worker status LogID.
            status.LogID = ral.ReadAnalysisLogDetailID;
            status.MessageDesc = string.Format("Checking for new reads ({0} max)...", numberToAnalyze);
            SetReadAnalysisStatus(status);

            // Of the above List get RIDs whose Badges have Baseline information associated.
            // Since the Production DB's IsAnalyzed column is NULL, the following will check for either NULL or False (0) IsAnalyzed value.
            var readIDs = (from udr in idc.UserDeviceReads
                           where (udr.IsAnalyzed.Equals(null)
                           || udr.IsAnalyzed == false)
                           && udr.DeviceInventory.Product.ProductGroupID == 1  // Instadose 1 (only).
                           orderby udr.CreatedDate
                           select udr).Take(numberToAnalyze).ToList();

            // This is the actual COUNT that will be analyzed.
            status.TotalReads = readIDs.Count();

            // IF there are not Reads to be analyzed, do the following;
            if (status.TotalReads == 0)
            {
                status.CompletedDate = DateTime.Now;
                status.WorkerStatus = ReadAnalysisStatus.AnalysisStatus.Complete;
                return ral.ReadAnalysisLogDetailID;
            }

            // Save the status.
            status.MessageDesc = "Preparing read anaylsis...";
            SetReadAnalysisStatus(status);

            var ra = new ReadAnalysis(idc, numberToAnalyze, recallLimit, watchlistHighLimit, cumulativeDoseLimit, expirationYearsLimit);

            // LOOP through each RID in the List.
            foreach (var udr in readIDs)
            {
                udr.ReadAnalysisLogDetailID = ral.ReadAnalysisLogDetailID;

                try
                {
                    // Perform the Read Analysis.
                    ra.PerformAnalysis(udr);

                    // If UserDeviceRead record is coming from the following criteria, please mark as Analyzed/Good.
                    // 1.) AccountID 2 (House Account/Internal)
                    // 2.) Read Type is Adjustment (11).
                    // 3.) Read Type is Lifetime (12).
                    // 4.) Read Type is Estimate (14).
                    // 5.) Read Type is Soft User Read (21).
                    // 6.) InitialRead = TRUE & HasAnomaly = FALSE.
                    if ((udr.AccountID == 2 || udr.ReadTypeID == 11 || udr.ReadTypeID == 12 || udr.ReadTypeID == 14 || udr.ReadTypeID == 21) || (udr.InitRead == true && udr.HasAnomaly == false))
                    {
                        udr.ReviewedDate = DateTime.Now;
                        udr.ReviewedBy = User.Identity.Name.Split('\\')[1];
                        udr.AnalysisActionTypeID = 1;
                    }

                    udr.IsAnalyzed = true;
                }
                catch (Exception ex)
                {
                    udr.IsAnalyzed = null;
                    udr.ErrorText = ex.ToString();

                    status.ReadErrors++;
                }

                status.ReadsCompleted++;

                // Save the status.
                status.MessageDesc = string.Format("Processing read #: {0}", udr.RID);
                SetReadAnalysisStatus(status);
            }

            status.MessageDesc = "Please wait while the analysis results are committed...";
            SetReadAnalysisStatus(status);

            // SUBMIT all changes to Database.
            idc.SubmitChanges();

            status.CompletedDate = DateTime.Now;
            status.WorkerStatus = ReadAnalysisStatus.AnalysisStatus.Complete;
            status.MessageDesc = "Read analysis complete. Please wait...";
            SetReadAnalysisStatus(status);

            return ral.ReadAnalysisLogDetailID;
        }
        catch (Exception ex)
        {
            status.CompletedDate = DateTime.Now;
            status.WorkerStatus = ReadAnalysisStatus.AnalysisStatus.Error;
            status.MessageDesc = "Error: " + ex.Message;
            SetReadAnalysisStatus(status);

            return -1;
        }
    }
}

public class ReadAnalysisStatus
{
    public enum AnalysisStatus { NotRunning, Running, Complete, Error }

    public int ReadAnalysisWorkerID { get; set; }
    public int LogID { get; set; }
    public AnalysisStatus WorkerStatus { get; set; }
    public int TotalReads { get; set; }
    public int ReadErrors { get; set; }
    public int ReadsCompleted { get; set; }
    public string MessageDesc { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public ReadAnalysisStatus()
    {
        this.WorkerStatus = AnalysisStatus.NotRunning;
        this.MessageDesc = "Read analysis is not running.";
    }
}