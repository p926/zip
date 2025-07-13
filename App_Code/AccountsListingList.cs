using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//**********************************************************************************
//  Created By: Anuradha Nandi
//  Create Date: November 6, 2012
//  New List Class for Accounts Listing
//  Used In: CustomerService/DealerAccountSales.aspx.cs
//**********************************************************************************
public class AccountsListingList
{
	public AccountsListingList()
	{
	}

    // Collection of Accounts Listing list items.
    public List<AccountsListingListItem> AccountsListings { get; set; }
}

[Serializable()]
public class AccountsListingListItem
{
    public AccountsListingListItem()
    {
    }

    // Identifier of the Account Listing.
    public string SalesRepDistID { get; set; }
    public string ReferralCode { get; set; }
    public string SalesRepCompanyName { get; set; }
    public int AccountID { get; set; }
    public string CompanyName { get; set; }
    public DateTime ContractStartDate { get; set; }
    public DateTime ContractEndDate { get; set; }
    public int NumberOfActiveDevices { get; set; }
    public bool Active { get; set; }
}