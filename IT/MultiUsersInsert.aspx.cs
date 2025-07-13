using System;
using System.Data;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using Instadose.Data;
using Instadose.Security;
using Mirion.DSD.Consumer;

public partial class IT_MultiUsersInsert : System.Web.UI.Page
{
	private CSVParser userParser = new CSVParser();
	DataTable dtSuccess = new DataTable();
	DataTable dtFail = new DataTable();
	// Create the database reference
	InsDataContext idc = new InsDataContext();

	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.Page.IsPostBack)
		{
			this.successSection.Visible = false;
			this.failSection.Visible = false;
		}
		InvisibleErrors();
		InvisibleMessages();
	}

	private void InitiateSuccessTable()
	{
		dtSuccess = new DataTable();
		dtSuccess.Columns.Add("AccountID", typeof(string));
		dtSuccess.Columns.Add("LocationID", typeof(string));
		dtSuccess.Columns.Add("GroupID", typeof(string));
		dtSuccess.Columns.Add("FirstName", typeof(string));
		dtSuccess.Columns.Add("LastName", typeof(string));
		dtSuccess.Columns.Add("UserName", typeof(string));
		dtSuccess.Columns.Add("Password", typeof(string));
	}

	private void InitiateFailTable()
	{
		dtFail = new DataTable();
		dtFail.Columns.Add("FirstName", typeof(string));
		dtFail.Columns.Add("LastName", typeof(string));
		dtFail.Columns.Add("ErrorMessage", typeof(string));
	}

	private void InsertSuccessListTable(string pAccountID, string pLocationID, string pGroupID, string pFirstName, string pLastName, string pUserName, string pPassword)
	{
		DataRow dtr = dtSuccess.NewRow();

		dtr["AccountID"] = pAccountID;
		dtr["LocationID"] = pLocationID;
		dtr["GroupID"] = pGroupID;
		dtr["FirstName"] = pFirstName;
		dtr["LastName"] = pLastName;
		dtr["UserName"] = pUserName;
		dtr["Password"] = pPassword;

		// Add the row to the DataTable.
		dtSuccess.Rows.Add(dtr);
	}

	private void InsertFailListTable(string pFirstName, string pLastName, string pErrorMessage)
	{
		DataRow dtr = dtFail.NewRow();

		dtr["FirstName"] = pFirstName;
		dtr["LastName"] = pLastName;
		dtr["ErrorMessage"] = pErrorMessage;

		// Add the row to the DataTable.
		dtFail.Rows.Add(dtr);
	}

	private void Reset()
	{
		this.successSection.Visible = false;
		this.failSection.Visible = false;        

		dtSuccess = null;
		dtFail = null;

		// Bind the data to the GridView
		//GridViewSuccess.DataSource = ds.Tables[0].DefaultView;
		GridViewSuccess.DataSource = null;
		GridViewSuccess.DataBind();
		GridViewFail.DataSource = null;
		GridViewFail.DataBind();
	}

	private bool UploadFileValidation()
	{
		//StreamReader userFileContent ;
		try
		{

			if (!FileUpload1.HasFile)
			{                
				this.VisibleErrors("Please select file to upload.");
				return false;
			}

			using (StreamReader userFileContent = new StreamReader(FileUpload1.FileContent))
			{
				userParser.Parse(userFileContent);
			}
			//userFileContent = new StreamReader(FileUpload1.FileContent);
			//userParser.Parse(userFileContent);
			if (userParser.Data == null)
			{               
				this.VisibleErrors("Could not parse the upload file.");
				return false;            
			}

			if (userParser.Data.Columns["AccountID"] == null
				|| userParser.Data.Columns["LocationID"] == null
				|| userParser.Data.Columns["GroupID"] == null
				|| userParser.Data.Columns["Prefix"] == null
				|| userParser.Data.Columns["FirstName"] == null
				|| userParser.Data.Columns["MiddleName"] == null
				|| userParser.Data.Columns["LastName"] == null
				|| userParser.Data.Columns["UserName"] == null
				|| userParser.Data.Columns["BirthDate"] == null
				|| userParser.Data.Columns["Gender"] == null
				|| userParser.Data.Columns["Address1"] == null
				|| userParser.Data.Columns["Address2"] == null
				|| userParser.Data.Columns["Address3"] == null
				|| userParser.Data.Columns["City"] == null
				|| userParser.Data.Columns["StateID"] == null
				|| userParser.Data.Columns["PostalCode"] == null
				|| userParser.Data.Columns["CountryID"] == null
				|| userParser.Data.Columns["Email"] == null
				|| userParser.Data.Columns["Telephone"] == null
				|| userParser.Data.Columns["Fax"] == null
				|| userParser.Data.Columns["Password"] == null
				|| userParser.Data.Columns["MustChangePassword"] == null
				|| userParser.Data.Columns["SecurityQuestionID"] == null
				|| userParser.Data.Columns["SecurityQuestionAnswer"] == null
				|| userParser.Data.Columns["CreatedDate"] == null
				|| userParser.Data.Columns["Active"] == null
				|| userParser.Data.Columns["OkToEmail"] == null
				|| userParser.Data.Columns["OkToEmailHDN"] == null
				|| userParser.Data.Columns["UserEmployeeID"] == null
				|| userParser.Data.Columns["UserEmployeeType"] == null
				|| userParser.Data.Columns["UserRoleID"] == null
				|| userParser.Data.Columns["AutoDeviceAssign"] == null
				|| userParser.Data.Columns["MovableUser"] == null
				|| userParser.Data.Columns["LastReminded"] == null)
			{               
				this.VisibleErrors("The upload file data schema does not match the required format.");
				return false;
			}

			if (userParser.Data.Rows.Count <= 0)
			{                
				this.VisibleErrors("No users added. File is empty.");
				return false;
			}                                  
		   
		}
		catch (Exception ex)
		{            
			this.VisibleErrors("System uploading file error: <br/>" + ex.ToString());         
			return false;
		}        

		return true;
	}

	private bool isRowBlank(DataRow pRow)
	{
		try
		{
			if (string.IsNullOrEmpty(pRow["AccountID"].ToString())
				&& string.IsNullOrEmpty(pRow["LocationID"].ToString())
				&& string.IsNullOrEmpty(pRow["GroupID"].ToString())
				&& string.IsNullOrEmpty(pRow["Prefix"].ToString()) 
				&& string.IsNullOrEmpty(pRow["FirstName"].ToString()) 
				&& string.IsNullOrEmpty(pRow["MiddleName"].ToString())
				&& string.IsNullOrEmpty(pRow["LastName"].ToString())
				&& string.IsNullOrEmpty(pRow["UserName"].ToString()) 
				&& string.IsNullOrEmpty(pRow["BirthDate"].ToString()) 
				&& string.IsNullOrEmpty(pRow["Gender"].ToString()) 
				&& string.IsNullOrEmpty(pRow["Address1"].ToString())
				&& string.IsNullOrEmpty(pRow["Address2"].ToString()) 
				&& string.IsNullOrEmpty(pRow["Address3"].ToString()) 
				&& string.IsNullOrEmpty(pRow["City"].ToString())
				&& string.IsNullOrEmpty(pRow["StateID"].ToString())
				&& string.IsNullOrEmpty(pRow["PostalCode"].ToString()) 
				&& string.IsNullOrEmpty(pRow["CountryID"].ToString()) 
				&& string.IsNullOrEmpty(pRow["Email"].ToString()) 
				&& string.IsNullOrEmpty(pRow["Telephone"].ToString())
				&& string.IsNullOrEmpty(pRow["Fax"].ToString()) 
				&& string.IsNullOrEmpty(pRow["Password"].ToString())
				&& string.IsNullOrEmpty(pRow["MustChangePassword"].ToString()) 
				&& string.IsNullOrEmpty(pRow["SecurityQuestionID"].ToString())
				&& string.IsNullOrEmpty(pRow["SecurityQuestionAnswer"].ToString())
				&& string.IsNullOrEmpty(pRow["CreatedDate"].ToString()) 
				&& string.IsNullOrEmpty(pRow["Active"].ToString()) 
				&& string.IsNullOrEmpty(pRow["OkToEmail"].ToString())
				&& string.IsNullOrEmpty(pRow["OkToEmailHDN"].ToString())
				&& string.IsNullOrEmpty(pRow["UserEmployeeID"].ToString()) 
				&& string.IsNullOrEmpty(pRow["UserEmployeeType"].ToString()) 
				&& string.IsNullOrEmpty(pRow["UserRoleID"].ToString())
				&& string.IsNullOrEmpty(pRow["AutoDeviceAssign"].ToString()) 
				&& string.IsNullOrEmpty(pRow["MovableUser"].ToString()) 
				&& string.IsNullOrEmpty(pRow["LastReminded"].ToString()) )
			{
				return true;
			}
		}
		catch
		{ }
		return false;
	}

	private bool PassValidation(int AccountID,
								int LocationID,
								int? GroupID,
								string Prefix,
								string FirstName,
								string LastName,
								string UserName,
								string BirthDate,
								string Gender,
								int? StateID,
								int? CountryID,
								string Email,
								string Password,
								int SecurityQuestionID,
								string SecurityQuestionAnswer,
								string CreatedDate,
								string UserEmployeeType,
								int UserRoleID,
								int MovableUser,
								string LastReminded,
								ref string pErrorMessage)
	{
		// VALIDATE ACCOUNT
		if (AccountID == 0)
		{
			pErrorMessage = "Account is required.";
			return false;
		}
		else
		{
			int count = (from c in idc.Accounts  where c.AccountID == AccountID  select c).Count();
			if (count == 0)
			{
				pErrorMessage = "Account does not exist.";
				return false;
			}
		}

		// VALIDATE LOCATION
		if (LocationID == 0)
		{
			pErrorMessage = "Location is required.";
			return false;
		}
		else
		{
			int count = (from c in idc.Locations where c.AccountID == AccountID && c.LocationID == LocationID select c).Count();
			if (count == 0)
			{
				pErrorMessage = "Location does not exist.";
				return false;
			}
		}

		// VALIDATE GROUP
		if (GroupID > 0)
		{
			int count = (from c in idc.LocationGroups where c.GroupID == GroupID && c.LocationID == LocationID select c).Count();
			if (count == 0)
			{
				pErrorMessage = "Group does not exist.";
				return false;
			}
		}

		// VALIDATE PREFIX
		if (Prefix != null )
		{
			if (Prefix.ToUpper() != "DR." && Prefix.ToUpper() != "MR." && Prefix.ToUpper() != "MRS." && Prefix.ToUpper() != "MS.")
			{
				pErrorMessage = "Prefix is incorrect.";
				return false;
			}
		}

		// VALIDATE FIRSTNAME
		if (FirstName == null )
		{
			pErrorMessage = "First Name is required.";
			return false;
		}

		// VALIDATE LASTNAME
		if (LastName == null)
		{
			pErrorMessage = "Last Name is required.";
			return false;
		}

		// VALIDATE BIRTHDAY
		if (BirthDate.Length > 0)
		{
			if (BirthDate.ToUpper() != "NULL")
			{
				DateTime myDate;
				if (DateTime.TryParse(BirthDate, out myDate) == false)
				{
					pErrorMessage = "BirthDate is incorrect date.";
					return false;
				}
				else
				{
					if (myDate > DateTime.Today)
					{
						pErrorMessage = "BirthDate can not be in the future.";
						return false;
					}
				}
			}
		}

		// VALIDATE GENDER
		if (Gender == null )
		{
			pErrorMessage = "Gender is required.";
			return false;
		}
		else
		{
			if (Gender.ToUpper() != "U" && Gender.ToUpper() != "M" && Gender.ToUpper() != "F")
			{
				pErrorMessage = "Gender is incorrect.";
				return false;
			}
		}

		// VALIDATE COUNTRYID
		if (CountryID > 0)
		{
			int count = (from c in idc.Countries where c.CountryID == CountryID select c).Count();
			if (count == 0)
			{
				pErrorMessage = "CountryID does not exist.";
				return false;
			}
		}

		// VALIDATE STATEID
		if (StateID > 0)
		{
			if (CountryID == 0)
			{
				pErrorMessage = "Missing CountryID.";
				return false;
			}
			else
			{
				int count = (from c in idc.States where c.CountryID == CountryID && c.StateID == StateID select c).Count();
				if (count == 0)
				{
					pErrorMessage = "StateID does not exist.";
					return false;
				}
			}
		}

		// VALIDATE SecurityQuestionID
		if (SecurityQuestionID == 0)
		{
			pErrorMessage = "SecurityQuestionID is required.";
			return false;
		}
		else
		{
			int count = (from c in idc.SecurityQuestions where c.SecurityQuestionID == SecurityQuestionID select c).Count();
			if (count == 0)
			{
				pErrorMessage = "SecurityQuestionID does not exist.";
				return false;
			}
		}

		// VALIDATE SecurityQuestionAnswer
		if (SecurityQuestionAnswer == null)
		{
			pErrorMessage = "SecurityQuestionAnswer is required.";
			return false;
		}

		// VALIDATE CREATEDDAY
		if (CreatedDate.Length == 0 || CreatedDate.ToUpper() == "NULL")
		{
			pErrorMessage = "CreatedDate is required.";
			return false;
		}
		else
		{
			DateTime myDate;
			if (DateTime.TryParse(CreatedDate, out myDate) == false)
			{
				pErrorMessage = "CreatedDate is incorrect.";
				return false;
			}
		}

		// VALIDATE UserEmployeeType
		if (UserEmployeeType == null)
		{
			pErrorMessage = "UserEmployeeType is required.";
			return false;
		}
		else
		{
			if (UserEmployeeType.ToUpper() != "EMPLOYEE ID" && UserEmployeeType.ToUpper() != "SSN")
			{
				pErrorMessage = "UserEmployeeType is incorrect.";
				return false;
			}
		}

		// VALIDATE USERROLEID
		if (UserRoleID == 0)
		{
			pErrorMessage = "UserRoleID is required.";
			return false;
		}
		else
		{
			int count = (from c in idc.UserRoles where c.UserRoleID == UserRoleID select c).Count();
			if (count == 0)
			{
				pErrorMessage = "UserRoleID does not exist.";
				return false;
			}
		}

		// VALIDATE MOVABLEUSER
		if (MovableUser == 0)
		{
			pErrorMessage = "MovableUser is required.";
			return false;
		}
		else
		{
			if (MovableUser > 3 && MovableUser < 1)
			{
				pErrorMessage = "MovableUser does not exist.";
				return false;
			}
		}

		// VALIDATE LASTREMINDED
		if (LastReminded.Length > 0)
		{
			if (LastReminded.ToUpper() != "NULL")
			{
				DateTime myDate;
				if (DateTime.TryParse(LastReminded, out myDate) == false)
				{
					pErrorMessage = "LastReminded is incorrect.";
					return false;
				}
			}
		}


		string regExEmail = @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$";
		string regExUserName = @"^[a-zA-Z0-9._-]{5,50}$"; //"^[a-zA-Z''-'\s]{1,50}"

		// VALIDATE EMAIL
		// Create a regular expression for the email.
		Regex regEmail = new Regex(regExEmail);
		if (Email == null)
		{
			pErrorMessage = "Email is required.";
			return false;
		}
		else if (Email.Length > 50)
		{
			pErrorMessage = "Email address is too large.";
			return false;
		}
		else if (!regEmail.IsMatch(Email))
		{
			pErrorMessage = "Email address is not valid.";
			return false;
		}

		// VALIDATE USERNAME
		// Create a regular expression for the username
		Regex regUser = new Regex(regExUserName);

		if (UserName == null)
		{
			pErrorMessage = "UserName is required.";
			return false;
		}
		else if (UserName.Length > 50)
		{
			pErrorMessage = "UserName is too large.";
			return false;
		}
		else if (UserName.Length < 5)
		{
			pErrorMessage = "UserName is too small.";
			return false;
		}
		else if (ValidUserName(UserName) != 1)
		{
			pErrorMessage = "UserName is not available.";
			return false;
		}
		else if (!regUser.IsMatch(UserName))
		{
			// And if the username is not an email address
			if (!regEmail.IsMatch(UserName))
			{
				pErrorMessage = "UserName cannot contain special characters unless it is an email address.";
				return false;
			}
		}

		// VALIDATE PASSWORD
		if (Password == null)
		{
			pErrorMessage = "Password is required.";
			return false;
		}
		else if (Password.Length > 20)
		{
			pErrorMessage = "Password is too large.";
			return false;
		}
		else if (Password.Length < 7)
		{
			pErrorMessage = "Password must be 7 characters length at least.";
			return false;
		}


		return true;
	}

	private int ValidUserName(string userName)
	{
		// -1 Error
		//  0 Not Valid 
		//  1 Valid
		try
		{
			// Try to find a number of users with this username
			int userCount = (from u in idc.AuthUsers where u.UserName.Trim().ToLower() == userName.Trim().ToLower() select u).Count();

			// If any number other than zero, the username is in use.
			return (userCount == 0) ? 1 : 0;
		}
		catch
		{
			return -1;
		}
	}

	protected void Button1_Click(object sender, EventArgs e)
	{
		bool success = true;

		try 
		{
			Reset();
			InitiateSuccessTable();
			InitiateFailTable();

			if (UploadFileValidation())
			{
				int myAccountID;
				int myLocationID;
				int? myGroupID;
				string myPrefix;
				string myFirstName;
				string myMiddleName;
				string myLastName;
				string myUserName;
				DateTime? myBirthDate;
				string myGender;
				string myAddress1;
				string myAddress2;
				string myAddress3;
				string myCity;
				int? myStateID;
				string myPostalCode;
				int? myCountryID;
				string myEmail;
				string myTelephone;
				string myFax;
				string myPassword;
				bool myMustChangePassword;
				int mySecurityQuestionID;
				string mySecurityQuestionAnswer;
				DateTime myCreatedDate;
				bool myActive;
				bool myOkToEmail;
				bool myOkToEmailHDN;
				string myUserEmployeeID;
				string myUserEmployeeType;
				int myUserRoleID;
				bool myAutoDeviceAssign;
				int myMovableUser;
				DateTime? myLastReminded;

				DateTime? NullDateTime = null;
				Int32? NullInt = null;
				string errorMessage = "";
				string myHashPassword = "";

				// Loop through each of the rows            
				foreach (DataRow row in userParser.Data.Rows)
				{
					// Skip if it's a blank row
					if (isRowBlank(row)) continue;

					if (row["AccountID"].ToString().Trim().Length == 0 || row["AccountID"].ToString().Trim().ToUpper() == "NULL")
						myAccountID = 0;
					else
						myAccountID = int.Parse(row["AccountID"].ToString().Trim());

					if (row["LocationID"].ToString().Trim().Length == 0 || row["LocationID"].ToString().Trim().ToUpper() == "NULL")
						myLocationID = 0;
					else
						myLocationID = int.Parse(row["LocationID"].ToString().Trim());

					if (row["GroupID"].ToString().Trim().Length == 0 || row["GroupID"].ToString().Trim().ToUpper() == "NULL")
						myGroupID = NullInt;
					else
						myGroupID = int.Parse(row["GroupID"].ToString().Trim());

					if (row["Prefix"].ToString().Trim().Length == 0 || row["Prefix"].ToString().Trim().ToUpper() == "NULL")
						myPrefix = null;
					else
						myPrefix = row["Prefix"].ToString().Trim();

					if (row["FirstName"].ToString().Trim().Length == 0 || row["FirstName"].ToString().Trim().ToUpper() == "NULL")
						myFirstName = null;
					else
						myFirstName = row["FirstName"].ToString().Trim();

					if (row["MiddleName"].ToString().Trim().Length == 0 || row["MiddleName"].ToString().Trim().ToUpper() == "NULL")
						myMiddleName = null;
					else
						myMiddleName = row["MiddleName"].ToString().Trim();

					if (row["LastName"].ToString().Trim().Length == 0 || row["LastName"].ToString().Trim().ToUpper() == "NULL")
						myLastName = null;
					else
						myLastName = row["LastName"].ToString().Trim();

					if (row["UserName"].ToString().Trim().Length == 0 || row["UserName"].ToString().Trim().ToUpper() == "NULL")
						myUserName = null;
					else
						myUserName = row["UserName"].ToString().Trim();

					if (row["BirthDate"].ToString().Trim().Length == 0 || row["BirthDate"].ToString().Trim().ToUpper() == "NULL")
						myBirthDate = NullDateTime;
					else
					{
						DateTime myDate;
						if (DateTime.TryParse(row["BirthDate"].ToString().Trim(), out myDate) == false)
						{
							myBirthDate = NullDateTime;
						}
						else
						{
							myBirthDate = DateTime.Parse(row["BirthDate"].ToString().Trim());
						}
					}


					if (row["Gender"].ToString().Trim().Length == 0 || row["Gender"].ToString().Trim().ToUpper() == "NULL")
						myGender = null;
					else
						myGender = row["Gender"].ToString().Trim().ToUpper();

					if (row["Address1"].ToString().Trim().Length == 0 || row["Address1"].ToString().Trim().ToUpper() == "NULL")
						myAddress1 = null;
					else
						myAddress1 = row["Address1"].ToString().Trim();

					if (row["Address2"].ToString().Trim().Length == 0 || row["Address2"].ToString().Trim().ToUpper() == "NULL")
						myAddress2 = null;
					else
						myAddress2 = row["Address2"].ToString().Trim();

					if (row["Address3"].ToString().Trim().Length == 0 || row["Address3"].ToString().Trim().ToUpper() == "NULL")
						myAddress3 = null;
					else
						myAddress3 = row["Address3"].ToString().Trim();

					if (row["City"].ToString().Trim().Length == 0 || row["City"].ToString().Trim().ToUpper() == "NULL")
						myCity = null;
					else
						myCity = row["City"].ToString().Trim();

					if (row["StateID"].ToString().Trim().Length == 0 || row["StateID"].ToString().Trim().ToUpper() == "NULL")
						myStateID = NullInt;
					else
						myStateID = int.Parse(row["StateID"].ToString().Trim());

					if (row["PostalCode"].ToString().Trim().Length == 0 || row["PostalCode"].ToString().Trim().ToUpper() == "NULL")
						myPostalCode = null;
					else
						myPostalCode = row["PostalCode"].ToString().Trim();

					if (row["CountryID"].ToString().Trim().Length == 0 || row["CountryID"].ToString().Trim().ToUpper() == "NULL")
						myCountryID = NullInt;
					else
						myCountryID = int.Parse(row["CountryID"].ToString().Trim());

					if (row["Email"].ToString().Trim().Length == 0 || row["Email"].ToString().Trim().ToUpper() == "NULL")
						myEmail = null;
					else
						myEmail = row["Email"].ToString().Trim();

					if (row["Telephone"].ToString().Trim().Length == 0 || row["Telephone"].ToString().Trim().ToUpper() == "NULL")
						myTelephone = null;
					else
						myTelephone = row["Telephone"].ToString().Trim();

					if (row["Fax"].ToString().Trim().Length == 0 || row["Fax"].ToString().Trim().ToUpper() == "NULL")
						myFax = null;
					else
						myFax = row["Fax"].ToString().Trim();

					if (row["Password"].ToString().Trim().Length == 0 || row["Password"].ToString().Trim().ToUpper() == "NULL")
						myPassword = null;
					else
						myPassword = row["Password"].ToString().Trim();

					if (row["MustChangePassword"].ToString().Trim().Length == 0 || row["MustChangePassword"].ToString().Trim().ToUpper() == "NULL")
						myMustChangePassword = false;
					else
						myMustChangePassword = Convert.ToBoolean(row["MustChangePassword"]);

					if (row["SecurityQuestionID"].ToString().Trim().Length == 0 || row["SecurityQuestionID"].ToString().Trim().ToUpper() == "NULL")
						mySecurityQuestionID = 0;
					else
						mySecurityQuestionID = int.Parse(row["SecurityQuestionID"].ToString().Trim());

					if (row["SecurityQuestionAnswer"].ToString().Trim().Length == 0 || row["SecurityQuestionAnswer"].ToString().Trim().ToUpper() == "NULL")
						mySecurityQuestionAnswer = null;
					else
						mySecurityQuestionAnswer = row["SecurityQuestionAnswer"].ToString().Trim();

					if (row["CreatedDate"].ToString().Trim().Length == 0 || row["CreatedDate"].ToString().Trim().ToUpper() == "NULL")
						myCreatedDate = DateTime.Today;
					else
					{
						DateTime myDate;
						if (DateTime.TryParse(row["CreatedDate"].ToString().Trim(), out myDate) == false)
						{
							myCreatedDate = DateTime.Today;
						}
						else
						{
							myCreatedDate = DateTime.Parse(row["CreatedDate"].ToString().Trim());
						}
					}

					if (row["Active"].ToString().Trim().Length == 0 || row["Active"].ToString().Trim().ToUpper() == "NULL")
						myActive = false;
					else
						myActive = Convert.ToBoolean(row["Active"]);

					if (row["OkToEmail"].ToString().Trim().Length == 0 || row["OkToEmail"].ToString().Trim().ToUpper() == "NULL")
						myOkToEmail = false;
					else
						myOkToEmail = Convert.ToBoolean(row["OkToEmail"]);

					if (row["OkToEmailHDN"].ToString().Trim().Length == 0 || row["OkToEmailHDN"].ToString().Trim().ToUpper() == "NULL")
						myOkToEmailHDN = false;
					else
						myOkToEmailHDN = Convert.ToBoolean(row["OkToEmailHDN"]);

					if (row["UserEmployeeID"].ToString().Trim().Length == 0 || row["UserEmployeeID"].ToString().Trim().ToUpper() == "NULL")
						myUserEmployeeID = null;
					else
						myUserEmployeeID = row["UserEmployeeID"].ToString().Trim();

					if (row["UserEmployeeType"].ToString().Trim().Length == 0 || row["UserEmployeeType"].ToString().Trim().ToUpper() == "NULL")
						myUserEmployeeType = null;
					else
						myUserEmployeeType = row["UserEmployeeType"].ToString().Trim();

					if (row["UserRoleID"].ToString().Trim().Length == 0 || row["UserRoleID"].ToString().Trim().ToUpper() == "NULL")
						myUserRoleID = 0;
					else
						myUserRoleID = int.Parse(row["UserRoleID"].ToString().Trim());

					if (row["AutoDeviceAssign"].ToString().Trim().Length == 0 || row["AutoDeviceAssign"].ToString().Trim().ToUpper() == "NULL")
						myAutoDeviceAssign = false;
					else
						myAutoDeviceAssign = Convert.ToBoolean(row["AutoDeviceAssign"]);

					if (row["MovableUser"].ToString().Trim().Length == 0 || row["MovableUser"].ToString().Trim().ToUpper() == "NULL")
						myMovableUser = 0;
					else
						myMovableUser = int.Parse(row["MovableUser"].ToString().Trim());

					if (row["LastReminded"].ToString().Trim().Length == 0 || row["LastReminded"].ToString().Trim().ToUpper() == "NULL")
						myLastReminded = NullDateTime;
					else
					{
						DateTime myDate;
						if (DateTime.TryParse(row["LastReminded"].ToString().Trim(), out myDate) == false)
						{
							myLastReminded = NullDateTime;
						}
						else
						{
							myLastReminded = DateTime.Parse(row["LastReminded"].ToString().Trim());
						}
					}


					if (myAccountID != 0)
					{
						errorMessage = "";
						if (
							PassValidation(myAccountID,
								myLocationID,
								myGroupID,
								myPrefix,
								myFirstName,
								myLastName,
								myUserName,
								row["BirthDate"].ToString().Trim(),
								myGender,
								myStateID,
								myCountryID,
								myEmail,
								myPassword,
								mySecurityQuestionID,
								mySecurityQuestionAnswer,
								row["CreatedDate"].ToString().Trim(),
								myUserEmployeeType,
								myUserRoleID,
								myMovableUser,
								row["LastReminded"].ToString().Trim(),
								ref errorMessage)
							)
						{
							// Commit user record here !!!!!!!!!!!!!!

							//myHashPassword = EncryptPassword(myPassword);
							if (myPassword != null)
								myHashPassword = HashPassword(myPassword);

							// Add user record.
							User myUser = new User
							{
								AccountID = myAccountID,
								LocationID = myLocationID,
								GroupID = myGroupID,
								Prefix = myPrefix,
								FirstName = myFirstName,
								MiddleName = myMiddleName,
								LastName = myLastName,
								UserName = myUserName,
								BirthDate = myBirthDate,
								Gender = Convert.ToChar(myGender),
								Address1 = myAddress1,
								Address2 = myAddress2,
								Address3 = myAddress3,
								City = myCity,
								StateID = myStateID,
								PostalCode = myPostalCode,
								CountryID = myCountryID,
								Email = myEmail,
								Telephone = myTelephone,
								Fax = myFax,
								CreatedDate = myCreatedDate,
								LastReminded = myLastReminded,
								Active = myActive,
								OkToEmail = myOkToEmail,
								OkToEmailHDN = myOkToEmailHDN,
								UserEmployeeID = myUserEmployeeID,
								UserEmployeeType = myUserEmployeeType,
								UserRoleID = myUserRoleID,
								AutoDeviceAssign = myAutoDeviceAssign,
								MovableUser = myMovableUser
							};

							// Add the auth user record.
							AuthUser newAuthUser = new AuthUser()
							{
								Active = true,
								MustChangePassword = true,
								CreatedDate = myCreatedDate,
								ModifiedDate = myCreatedDate,
								UserName = myUserName,
								Email = myEmail,
								PasswordHash = myHashPassword,
								SecurityQuestionID1 = mySecurityQuestionID,
								SecurityAnswer1 = mySecurityQuestionAnswer
							};

							// Add the user to the instadose application group. This grants the user access to instadose applications.
							newAuthUser.AuthUserApplicationGroups.Add(new AuthUserApplicationGroup()
							{
								ApplicationGroupID = (from a in idc.AuthApplicationGroups
													  where a.ApplicationGroupName == "Instadose"
													  select a.AuthApplicationGroupID).FirstOrDefault()
							});

							idc.Users.InsertOnSubmit(myUser);
							idc.AuthUsers.InsertOnSubmit(newAuthUser);
							idc.SubmitChanges();

							// Insert user to success list table
							InsertSuccessListTable(myAccountID.ToString(), myLocationID.ToString(), (myGroupID == null ? "" : myGroupID.ToString()), myFirstName, myLastName, myUserName, myPassword);
						}
						else
						{
							// Insert user to fail list table
							InsertFailListTable(myFirstName, myLastName, errorMessage);
							success = false;
						}
					}

				}   // End For loop


				if (success)    // if all users were loaded successfully
				{
					if (dtSuccess.Rows.Count > 0)
					{
						// Bind the data to the GridView
						GridViewSuccess.DataSource = dtSuccess.DefaultView;
						GridViewSuccess.DataBind();
						this.successSection.Visible = true;                       
						this.VisibleMessages("All users have been successfully uploaded.");
					}
				}
				else   // if all users were not loaded successfully
				{
					if (dtSuccess.Rows.Count > 0)
					{
						// Bind the data to the GridView
						GridViewSuccess.DataSource = dtSuccess.DefaultView;
						GridViewSuccess.DataBind();
						this.successSection.Visible = true;                        
						this.VisibleErrors("Some users have been successfully uploaded. Please adjust the users list and reload it.");  
					}
					else
					{                       
						this.VisibleErrors("None of users have been successfully uploaded. Please adjust the users list and reload it.");  
					}

					if (dtFail.Rows.Count > 0)
					{
						// Bind the data to the GridView
						GridViewFail.DataSource = dtFail.DefaultView;
						GridViewFail.DataBind();
						this.failSection.Visible = true;
					}
				}

			}
		}
		catch (Exception ex)
		{           
			this.VisibleErrors(ex.ToString());
		}
				
	}

	private string HashPassword(string password)
	{
		try
		{
			return Hash.GetHash(password, Hash.HashType.SHA256);
		}
		catch { return ""; }
	}

	//private string EncryptPassword(string password)
	//{
	//    try
	//    {
	//        return TripleDES.Encrypt(password.Trim());
	//    }
	//    catch { return ""; }
	//}

	private void InvisibleErrors()
	{
		// Reset submission form error message      
		this.errorMsg.InnerHtml = "";
		this.errors.Visible = false;
	}

	private void VisibleErrors(string error)
	{
		this.errorMsg.InnerHtml  = error;
		this.errors.Visible = true;
	}

	private void InvisibleMessages()
	{
		// Reset submission form error message      
		this.submitMsg.InnerHtml = "";
		this.messages.Visible = false;
	}

	private void VisibleMessages(string error)
	{
		this.submitMsg.InnerHtml = error;
		this.messages.Visible = true;
	}

}