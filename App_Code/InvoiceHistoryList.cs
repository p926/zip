using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//**********************************************************************************
//  Created By: Anuradha Nandi
//  Create Date: November 7, 2012
//  New List Class for Invoice History
//  Used In: CustomerService/DealerAccountSales.aspx.cs
//**********************************************************************************
public class InvoiceHistoryList
{
	public InvoiceHistoryList()
	{
	}

    // Collection of Accounts Listing list items.
    public List<InvoiceHistoryListItem> InvoiceHistories { get; set; }
}

[Serializable()]
public class InvoiceHistoryListItem
{
    public InvoiceHistoryListItem()
    {
    }

    // Identifier of the Account Listing.
    public string SalesPersonNo { get; set; }
    public string SalesPersonName { get; set; }
    public string CustomerNo { get; set; }
    public string InvoiceNo { get; set; }
    public string BillToName { get; set; }
    public string InvoiceDate { get; set; }
    public string InvoiceAmount { get; set; }
}