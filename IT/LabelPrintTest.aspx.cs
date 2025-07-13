using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class IT_LabelPrintTest : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack )
            this.txtQuantity.Text = "1";

        InvisibleErrors();
        InvisibleSuccess();
    }   

    protected void btnPrint_Click(object sender, EventArgs e)
    {
        try
        {
            string labelName, fieldAccountName, fieldUserName, fieldLocation, fieldBodyRegion, fieldUserLocation, fieldBarcode, labelQuantity, fieldUserIDAccountID, fieldUserID, fieldAccountID;
            string labelNameStr, fieldLogoStr, labelQuantityStr, printSetStr, printerStr, fieldAccountNameStr, fieldUserNameStr, fieldLocationStr, fieldUserIDAccountIDStr, fieldBarcodeStr;
            bool standardLabel = false;

            // if userID or accountID entered then print Optional label otherwise, print standard label
            if (string.IsNullOrWhiteSpace(this.txtAccountID.Text) && string.IsNullOrWhiteSpace(this.txtWearerID.Text))
                standardLabel = true;

            switch (this.ddlLogo.SelectedValue.ToUpper())
            {
                case "ICCARE":
                    labelName = (standardLabel == true) ? "ICCare.alf" : "ICCare_Optional.alf";
                    fieldAccountName = "\"" + "IC Care" + "\"";
                    break;
                case "MIRION":
                    labelName = (standardLabel == true) ? "Mirion.alf" : "Mirion_Optional.alf";
                    fieldAccountName = "mirion.png";
                    break;
                case "QUANTUM":
                    labelName = (standardLabel == true) ? "Quantum.alf" : "Quantum_Optional.alf";
                    fieldAccountName = "quantum.png";
                    break;
                default:
                    labelName = (standardLabel == true) ? "CompanyName.alf" : "CompanyName_Optional.alf";
                    fieldAccountName = "\"" + this.txtAccountName.Text.Trim() + "\"";
                    break;
            }

            fieldUserName = this.txtWearerName.Text.Trim();

            fieldBodyRegion = this.ddlBodyRegion.SelectedValue.ToUpper();
            fieldUserLocation = this.txtWearerLocation.Text.Trim();

            if (fieldBodyRegion != "" && fieldUserLocation != "")
                fieldLocation = fieldBodyRegion + " | " + fieldUserLocation;
            else
                fieldLocation = fieldBodyRegion + fieldUserLocation;

            fieldUserID = this.txtWearerID.Text.Trim();
            if (!string.IsNullOrWhiteSpace(this.txtAccountID.Text))
                fieldAccountID = "acct# " + this.txtAccountID.Text.Trim();
            else
                fieldAccountID = "";

            if (fieldUserID != "" && fieldAccountID != "")
                fieldUserIDAccountID = fieldUserID + " | " + fieldAccountID;
            else
                fieldUserIDAccountID = fieldUserID + fieldAccountID;

            fieldBarcode = this.txtBarcode.Text;

            int quantity = 1;
            if (!int.TryParse(this.txtQuantity.Text, out quantity))
            {
                quantity = 1;
            }
            else
            {
                if (quantity < 1)
                {
                    quantity = 1;
                }
            }
            labelQuantity = quantity.ToString();

            fieldAccountNameStr = "FieldCompanyName=" + fieldAccountName;
            fieldUserNameStr = "FieldUserName=" + "\"" + fieldUserName + "\"";

            fieldLocationStr = "FieldLocation=" + "\"" + fieldLocation + "\"";
            fieldUserIDAccountIDStr = "FieldUserIDAccountID=" + "\"" + fieldUserIDAccountID + "\"";
            fieldBarcodeStr = "FieldBarcode=" + "\"" + fieldBarcode + "\"";

            labelNameStr = "LABELNAME=" + labelName;
            labelQuantityStr = "LABELQUANTITY=" + labelQuantity;
            printSetStr = "PRINTSET=5,16,16,16,12,0,0,0,2G";
            printerStr = "PRINTER=QuickLabel Kiaro;Kiaro!";

            string curTime = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            string filename = "print" + "_" + fieldBarcode + "_" + curTime + ".acf";

            string commandFilePath = @"\\irv-sapl-file1.mirion.local\POReceipt$\PrintTemplates\QuickCommand\MyCommandFiles\" + filename;
            string monitorFilePath = @"\\irv-sapl-file1.mirion.local\POReceipt$\PrintTemplates\QuickCommand\Monitor\" + filename;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(commandFilePath))
            {
                file.WriteLine(labelNameStr);
                file.WriteLine(fieldAccountNameStr);
                file.WriteLine(fieldUserNameStr);
                file.WriteLine(fieldLocationStr);

                if (!standardLabel)
                    file.WriteLine(fieldUserIDAccountIDStr);

                file.WriteLine(fieldBarcodeStr);
                file.WriteLine(labelQuantityStr);

                // printset and printer string will be put at the end of file
                file.WriteLine(printSetStr);
                file.WriteLine(printerStr);
            }

            // copy command file from CommandFiles directory to Monitor directory
            if (File.Exists(commandFilePath))
            {
                File.Copy(commandFilePath, monitorFilePath, true);
            }

            VisibleSuccess("Print job successful.");                    
        }
        catch (Exception ex)
        {
            VisibleErrors(ex.ToString());
        }
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        this.ddlLogo.Text = "";
        this.txtAccountName.Text = "";
        this.txtAccountID.Text = "";
        this.txtWearerName.Text = "";
        this.ddlBodyRegion.Text = "";
        this.txtWearerLocation.Text = "";
        this.txtWearerID.Text = "";
        this.txtBarcode.Text = "";
        this.txtQuantity.Text = "1";
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
    protected void ddlLogo_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (this.ddlLogo.Text.Length > 0)
        {
            this.txtAccountName.Text = "";
            this.txtAccountName.Enabled = false;
        }
        else
        {
            this.txtAccountName.Enabled = true;
        }
            
    }
}