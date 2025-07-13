using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Instadose;
using Instadose.Data;

using Actuate;

/// <summary>
/// Create an address to the actuate report.
/// </summary>
public class ActuateReport : IDisposable
{
    #region Private Variables

    private InsDataContext idc;
    private ReportConfig _report;
    private string _reportUri;
    private string _reportName;
    private int _reportId;

    #endregion

    #region Read-Only Properties

    public string ReportUri { get { return _reportUri; } }
    public string ReportName { get { return _reportName; } }
    public int ReportID { get { return _reportId; } }

    #endregion

    #region Properties

    public bool UseA4Report { get; set; }
    public string RequestType { get; set; }
    public string RepositoryType { get; set; }
    public bool InvokeSubmit { get; set; }
    public Dictionary<string, string> Arguments { get; set; }

    #endregion

    #region Constructors

    public ActuateReport(string reportName)
    {
        if (reportName == null || reportName == string.Empty)
            // Throw an error because the report name was not specified
            throw new Exception("The Actuate report name was not specified.");

        // Set the report name
        this._reportName = reportName;

        // Load the report settings
        LoadReportSettings();

    }

    public ActuateReport(int reportId)
    {
        if (reportId == null || reportId <= 0)
            // Throw an error because the report id was not specified
            throw new Exception("The Actuate report ID was not specified.");

        // Set the report ID
        this._reportId = reportId;

        // Load the report settings
        LoadReportSettings();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Load the report configure from SQL and set the defaults.
    /// </summary>
    private void LoadReportSettings()
    {
        // Set the default properties
        this.RequestType = "immediate";
        this.RepositoryType = "Enterprise";
        this.InvokeSubmit = true;
        this.Arguments = new Dictionary<string, string>();

        // Create the context to query for the report.
        idc = new InsDataContext();
        
        if (_reportName == string.Empty)
        {
            // Try to find the report based on the ID
            int reportFound = (from rc in idc.ReportConfigs where rc.ReportConfigID == this._reportId select rc).Count();

            // Determine if the report was found
            if (reportFound > 0)
                // Load the report into the report config object
                _report = (from rc in idc.ReportConfigs where rc.ReportConfigID == this._reportId select rc).First();
        }
        else
        {
            // Try to find the report based on the name
            int reportFound = (from rc in idc.ReportConfigs where rc.ReportName == this._reportName select rc).Count();
            
            // Determine if the report was found
            if (reportFound > 0)
                // Load the report into the report config object
                _report = (from rc in idc.ReportConfigs where rc.ReportName == this._reportName select rc).First();
        }
        
        // Check to see if the report config was found.
        if (_report == null)
            // Throw an error since the report was missing.
            throw new Exception("The Actuate report with the name specified was not found.");


        // Set the report info
        _reportName = _report.ReportName;
        _reportId = _report.ReportConfigID;
        _reportUri = _report.ReportURL;

        // Check to see if the report URI was set
        if (_reportUri == null || _reportUri == string.Empty)
            // Throw an error because the report Uri was not found.
            throw new Exception("The Actuate report server Uri was not found.");
    }

    #endregion

    #region Public Methods

    public void Dispose()
    {
        // Close the data context
        idc.Dispose();
    }

    /// <summary>
    /// Retrieve the Actuate report URI.
    /// </summary>
    /// <returns></returns>
    public string GetReportUri()
    {
        // Stores the location of the report path
        string reportPath = (UseA4Report) ? _report.A4Path : _report.ReportPath;

        // Ensure the report path was set
        if (reportPath == null) throw new Exception("The executable name cannot be null.");

        // Create the encoder object used to encrypt parts of the actuate string.
        Encode encoder = new Encode();

        // Build the token. The token consists of properties used when calling the report along with the executable name.
        StringBuilder token = new StringBuilder("__executableName=" + reportPath);

        // Build the string with each of the arguments
        foreach (KeyValuePair<string, string> argument in Arguments)
            token.AppendFormat("&{0}={1}", argument.Key, argument.Value);

        // Encrypt the required data
        string encodedUsername = encoder.EncodeStr(_report.UserName);
        string encodedPassword = encoder.EncodeStr(_report.Password);
        string encodedToken = encoder.EncodeStr(token.ToString());
        string encodedTime = encoder.EncodeStr(DateTime.Now.ToUniversalTime().ToString("s")); // yyyy-MM-ddTHH:mm:ss

        // Declare the string builder
        StringBuilder sb = new StringBuilder(_reportUri);
        sb.AppendFormat("?__requesttype={0}", RequestType);
        
        // Include the token if it is not empty
        if(token.Length > 0)
            sb.AppendFormat("&{0}", token.ToString());

        sb.AppendFormat("&repositoryType={0}", RepositoryType);
        sb.AppendFormat("&invokeSubmit={0}", InvokeSubmit.ToString().ToLower());
        sb.AppendFormat("&actuatenm={0}", encodedUsername);
        sb.AppendFormat("&actuatecnf={0}", encodedPassword);
        sb.AppendFormat("&tm={0}", encodedTime);
        sb.AppendFormat("&token={0}", encodedToken);
        
        return sb.ToString();
    }

    #endregion
}
