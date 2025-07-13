/*
 * Maintain PO Receipts
 * 
 *  Created By: Tdo, 10/16/2012
 *  Copyright (C) 2010 Mirion Technologies. All Rights Reserved.
 */

using System;
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

using Instadose;
using Instadose.Data;

public partial class TechOps_MasterBaselineFileUpload : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext(); 

    public bool DisplayUploadButton(string Status)
    {
        return Status.ToUpper() == "FALSE" ? true : false;
    }

    public string GetUrl(string receiptID)
    {
        if (receiptID != "")
        {
            List<POReceiptDetail> poReceiptDetails = (from p in idc.POReceiptDetails
                                                      where p.ReceiptID == int.Parse(receiptID)
                                                      select p).ToList();
            foreach (POReceiptDetail poDetail in poReceiptDetails)
            {
                if (poDetail.ItemNumber.Trim() == "PCBA" || poDetail.ItemNumber.Trim() == "PCBA+BATTERY")
                    return "MasterBaselineUploadDetail3.aspx?receiptID=" + receiptID;
                else if (poDetail.ItemNumber.Trim() == "FDA20" || poDetail.ItemNumber.Trim() == "INSTALINK" || poDetail.ItemNumber.Trim() == "INSTALINKUSB")
                    return "MasterBaselineUploadDetail2.aspx?receiptID=" + receiptID;
                else if (poDetail.ItemNumber.Trim() == "PCBA ID PLUS")
                    return "MasterBaselineUploadDetailIDPlus.aspx?receiptID=" + receiptID;
                else if (poDetail.ItemNumber.Trim() == "PCBA ID2")
                    return "MasterBaselineUploadDetailID2New.aspx?receiptID=" + receiptID;
                else
                    return "MasterBaselineUploadDetail.aspx?receiptID=" + receiptID;
            }
        }
        return "";
    }

}