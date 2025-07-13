using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using OnTimeApi;
using System.Security.Cryptography;

public partial class IT_CreateOntimeItem : System.Web.UI.Page
{
    #region Priavte/Public Properties

    private bool _refreshState;
    private bool _isRefresh;
    public bool IsRefresh
    {
        get
        {
            return _isRefresh;
        }
    }
    protected override void LoadViewState(object savedState)
    {
        object[] allStates = (object[])savedState;
        base.LoadViewState(allStates[0]);
        _refreshState = (bool)allStates[1];
        _isRefresh = _refreshState == (bool)Session["__ISREFRESH"];
    }
    protected override object SaveViewState()
    {
        Session["__ISREFRESH"] = _refreshState;
        object[] allStates = new object[2];
        allStates[0] = base.SaveViewState();
        allStates[1] = !_refreshState;
        return allStates;
    }

    private MyOntime myOntimeObj = new MyOntime();
    private BindingList<Project> Projects = new BindingList<Project>();
    private BindingList<Project> AuthorizedProjects = new BindingList<Project>();
    private BindingList<Item> Items = new BindingList<Item>();
    BindingList<Reported_By_Customer_Contact> Contacts = new BindingList<Reported_By_Customer_Contact>();
    BindingList<ResultItem> ResultItems = new BindingList<ResultItem>();

    // 9/2013 WK - Additional dropdowns for viewing grid filters.
    private BindingList<Project> viewProjects = new BindingList<Project>();
    private BindingList<Status> viewStatus = new BindingList<Status>();
    private BindingList<Type> viewType = new BindingList<Type>();
    private BindingList<Workflow_Step> viewWorkFlow = new BindingList<Workflow_Step>();
    private BindingList<Assigned_To> viewAssignTo = new BindingList<Assigned_To>();

    private string resourceType;
    private int loginUserContactID;   // = -1;   // No one
    private string loginUser = "";
    private string redirectURL = "Default.aspx";
    DataTable itemsDT;   // DataTable of all Ontime items returned by searching 

    /// <summary>
    /// Specifications of devices that should renew.
    /// </summary>
    public class OntimeItems
    {
        /// <summary>
        /// Item number of the On Time issue.
        /// </summary>
        public string ItemID { get; set; }

        /// <summary>
        /// Project of the On time issue.
        /// </summary>
        public string OnTimeProject { get; set; }

        /// <summary>
        /// Defect or Incident?
        /// </summary>
        public string BacklogType { get; set; }

        /// <summary>
        /// Description of On time issue.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Work flow of On time issue.
        /// </summary>
        public string WorkflowStep { get; set; }

        /// <summary>
        /// Assigned person to investigate/resolve On time issue.
        /// </summary>
        public string AssignTo { get; set; }

        /// <summary>
        /// Reported date of On time issue.
        /// </summary>
        public string ReportedDate { get; set; }

        /// <summary>
        /// Completed date of On time issue.
        /// </summary>
        public string CompleteDate { get; set; }

        /// <summary>
        /// Status of On time issue.
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// collection of On time items.
    /// </summary>
    private List<OntimeItems> onTimeItemsList;

    #endregion

    protected void Page_Unload(object sender, EventArgs e)
    {
        try
        {
            if (myOntimeObj.OnTime != null && myOntimeObj.OnTime.HasAccessToken())
                myOntimeObj.OnTime.Get<MessageResponse>("oauth2/revoke");
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch (Exception ex)
        {
            //throw ex; 
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            // Get the RedirectUrl       
            if (Request.QueryString["RedirectUrl"] != null)
            {
                redirectURL = Request.QueryString["RedirectUrl"];
            }

            loginUser = User.Identity.Name.Split('\\')[1];

            //Load all dropdowns only once.  Intialize ContactID only once also.
            if (!IsPostBack)
            {
                //loginUserContactID = -1;

                if (!loadAllControls()) return;

                if (loginUser == "tdo" || loginUser == "wkakemoto")
                {
                    //Delete option is for developers only!
                    this.deleteSection.Visible = true;
                    this.btnDelete.Visible = true;
                }
                else
                {
                    this.deleteSection.Visible = false;
                    this.btnDelete.Visible = false;
                }

            }

            // If loginUser is among Customer Contacts then allowed to use this page to submit Ontime item, 
            // else redirect to Default page
            //if (loginUserContactID > -1 || loginUser == "wkakemoto")

            //**********************************************************
            //9/2013 WK - manually set wk since account setting is not stable.
            //  this allow me to create On Time issues.
            //***********************************************************
            //if (loginUser == "wkakemoto") loginUserContactID = 48;
            //this.ddlContacts.SelectedValue = loginUserContactID.ToString();
            //else
            //    loginUserContactID = 12;  //6 - LP   3 -Craig
            //                             //12 - Kip  5 - Thihn
            ////***********************************************************


            InvisibleErrors();
            InvisibleSuccess();

            //get type of On time ticket to be created (based on radio button)
            this.resourceType = setResourceType();

            //if (this.radBacklogType.SelectedItem.Text == "Help Desk")
            //    this.resourceType = "v2/incidents";
            //else if (this.radBacklogType.SelectedItem.Text == "Defect")
            //    this.resourceType = "v2/defects";
            //else
            //    this.resourceType = "v2/features";

            // Display all items created by loginUser
            DisplayAllOntimeItems();
        }
        catch (Exception ex)
        {
            //Is On Time server unavailable?
            if (ex.Message.Contains("API call limit"))
            {
                //disable form data entry buttons.
                btnAdd.Enabled = false;
                btnReset.Enabled = false;
                attachment1.Enabled = false;
                attachment2.Enabled = false;

                this.radBacklogType.Enabled = false;
                this.ddlProjects.Enabled = false;
                this.txtDescription.Enabled = false;
                this.ddlSeverity.Enabled = false;
                this.txtTitle.Enabled = false;
                this.txtReproStep.Enabled = false;

                //disable filter grid dropdowns and clear filter button.
                //loadProjectFilterDropdown();

                //ddlViewProject.Enabled = false;
                ddlBackLogType.Enabled = false;
                ddlStatus.Enabled = false;
                lnkbtnClearFilters.Visible = false;

                //turn on retrun link button to allow user to return to IT default menu.
                lnkReturn.Visible = true;

                backButton.Visible = true;

                VisibleErrors("Unable to create Help Desk ticket or view On Time issues at this time.  Please try again at later time.");
            }
        }

        // Display all items created by loginUser
        // displayItemsByCustomerContactID(loginUserContactID);   No need to reload grid again.
    }

    #region Form Data Events/Methods

    /// <summary>
    /// load all dropdowns for both forms.  (Data entry and view screens).
    /// </summary>
    private bool loadAllControls()
    {
        bool success = false;

        try
        {
            string myContactID = "";

            loadOnTimeItemProjectDropdown();

            loadContactDropdown(ref myContactID);

            loadStatusDropdown();

            if (myContactID != "")
            {
                this.ddlContacts.SelectedValue = myContactID;

                this.mainForm.Visible = true;
                this.backButton.Visible = false;
                success = true;
            }
            else
            {
                // Page.Response.Redirect("Default.aspx");
                VisibleErrors("You do not have access to create OnTime Requests.  Please e-mail irv-instadose@mirion.com to request access.");
                this.backButton.Visible = true;
                this.mainForm.Visible = false;
            }
            //10/2013 WK - Disabled for now.
            //loadProjectFilterDropdown();
        }
        catch (Exception ex)
        {
        }

        return success;
    }

    /// <summary>
    /// load contacts dropdown - data entry screen.
    /// </summary>
    private void loadContactDropdown(ref string pContactID)
    {
        try
        {
            var result = myOntimeObj.OnTime.Get<DataResponse<List<Reported_By_Customer_Contact>>>("v2/contacts");

            Contacts.Clear();

            foreach (var contact in result.data)
            {
                Contacts.Add(contact);

                // Looking for ContactID of window authenticate login user          
                string username = contact.email.Split('@')[0];
                if (username == loginUser)
                {
                    //this.loginUserContactID = contact.id;
                    pContactID = contact.id.ToString();
                }

                //if (username == loginUser) this.loginUserContactID = contact.id;
                //***************************************************************************************
                // 9/25/13 WK - Set for my login.
                //if (loginUser == "wkakemoto") loginUserContactID = 48; //12;  //6 - LP  3 -Craig   5 - Thinh.
                //             Kip - 12
                //***************************************************************************************
            }

            this.ddlContacts.DataSource = Contacts;
            this.ddlContacts.DataValueField = "id";
            this.ddlContacts.DataTextField = "name";
            this.ddlContacts.DataBind();

            ListItem item1 = new ListItem("", "");
            this.ddlContacts.Items.Insert(0, item1);
        }
        catch (Exception ex)
        {
            //    if (ex.Message.Contains("API call limit"))
            //        VisibleErrors("Unable to create Help Desk ticket or view On Time issues at this time.  Please try again at later time."); 

            //throw ex;
        }
    }

    /// <summary>
    /// set resource type for On Time POST method based on radio button selected.
    /// </summary>
    /// <returns></returns>
    private string setResourceType()
    {
        string resourceType = null;

        if (this.radBacklogType.SelectedItem.Text == "Help Desk")
            resourceType = "v2/incidents";
        else if (this.radBacklogType.SelectedItem.Text == "Defect")
            resourceType = "v2/defects";
        else
            resourceType = "v2/features";

        return resourceType;
    }

    /// <summary>
    /// load all projects for On time issue entry dropdown.
    /// </summary>
    private void loadOnTimeItemProjectDropdown()
    {
        //try
        //{
        var result = myOntimeObj.OnTime.Get<DataResponse<List<ParentProject>>>("v2/projects");

        Projects.Clear();
        AuthorizedProjects.Clear();

        foreach (var project in result.data)
        {
            string parentName = "";
            populateProjects(project, parentName);
        }

        // Filter out Projects list. Currently, only "1-InBox" 
        //and its sub-projects are authorized to use used by all Windows Authenticate users
        //for new On time issue data entry.
        foreach (Project proj in Projects)
        {
            if (proj.name.Contains("1 - InBox"))
            {
                AuthorizedProjects.Add(proj);
            }
        }

        this.ddlProjects.DataSource = AuthorizedProjects;
        this.ddlProjects.DataValueField = "id";
        this.ddlProjects.DataTextField = "name";
        this.ddlProjects.DataBind();
        //}
        //catch (Exception ex)
        //{
        //    //throw new Exception(ex.Message);
        //}
    }

    /// <summary>
    /// load all projects for new On Time issue data entry projects dropdown.
    /// </summary>
    /// <param name="pParentProjects"></param>
    /// <param name="pParentName"></param>
    private void populateProjects(ParentProject pParentProjects, string pParentName)
    {
        if (pParentProjects.children == null)
        {
            Project myProject = new Project();
            myProject.id = pParentProjects.id;
            myProject.name = pParentName + "--->" + pParentProjects.name;
            Projects.Add(myProject);
            return;
        }

        Project pProject = new Project();
        pProject.id = pParentProjects.id;
        pProject.name = pParentName + "--->" + pParentProjects.name;
        Projects.Add(pProject);

        string parentName = pProject.name;
        foreach (ParentProject item in pParentProjects.children)
        {
            populateProjects(item, parentName);
        }
    }

    //private void GetContacts()
    //{
    //    var result = myOntimeObj.OnTime.Get<DataResponse<List<Reported_By_Customer_Contact>>>("v1/contacts");

    //    Contacts.Clear();
    //    foreach (var contact in result.data)
    //    {
    //        Contacts.Add(contact);

    //        // Looking for ContactID of window authenticate login user          
    //        string username = contact.email.Split('@')[0];
    //        if (username == loginUser)
    //        {
    //            this.loginUserContactID = contact.id;
    //        }
    //    }
    //}
    ///**************************************************************************************
    ///  10/2013 WK - view project filter dropdown disabled for now.  May use in the near
    ///     future.
    ///**************************************************************************************
    ///
    ///// <summary>
    ///// load project dropdown for view screen.
    ///// </summary>
    //private void loadProjectFilterDropdown()
    //{
    //    //try
    //    //{
    //        var result = myOntimeObj.OnTime.Get<DataResponse<List<ParentProject>>>("v2/projects");

    //        ////var result2 = myOntimeObj.OnTime.Get<DataResponse<List<Project>>>("v2/projects/Instadose");
    //        ////var result2 = myOntimeObj.OnTime.Get<DataResponse<List<Workflow_Step>>>("v2/picklists/work_log_types");
    //        ////var result3 = myOntimeObj.OnTime.Get<DataResponse<List<Workflow_Step>>>("v2/picklists/item_relations");
    //        ////var result4 = myOntimeObj.OnTime.Get<DataResponse<List<Workflow_Step>>>("v2/picklists/status");

    //        //Initialize the binding list of filtering projects.
    //        viewProjects.Clear();

    //        //build list of On Time projects for view projects
    //        //dropdown (grid filtering dropdown).
    //        foreach (var project in result.data)
    //        {
    //            string parentName = "";
    //            loadViewProjects(project, parentName);
    //        }

    //        //getProjects2();

    //        //var projs = viewProjects.OrderBy(p=>p.name).Distinct().ToList();

    //        //this.ddlViewProject.DataSource = viewProjects;  // projs;
    //        //this.ddlViewProject.DataValueField = "id";
    //        //this.ddlViewProject.DataTextField = "name";
    //        //this.ddlViewProject.DataBind();

    //        //ListItem item1 = new ListItem("", "");
    //        //this.ddlViewProject.Items.Insert(0, "ALL");
    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //}
    //}

    ///// <summary>
    /////  load projects for on time issues grid - filtering dropdown.
    ///// </summary>
    ///// <param name="parentProjects"></param>
    ///// <param name="parentName"></param>
    ///// <param name="addIndentation"></param>
    //private void loadViewProjects(ParentProject parentProjects, 
    //        string parentName, bool addIndentation = false)
    //{
    //    //If no children under this parent project then
    //    //add or don't add indentation and load
    //    //ID and Name of parent project and return List.
    //    if (parentProjects.children == null)
    //    {
    //        Project parentProj = new Project();
    //        parentProj.id = parentProjects.id;

    //        //myProject.name = pParentName + "--->" + pParentProjects.name;

    //        //Is this parent name or part of list of children
    //        //projects?  If part of list of children then 
    //        //indent project name.
    //        if (addIndentation)
    //            parentProj.name = "--->" + parentProjects.name;
    //        else
    //            parentProj.name = parentProjects.name;

    //        viewProjects.Add(parentProj);
    //        return;
    //    }
    //    else
    //    {
    //        //Load parent (thas has children projects) into List.
    //        Project project2 = new Project();
    //        project2.id = parentProjects.id;
    //        project2.name = parentProjects.name;

    //        viewProjects.Add(project2);
    //        ////**************************************************************

    //        //Load each child project under this parent into List.
    //        string parentName2 = project2.name;
    //        foreach (ParentProject item in parentProjects.children)
    //        {
    //            //Recurse into children projects and indent project name.
    //            //"Parent project" = child project.

    //            if (item.name.Contains("."))  //Itemized sub projects (1., 2., etc.).  Do not indent.
    //                loadViewProjects(item, parentName2, false);
    //            else
    //                loadViewProjects(item, parentName2, true);
    //        }
    //    }
    //}

    //****************************************************************************************

    /// <summary>
    /// load status dropdown for view screen.
    /// </summary>
    private void loadStatusDropdown()
    {
        //try
        //{
        var result = myOntimeObj.OnTime.Get<DataResponse<List<Status>>>("v2/picklists/status");

        viewStatus.Clear();

        foreach (var status in result.data)
        {
            Status onTimeStatus = new Status();

            onTimeStatus.id = status.id;
            onTimeStatus.name = status.name;
            viewStatus.Add(onTimeStatus);
        }

        this.ddlStatus.DataSource = viewStatus;
        this.ddlStatus.DataValueField = "id";
        this.ddlStatus.DataTextField = "name";
        this.ddlStatus.DataBind();

        ListItem item1 = new ListItem("", "");
        this.ddlStatus.Items.Insert(0, "ALL");
        //}
        //catch (Exception ex)
        //{
        //    int x = 1;
        //}
    }

    //private void defineDetailDataTable()
    //{
    //    itemsDT = new DataTable();
    //    itemsDT.Columns.Add("ItemID", typeof(string));
    //    itemsDT.Columns.Add("OnTimeProject", typeof(string));
    //    itemsDT.Columns.Add("BacklogType", typeof(string));
    //    itemsDT.Columns.Add("Name", typeof(string));
    //    itemsDT.Columns.Add("WorkflowStep", typeof(string));
    //    itemsDT.Columns.Add("AssignTo", typeof(string));
    //    itemsDT.Columns.Add("ReportedDate", typeof(string));
    //    itemsDT.Columns.Add("CompleteDate", typeof(string));
    //    itemsDT.Columns.Add("Status", typeof(string));
    //}

    /// <summary>
    /// build and/or filter on time grid for login user.
    /// </summary>
    /// <param name="customerContactID"></param>
    private void DisplayAllOntimeItems()
    {
        try
        {
            ////set contacts dropdown to authorized user. (customer).
            //if (loginUserContactID == 0)
            //{
            //    loginUserContactID = int.Parse(Session["ContactID"].ToString());
            //    ddlContacts.SelectedValue = loginUserContactID.ToString();
            //        //int.Parse(ddlContacts.SelectedValue);
            //}
            ////this.ddlContacts.SelectedValue = loginUserContactID.ToString();

            bool filterProject = false;
            bool filterType = false;
            bool filterStatus = false;

            List<OntimeItems> itemsList = new List<OntimeItems>();

            //onTimeItemsList.Clear();

            //Build grid with all data for contact.           
            buildOnTimeIssuesGrid(int.Parse(this.ddlContacts.SelectedValue));

            //Has Project dropdown being used?
            //if (!ddlViewProject.SelectedItem.Text.Contains("ALL")) filterProject = true;

            //Is BackLog type dropdown being used?
            if (!ddlBackLogType.SelectedItem.Text.Contains("ALL")) filterType = true;

            //Is Status dropdown being used?
            if (!ddlStatus.SelectedItem.Text.Contains("ALL")) filterStatus = true;

            //Any filtering required for grid display?
            if (filterProject || filterType || filterStatus)
            {
                //If any dropdowns used, then filter issues grid.
                itemsList = filterOntimeIssueGrid(filterProject, filterType, filterStatus);

                onTimeItemsList = itemsList;
            }

            grdItemsView.DataSource = onTimeItemsList;
            grdItemsView.DataBind();
        }
        catch (Exception ex)
        { }
        //{ throw ex; }
    }

    /// <summary>
    /// Build default On Time issues and defects grid based on login user name (contractID).
    /// </summary>
    /// <param name="customerContactID"></param>
    private void buildOnTimeIssuesGrid(int customerContactID)
    {
        try
        {
            onTimeItemsList = new List<OntimeItems>();

            DataRow dtr;
            string columns = "id,project,name,workflow_step,assigned_to,reported_date,completion_date,status";
            var searchDefectsParameters = new Dictionary<string, object>();
            searchDefectsParameters.Add("contact_id", customerContactID);
            searchDefectsParameters.Add("columns", columns);

            var searchIncidentsParameters = new Dictionary<string, object>();
            searchIncidentsParameters.Add("contact_id", customerContactID);
            searchIncidentsParameters.Add("columns", columns);

            var searchProductBacklogParameters = new Dictionary<string, object>();
            searchProductBacklogParameters.Add("contact_id", customerContactID);
            searchProductBacklogParameters.Add("columns", columns);

            // GET ALL DEFECTS created by a customer contact id = 12
            var defectsResult = myOntimeObj.OnTime.Get<DataResponse<List<ResultItem>>>("v2/defects", searchDefectsParameters);

            // GET ALL INCIDENTS created by a customer contact id = 19
            var incidentsResult = myOntimeObj.OnTime.Get<DataResponse<List<ResultItem>>>("v2/incidents", searchIncidentsParameters);

            // GET ALL FEATURES created by a customer contact id = 19
            var productBacklogResult = myOntimeObj.OnTime.Get<DataResponse<List<ResultItem>>>("v2/features", searchProductBacklogParameters);

            //defineDetailDataTable();

            //Load defects.
            foreach (ResultItem item in defectsResult.data)
            {
                // Create a On Time grid record.
                OntimeItems gridItems = new OntimeItems()
                {
                    ItemID = item.id.ToString(),
                    OnTimeProject = item.project.name,
                    BacklogType = "defects",
                    Name = item.name,
                    WorkflowStep = item.workflow_step.name,
                    AssignTo = item.assigned_to.name,

                    // Convert time to UTC
                    ReportedDate = (item.reported_date != null && item.reported_date.Length > 0) ?
                                        TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(item.reported_date)).ToShortDateString() : "",

                    // Convert time to UTC
                    CompleteDate = (item.completion_date != null && item.completion_date.Length > 0) ?
                                    TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(item.completion_date)).ToShortDateString() : "",

                    Status = item.status.name,
                };

                // Add filter on time issues.
                onTimeItemsList.Add(gridItems);
            }

            //Load incidents.
            foreach (ResultItem item in incidentsResult.data)
            {
                // Create a On Time grid record.
                OntimeItems gridItems = new OntimeItems()
                {
                    ItemID = item.id.ToString(),
                    OnTimeProject = item.project.name,
                    BacklogType = "incidents",
                    Name = item.name,
                    WorkflowStep = item.workflow_step.name,
                    AssignTo = item.assigned_to.name,

                    // Convert time to UTC
                    ReportedDate = (item.reported_date != null && item.reported_date.Length > 0) ?
                                    TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(item.reported_date)).ToShortDateString() : "",

                    // Convert time to UTC
                    CompleteDate = (item.completion_date != null && item.completion_date.Length > 0) ?
                                    TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(item.completion_date)).ToShortDateString() : "",

                    Status = item.status.name,
                };

                // Add filter on time issues.
                onTimeItemsList.Add(gridItems);
            }

            //Load product backlogs (features).
            foreach (ResultItem item in productBacklogResult.data)
            {
                // Create a On Time grid record.
                OntimeItems gridItems = new OntimeItems()
                {
                    ItemID = item.id.ToString(),
                    OnTimeProject = item.project.name,
                    BacklogType = "features",
                    Name = item.name,
                    WorkflowStep = item.workflow_step.name,
                    AssignTo = item.assigned_to.name,

                    // Convert time to UTC
                    ReportedDate = (item.reported_date != null && item.reported_date.Length > 0) ?
                                        TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(item.reported_date)).ToShortDateString() : "",

                    // Convert time to UTC
                    CompleteDate = (item.completion_date != null && item.completion_date.Length > 0) ?
                                    TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(item.completion_date)).ToShortDateString() : "",

                    Status = item.status.name,
                };

                // Add filter on time issues.
                onTimeItemsList.Add(gridItems);
            }

        }
        catch (Exception ex)
        {
            int x = 1;
        }
    }

    /// <summary>
    /// filter On time issues grid based on dropdown(s) selected.
    /// </summary>
    /// <param name="filterProject"></param>
    /// <param name="filterType"></param>
    /// <param name="filterStatus"></param>
    private List<OntimeItems> filterOntimeIssueGrid(bool filterProject, bool filterType, bool filterStatus)
    {
        onTimeItemsList = new List<OntimeItems>();

        //Load all defects and incidents for user.
        buildOnTimeIssuesGrid(int.Parse(this.ddlContacts.SelectedValue));

        string selectedProj = null;

        //**********************************************************************
        //10/2013 - Do not use view project dropdown until further notice.
        //**********************************************************************

        //If project dropdown used - remnove "--->" from
        //project name in order to filter grid.
        //if (!ddlViewProject.SelectedItem.Text.Contains("ALL")) 
        //{
        //    selectedProj = ddlViewProject.SelectedItem.Text;
        //    selectedProj = selectedProj.Replace("--->", string.Empty);
        //}

        //Determine where clause depending on which dropdown(s) are used.
        // Using linq to create the new DataTable;  
        //if (filterProject & filterType & filterStatus)
        //{
        //    // project, type and status dropdowns used.
        //    var onTimeGridItems = (from i in onTimeItemsList
        //                           where i.OnTimeProject == selectedProj
        //                           && i.BacklogType == ddlBackLogType.SelectedItem.Text
        //                           && i.Status == ddlStatus.SelectedItem.Text
        //                           select i).ToList<OntimeItems>();

        //    return onTimeGridItems;
        //}
        //else if (filterProject & filterType & !filterStatus)
        //{
        //    // project abd type dropdowns used.
        //    var onTimeGridItems = (from i in onTimeItemsList
        //                           where i.OnTimeProject == selectedProj
        //                           && i.BacklogType == ddlBackLogType.SelectedItem.Text
        //                           select i).ToList<OntimeItems>();

        //     return onTimeGridItems;
        //}
        //else if (filterProject & !filterType & filterStatus)
        //{
        //    // project, status dropdown used.
        //    var onTimeGridItems = (from i in onTimeItemsList
        //                           where i.OnTimeProject == selectedProj
        //                           && i.Status == ddlStatus.SelectedItem.Text
        //                           select i).ToList<OntimeItems>();

        //     return onTimeGridItems;

        //}
        //else if (filterProject & !filterType & !filterStatus)
        //{
        //    // project dropdown used.
        //    var onTimeGridItems = (from i in onTimeItemsList
        //                           where i.OnTimeProject == selectedProj
        //                           select i).ToList<OntimeItems>();

        //     return onTimeGridItems;

        //}

        if (!filterProject & filterType & !filterStatus)
        {
            // type dropdown used.
            var onTimeGridItems = (from i in onTimeItemsList
                                   where
                                   i.BacklogType == ddlBackLogType.SelectedItem.Text
                                   select i).ToList<OntimeItems>();
            return onTimeGridItems;
        }
        else if (!filterProject & filterType & filterStatus)
        {
            // type and status dropdown used.
            var onTimeGridItems = (from i in onTimeItemsList
                                   where
                                   i.BacklogType == ddlBackLogType.SelectedItem.Text
                                   && i.Status == ddlStatus.SelectedItem.Text
                                   select i).ToList<OntimeItems>();

            return onTimeGridItems;
        }
        else if (!filterProject & !filterType & filterStatus)
        {
            // status dropdown used.
            var onTimeGridItems = (from i in onTimeItemsList
                                   where i.Status == ddlStatus.SelectedItem.Text
                                   select i).ToList<OntimeItems>();

            return onTimeGridItems;
        }

        return null;
    }

    //private void filterOntimeIssueGrid2(bool filterProject, bool filterType, bool filterStatus)
    //{
    //    onTimeItemsList = new List<OntimeItems>();

    //    buildOnTimeIssuesGrid(loginUserContactID);

    //    //Determine where clause depending on which dropdown(s) are used.
    //    if (filterProject & filterType & filterStatus)
    //    {
    //        // Using linq to create the new DataTable.
    //        var onTimeGridItems = itemsDT.AsEnumerable()
    //                              .Where(i => i.Field<string>("OnTimeProject") == ddlViewProject.SelectedItem.Text
    //                              && i.Field<string>("BacklogType") == ddlBackLogType.SelectedItem.Text
    //                              && i.Field<string>("Status") == ddlStatus.SelectedItem.Text);

    //        if (onTimeGridItems.Count() > 0)
    //        {
    //            // onTimeGridItems.CopyToDataTable();
    //            //grdItemsView.DataSource = itms.ToList();
    //            foreach (var i in onTimeGridItems)
    //            {
    //                // Create a On Time grid record.
    //                OntimeItems gridItems = new OntimeItems()
    //                {
    //                    ItemID = i.Field<string>("ItemID"),
    //                    OnTimeProject = i.Field<string>("OnTimeProject"),
    //                    BacklogType = i.Field<string>("BacklogType"),
    //                    Name = i.Field<string>("Name"),
    //                    WorkflowStep = i.Field<string>("WorkflowStep"),
    //                    AssignTo = i.Field<string>("AssignTo"),
    //                    ReportedDate = i.Field<string>("ReportedDate"),
    //                    CompleteDate = i.Field<string>("CompleteDate"),
    //                    Status = i.Field<string>("Status")
    //                };

    //                // Add filter on time issues.
    //                onTimeItemsList.Add(gridItems);
    //            }
    //        }
    //    }
    //    else if (filterProject & filterType & !filterStatus)
    //    {
    //        // Using linq to create the new DataTable.
    //        var onTimeGridItems = itemsDT.AsEnumerable()
    //                              .Where(i => i.Field<string>("OnTimeProject") == ddlViewProject.SelectedItem.Text
    //                              && i.Field<string>("BacklogType") == ddlBackLogType.SelectedItem.Text);

    //        if (onTimeGridItems.Count() > 0)
    //        {
    //            // onTimeGridItems.CopyToDataTable();
    //            //grdItemsView.DataSource = itms.ToList();
    //            foreach (var i in onTimeGridItems)
    //            {
    //                // Create a On Time grid record.
    //                OntimeItems gridItems = new OntimeItems()
    //                {
    //                    ItemID = i.Field<string>("ItemID"),
    //                    OnTimeProject = i.Field<string>("OnTimeProject"),
    //                    BacklogType = i.Field<string>("BacklogType"),
    //                    Name = i.Field<string>("Name"),
    //                    WorkflowStep = i.Field<string>("WorkflowStep"),
    //                    AssignTo = i.Field<string>("AssignTo"),
    //                    ReportedDate = i.Field<string>("ReportedDate"),
    //                    CompleteDate = i.Field<string>("CompleteDate"),
    //                    Status = i.Field<string>("Status")
    //                };

    //                // Add filter on time issues.
    //                onTimeItemsList.Add(gridItems);
    //            }
    //        }
    //    }
    //    else if (filterProject & !filterType & filterStatus)
    //    {
    //        // Using linq to create the new DataTable.
    //        var onTimeGridItems = itemsDT.AsEnumerable()
    //                              .Where(i => i.Field<string>("OnTimeProject") == ddlViewProject.SelectedItem.Text
    //                              && i.Field<string>("Status") == ddlStatus.SelectedItem.Text);

    //        if (onTimeGridItems.Count() > 0)
    //        {
    //            // onTimeGridItems.CopyToDataTable();
    //            //grdItemsView.DataSource = itms.ToList();
    //            foreach (var i in onTimeGridItems)
    //            {
    //                // Create a On Time grid record.
    //                OntimeItems gridItems = new OntimeItems()
    //                {
    //                    ItemID = i.Field<string>("ItemID"),
    //                    OnTimeProject = i.Field<string>("OnTimeProject"),
    //                    BacklogType = i.Field<string>("BacklogType"),
    //                    Name = i.Field<string>("Name"),
    //                    WorkflowStep = i.Field<string>("WorkflowStep"),
    //                    AssignTo = i.Field<string>("AssignTo"),
    //                    ReportedDate = i.Field<string>("ReportedDate"),
    //                    CompleteDate = i.Field<string>("CompleteDate"),
    //                    Status = i.Field<string>("Status")
    //                };

    //                // Add filter on time issues.
    //                onTimeItemsList.Add(gridItems);
    //            }
    //        }
    //    }
    //    else if (filterProject & !filterType & !filterStatus)
    //    {
    //        // Using linq to create the new DataTable.
    //        var onTimeGridItems = itemsDT.AsEnumerable()
    //                              .Where(i => i.Field<string>("OnTimeProject") == ddlViewProject.SelectedItem.Text);

    //        if (onTimeGridItems.Count() > 0)
    //        {
    //            // onTimeGridItems.CopyToDataTable();
    //            //grdItemsView.DataSource = itms.ToList();
    //            foreach (var i in onTimeGridItems)
    //            {
    //                // Create a On Time grid record.
    //                OntimeItems gridItems = new OntimeItems()
    //                {
    //                    ItemID = i.Field<string>("ItemID"),
    //                    OnTimeProject = i.Field<string>("OnTimeProject"),
    //                    BacklogType = i.Field<string>("BacklogType"),
    //                    Name = i.Field<string>("Name"),
    //                    WorkflowStep = i.Field<string>("WorkflowStep"),
    //                    AssignTo = i.Field<string>("AssignTo"),
    //                    ReportedDate = i.Field<string>("ReportedDate"),
    //                    CompleteDate = i.Field<string>("CompleteDate"),
    //                    Status = i.Field<string>("Status")
    //                };

    //                // Add filter on time issues.
    //                onTimeItemsList.Add(gridItems);
    //            }

    //        }
    //    }
    //    else if (!filterProject & filterType & !filterStatus)
    //    {
    //        // Using linq to create the new DataTable.
    //        var onTimeGridItems = itemsDT.AsEnumerable()
    //                              .Where(i => i.Field<string>("BacklogType") == ddlBackLogType.SelectedItem.Text);

    //        if (onTimeGridItems.Count() > 0)
    //        {
    //            // onTimeGridItems.CopyToDataTable();
    //            //grdItemsView.DataSource = itms.ToList();
    //            foreach (var i in onTimeGridItems)
    //            {
    //                // Create a On Time grid record.
    //                OntimeItems gridItems = new OntimeItems()
    //                {
    //                    ItemID = i.Field<string>("ItemID"),
    //                    OnTimeProject = i.Field<string>("OnTimeProject"),
    //                    BacklogType = i.Field<string>("BacklogType"),
    //                    Name = i.Field<string>("Name"),
    //                    WorkflowStep = i.Field<string>("WorkflowStep"),
    //                    AssignTo = i.Field<string>("AssignTo"),
    //                    ReportedDate = i.Field<string>("ReportedDate"),
    //                    CompleteDate = i.Field<string>("CompleteDate"),
    //                    Status = i.Field<string>("Status")
    //                };

    //                // Add filter on time issues.
    //                onTimeItemsList.Add(gridItems);
    //            }
    //        }

    //    }
    //    else if (!filterProject & filterType & filterStatus)
    //    {
    //        // Using linq to create the new DataTable.
    //        var onTimeGridItems = itemsDT.AsEnumerable()
    //                              .Where(i => i.Field<string>("BacklogType") == ddlBackLogType.SelectedItem.Text
    //                              && i.Field<string>("Status") == ddlStatus.SelectedItem.Text
    //                             );

    //        if (onTimeGridItems.Count() > 0)
    //        {
    //            // onTimeGridItems.CopyToDataTable();
    //            //grdItemsView.DataSource = itms.ToList();
    //            foreach (var i in onTimeGridItems)
    //            {
    //                // Create a On Time grid record.
    //                OntimeItems gridItems = new OntimeItems()
    //                {
    //                    ItemID = i.Field<string>("ItemID"),
    //                    OnTimeProject = i.Field<string>("OnTimeProject"),
    //                    BacklogType = i.Field<string>("BacklogType"),
    //                    Name = i.Field<string>("Name"),
    //                    WorkflowStep = i.Field<string>("WorkflowStep"),
    //                    AssignTo = i.Field<string>("AssignTo"),
    //                    ReportedDate = i.Field<string>("ReportedDate"),
    //                    CompleteDate = i.Field<string>("CompleteDate"),
    //                    Status = i.Field<string>("Status")
    //                };

    //                // Add filter on time issues.
    //                onTimeItemsList.Add(gridItems);
    //            }
    //        }

    //    }
    //    else if (!filterProject & !filterType & filterStatus)
    //    {
    //        // Using linq to create the new DataTable.
    //        var onTimeGridItems = itemsDT.AsEnumerable()
    //                              .Where(i => i.Field<string>("Status") == ddlStatus.SelectedItem.Text);

    //        if (onTimeGridItems.Count() > 0)
    //        {
    //            foreach (var i in onTimeGridItems)
    //            {
    //                // Create a On Time grid record.
    //                OntimeItems gridItems = new OntimeItems()
    //                {
    //                    ItemID = i.Field<string>("ItemID"),
    //                    OnTimeProject = i.Field<string>("OnTimeProject"),
    //                    BacklogType = i.Field<string>("BacklogType"),
    //                    Name = i.Field<string>("Name"),
    //                    WorkflowStep = i.Field<string>("WorkflowStep"),
    //                    AssignTo = i.Field<string>("AssignTo"),
    //                    ReportedDate = i.Field<string>("ReportedDate"),
    //                    CompleteDate = i.Field<string>("CompleteDate"),
    //                    Status = i.Field<string>("Status")
    //                };

    //                // Add filter on time issues.
    //                onTimeItemsList.Add(gridItems);
    //            }
    //        }
    //    }

    //    grdItemsView.DataSource = onTimeItemsList;
    //}

    #endregion

    #region Form Control Events/Methods

    protected void btnBack_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect(redirectURL);
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        this.txtTitle.Text = "";
        this.txtDescription.Text = "";
        this.txtReproStep.Text = "";
        this.txtNotes.Text = "";
        this.ddlProjects.SelectedIndex = 0;
        this.ddlSeverity.SelectedIndex = 0;
    }

    private bool passInputsValidation()
    {
        if (this.ddlProjects.SelectedItem.Text == "")
        {
            VisibleErrors("Project is required. Please select a project to add an Ontime item.");
            SetFocus(this.ddlProjects);
            return false;
        }
        if (this.txtTitle.Text.Trim().Length == 0)
        {
            VisibleErrors("Item Title is required!");
            SetFocus(this.txtTitle);
            return false;
        }

        if (this.txtDescription.Text.Trim().Length == 0)
        {
            VisibleErrors("Item Description is required!");
            SetFocus(this.txtDescription);
            return false;
        }

        return true;
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        try
        {
            if (!IsRefresh)
            {
                if (passInputsValidation())
                {
                    int id = InsertItem();

                    if (id > 0)
                    {
                        if (attachment1.HasFile)
                        {
                            // Only work for v.1
                            //Stream stream = attachment1.FileContent;
                            //using (var utf8EncodedStream = new CryptoStream(stream, new UTF8ByteEncoder(), CryptoStreamMode.Read))
                            //{
                            //    myOntimeObj.OnTime.Post<MessageResponse>(string.Format(resourceType + "/{0}/attachments", id), utf8EncodedStream, new Dictionary<string, object> {
                            //        { "file_name",  attachment1.FileName}
                            //    ,});
                            //}

                            using (Stream stream = attachment1.FileContent)
                            {
                                myOntimeObj.OnTime.Post<MessageResponse>(string.Format(resourceType + "/{0}/attachments", id), stream, new Dictionary<string, object> {
                                    { "file_name",  attachment1.FileName},}
                                );
                            }
                        }

                        if (attachment2.HasFile)
                        {
                            // Only work for v.1
                            //Stream stream = attachment2.FileContent;
                            //using (var utf8EncodedStream = new CryptoStream(stream, new UTF8ByteEncoder(), CryptoStreamMode.Read))
                            //{
                            //    myOntimeObj.OnTime.Post<MessageResponse>(string.Format(resourceType + "/{0}/attachments", id), utf8EncodedStream, new Dictionary<string, object> {
                            //    { "file_name",  attachment2.FileName},                                
                            //});
                            //}

                            using (Stream stream = attachment2.FileContent)
                            {
                                myOntimeObj.OnTime.Post<MessageResponse>(string.Format(resourceType + "/{0}/attachments", id), stream, new Dictionary<string, object> {
                                    { "file_name",  attachment2.FileName},}
                                );
                            }
                        }
                    }

                    //VisibleSuccess("Insert successful.");

                    btnReset_Click(sender, e);

                    // Display all items created by loginUser
                    DisplayAllOntimeItems();
                }
            }
        }
        catch (Exception ex)
        {
            VisibleErrors(ex.ToString());
        }

    }

    private int InsertItem()
    {
        int newDefectID = 0;

        try
        {
            if (myOntimeObj.OnTime.HasAccessToken())    // insert defects
            {
                var item = new Item();

                item.name = this.txtTitle.Text;
                item.project = new Project
                {
                    id = int.Parse(this.ddlProjects.SelectedValue)
                };
                item.severity = new Severity
                {
                    id = int.Parse(this.ddlSeverity.SelectedValue)
                };
                item.description = this.txtDescription.Text;
                item.replication_procedures = this.txtReproStep.Text;
                item.notes = this.txtNotes.Text;

                if (this.ddlContacts.SelectedItem.Text != "")
                {
                    item.reported_by_customer_contact = new Reported_By_Customer_Contact
                    {
                        id = int.Parse(this.ddlContacts.SelectedValue)
                    };
                }

                //get type of On time ticket to be created (based on radio button)
                this.resourceType = setResourceType();

                //if (this.radBacklogType.SelectedItem.Text == "Help Desk")
                //    this.resourceType = "v2/incidents";
                //else if (this.radBacklogType.SelectedItem.Text == "Defect")
                //    this.resourceType = "v2/defects";
                //else
                //    this.resourceType = "v2/features";

                var retObj = myOntimeObj.OnTime.Post<DataResponse<Item>>(resourceType, new { item = item });
                newDefectID = retObj.data.id;

            }
            // return newDefectID;
        }
        catch (Exception ex)
        {
            VisibleErrors(ex.ToString());
        }

        return newDefectID;
    }

    protected void lnkbtnReturn_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect(redirectURL);
    }

    /// <summary>
    /// Hidden delete button for developers only!
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        try
        {
            if (this.txtItemNo.Text.Trim().Length > 0)
            {
                //get type of On time ticket to be created (based on radio button)
                this.resourceType = setResourceType();

                //if (this.radBacklogType.SelectedItem.Text == "Help Desk")
                //    this.resourceType = "v2/incidents";
                //else if (this.radBacklogType.SelectedItem.Text == "Defect")
                //    this.resourceType = "v2/defects";
                //else
                //    this.resourceType = "v2/features";

                myOntimeObj.OnTime.Delete(resourceType, this.txtItemNo.Text.Trim());

                //VisibleSuccess("Delete successful.");

                // Display all items created by loginUser
                DisplayAllOntimeItems();
            }
            else
            {
                VisibleErrors("Please enter item number to delete.");
                SetFocus(this.txtItemNo);
            }
        }
        catch (Exception ex)
        {
            VisibleErrors(ex.ToString());
            // Display all items created by loginUser
            DisplayAllOntimeItems();
        }
    }

    //Main sorting method to sort issues grid.
    protected void grdItemsView_Sorting(object sender, GridViewSortEventArgs e)
    {
        bindIssueItemsGrid();
    }

    /// <summary>
    /// sort grid based on which column was selected and by ascending or descending.
    /// </summary>
    private void bindIssueItemsGrid()
    {
        // Display all items created by loginUser
        DisplayAllOntimeItems();

        var itms = (from o in onTimeItemsList
                    select o).OrderByDescending(o => o.ItemID).ToList();

        // Sort the list based on the column
        switch (grdItemsView.CurrentSortedColumn)
        {
            case "ItemID":
                // Sort the list by id
                itms.Sort(delegate(OntimeItems i1, OntimeItems i2) { return i1.ItemID.CompareTo(i2.ItemID); });
                break;

            case "OnTimeProject":
                // Sort the list by id
                itms.Sort(delegate(OntimeItems i1, OntimeItems i2) { return i1.OnTimeProject.CompareTo(i2.OnTimeProject); });
                break;

            case "BacklogType":
                // Sort the list by id
                itms.Sort(delegate(OntimeItems i1, OntimeItems i2) { return i1.BacklogType.CompareTo(i2.BacklogType); });
                break;

            case "Name":
                // Sort the list by id
                itms.Sort(delegate(OntimeItems i1, OntimeItems i2) { return i1.Name.CompareTo(i2.Name); });
                break;

            case "WorkflowStep":
                // Sort the list by id
                itms.Sort(delegate(OntimeItems i1, OntimeItems i2) { return i1.WorkflowStep.CompareTo(i2.WorkflowStep); });
                break;

            case "AssignTo":
                // Sort the list by id
                itms.Sort(delegate(OntimeItems i1, OntimeItems i2) { return i1.AssignTo.CompareTo(i2.AssignTo); });
                break;

            case "ReportedDate":
                // Sort the list by id
                itms.Sort(delegate(OntimeItems i1, OntimeItems i2) { return i1.ReportedDate.CompareTo(i2.ReportedDate); });
                break;

            case "CompleteDate":
                // Sort the list by id
                itms.Sort(delegate(OntimeItems i1, OntimeItems i2) { return i1.CompleteDate.CompareTo(i2.CompleteDate); });
                break;

            case "Status":
                // Sort the list by id
                itms.Sort(delegate(OntimeItems i1, OntimeItems i2) { return i1.Status.CompareTo(i2.Status); });
                break;

            //Code to soert DateTime fields - not used for this screen.
            //case "WearDate":
            //    // Sort the list by the last read date
            //    po.Sort(delegate(GPurchaseOrder i1, GPurchaseOrder i2)
            //    {
            //        return (i1.WearDate.HasValue) ? i1.WearDate.Value.CompareTo((i2.WearDate.HasValue) ? i2.WearDate.Value : DateTime.MinValue) : 0;
            //    });
            //    break;

            //case "EndDate":
            //    // Sort the list by the last read date
            //    po.Sort(delegate(GPurchaseOrder i1, GPurchaseOrder i2)
            //    {
            //        return (i1.EndDate.HasValue) ?
            //            i1.EndDate.Value.CompareTo((i2.EndDate.HasValue) ?
            //            i2.EndDate.Value : DateTime.MinValue) : 0;
            //    });
            //    break;
        }

        // Flip the list
        if (grdItemsView.CurrentSortDirection == SortDirection.Descending) itms.Reverse();

        // Display the results to the gridview.
        grdItemsView.DataSource = itms;
        grdItemsView.DataBind();
    }

    protected void ddlProject_SelectedIndexChanged(object sender, EventArgs e)
    {
        //if view projects dropdown is not loaded; re-load it.
        //if (ddlViewProject.Items.Count == 0) loadProjectFilterDropdown();

        // Display all or filtered items created by loginUser
        DisplayAllOntimeItems();
    }

    protected void ddlBackLogType_SelectedIndexChanged(object sender, EventArgs e)
    {
        //if view projects dropdown is not loaded; re-load it.
        //if (ddlViewProject.Items.Count == 0) loadProjectFilterDropdown();

        // Display all or filtered items created by loginUser
        DisplayAllOntimeItems();
    }

    protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
    {
        //if view projects dropdown is not loaded; re-load it.
        //if (ddlViewProject.Items.Count == 0) loadProjectFilterDropdown();

        // Display all or filtered items created by loginUser
        DisplayAllOntimeItems();
    }

    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        //try
        //{
        //    loadAllControls();

        //    ddlViewProject.SelectedIndex = 0;
        //    ddlBackLogType.SelectedIndex = 0;
        //    ddlStatus.SelectedIndex = 0;
        //}
        //catch
        //{
        //    //if exception or error thrown then re-load all dropdowns
        //    //and reset the view/filter grid dropdowns.
        //    loadAllControls();

        //    ddlViewProject.SelectedIndex = 0;
        //    ddlBackLogType.SelectedIndex = 0;
        //    ddlStatus.SelectedIndex = 0;
        //}

        if (loadAllControls())
        {
            //ddlViewProject.SelectedIndex = 0;
            ddlBackLogType.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;

            // Display all items created by loginUser
            DisplayAllOntimeItems();

            InvisibleErrors();
        }
    }

    protected void radBacklogType_SelectedIndexChanged(object sender, EventArgs e)
    {
        //get type of On time ticket to be created (based on radio button)
        this.resourceType = setResourceType();
    }

    #endregion

    #region Miscellaneous Functions/Methods

    private void InvisibleErrors()
    {
        // Reset submission form error message      
        this.dialogErrorMsg.InnerText = "";
        this.dialogErrors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.dialogErrorMsg.InnerText = error;
        this.dialogErrors.Visible = true;
    }

    private void InvisibleSuccess()
    {
        // Reset submission form error message      
        this.dialogSuccessMsg.InnerText = "";
        this.dialogSuccess.Visible = false;
    }

    private void VisibleSuccess(string error)
    {
        this.dialogSuccessMsg.InnerText = error;
        this.dialogSuccess.Visible = true;
    }

    #endregion


}