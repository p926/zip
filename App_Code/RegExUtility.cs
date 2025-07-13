using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

/// <summary>
/// Summary description for RegExUtility
/// </summary>
public class RegExUtility
{
	public RegExUtility()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    private bool invalidEmailAddress = false;

    public bool ValidateEmailAddress(string emailAddress)
    {
        invalidEmailAddress = false;
        if (String.IsNullOrEmpty(emailAddress))
            return false;

        // Use IdnMapping class to convert Unicode domain names.
        emailAddress = Regex.Replace(emailAddress, @"(@)(.+)$", DomainMapper);
        if (invalidEmailAddress)
            return false;

        // Return true if strIn is in valid e-mail format.
        return Regex.IsMatch(emailAddress,
               @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
               @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
               RegexOptions.IgnoreCase);
    }
    private string DomainMapper(Match match)
    {
        // IdnMapping class with default property values.
        IdnMapping idn = new IdnMapping();

        string domainName = match.Groups[2].Value;
        try
        {
            domainName = idn.GetAscii(domainName);
        }
        catch (ArgumentException)
        {
            invalidEmailAddress = true;
        }
        return match.Groups[1].Value + domainName;
    }
}