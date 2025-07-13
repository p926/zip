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

public partial class IT_CreateOntimeItemTester : System.Web.UI.Page
{
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
    
    private string resourceDefinition;
    private int loginUserContactID = -1;   // No one
    private string loginUser = "";

    protected void Page_Unload(object sender, EventArgs e)
    {
        try
        {
            if (myOntimeObj.OnTime != null && myOntimeObj.OnTime.HasAccessToken())
                myOntimeObj.OnTime.Get<MessageResponse>("oauth2/revoke");
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch { }
    }

    protected void Page_Load(object sender, EventArgs e)
    {       
        if (!this.IsPostBack)
        {
            loginUser = User.Identity.Name.Split('\\')[1];

            LoadAllControls();            
            
            // If loginUser is among Customer Contacts then allowed to use this page to submit Ontime item, 
            // else redirect to Default page
            if (loginUserContactID > -1)
                this.ddlContacts.SelectedValue = loginUserContactID.ToString(); 
            else
                Page.Response.Redirect("Default.aspx");


            if (loginUser == "tdo")
            {
                this.deleteSection.Visible = true;
                this.btnDelete.Visible = true;
            }
            else
            {
                this.deleteSection.Visible = false;
                this.btnDelete.Visible = false;
            }            
        }
        InvisibleErrors();
        InvisibleSuccess();

        if (this.radBacklogType.SelectedItem.Text == "Defect")
            this.resourceDefinition = "v1/defects";
        else
            this.resourceDefinition = "v1/incidents";

    }

    private void LoadAllControls()
    {
        GetProjects();
        this.ddlProjects.DataSource = AuthorizedProjects;
        this.ddlProjects.DataValueField = "id";
        this.ddlProjects.DataTextField = "name";
        this.ddlProjects.DataBind();        

        GetContacts();
        this.ddlContacts.DataSource = Contacts;
        this.ddlContacts.DataValueField = "id";
        this.ddlContacts.DataTextField = "name";
        this.ddlContacts.DataBind();

        ListItem item1 = new ListItem("", "");
        this.ddlContacts.Items.Insert(0, item1);
    }

    private void GetProjects()
    {
        var result = myOntimeObj.OnTime.Get<DataResponse<List<ParentProject>>>("v1/projects");

        Projects.Clear();
        AuthorizedProjects.Clear();

        foreach (var project in result.data)
        {
            string parentName = "";
            populateProjects(project, parentName);
        }  
        
        // Filtering out the Projects list. For this time being, only "1-InBox" and its sub-projects are authorized by all Window Authenticate users
        foreach (Project proj in Projects)
        {
            if (proj.name.Contains("1 - InBox"))
            {
                AuthorizedProjects.Add(proj);
            }
        }  
    }

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

    private void GetContacts()
    {
        var result = myOntimeObj.OnTime.Get<DataResponse<List<Reported_By_Customer_Contact>>>("v1/contacts");

        Contacts.Clear();
        foreach (var contact in result.data)
        {
            Contacts.Add(contact);

            // Looking for ContactID of window authenticate login user          
            string username = contact.email.Split('@')[0];
            if (username == loginUser)
            {
                this.loginUserContactID = contact.id;
            }       
        }            
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        this.txtTitle.Text = "";
        this.txtDescription.Text = "";
        this.txtReproStep.Text = "";
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
        if (this.txtTitle.Text.Trim().Length == 0 )
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
                            Stream stream = attachment1.FileContent;
                            using (var utf8EncodedStream = new CryptoStream(stream, new UTF8ByteEncoder(), CryptoStreamMode.Read))
                            {
                                myOntimeObj.OnTime.Post<MessageResponse>(string.Format(resourceDefinition + "/{0}/attachments", id), utf8EncodedStream, new Dictionary<string, object> {
                                { "file_name",  attachment1.FileName},
                            });
                            }
                        }

                        if (attachment2.HasFile)
                        {
                            Stream stream = attachment2.FileContent;
                            using (var utf8EncodedStream = new CryptoStream(stream, new UTF8ByteEncoder(), CryptoStreamMode.Read))
                            {
                                myOntimeObj.OnTime.Post<MessageResponse>(string.Format(resourceDefinition + "/{0}/attachments", id), utf8EncodedStream, new Dictionary<string, object> {
                                { "file_name",  attachment2.FileName},
                            });
                            }
                        }
                    }

                    VisibleSuccess("Insert successful.");

                    btnReset_Click(sender, e);
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
        try
        {
            int newDefectID = 0;

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

                if (this.ddlContacts.SelectedItem.Text != "")
                {
                    item.reported_by_customer_contact = new Reported_By_Customer_Contact
                    {
                        id = int.Parse(this.ddlContacts.SelectedValue)
                    };
                }

                var retObj = myOntimeObj.OnTime.Post<DataResponse<Item>>(resourceDefinition, new { item = item });
                newDefectID = retObj.data.id;
               
            }
            return newDefectID;
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        try
        {
            if (this.txtItemNo.Text.Trim().Length > 0)
            {
                myOntimeObj.OnTime.Delete(resourceDefinition, this.txtItemNo.Text.Trim());

                VisibleSuccess("Delete successful.");
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
        }
    }

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
    
}