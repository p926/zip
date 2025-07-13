using Instadose.Data;
using Instadose.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace portal_instadose_com_v3.Finance.Renewal
{
	public partial class RenewalProcess : System.Web.UI.Page
	{
		InsDataContext idc = new InsDataContext();
		
		#region Events
		protected void Page_Load(object sender, EventArgs e)
		{
			pnErrorMessage.Visible = false;
			pnSuccessMessages.Visible = false;

			if (!IsPostBack)
			{
				Session["RenewalBatches"] = new List<vw_UpcomingRenewalsBatch>();
				Session["RenewalBatchAccountIDs"] = new List<int>();
				Session["sortDirection"] = SortDirection.Ascending;
				
				this.LoadDropDowns();
			}
		}

		protected void lnkbtnDisplay_Click(object sender, EventArgs e)
		{
			List<vw_UpcomingRenewalsBatch> renewals = this.GetUpcomingRenewalList();
			Session["RenewalBatches"] = renewals;
			Session["RenewalBatchAccountIDs"] = renewals.Select(r => r.AccountID).ToList();

			gvUpcomingRenewals.PageIndex = 0;
			gvUpcomingRenewals.DataSource = Session["RenewalBatches"];
			gvUpcomingRenewals.DataBind();
		}

		protected void lnkClear_Click(object sender, EventArgs e)
		{
			this.ClearFilterInputs();

			Session["RenewalBatches"] = new List<vw_UpcomingRenewalsBatch>();
			Session["RenewalBatchAccountIDs"] = new List<int>();

			gvUpcomingRenewals.DataSource = new List<vw_UpcomingRenewalsBatch>();
			gvUpcomingRenewals.DataBind();
		}

		protected void gvUpcomingRenewals_Sorting(object sender, GridViewSortEventArgs e)
		{
			this.SaveCheckedAccountIDsToSession();

			if ((SortDirection)Session["sortDirection"] == SortDirection.Ascending)
			{
				e.SortDirection = SortDirection.Ascending;
				Session["sortDirection"] = SortDirection.Descending;
			}
			else
			{
				e.SortDirection = SortDirection.Descending;
				Session["sortDirection"] = SortDirection.Ascending;
			}

			List<vw_UpcomingRenewalsBatch> renewals = Session["RenewalBatches"] as List<vw_UpcomingRenewalsBatch>;

			if (renewals != null && renewals.Count > 0)
			{
				var pInfo = typeof(vw_UpcomingRenewalsBatch).GetProperty(e.SortExpression);

				if (e.SortDirection == SortDirection.Ascending)
					renewals = renewals.OrderBy(r => pInfo.GetValue(r, null)).ToList();
				else
					renewals = renewals.OrderByDescending(r => pInfo.GetValue(r, null)).ToList();
			}

			Session["RenewalBatches"] = renewals;

			gvUpcomingRenewals.PageIndex = 0;
			gvUpcomingRenewals.DataSource = Session["RenewalBatches"];
			gvUpcomingRenewals.DataBind();
		}

		protected void gvUpcomingRenewals_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			this.SaveCheckedAccountIDsToSession();

			gvUpcomingRenewals.PageIndex = e.NewPageIndex;
			gvUpcomingRenewals.DataSource = Session["RenewalBatches"];
			gvUpcomingRenewals.DataBind();
		}

		protected void gvUpcomingRenewals_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				List<int> accountIDs = Session["RenewalBatchAccountIDs"] as List<int>;
				vw_UpcomingRenewalsBatch upcomingRenewal = e.Row.DataItem as vw_UpcomingRenewalsBatch;

				if (upcomingRenewal != null)
				{
					CheckBox chkAccountID = e.Row.FindControl("chkAccountID") as CheckBox;
					Literal ltContractEndDate = e.Row.FindControl("ltContractEndDate") as Literal;

					chkAccountID.Checked = accountIDs.Contains(upcomingRenewal.AccountID);

					ltContractEndDate.Text = upcomingRenewal.ContractEndDate != null ? ((DateTime)upcomingRenewal.ContractEndDate).ToString("d") : string.Empty;
				}
			}
		}

		protected void btnGenerate_Click(object sender, EventArgs e)
		{
			pnErrorMessage.Visible = false;
			pnSuccessMessages.Visible = false;

			this.SaveCheckedAccountIDsToSession();

			//List<int> accountIDs = this.GetUpcomingRenewalAccountIDs();
			//List<int> accountIDs = this.GetAccountIDsByGrid();
			List<int> accountIDs = Session["RenewalBatchAccountIDs"] as List<int>;

			if (accountIDs == null || accountIDs.Count <= 0)
			{
				this.ShowError("Account(s) is(are) already in Renewal Process.");
				return;
			}

			RenewalByThread renewals = new RenewalByThread(ActiveDirectoryQueries.GetUserName());
			
			string renewalLogNo = renewals.GenerateRenewalNo();

			if (string.IsNullOrEmpty(renewalLogNo))
			{
				this.ShowError("Error occured while creating Renewal Number.");
				return;
			}

			int renewalThreadLogID = renewals.InsertInitialRenewalThreadLog(renewalLogNo);
			
			if (renewalThreadLogID <= 0)
			{
				this.ShowError("Renewal Thread Log is not added.");
				return;
			}

			renewals.InsertInitialRenewalThreadLogDetails(renewalThreadLogID, accountIDs);

			Thread trd = new Thread(() =>
			{
				string errMsg = string.Empty;

				try
				{
					renewals.ProcessRenewals(accountIDs, renewalThreadLogID, renewalLogNo);
				}
				catch (ThreadAbortException taex)
				{
					errMsg = taex.Message;
				}
				catch (Exception ex)
				{
					errMsg = ex.Message;
				}

				renewals.UpdateCompleteRenewalThreadLog(renewalThreadLogID, errMsg);
			});

			trd.IsBackground = true;
			trd.Start();

			this.ShowSuccess(string.Format("Renewal Number is <string>{0}</string>. Renewal Process may take several minutes. Plese check <a href='/Finance/Renewal/ThreadLog.aspx?no={0}'>Renewal Progress page</a> after few minutes.", renewalLogNo));

			List<vw_UpcomingRenewalsBatch> upcomingRenewals = this.GetUpcomingRenewalList();
			Session["RenewalBatches"] = upcomingRenewals;
			Session["RenewalBatchAccountIDs"] = upcomingRenewals.Select(r => r.AccountID).ToList();

			gvUpcomingRenewals.PageIndex = 0;
			gvUpcomingRenewals.DataSource = Session["RenewalBatches"];
			gvUpcomingRenewals.DataBind();
		}
		#endregion

		#region Helper Methods
		private void ShowError(string errorMessage)
		{
			pnSuccessMessages.Visible = false;
			pnErrorMessage.Visible = true;
			lblError.Text = errorMessage;
		}

		private void ShowSuccess(string successMessage)
		{
			pnErrorMessage.Visible = false;
			pnSuccessMessages.Visible = true;
			lblSuccess.Text = successMessage;
		}

		private void LoadDropDowns()
		{
			ddlBillingMethod.DataSource = idc.PaymentMethods.Select(p => new { p.PaymentMethodID, p.PaymentMethodName }).ToList();
			ddlBillingMethod.DataBind();
			ddlBillingMethod.Items.Insert(0, new ListItem("All", string.Empty));

			ddlBillingTerm.DataSource = idc.BillingTerms.Select(b => new { b.BillingTermID, b.BillingTermDesc }).ToList();
			ddlBillingTerm.DataBind();
			ddlBillingTerm.Items.Insert(0, new ListItem("All", string.Empty));

			ddlBrand.DataSource = idc.BrandSources.Select(bs => new { bs.BrandName }).ToList();
			ddlBrand.DataBind();
			ddlBrand.Items.Insert(0, new ListItem("All", string.Empty));

			ddlCustomerType.DataSource = idc.CustomerTypes.OrderBy(ct => ct.CustomerTypeDesc).Select(ct => new { ct.CustomerTypeID, CustomerTypeName = string.Format("{0} ({1})", ct.CustomerTypeDesc, ct.CustomerTypeCode) });
			ddlCustomerType.DataBind();
			ddlCustomerType.Items.Insert(0, new ListItem("All", string.Empty));
		}

		private void ClearFilterInputs()
		{
			ddlBillingMethod.SelectedIndex = 0;
			ddlBillingTerm.SelectedIndex = 0;
			ddlBrand.SelectedIndex = 0;
			ddlCustomerType.SelectedIndex = 0;

			txtPeriodFrom.Text = string.Empty;
			txtPeriodTo.Text = string.Empty;
		}

		private List<vw_UpcomingRenewalsBatch> GetUpcomingRenewalList()
		{
			IQueryable<vw_UpcomingRenewalsBatch> upcomingRenewals = idc.vw_UpcomingRenewalsBatches;

			upcomingRenewals = upcomingRenewals.Where(ur => ur.RenewalLogCreated != true);

			if (ddlBillingMethod.SelectedItem != null && ddlBillingMethod.SelectedItem.Text != "All")
				upcomingRenewals = upcomingRenewals.Where(ur => ur.BillingMethod == ddlBillingMethod.SelectedItem.Text);

			if (ddlBillingTerm.SelectedItem != null && ddlBillingTerm.SelectedItem.Text != "All")
				upcomingRenewals = upcomingRenewals.Where(ur => ur.BillingTerm == ddlBillingTerm.SelectedItem.Text);

			if (!string.IsNullOrEmpty(ddlCustomerType.SelectedValue))
				upcomingRenewals = upcomingRenewals.Where(ur => ur.CustomerTypeID == int.Parse(ddlCustomerType.SelectedValue));

			if (ddlBrand.SelectedItem != null && ddlBrand.SelectedItem.Text != "All")
				upcomingRenewals = upcomingRenewals.Where(ur => ur.BrandName == ddlBrand.SelectedItem.Text);

			if (!string.IsNullOrEmpty(txtPeriodFrom.Text) && !string.IsNullOrEmpty(txtPeriodTo.Text))
			{
				DateTime tmpFromDate;
				DateTime tmpToDate;

				if (DateTime.TryParse(txtPeriodFrom.Text, out tmpFromDate) && DateTime.TryParse(txtPeriodTo.Text, out tmpToDate))
					upcomingRenewals = upcomingRenewals.Where(ur => ur.RenewalDate >= new DateTime(tmpFromDate.Year, tmpFromDate.Month, tmpFromDate.Day, 0, 0, 0) && ur.RenewalDate <= new DateTime(tmpToDate.Year, tmpToDate.Month, tmpToDate.Day, 23, 59, 59));
			}

			// Get AccountIDs which is processing
			List<int> processingAccountIDs = idc.RenewalThreadLogDetails.Where(rtld => !rtld.RenewalThreadLog.Complete && rtld.RenewalLogs == null).Select(rtld => rtld.AccountID).ToList();
			
			if (processingAccountIDs != null && processingAccountIDs.Count > 0)
				upcomingRenewals = upcomingRenewals.Where(ur => !processingAccountIDs.Contains(ur.AccountID));

			// Exception Code PP - No Exception is found. Renewal Code U - Renewal can be processed
			return upcomingRenewals.Where(ur => ur.RenewalExceptionCode == "PP" && ur.RenewalCode == "U").OrderBy(ur => ur.RenewalDate).ThenBy(ur => ur.AccountID).ToList();
		}

		private List<int> GetUpcomingRenewalAccountIDs()
		{
			//List<vw_UpcomingRenewalsBatch> upcomingRenewals = this.GetUpcomingRenewalList();
			List<vw_UpcomingRenewalsBatch> upcomingRenewals = Session["RenewalBatches"] as List<vw_UpcomingRenewalsBatch>;

			if (upcomingRenewals == null || upcomingRenewals.Count <= 0)
				return null;
			
			List<int> accountIDs = new List<int>();

			foreach (vw_UpcomingRenewalsBatch renewal in upcomingRenewals)
			{
				if (renewal.ContractEndDate == null)
					continue;

				if (!this.IsRenewalLogExist(renewal.AccountID, (DateTime)renewal.ContractEndDate))
					accountIDs.Add(renewal.AccountID);
			}

			return accountIDs;
		}

		private List<int> GetAccountIDsByGrid()
		{
			List<int> accountIDs = new List<int>();

			foreach (GridViewRow row in gvUpcomingRenewals.Rows)
			{
				HiddenField hfAccountID = row.FindControl("hfAccountID") as HiddenField;
				CheckBox chkAccountID = row.FindControl("chkAccountID") as CheckBox;
				Literal ltContractEndDate = row.FindControl("ltContractEndDate") as Literal;

				int accountID = 0;

				if (chkAccountID.Checked)
				{
					if (int.TryParse(hfAccountID.Value, out accountID))
					{
						DateTime contractEndDate;

						if (DateTime.TryParse(ltContractEndDate.Text, out contractEndDate))
						{
							if (!this.IsRenewalLogExist(accountID, contractEndDate))
								accountIDs.Add(accountID);
						}
					}
				}
			}

			return accountIDs;
		}

		private void SaveCheckedAccountIDsToSession()
		{
			// Store checked Renewal Account ID into Session
			List<int> accountIDs = Session["RenewalBatchAccountIDs"] as List<int>;

			foreach (GridViewRow row in gvUpcomingRenewals.Rows)
			{
				HiddenField hfAccountID = row.FindControl("hfAccountID") as HiddenField;
				CheckBox chkAccountID = row.FindControl("chkAccountID") as CheckBox;

				int accountID = 0;
				if (int.TryParse(hfAccountID.Value, out accountID))
				{
					if (chkAccountID.Checked)
					{
						if (!accountIDs.Contains(accountID))
							accountIDs.Add(accountID);
					}
					else
					{
						if (accountIDs.Contains(accountID))
							accountIDs.Remove(accountID);
					}
				}
			}

			Session["RenewalBatchAccountIDs"] = accountIDs;
		}

		private bool IsRenewalLogExist(int accountID, DateTime contractEndDate)
		{
			//int cnt = idc.RenewalLogs.Where(r => r.AccountID == accountID && r.OrgContractEndDate == contractEndDate).Count();
			//int cnt = idc.RenewalLogs.Where(r => r.AccountID == accountID && r.OrgContractEndDate == contractEndDate && r.Order.OrderType == "RENEWAL" && r.Order.OrderSource != "Quarterly Billing").Count();
			int cnt = idc.RenewalLogs.Where(r => r.AccountID == accountID && r.OrgContractEndDate == contractEndDate && r.Order.OrderType == "RENEWAL" && r.Order.OrderSource == "Renewals").Count();

			return cnt > 0;
		}
		#endregion
	}
}