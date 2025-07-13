using System;
using System.Linq;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class IT_DeviceCommInformation : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.IsPostBack) return;

        rgDeviceReadsTracking.DataBind();
    }

    #region EXPORT TO EXCEL.
    /// <summary>
    /// Export filtered RadGrid results to an Excel SpreadSheet.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnExportToExcel_Click(object sender, EventArgs e)
    {
        rgDeviceReadsTracking.ExportSettings.ExportOnlyData = true;

        rgDeviceReadsTracking.MasterTableView.ShowHeader = true;

        rgDeviceReadsTracking.ExportSettings.HideStructureColumns = true;

        rgDeviceReadsTracking.ExportSettings.FileName = "DeviceReadsTracking";

        rgDeviceReadsTracking.ExportSettings.IgnorePaging = true;

        rgDeviceReadsTracking.ExportSettings.OpenInNewWindow = true;

        rgDeviceReadsTracking.ExportSettings.Excel.Format = Telerik.Web.UI.GridExcelExportFormat.ExcelML;

        rgDeviceReadsTracking.MasterTableView.ExportToExcel();
    }

    protected void rgDeviceReadsTracking_ExportCellFormatting(object sender, ExportCellFormattingEventArgs e)
    {
        GridItem item = e.Cell.Parent as GridItem;
        e.Cell.CssClass = (item.ItemType == GridItemType.Item) ? "excelCss" : "excelAltCss";
    }

    protected void rgDeviceReadsTracking_HTMLExporting(object sender, GridHTMLExportingEventArgs e)
    {
        string excelCss = ".table { border: 1px solid #3A6A79; font-family: Arial, Helvetical, Sans-Serif; font-size: 10pt; } ";
        excelCss += ".row { background-color: white; } ";
        string excelAltCss = ".row-alternate { background-color: #D5E3E7; }";
        string excelHeaderCss = ".heading { font-family: Tahoma, Helvetical, Arial, Sans-Serif; background-color: #3A6A79; font-weight: bold; text-align: center; color: White; } ";
        e.Styles.AppendLine(excelCss);
        e.Styles.AppendLine(excelAltCss);
        e.Styles.AppendLine(excelHeaderCss);

        foreach (TableCell cell in rgDeviceReadsTracking.MasterTableView.GetItems(GridItemType.Header)[0].Cells)
        {
            cell.CssClass = "excelHeaderCss";
        }
    }
    #endregion

    #region RADGRID FUNCTIONS.
    protected void rgDeviceReadsTracking_PreRender(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            rgDeviceReadsTracking.MasterTableView.FilterExpression = string.Format("([CommunicatedOn] >= '{0}')", DateTime.Today.ToShortDateString());
            GridColumn column = rgDeviceReadsTracking.MasterTableView.GetColumnSafe("CommunicatedOn");
            column.CurrentFilterFunction = GridKnownFunction.GreaterThanOrEqualTo;
            column.CurrentFilterValue = DateTime.Today.ToShortDateString();
            rgDeviceReadsTracking.MasterTableView.Rebind();
        }
    }

    protected void rgDeviceReadsTracking_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        if (e.CommandName == RadGrid.FilterCommandName)
        {
            this.fromDate = ((e.Item as GridFilteringItem)["CommunicatedOn"].FindControl("FromDatePicker") as RadDatePicker).SelectedDate;
            this.toDate = ((e.Item as GridFilteringItem)["CommunicatedOn"].FindControl("ToDatePicker") as RadDatePicker).SelectedDate;
        }

        if (!rgDeviceReadsTracking.ExportSettings.IgnorePaging) rgDeviceReadsTracking.Rebind();

        rgDeviceReadsTracking.Rebind();
    }
    #endregion

    #region GET/SET FROM AND TO DATES.
    protected DateTime? fromDate
    {
        set
        {
            ViewState["FromCommunicatedOnDate"] = value;
        }
        get
        {
            if (ViewState["FromCommunicatedOnDate"] != null)
            {
                return (DateTime)ViewState["FromCommunicatedOnDate"];
            }
            else
            {
                return DateTime.Today;
            }
        }
    }

    protected DateTime? toDate
    {
        set
        {
            ViewState["ToCommunicatedOnDate"] = value;
        }
        get
        {
            if (ViewState["ToCommunicatedOnDate"] != null)
            {
                return (DateTime)ViewState["ToCommunicatedOnDate"];
            }
            else
            {
                return DateTime.Today.AddDays(1);
            }
        }
    }
    #endregion

    #region CLEAR FILTERS.
    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        // Serial # Column.
        GridColumn serialNumber = rgDeviceReadsTracking.MasterTableView.GetColumnSafe("SerialNumber");
        serialNumber.CurrentFilterFunction = GridKnownFunction.NoFilter;
        serialNumber.CurrentFilterValue = string.Empty;

        // Communicated On Column.
        this.fromDate = DateTime.Today;
        this.toDate = DateTime.Today.AddDays(1);
        rgDeviceReadsTracking.MasterTableView.FilterExpression = string.Format("([CommunicatedOn] >= '{0}')", DateTime.Today.ToShortDateString());
        GridColumn communicatedOn = rgDeviceReadsTracking.MasterTableView.GetColumnSafe("CommunicatedOn");
        communicatedOn.CurrentFilterFunction = GridKnownFunction.GreaterThanOrEqualTo;
        communicatedOn.CurrentFilterValue = DateTime.Today.ToShortDateString();

        // Account ID Column.
        GridColumn accountID = rgDeviceReadsTracking.MasterTableView.GetColumnSafe("AccountID");
        accountID.CurrentFilterFunction = GridKnownFunction.NoFilter;
        accountID.CurrentFilterValue = string.Empty;

        rgDeviceReadsTracking.Rebind();
    }
    #endregion
}