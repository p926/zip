using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using Instadose.Integration;
using Instadose.Processing;

public partial class InstaDose_Admin_Renewal_Renewals : System.Web.UI.Page
{
    // Create the database LINQ objects.
    InsDataContext idc = new InsDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        // redirect to new renewal page
        Response.Redirect("RenewalProcess.aspx", true);
        // Prevent the page from redrawing the table if not a first time load.
        if (!IsPostBack)
        {
            //Turn Error msg off.
            this.renewalRowError.Visible = false;

            bindBillingTermsCombo();
            bindBillingMethodCombo();
            bindBrandNameCombo();

            if (Request.QueryString["AccountID"] != null)
            {
                int accountID = 0;
                int.TryParse(Request.QueryString["AccountID"], out accountID);
                
                // Skip if the account is not passed.
                if (accountID <= 0) return;

                GridViewCommandEventArgs evnt = new GridViewCommandEventArgs(null, null, new CommandEventArgs("Renew", accountID));
                gvUpcomingRenewals_RowCommand(null, evnt);
            }

            // Bind the grid
            bindRenewalGrid();  
        }

    }

    private void bindBillingMethodCombo()
    {

        this.ddlBillingMethod.DataSource = (from idcBillingMethod in idc.PaymentMethods
                                            select new
                                            {
                                                DESC = idcBillingMethod.PaymentMethodID,
                                                ID = idcBillingMethod.PaymentMethodName
                                            }).ToList();

        this.ddlBillingMethod.DataTextField = "ID";
        this.ddlBillingMethod.DataValueField = "DESC";

        this.ddlBillingMethod.DataBind();
        ddlBillingMethod.Items.Insert(0, new ListItem("ALL", ""));
    }

    private void bindBillingTermsCombo()
    {

        this.ddlBillingTerm.DataSource = (from idcBillingMethod in idc.BillingTerms
                                          select new
                                          {
                                              DESC = idcBillingMethod.BillingTermID,
                                              ID = idcBillingMethod.BillingTermDesc
                                          }).ToList();

        this.ddlBillingTerm.DataTextField = "ID";
        this.ddlBillingTerm.DataValueField = "DESC";

        this.ddlBillingTerm.DataBind();
        ddlBillingTerm.Items.Insert(0, new ListItem("ALL", ""));
    }

    private void bindBrandNameCombo()
    {

        this.ddlBrand.DataSource = (from idcBillingMethod in idc.BrandSources
                                    select new
                                    {
                                        DESC = idcBillingMethod.BrandSourceID,
                                        ID = idcBillingMethod.BrandName
                                    }).ToList();
        this.ddlBrand.DataTextField = "ID";
        this.ddlBrand.DataValueField = "DESC";
        this.ddlBrand.DataBind();
        ddlBrand.Items.Insert(0, new ListItem("ALL", ""));
    }
    /// <summary>
    /// When the text is change in the from period textbox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtPeriodFrom_TextChange(object sender, EventArgs e)
    {
        // Clear the style.
        txtPeriodFrom.CssClass = "";

        // Exit if there is no text.
        if (txtPeriodFrom.Text == "") return;

        // Test the see if the date is valid
        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodFrom.Text, out fromDate))
        {
            txtPeriodFrom.CssClass = "invalidText";
            return;
        }

        // Test to ensure the date is less than To Period
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodTo.Text, out toDate)) return;

        // Ensure the from date is less than to date
        if (fromDate > toDate)
        {
            txtPeriodFrom.CssClass = "invalidText";
            return;
        }

        // Reset the page to 0
        gvUpcomingRenewals.PageIndex = 0;

        // Rebind the renewals grid.
        bindRenewalGrid();
    }

    /// <summary>
    /// When the text is change in the to period textbox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtPeriodTo_TextChange(object sender, EventArgs e)
    {
        // Clear the style.
        txtPeriodTo.CssClass = "";

        // Exit if there is no text.
        if (txtPeriodTo.Text == "") return;

        // Test the see if the date is valid
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodTo.Text, out toDate))
        {
            txtPeriodTo.CssClass = "invalidText";
            return;
        }

        // Test to ensure the date is less than From Period
        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodFrom.Text, out fromDate)) return;

        // Ensure the from date is less than to date
        if (fromDate > toDate)
        {
            txtPeriodTo.CssClass = "invalidText";
            return;
        }

        // Reset the page to 0
        gvUpcomingRenewals.PageIndex = 0;

        // Rebind the renewals grid.
        bindRenewalGrid();
    }

    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvUpcomingRenewals_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvUpcomingRenewals.PageIndex = e.NewPageIndex;
        bindRenewalGrid();
    }

    /// <summary>
    /// Fired when a grid view command is fired like the renewals button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvUpcomingRenewals_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        //Turn Error msg off.
        this.renewalRowError.Visible = false;

        // Ensure the command coming in is the renew command
        if (e.CommandName != "Renew") return;

        // Convert the argument to the account ID
        int accountID = int.Parse(e.CommandArgument.ToString());

        // Generate the renewal screen.
        Renewals renewal = new Renewals()
        {
            UserName = ActiveDirectoryQueries.GetUserName()
        };

        // Show the correct panel
        hfAccountID.Value = accountID.ToString();
        pnlReview.Visible = true;
        pnlRenewalsTable.Visible = false;
        lblCreditCard.Visible = false;
        btnGenerate.Enabled = true;
        
        // Get the review screen for the renewal and display
        gvReview.DataSource = renewal.ReviewRenewal(accountID, null); ;
        gvReview.DataBind();

        // Set the renewal total
        lblRenewalTotal.Text = String.Format("{0:C}", renewal.GetRenewalTotalPrice(accountID, null));
        
        // Ensure the credit card is on file.
        Account account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

        // Update the billing method label.
        lblBillingMethod.Text = "This account is going to be billed as a " + account.BillingMethod + ".";

        // If the billing method is by credit card make sure the accounts CC is on file.
        if (account.BillingMethod == "Credit Card")
        {
            int cardCount = (from acc in account.AccountCreditCards where acc.Active == true select acc).Count();
            if (cardCount == 0)
            {
                lblCreditCard.Visible = true;
                btnGenerate.Enabled = false;
            }
        }
    }

    /// <summary>
    /// Generate the renewal.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnGenerate_Click(object sender, EventArgs e)
    {
        // Generate the renewal
        int accountID = int.Parse(hfAccountID.Value);
        Renewals renewal = new Renewals()
        {
            UserName = ActiveDirectoryQueries.GetUserName()
        };
        
        var account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

        // Grab the status after the renewal has been made.
        var status = renewal.CreateRenewal(accountID, null);
        if (account.BrandSourceID == 3)
        {
            //@todo Perform Dealer PO Generation
        }

        lblProcessingErrors.Text = "The following errors occurred from PayPal: <br />\r\n";
        lblProcessingErrors.Visible = false;
        pnlReview.Visible = false;
        pnlCompleted.Visible = true;

        // If softrax was sent, then all finished.
        this.respMessage.Controls.Clear();
        if (status.SentToSoftrax == true)
        {
            this.respMessage.Controls.Add(
                new LiteralControl("The renewal process completed."));
        }
        else
        {
            this.respMessage.Controls.Add(
                new LiteralControl("The renewal process has failed."));
        }

        // Order message.
        if (status.OrderID > 0)
        {
            this.respMessage2.Controls.Add(
                new LiteralControl("Order #: " + status.OrderID.ToString() + " was generated."));
        }
        // Orders action item
        if (status.GeneratedOrder)
        {
            this.respOrder.CssClass = "response_accept";
            this.respOrder.Controls.Clear();
            this.respOrder.Controls.Add(new LiteralControl("The order was created successfully."));
        }

        // Account has been updated.
        if (status.UpdateAccountDeviceDate)
        {
            this.respUpdateAccount.CssClass = "response_accept";
            this.respUpdateAccount.Controls.Clear();
            this.respUpdateAccount.Controls.Add(new LiteralControl("The account contract and billings have been updated."));
        }

        // If an IC Care renewal
        if (account.BrandSourceID == 3)
        {
            // @todo generate PO Request...


            // Hide
            this.respPORequest.Visible = true;
            this.respPayment.Visible = false;
            this.respMAS.Visible = false;
            this.respSoftrax.Visible = false;
        }
        else // IF A STANDARD RENEWAL
        {
            // Hide
            this.respPORequest.Visible = false;
            this.respPayment.Visible = true;
            this.respMAS.Visible = true;
            this.respSoftrax.Visible = true;

            // Payment action item
            if (status.PaymentProcessed == true)
            {
                this.respPayment.CssClass = "response_accept";
                this.respPayment.Controls.Clear();
                this.respPayment.Controls.Add(new LiteralControl("The payment has been processed."));
            }

            // MAS action item
            if (status.SentToMAS == true)
            {
                this.respMAS.CssClass = "response_accept";
                this.respMAS.Controls.Clear();
                this.respMAS.Controls.Add(new LiteralControl("The order was sent to MAS."));
            }
            // Softrax item
            if (status.SentToSoftrax == true)
            {
                this.respSoftrax.CssClass = "response_accept";
                this.respSoftrax.Controls.Clear();
                this.respSoftrax.Controls.Add(new LiteralControl("The order was sent to Softrax."));
            }
        }

    }

    /// <summary>
    /// Hides the review and completed panels and displays the renewals table.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnBack_Click(object sender, EventArgs e)
    {
        pnlCompleted.Visible = false;
        pnlRenewalsTable.Visible = true;
        pnlReview.Visible = false;

        // Force the screen to redraw.
        bindRenewalGrid();
    }

    /// <summary>
    /// Called from many objects, force binds the gridview.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gridviewBinder(object sender, EventArgs e)
    {
        // Reset the page to 0
        gvUpcomingRenewals.PageIndex = 0;

        // Bind the gridview
        bindRenewalGrid();
    }

    /// <summary>
    /// When the column is being sorted.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvUpcomingRenewals_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvUpcomingRenewals.PageIndex = 0;
        bindRenewalGrid();
    }

    /// <summary>
    /// Query the data source and bind the data grid.
    /// </summary>
    private void bindRenewalGrid()
    {
        // Create the sql connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the sql command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.Connection = sqlConn;
        sqlCmd.CommandType = CommandType.Text;

        // Create the query for the view.
        string query = "SELECT * FROM [vw_UpcomingRenewals]";
        string filters = "";
        
        // Set the filter parameter for billing method
        if (Convert.ToString(ddlBillingMethod.SelectedItem) != "ALL")
        {
            filters += "([BillingMethod] = @billingMethod)";
            sqlCmd.Parameters.Add(new SqlParameter("billingMethod", Convert.ToString(ddlBillingMethod.SelectedItem)));
        }

        // Set the filter parameter for billing term
        if (Convert.ToString(ddlBillingTerm.SelectedItem) !=  "ALL")
        {
            // Append the AND if needed.
            if (filters != "") filters += " AND ";

            filters += "([BillingTerm] = @billingTerm)";
            sqlCmd.Parameters.Add(new SqlParameter("billingTerm", Convert.ToString(ddlBillingTerm.SelectedItem)));
        }

        // Set the filter parameter for the brand
        if (Convert.ToString(ddlBrand.SelectedItem) !=  "ALL")
        {
            // Append the AND if needed.
            if (filters != "") filters += " AND ";

            filters += "([BrandName] = @brandName)";
            sqlCmd.Parameters.Add(new SqlParameter("brandName", Convert.ToString(ddlBrand.SelectedItem)));
        }

        // Period From and To
        if (txtPeriodFrom.Text != "" && txtPeriodTo.Text != "")
        {
            DateTime dtPeriodFrom = DateTime.Now;
            DateTime dtPeriodTo = DateTime.Now;
            
            // Ensure the date time stuff can be parsed.
            if(DateTime.TryParse(txtPeriodFrom.Text, out dtPeriodFrom) && DateTime.TryParse(txtPeriodTo.Text, out dtPeriodTo))
            {
                if(dtPeriodFrom <= dtPeriodTo)
                {
                    // Append the AND if needed.
                    if (filters != "") filters += " AND ";

                    filters += "([ContractEndDate] BETWEEN @periodFrom AND @periodTo)";
                    sqlCmd.Parameters.Add(new SqlParameter("periodFrom", dtPeriodFrom));
                    sqlCmd.Parameters.Add(new SqlParameter("periodTo", dtPeriodTo));
                }
                else
                {
                    // From date must be BEFORE the to date.
                }
            }
            else
            {
                // Dates are not in the correct format.
            }
        }

        // Append the AND if needed.
        
        // Add the filters to the query if needed.
        if (filters != "") query += " WHERE " + filters;

        // Add the order by and the direction
        query += " ORDER BY " + gvUpcomingRenewals.CurrentSortedColumn + ((gvUpcomingRenewals.CurrentSortDirection == SortDirection.Ascending) ? " ASC" : " DESC");

        // Pass the query string to the command
        sqlCmd.CommandText = query;


        // Create a data table to place the results
        DataTable results = new DataTable();

        // Retrieve the data from SQL
        using (SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd))
        {
            sqlDA.Fill(results);
        }

        // Bind the results to the gridview
        gvUpcomingRenewals.DataSource = results;
        gvUpcomingRenewals.DataBind();
    }

}