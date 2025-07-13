using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

using Instadose.Data;

public partial class CustomerService_RequestQueue : System.Web.UI.Page
{
    // Create the Instadose DataContext variable.
    private InsDataContext idc = new InsDataContext();
    
    // Current Username.
    private string UserName = "Unknown";

    // Public Method that LIMITS the character length of a Label.
    // Primary used for Case Notes in the GridView.
    // Limit (string, int).
    public static string LimitDisplayedCharacters(object desc, int length)
    {
        StringBuilder strDesc = new StringBuilder();
        strDesc.Insert(0, desc.ToString());

        if (strDesc.Length > length)
            return strDesc.ToString().Substring(0, length) + "...";
        else 
            return strDesc.ToString();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the Username.
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch 
        { 
            this.UserName = "Unknown"; 
        }

        // Display the Created Message.
        if (Request.QueryString["CaseID"] != null && Request.QueryString["IsNew"] != null)
        {
            // Display the Message.
            int caseID = 0;
            int.TryParse(Request.QueryString["CaseID"], out caseID);

            bool isNewCase = false;
            bool.TryParse(Request.QueryString["IsNew"], out isNewCase);

            string message = "Request Case#: {0} has been created successfully.";
            if (!isNewCase) message = "Request Case#: {0} has been modified successfully.";
            
            // Display the message.
            this.submitMsg.InnerText = string.Format(message, caseID);
            this.messages.Visible = true;
        }

        int CaseStatusID = int.Parse(ddlCaseStatus.SelectedValue.ToString());

        // Show Total (number) of Open Cases.
        int CaseCount = 0;

        if (CaseStatusID == 0) 
            CaseCount = (from a in idc.Cases select a).Count();
        else
            CaseCount = (from a in idc.Cases where a.CaseStatusID == CaseStatusID select a).Count();

        lblTotalCases.Text  = "Total Cases: " + CaseCount.ToString();

        if (Page.IsPostBack) return;

        this.PopulateCaseStatusDDL();
    }

    // Case Status DropDownList.
    private void PopulateCaseStatusDDL()
    {
        var caseStatus = from cs in idc.CaseStatus
                         orderby cs.CaseStatusID
                         where cs.Active
                         select new
                         {
                            cs.CaseStatusID,
                            cs.CaseStatusDesc
                         };

        // Add Case Status to DropDownList values.
        this.ddlCaseStatus.DataSource = caseStatus;
        this.ddlCaseStatus.DataTextField = "CaseStatusDesc";
        this.ddlCaseStatus.DataValueField = "CaseStatusID";
        this.ddlCaseStatus.DataBind();
    }

    // Create NEW CS Request Button.
    protected void btnNewRequest_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("CreateRequestCase.aspx");
    }

    protected void btnShowAllRequest_Click(object sender, EventArgs e)
    {
        gvRequestCases.AllowPaging = !gvRequestCases.AllowPaging;
        btnShowAllRequest.Text = (gvRequestCases.AllowPaging == false) ? "Turn Paging On" : "Turn Paging Off";
        gvRequestCases.DataBind();
    }

    protected void gvRequestCases_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // Must have the OpenDialog CommandName to continue.
        if (e.CommandName != "EditCSRequestCase") return;

        // Parse through the contents of the CommandArgument to get the CaseID.
        int caseID = 0;
        int.TryParse(e.CommandArgument.ToString(), out caseID);

        if (caseID == 0) return;

        // Use CaseID (from CommandArgument) as part of URL.
        string completeURL = String.Format("CreateRequestCase.aspx?CaseID={0}", caseID);

        Response.Redirect(completeURL);
    }
}
