using Instadose.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace portal_instadose_com_v3.Finance.Renewal
{
	public partial class RenewalExceptions : System.Web.UI.Page
	{
		private InsDataContext _idc;
		#region Events
		protected void Page_Load(object sender, EventArgs e)
		{
			_idc = new InsDataContext();

			if (!IsPostBack)
			{
				Session["RenewalBatchExceptions"] = new List<vw_UpcomingRenewalsBatch>();
				Session["sortDirection"] = SortDirection.Ascending;
				this.LoadDropDowns();
			}
		}

		protected void lnkbtnDisplay_Click(object sender, EventArgs e)
		{
			Session["RenewalBatchExceptions"] = this.GetRenewalExceptionList();

			gvRenewalExceptions.PageIndex = 0;
			gvRenewalExceptions.DataSource = Session["RenewalBatchExceptions"];
			gvRenewalExceptions.DataBind();
		}

		protected void lnkClear_Click(object sender, EventArgs e)
		{
			this.ClearFilterInputs();

			Session["RenewalBatchExceptions"] = new List<vw_UpcomingRenewalsBatch>();
			gvRenewalExceptions.DataSource = new List<vw_UpcomingRenewalsBatch>();
			gvRenewalExceptions.DataBind();
		}

		protected void gvRenewalExceptions_Sorting(object sender, GridViewSortEventArgs e)
		{
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

			//List<vw_UpcomingRenewalsBatch> exceptions = this.GetRenewalExceptionList();
			List<vw_UpcomingRenewalsBatch> exceptions = Session["RenewalBatchExceptions"] as List<vw_UpcomingRenewalsBatch>;

			if (exceptions != null && exceptions.Count > 0)
			{
				var pInfo = typeof(vw_UpcomingRenewalsBatch).GetProperty(e.SortExpression);

				if (e.SortDirection == SortDirection.Ascending)
					exceptions = exceptions.OrderBy(r => pInfo.GetValue(r, null)).ToList();
				else
					exceptions = exceptions.OrderByDescending(r => pInfo.GetValue(r, null)).ToList();
			}

			gvRenewalExceptions.PageIndex = 0;
			gvRenewalExceptions.DataSource = exceptions;
			gvRenewalExceptions.DataBind();
		}

		protected void gvRenewalExceptions_PageIndexChanging(object sender, GridViewPageEventArgs e)
		{
			gvRenewalExceptions.PageIndex = e.NewPageIndex;
			//gvRenewalExceptions.DataSource = this.GetRenewalExceptionList();
			gvRenewalExceptions.DataSource = Session["RenewalBatchExceptions"];
			gvRenewalExceptions.DataBind();
		}

		protected void gvRenewalExceptions_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				vw_UpcomingRenewalsBatch exceptions = e.Row.DataItem as vw_UpcomingRenewalsBatch;

				if (exceptions != null)
				{
					HyperLink hlAccountID = e.Row.FindControl("hlAccountID") as HyperLink;
					Literal ltExceptionCode = e.Row.FindControl("ltExceptionCode") as Literal;

					hlAccountID.Text = exceptions.AccountID.ToString();
					hlAccountID.NavigateUrl = string.Format("/InformationFinder/Details/Account.aspx?ID={0}", exceptions.AccountID);

					ltExceptionCode.Text = exceptions.RenewalExceptionCode;
				}
			}
		}
		#endregion

		#region Helpers
		private void LoadDropDowns()
		{
			ddlBillingMethod.DataSource = _idc.PaymentMethods.Select(p => new { p.PaymentMethodID, p.PaymentMethodName }).ToList();
			ddlBillingMethod.DataBind();
			ddlBillingMethod.Items.Insert(0, new ListItem("All", string.Empty));

			ddlBillingTerm.DataSource = _idc.BillingTerms.Select(b => new { b.BillingTermID, b.BillingTermDesc }).ToList();
			ddlBillingTerm.DataBind();
			ddlBillingTerm.Items.Insert(0, new ListItem("All", string.Empty));

			ddlBrand.DataSource = _idc.BrandSources.Select(bs => new { bs.BrandName }).ToList();
			ddlBrand.DataBind();
			ddlBrand.Items.Insert(0, new ListItem("All", string.Empty));

			ddlCustomerType.DataSource = _idc.CustomerTypes.OrderBy(ct => ct.CustomerTypeDesc).Select(ct => new { ct.CustomerTypeID, CustomerTypeName = string.Format("{0} ({1})", ct.CustomerTypeDesc, ct.CustomerTypeCode) });
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
			ddlExceptionCode.SelectedIndex = 0;
		}

		private List<vw_UpcomingRenewalsBatch> GetRenewalExceptionList()
		{
			IQueryable<vw_UpcomingRenewalsBatch> upcomingRenewals = _idc.vw_UpcomingRenewalsBatches;

			if (ddlBillingMethod.SelectedItem != null && ddlBillingMethod.SelectedItem.Text != "All")
				upcomingRenewals = upcomingRenewals.Where(ur => ur.BillingMethod == ddlBillingMethod.SelectedItem.Text);

			if (ddlBillingTerm.SelectedItem != null && ddlBillingTerm.SelectedItem.Text != "All")
				upcomingRenewals = upcomingRenewals.Where(ur => ur.BillingTerm == ddlBillingTerm.SelectedItem.Text);

			if (!string.IsNullOrEmpty(ddlCustomerType.SelectedValue))
				upcomingRenewals = upcomingRenewals.Where(ur => ur.CustomerTypeID == int.Parse(ddlCustomerType.SelectedValue));

			if (ddlBrand.SelectedItem != null && ddlBrand.SelectedItem.Text != "All")
				upcomingRenewals = upcomingRenewals.Where(ur => ur.BrandName == ddlBrand.SelectedItem.Text);

			//// Exception Code PP - No Exception is found.
			//if (!string.IsNullOrEmpty(ddlExceptionCode.SelectedValue))
			//	upcomingRenewals = upcomingRenewals.Where(ur => ur.RenewalExceptionCode == ddlExceptionCode.SelectedValue || ur.RenewalCode == "E");
			//else
			//	upcomingRenewals = upcomingRenewals.Where(ur => ur.RenewalExceptionCode != "PP" || ur.RenewalCode == "E");

			// Exception Code PP - No Exception is found.
			if (!string.IsNullOrEmpty(ddlExceptionCode.SelectedValue))
				upcomingRenewals = upcomingRenewals.Where(ur => ur.RenewalExceptionCode == ddlExceptionCode.SelectedValue);
			else
				upcomingRenewals = upcomingRenewals.Where(ur => ur.RenewalExceptionCode != "PP");

			if (!string.IsNullOrEmpty(txtPeriodFrom.Text) && !string.IsNullOrEmpty(txtPeriodTo.Text))
			{
				DateTime tmpFromDate;
				DateTime tmpToDate;

				if (DateTime.TryParse(txtPeriodFrom.Text, out tmpFromDate) && DateTime.TryParse(txtPeriodTo.Text, out tmpToDate))
					upcomingRenewals = upcomingRenewals.Where(ur => ur.RenewalDate >= new DateTime(tmpFromDate.Year, tmpFromDate.Month, tmpFromDate.Day, 0, 0, 0) && ur.RenewalDate <= new DateTime(tmpToDate.Year, tmpToDate.Month, tmpToDate.Day, 23, 59, 59));
			}

			return upcomingRenewals.OrderBy(ur => ur.RenewalDate).ThenBy(ur => ur.AccountID).ToList();
		}

		private string ExceptionDescription(string exceptionCode)
		{
			string rtn = string.Empty;

			if (string.IsNullOrEmpty(exceptionCode))
				return string.Empty;

			switch (exceptionCode)
			{
				case "NR":
					rtn = "This account does not have a rate code.";
					break;
				case "ND":
					rtn = "This account does not have any device.";
					break;
				case "RN":
					rtn = "A rate cannot be found for the number of devices specified.";
					break;
				case "DR":
					rtn = "Discount rate is 100%.";
					break;
				case "RP":
					rtn = "Price rate is $0.00.";
					break;
			}

			return rtn;
		}
		#endregion
	}
}