using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Security.Cryptography;

using Instadose.Data;
using System.IO;
using System.Text;

public partial class IT_HardwareUpdates : System.Web.UI.Page
{
    private const string ENCRYPTION_STRING = "ln9D3LAppXmd3V1n7VftPIXN";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["Guid"] != null)
        {
            var decrypted = (Request.QueryString["d"] != null);
            // Download build
            downloadUpdate(Request.QueryString["Guid"], decrypted);
            return;
        }

        if (IsPostBack) return;

        var idc = new InsDataContext();
        ddlHardwareList.DataSource = idc.Hardwares.Select(h => new { h.HardwareID, h.HardwareDescription });
        ddlHardwareList.DataTextField = "HardwareDescription";
        ddlHardwareList.DataValueField = "HardwareID";
        ddlHardwareList.DataBind();

        ddlEditHardwareList.DataSource = idc.Hardwares.Select(h => new { h.HardwareID, h.HardwareDescription });
        ddlEditHardwareList.DataTextField = "HardwareDescription";
        ddlEditHardwareList.DataValueField = "HardwareID";
        ddlEditHardwareList.DataBind();

        try
        {
            bindGridView();
        }
        catch(Exception ex)
        {
            Response.Write(ex.Message);
        }
    }

    protected void btnUploadVersion_Click(object sender, EventArgs e)
    {
        try
        {
            uploadSuccess.Visible = false;
            uploadError.Visible = false;
            editSuccess.Visible = false;
            editError.Visible = false;

            int vMajor = 0;
            int vMinor = 0;
            int vRevision = 0;
            int hardwareID = 0;
            int upSequence = 1;
            DateTime? publishedDate = null;
            DateTime tmpDate;

            if (!int.TryParse(txtVersionMajor.Text, out vMajor))
                throw new Exception("The major version number is required.");

            if (!int.TryParse(txtVersionMinor.Text, out vMinor))
                throw new Exception("The minor version number is required.");

            if (!int.TryParse(txtVersionRevision.Text, out vRevision))
                throw new Exception("The revision version number is required.");

            if (!int.TryParse(ddlHardwareList.SelectedValue, out hardwareID))
                throw new Exception("The hardware is required.");

            if (reReleaseNotes.Content == "")
                throw new Exception("The release notes are required.");

            if (!fuBuildFile.HasFile)
                throw new Exception("An update package must be included.");

            if (DateTime.TryParse(txtPublishDate.Text, out tmpDate))
                publishedDate = tmpDate;

            if (!int.TryParse(txtUploadSequence.Text, out upSequence))
                throw new Exception("The sequence must be a number greater than 0.");

            var checkedUpdateType = chkUploadUpdateType.Items.Cast<ListItem>().Where(li => li.Selected).ToList();
            if (checkedUpdateType.Count == 0)
                throw new Exception("At least one update type should be selected.");

            // Upload and encrypt the file.
            var md5 = MD5.Create();
            var fileMD5Hash = ToHex(md5.ComputeHash(fuBuildFile.FileContent), true);

            // Read the file into a byte array and encrypt it.
            var fileBytes = ReadAllBytes(fuBuildFile.FileContent);

            var p = new Protection();
            //fileBytes = p.OpenSSLEncrypt(fileBytes, ENCRYPTION_STRING);

            var idc = new InsDataContext();

            // Ensure a version for this hardware doesn't already exist.
            var checkUpdate = (from hu in idc.HardwareUpdates
                               where
                                hu.Active &&
                                hu.UpdateMajor == vMajor &&
                                hu.UpdateMinor == vMinor &&
                                hu.UpdateRevision == vRevision &&
                                hu.HardwareID == hardwareID
                               select hu.UpdateID).Count();

            if (checkUpdate > 0)
                throw new Exception("This version already exists for this hardware.");

            // Select the hardware.
            var hardware = (from h in idc.Hardwares where h.HardwareID == hardwareID select h).FirstOrDefault();

            // Ensure the hardware was found.
            if (hardware == null) throw new Exception("The hardware could not be found.");

            var hardwareUpdate = new HardwareUpdate()
            {
                FileName = string.Format("{0}_build.{1}.{2}.{3}.bin", hardware.HardwareDescription, vMajor, vMinor, vRevision),
                HardwareID = hardwareID,
                PublishedDate = publishedDate,
                ReleaseNotes = reReleaseNotes.Content,
                UpdateMajor = vMajor,
                UpdateMinor = vMinor,
                UpdateRevision = vRevision,
                UploadedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                UpdateGuid = Guid.NewGuid(),
                MD5Hash = fileMD5Hash,
                UpdateContent = fileBytes,
                Active = true
            };
            
            foreach (var i in checkedUpdateType)
            {
                var path = new HardwareUpdatePath()
                {
                    CreatedDate = DateTime.Now,
                    Sequence = upSequence,
                    UpdateType = i.Value.ToCharArray()[0]
                };
                hardwareUpdate.HardwareUpdatePaths.Add(path);
            }

            // Add the hardware update.
            idc.HardwareUpdates.InsertOnSubmit(hardwareUpdate);

            // Save the new record.
            idc.SubmitChanges();

            uploadSuccess.Visible = true;

            txtPublishDate.Text = "";
            txtVersionMajor.Text = "";
            txtVersionMinor.Text = "";
            txtVersionRevision.Text = "";
            reReleaseNotes.Content = "";

            // Reload the grid.
            bindGridView();
        }
        catch (Exception ex)
        {
            lblUploadError.Text = ex.Message;
            uploadError.Visible = true;
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "openDialog", "$(function () {$('#openUploadDialog').click();});", true);
        }

    }

    protected void btnEditVersion_Click(object sender, EventArgs e)
    {
        try
        {
            uploadSuccess.Visible = false;
            uploadError.Visible = false;
            editSuccess.Visible = false;
            editError.Visible = false;

            int vMajor = 0;
            int vMinor = 0;
            int vRevision = 0;
            int updateID = 0;
            int sequence = 1;
            int hardwareID = 0;
            DateTime? publishedDate = null;
            DateTime tmpDate;

            int.TryParse(hfUpdateID.Value, out updateID);
            if (updateID == 0)
                throw new Exception("Could not find the update ID.");

            if (!int.TryParse(txtEditVersionMajor.Text, out vMajor))
                throw new Exception("The major version number is required.");

            if (!int.TryParse(txtEditVersionMinor.Text, out vMinor))
                throw new Exception("The minor version number is required.");

            if (!int.TryParse(txtEditVersionRevision.Text, out vRevision))
                throw new Exception("The revision version number is required.");

            if (!int.TryParse(ddlEditHardwareList.SelectedValue, out hardwareID))
                throw new Exception("The hardware is required.");

            if (reEditReleaseNotes.Content == "")
                throw new Exception("The release notes are required.");

            if (DateTime.TryParse(txtEditPublishDate.Text, out tmpDate))
                publishedDate = tmpDate;

            if (!int.TryParse(txtSequence.Text, out sequence))
                throw new Exception("The sequence must be a number greater than 0.");

            var updateType = chkUpdateType.Items.Cast<ListItem>().ToList();
            if (!updateType.Any(li => li.Selected))
                throw new Exception("At least one update type should be selected.");

            var idc = new InsDataContext();

            // Ensure a version for this hardware doesn't already exist.
            var checkUpdate = (from hu in idc.HardwareUpdates
                               where
                                hu.Active &&
                                hu.UpdateID != updateID &&
                                hu.UpdateMajor == vMajor &&
                                hu.UpdateMinor == vMinor &&
                                hu.UpdateRevision == vRevision &&
                                hu.HardwareID == hardwareID
                               select hu.UpdateID).Count();

            if (checkUpdate > 0)
                throw new Exception("This version already exists for this hardware.");

            // Select the hardware.
            var hardware = (from h in idc.Hardwares where h.HardwareID == hardwareID select h).FirstOrDefault();

            // Ensure the hardware was found.
            if (hardware == null) throw new Exception("The hardware could not be found.");

            var update = (from hu in idc.HardwareUpdates where hu.UpdateID == updateID select hu).FirstOrDefault();

            // Ensure the update was found.
            if (update == null) throw new Exception("The update could not be found.");

            // Update the record where needed.
            update.FileName = string.Format("{0}_build.{1}.{2}.{3}.bin", hardware.HardwareDescription, vMajor, vMinor, vRevision);
            update.HardwareID = hardwareID;
            update.PublishedDate = publishedDate;
            update.ReleaseNotes = reEditReleaseNotes.Content;
            update.UpdateMajor = vMajor;
            update.UpdateMinor = vMinor;
            update.UpdateRevision = vRevision;
            update.ModifiedDate = DateTime.Now;
            update.Active = !cbEditDelete.Checked;
            idc.HardwareUpdatePaths.DeleteAllOnSubmit(update.HardwareUpdatePaths);
            // Save the new record.
            idc.SubmitChanges();

            var pathsToAdd = new List<HardwareUpdatePath>();
            var checkedUpdateTypes = chkUpdateType.Items.Cast<ListItem>().Where(li => li.Selected).ToList();
            foreach (var item in checkedUpdateTypes)
            {
                var ut = item.Value.ToCharArray()[0];

                var path = new HardwareUpdatePath()
                {
                    HardwareUpdateID = update.UpdateID,
                    CreatedDate = DateTime.Now,
                    Sequence = sequence,
                    UpdateType = ut
                };
                pathsToAdd.Add(path);
            }

            idc.HardwareUpdatePaths.InsertAllOnSubmit(pathsToAdd);
            idc.SubmitChanges();

            editSuccess.Visible = true;

            // Reload the grid.
            bindGridView();
        }
        catch (Exception ex)
        {
            lblEditError.Text = ex.Message;
            editError.Visible = true;
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "openDialog", "$(function () {openEditDialog();});", true);
        }
    }

    protected void lbtnEdit_Click(object sender, EventArgs e)
    {
        uploadSuccess.Visible = false;
        editError.Visible = false;

        // Load the edit form.
        var lbtnEdit = (LinkButton)sender;
        int updateID = int.Parse(lbtnEdit.CommandArgument);

        var idc = new InsDataContext();


        // load the record.
        var update = (from u in idc.HardwareUpdates where u.UpdateID == updateID select u).FirstOrDefault();

        hfUpdateID.Value = update.UpdateID.ToString();
        txtEditPublishDate.Text = string.Format("{0:MM/dd/yyyy}", update.PublishedDate);
        lblEditUploadedBuild.Text = update.FileName;
        txtEditVersionMajor.Text = update.UpdateMajor.ToString();
        txtEditVersionMinor.Text = update.UpdateMinor.ToString();
        txtEditVersionRevision.Text = update.UpdateRevision.ToString();
        reEditReleaseNotes.Content = update.ReleaseNotes;

        ddlEditHardwareList.SelectedIndex = -1;
        var lstItemHardware = ddlEditHardwareList.Items.FindByValue(update.HardwareID.ToString());
        lstItemHardware.Selected = true;

        foreach (var path in update.HardwareUpdatePaths)
        {
            chkUpdateType.Items.FindByValue(path.UpdateType.ToString()).Selected = true;
            txtSequence.Text = path.Sequence.ToString();
        }

        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "openDialog", "$(function() {openEditDialog();});", true);
    }

    private void bindGridView()
    {
        var idc = new InsDataContext();
        gvHardwareUpdates.DataSource = idc.HardwareUpdates.Select(hu =>
            new
            {
                hu.FileName,
                hu.Hardware.HardwareDescription,
                hu.MD5Hash,
                hu.PublishedDate,
                hu.UpdateGuid,
                hu.UpdateID,
                hu.UpdateMajor,
                hu.UpdateMinor,
                hu.UpdateRevision,
                hu.UploadedDate,
                hu.ReleaseNotes,
                hu.Active,
                UpdatePaths = hu.HardwareUpdatePaths.Any() ? string.Join(", ", hu.HardwareUpdatePaths.Select(h=>h.UpdateType.ToString()).ToArray()) : "",
                Sequence = hu.HardwareUpdatePaths.Any() ? hu.HardwareUpdatePaths.Select(h=>h.Sequence).FirstOrDefault() : 0
            }).Where(u => u.Active).OrderByDescending(h => h.UploadedDate);

        gvHardwareUpdates.DataBind();
    }

    private void downloadUpdate(string guid, bool decrypted)
    {
        Guid updateGuid;
        if (!Guid.TryParse(guid, out updateGuid)) throw new Exception("The Guid in invalid.");

        var idc = new InsDataContext();
        var update = (from hu in idc.HardwareUpdates where hu.UpdateGuid == updateGuid select hu).FirstOrDefault();

        // decrypt the file.
        var content = update.UpdateContent.ToArray();

        if (decrypted)
        {
            var p = new Protection();
            content = p.OpenSSLDecrypt(update.UpdateContent.ToArray(), ENCRYPTION_STRING);
            update.FileName += ".tar";
        }

        if (update == null) throw new Exception("The update does not exist.");

        // Clear everything out.
        Response.Clear();
        Response.ClearHeaders();

        // Set the response headers.
        Response.ContentType = "application/octet-stream";

        //Content-Disposition: attachment; filename=<file name.ext>. 
        Response.AddHeader("Content-Disposition", "attachment; filename=\"" + update.FileName + "\"");

        // Write the file to the response.
        Response.BinaryWrite(content);

        Response.Flush();
        Response.End();
    }

    public static byte[] ReadAllBytes(Stream stream)
    {
        stream.Position = 0;
        byte[] buffer = new byte[stream.Length];
        for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
            totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
        return buffer;
    }

    public static string ToHex(byte[] bytes, bool upperCase)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);

        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

        return result.ToString();
    }
}