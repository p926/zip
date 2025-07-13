using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Instadose.Data;
using System.Web.UI;
using Portal.Instadose;
using System.Linq.Expressions;
using Mirion.DSD.Utilities.Constants;

public partial class TechOps_ReadAnalysisReview : System.Web.UI.Page
{
    // Declare Instadose Data Context.
    InsDataContext idc = new InsDataContext();

    // SP for Read Analysis Results as List<> Object.
    private List<sp_ReadAnalysisReview3Result> readAnalysisResults;

    // Declare LogID value (Request.QueryString).
    public int logID = 0;

    // String to hold the current Username.
    string userName = "Unknown";

    // Declare AccountIDs and CompanyName values.
    string accountIDs = "";
    string companyName = "";

    public class AccountLinks
    {
        private string accountIDs;
        private string companyName;

        public AccountLinks(string companyname, string accountids)
        {
            this.companyName = companyname;
            this.accountIDs = accountids;
        }

        public string CompanyName
        {
            get { return companyName; }
            set { companyName = value; }
        }

        public string AccountIDs
        {
            get { return accountIDs; }
            set { accountIDs = value; }
        }
    }

    public SortDirection GridViewSortDirection
    {
        get
        {
            if (ViewState["sortDirection"] == null)
                ViewState["sortDirection"] = SortDirection.Ascending;

            return (SortDirection)ViewState["sortDirection"];
        }
        set { ViewState["sortDirection"] = value; }
    }

    public string GridViewSortField
    {
        get
        {
            if (ViewState["sortExpression"] == null)
                ViewState["sortExpression"] = string.Empty;

            return ViewState["sortExpression"].ToString();
        }
        set { ViewState["sortExpression"] = value; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try { this.userName = User.Identity.Name.Split('\\')[1]; }
        catch { this.userName = "Unknown"; }

        int.TryParse(Request.QueryString["LogID"], out logID);

        if (logID == 0) Response.Redirect("ReadAnalysis.aspx");

        if (Page.IsPostBack) return;

        // Retrieve "All" LinkButton information.
        GetAllLnkBtnInformation(out accountIDs, out companyName);

        // Assign AccountIDs and CompanyName for "All" Information.
        // This will display the "All" Link's RadGrid on 1st time Page_Load.
        hdnfldAccountIDs.Value = accountIDs;
        hdnfldCompanyName.Value = companyName;

        List<AnalysisTypes> analysisTypes = idc.AnalysisTypes.ToList();
        Session["AnalysisTypes"] = analysisTypes;

        BindGridView("All", string.Empty);

        var nonPinkFailedTest = analysisTypes.Where(x => x.IsPinkTest == false).Select(x => x.AnalysisTypeAbbrev.Trim()).ToList();

        var failedTests = (from udr in idc.UserDeviceReads
                           join ard in idc.AnalysisReadDetails on udr.RID equals ard.RID
                           join at in idc.AnalysisTypes on ard.AnalysisTypeID equals at.AnalysisTypeID
                           where udr.ReadAnalysisLogDetailID == logID 
                               && udr.AnalysisActionTypeID == null 
                               && udr.IsAnalyzed == true 
                               && !nonPinkFailedTest.Contains(at.AnalysisTypeAbbrev)
                               // && ard.AnalysisTypeID == 1 
                               && ard.Passed == false
                           select at).Distinct().ToList();

        ddlFailedTestFilter.DataTextField = "AnalysisTypeAbbrev";
        ddlFailedTestFilter.DataValueField = "AnalysisTypeAbbrev";
        ddlFailedTestFilter.DataSource = failedTests;
        ddlFailedTestFilter.DataBind();
    }

    protected string GetProductTypeCodeByID(int id)
    {
        var productType = BadgeTypes.GetReadAnalysisBadgeTypes().Find(x => x.Id == id);
        if (productType != null)
            return productType.Code;
        else
            return string.Empty;
    }


    /// <summary>
    /// Gets and Set Data for GridView.
    /// </summary>
    /// <param name="filtertype"></param>
    private void BindGridView(string filtertype, string failedTest)
    {
        readAnalysisResults = idc.sp_ReadAnalysisReview3(logID).ToList();

        if (readAnalysisResults == null) return;

        var getAccountIDs = hdnfldAccountIDs.Value.Split(',').Select(s => int.Parse(s)).ToList();
        getAccountIDs.Add(2);

        var hasFailedTestFilter = !string.IsNullOrEmpty(failedTest);
        if (hasFailedTestFilter)
            failedTest = failedTest.Trim();

        IQueryable<sp_ReadAnalysisReview3Result> readResult;
        // Filter out Accounts by "All" and others (these are the "Special Accounts").
        if (filtertype == "All")
            readResult = readAnalysisResults.Where(r =>
                    !getAccountIDs.Contains(r.AccountID) 
                    && (!hasFailedTestFilter || (r.FailedTests != null &&  r.FailedTests.Contains(failedTest)))
            ).AsQueryable<sp_ReadAnalysisReview3Result>();
        else
            readResult = readAnalysisResults.Where(r => 
                    getAccountIDs.Contains(r.AccountID)
                    && (!hasFailedTestFilter || (r.FailedTests != null && r.FailedTests.Contains(failedTest)))
            ).AsQueryable<sp_ReadAnalysisReview3Result>();


        string sortField = GridViewSortField;
        if (!string.IsNullOrEmpty(sortField))
        {
            if (GridViewSortDirection == SortDirection.Ascending)
            {
                if (sortField == "SerialNumber")
                    readResult = readResult.OrderBy(x => IsInteger(x.SerialNumber) ? int.Parse(x.SerialNumber) : int.MaxValue);
                else
                    readResult = readResult.OrderBy(p => p.GetType()
                               .GetProperty(sortField)
                               .GetValue(p, null));
            }
            else
            {
                if (sortField == "SerialNumber")
                    readResult = readResult.OrderByDescending(x => IsInteger(x.SerialNumber) ? int.Parse(x.SerialNumber) : int.MaxValue);
                else
                    readResult = readResult.OrderByDescending(p => p.GetType()
                               .GetProperty(sortField)
                               .GetValue(p, null));
            }
        }

        readAnalysisResults = readResult.ToList();

        // Bind the final result.
        gvReadAnalysisReview.DataSource = readAnalysisResults;
        gvReadAnalysisReview.DataBind();
    }

    Func<string, bool> IsInteger = s => { int temp; return int.TryParse(s, out temp); };

    /// <summary>
    /// Get All AccountIDs and CompanyName ("All").
    /// </summary>
    /// <param name="accountids"></param>
    /// <param name="companyname"></param>
    private void GetAllLnkBtnInformation(out string accountids, out string companyname)
    {
        // Get all Account IDs that should be excluded from the General RadGrid's data.
        string getExcludedAccountIDs = (from app in idc.AppSettings
                                        where app.AppKey == "ReadAnalysisSpecialAccounts"
                                        select app.Value).FirstOrDefault();

        string[] excludedAccountIDs = getExcludedAccountIDs.Split(new Char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries);
        StringBuilder strBuilderAllLnkBtn = new StringBuilder();
        foreach (string eaids in excludedAccountIDs)
        {
            strBuilderAllLnkBtn.Append(eaids).Append(",");
        }

        // Add AccountID = 2 to the String. Also, format to "2,123,456,..."
        string allAccountIDs = "2," + strBuilderAllLnkBtn.ToString().TrimEnd(new char[] { ',' });

        // Assign to [Global Variables] AccountIDs and CompanyName.
        accountids = allAccountIDs;
        companyname = "All";
    }

    /// <summary>
    /// Formats CompanyName so that only the "first word" of the full CompanyName is displayed.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="numberofwords"></param>
    /// <returns></returns>
    private static string FirstWordOfString(string item, int numberofwords)
    {
        try
        {
            // Number of words we still want to display.
            int words = numberofwords;

            for (int i = 0; i < item.Length; i++)
            {
                // Increment words on a space.
                if (item[i] == ' ' || item[i] == '|')
                {
                    words--;
                }
                // If we have no more words to display, return the substring.
                if (words == 0)
                {
                    return item.Substring(0, i);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }

        return string.Empty;
    }

    protected void lnkbtnAll_Click(object sender, EventArgs e)
    {
        int.TryParse(Request.QueryString["LogID"], out logID);

        if (logID == 0) return;

        // Retrieve "All" AccountIDs and CompanyName.
        GetAllLnkBtnInformation(out accountIDs, out companyName);

        // Assign AccountIDs and LinkTitle to HiddenFields.
        hdnfldAccountIDs.Value = accountIDs;
        hdnfldCompanyName.Value = companyName;

        BindGridView("All", hdnFailedTest.Value.ToString());
    }

    protected void lnkBtnSpecialAccounts_Click(object sender, EventArgs e)
    {
        LinkButton lnkbtn = (LinkButton)sender;

        accountIDs = (from app in idc.AppSettings
                                    where app.AppKey == "ReadAnalysisSpecialAccounts"
                                    select app.Value).FirstOrDefault();

        companyName = lnkbtn.CommandName.ToString();
        int.TryParse(Request.QueryString["LogID"], out logID);

        if (logID == 0) return;

        if (lnkbtn.CommandName == null) return;
        if (lnkbtn.CommandArgument == null) return;

        if (accountIDs.Contains('|'))
        {
            accountIDs = accountIDs.Replace("|", ",");
        }

        // Assign AccountIDs and LinkTitle to HiddenFields.
        hdnfldAccountIDs.Value = accountIDs;
        hdnfldCompanyName.Value = companyName;

        BindGridView("", hdnFailedTest.Value.ToString());
    }

    /// <summary>
    /// Refreshes/Reloads page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnRefreshGridView_Click(object sender, EventArgs e)
    {
        int.TryParse(Request.QueryString["LogID"], out logID);

        Response.Redirect(string.Format("ReadAnalysisReview.aspx?LogID={0}", logID));
    }

    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        ddlFailedTestFilter.SelectedValue = string.Empty;
        hdnFailedTest.Value = ddlFailedTestFilter.SelectedValue;
        inptSearch.Value = string.Empty;
        BindGridView(hdnfldCompanyName.Value, hdnFailedTest.Value);
    }

    /// <summary>
    /// Maintains Header Area of GridView when performing Quick Search.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvReadAnalysisReview_PreRender(object sender, EventArgs e)
    {
        if (gvReadAnalysisReview.Rows.Count > 0)
        {
            gvReadAnalysisReview.UseAccessibleHeader = true;
            gvReadAnalysisReview.HeaderRow.TableSection = TableRowSection.TableHeader;
        }
    }

    protected void gvReadAnalysisReview_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.gvReadAnalysisReview.PageIndex = e.NewPageIndex;

        // Rebind to GridView DataSource.
        BindGridView(hdnfldCompanyName.Value.ToString(), hdnFailedTest.Value.ToString());
    }

    protected void gvReadAnalysisReview_OnSorting(object sender, GridViewSortEventArgs e)
    {
        GridViewSortField = e.SortExpression;
        GridViewSortDirection = GridViewSortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
        BindGridView(hdnfldCompanyName.Value.ToString(), hdnFailedTest.Value.ToString());
    }

    protected void gvReadAnalysisReview_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Includes the last row in the GridView (per page) which is considered a "Footer".
        if (e.Row.RowType != DataControlRowType.DataRow &&
            e.Row.RowType != DataControlRowType.Footer) return;

        if (e.Row.RowIndex == -1) return;

        CheckBox chkBx = (CheckBox)e.Row.FindControl("chkbxCompleteReview");

        HyperLink hyprlnkRecall = (HyperLink)e.Row.FindControl("hyprlnkRecall");

        var rowData = readAnalysisResults[e.Row.DataItemIndex];
        int getRID = rowData.RID;
        int indexOfFailedTests = GetColumnIndexByName(e.Row, "FailedTests");

        var failureException = rowData.ReadAnalysisFailureException;
        var hasFailureException = !string.IsNullOrEmpty(failureException);

        hyprlnkRecall.Visible = !rowData.IsInRecall && (hasFailureException || (rowData.FailedTests != null && rowData.FailedTests.Trim().Contains("FDRC")));

        string failedTests = rowData.FailedTests;
        bool isInRecall = rowData.IsInRecall;

        if (hasFailureException)
        {
            var failureExceptionValues = failureException.Split('|').ToList();
            var failureCode = failureExceptionValues.FirstOrDefault();
            var failureMessage = failureExceptionValues.LastOrDefault();

            e.Row.BackColor = System.Drawing.Color.FromArgb(255, 120, 120); // Lighter Red
            e.Row.Cells[indexOfFailedTests].Text = failureCode;
            e.Row.Cells[indexOfFailedTests].ToolTip = failureMessage;

            if (string.IsNullOrEmpty(failureCode))
                e.Row.Cells[indexOfFailedTests].Text = "Except Failure";
            else if (failureCode.Length > 8)
                e.Row.Cells[indexOfFailedTests].Style.Add("word-break", "break-all");

            chkBx.Checked = false;
        }
        else if (failedTests != null)
        {
            if (rowData.HasPinkFailedTest)
                e.Row.BackColor = System.Drawing.Color.LightPink;

            if (failedTests.Contains("DNLT"))
                e.Row.BackColor = System.Drawing.Color.Khaki;

            e.Row.Cells[indexOfFailedTests].ToolTip = GetAllFailedTestNamesByAbbrev(failedTests).ToString();
            chkBx.Checked = false;
        }
        // If value is NULL, then display "All Passed".
        // Set the CheckBoxes to checked and enabled.
        else
        {
            e.Row.Cells[indexOfFailedTests].Text = "All Passed";
            e.Row.Cells[indexOfFailedTests].ToolTip = "All Passed";
            chkBx.Checked = true;
        }

        // If Previous Read Analysis Status is Watchlist (set background color of row/record).
        if (readAnalysisResults[e.Row.DataItemIndex].PreviousReviewStatus == 3)
        {
            e.Row.BackColor = System.Drawing.Color.LightBlue;
        }

        // If Badge is Recalled (set background color of row/record).
        if (isInRecall)
        {
            e.Row.BackColor = System.Drawing.Color.MediumPurple;
        }        

        int productGroupId = readAnalysisResults[e.Row.DataItemIndex].ProductGroupID;

        int dldCalcColumnIndex = GetColumnIndexByName(e.Row, "DLDCalc");
        gvReadAnalysisReview.Columns[dldCalcColumnIndex].Visible = productGroupId != ProductGroupIDConstants.InstadoseVue;
        gvReadAnalysisReview.Columns[dldCalcColumnIndex + 1].Visible = productGroupId == ProductGroupIDConstants.InstadoseVue; //ActualDeepLowDose

        int cumulativeDoseColumnIndex = GetColumnIndexByName(e.Row, "CumulDose");
        gvReadAnalysisReview.Columns[cumulativeDoseColumnIndex].Visible = (productGroupId != ProductGroupIDConstants.Instadose2New);        
        gvReadAnalysisReview.Columns[cumulativeDoseColumnIndex + 1].Visible = productGroupId == ProductGroupIDConstants.Instadose2New; //DeepLowCumulativeDose
    }

    private bool CheckPinkFailedTest(string failedTestAbbrevs)
    {
        var analysisTypes = (List<AnalysisTypes>)Session["AnalysisTypes"];
        var nonPinkFailedTest = analysisTypes.Where(x => x.IsPinkTest == false).Select(x => x.AnalysisTypeAbbrev.Trim()).ToList();

        var result = false;
        var abbrevs = failedTestAbbrevs.Split(',').Select(p => p.Trim()).ToList();
        foreach (var abbrev in abbrevs)
        {
            var isPinkTest = !nonPinkFailedTest.Contains(abbrev);
            if (isPinkTest)
            {
                result = true;
                break;
            }
        }
        return result;
    }

    private sp_ID3_isException2Result CheckHasID3Exception(string serialNo, int? iotDeviceReadPackageHeaderId)
    {
        var headerId = 0;
        if (iotDeviceReadPackageHeaderId.HasValue)
            headerId = iotDeviceReadPackageHeaderId.Value;

        var exception = idc.sp_ID3_isException2(iotDeviceReadPackageHeaderId, serialNo, true).FirstOrDefault();

        return exception;
    }

    private string GetAllFailedTestNamesByAbbrev(string failedTestAbbrevs)
    {
        string result = string.Empty;
        List<AnalysisTypes> analysisTypes = (List<AnalysisTypes>)Session["AnalysisTypes"];
        var abbrevs = failedTestAbbrevs.Split(',').Select(p => p.Trim()).ToList();
        //var analysisTypeNames = analysisTypes.Where(x => abbrevs.Contains(x.AnalysisTypeAbbrev.Trim())).Select(x=> x.AnalysisTypeName).ToArray();
        //return String.Join("\r\n", analysisTypeNames);
        // changed to use loop so that the name will be ordered same with the abbrev
        foreach (var abbrev in abbrevs)
        {
            var analysisType = analysisTypes.FirstOrDefault(x => x.AnalysisTypeAbbrev.Trim() == abbrev.Trim());
            if (analysisType != null)
            {
                result += analysisType.AnalysisTypeName + "\r\n";
            }
        }
        return result.Trim();
    }

    /// <summary>
    /// Get Index of a specific column based on its name.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="columnname"></param>
    /// <returns></returns>
    private int GetColumnIndexByName(GridViewRow row, string columnname)
    {
        int columnIndex = 0;

        foreach (DataControlFieldCell cell in row.Cells)
        {
            if (cell.ContainingField is BoundField)
                if (((BoundField)cell.ContainingField).DataField.Equals(columnname))
                    break;
            columnIndex++; // Keep adding 1 while we don't have the correct Column Name.
        }

        return columnIndex;
    }

    /// <summary>
    /// Get all "Failed Test" full names (based on AnalysisTypeName).
    /// </summary>
    /// <param name="rid"></param>
    /// <returns></returns>
    private string GetAllFailedTestNames(int rid)
    {
        string allFailedTestNames = "";

        var listOfAllFailedTests = (from ard in idc.AnalysisReadDetails
                                    join at in idc.AnalysisTypes on ard.AnalysisTypeID equals at.AnalysisTypeID
                                    where ard.RID == rid && ard.Passed == false
                                    orderby at.AnalysisTypeAbbrev ascending
                                    select at.AnalysisTypeName).ToList();

        foreach (string test in listOfAllFailedTests)
        {
            // Format List (string).
            allFailedTestNames += test + "\r\n";
        }

        return allFailedTestNames;
    }

    protected void gvReadAnalysisReview_RowCreated(object sender, GridViewRowEventArgs e)
    {
        // Includes the last row in the GridView (per page) which is considered a "Footer".
        if (e.Row.RowType != DataControlRowType.DataRow &&
            e.Row.RowType != DataControlRowType.Footer) return;

        if (e.Row.RowIndex == -1) return;

        // Toggle GridView CheckBoxes based on HeaderTemplate CheckBox State (Checked/Unchecked).
        CheckBox chkBxSelect = (CheckBox)e.Row.Cells[0].FindControl("chkbxCompleteReview");
        CheckBox chkBxHeader = (CheckBox)this.gvReadAnalysisReview.HeaderRow.FindControl("headerChkBx");
        chkBxSelect.Attributes["onclick"] = string.Format
                                               (
                                                  "javascript:ChildClick(this,'{0}');",
                                                  chkBxHeader.ClientID
                                               );
    }

    #region MARK AS REVIEWED FUNCTIONS.
    /// <summary>
    /// Gets RID for each row/record that is Checked, and marks as Reviewed (Good/None) in UserDeviceReads table.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnMarkAsReviewed_Click(object sender, EventArgs e)
    {
        int rID = 0;

        foreach (GridViewRow row in gvReadAnalysisReview.Rows)
        {
            CheckBox chkBx = row.FindControl("chkbxCompleteReview") as CheckBox;
            if (chkBx.Checked == true)
            {
                // Get RID from each row/record.
                Int32.TryParse(gvReadAnalysisReview.DataKeys[row.RowIndex]["RID"].ToString(), out rID);

                UserDeviceRead udrRecord = (from udr in idc.UserDeviceReads
                                            where udr.RID == rID
                                            select udr).FirstOrDefault();

                if (!udrRecord.AnalysisActionTypeID.HasValue)
                {
                    udrRecord.ReviewedDate = DateTime.Now;
                    udrRecord.ReviewedBy = userName;


                    udrRecord.AnalysisActionTypeID = 1;

                    try
                    {
                        idc.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.ToString());
                    }
                }
            }
        }

        idc.sp_UpdateReadAnalysisLogDetailCounts(logID);

        BindGridView(hdnfldCompanyName.Value.ToString(), hdnFailedTest.Value.ToString());
    }
    #endregion

    protected void lnkbtnReturnToReadAnalysisPage_Click(object sender, EventArgs e)
    {
        Response.Redirect("ReadAnalysis.aspx");
    }

    protected void ddlFailedTestFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        hdnFailedTest.Value = ddlFailedTestFilter.SelectedValue;
        inptSearch.Value = string.Empty;

        BindGridView(hdnfldCompanyName.Value, hdnFailedTest.Value);


    }
}

