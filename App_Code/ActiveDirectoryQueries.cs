using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.DirectoryServices;
using System.Web;

public static class ActiveDirectoryQueries
{

    /// <summary>
    /// Gets a list of groups a user belongs to.
    /// </summary>
    /// <param name="loginName">Domain and username.</param>
    /// <returns>List of group names.</returns>
    public static List<string> GetADUserGroups(string loginName)
    {
        if (string.IsNullOrEmpty(loginName))
        {
            return new List<string>();
        }

        string userName = ExtractUserName(loginName);

        DirectorySearcher search = new DirectorySearcher();
        search.Filter = String.Format("(SAMAccountName={0})", userName);
        search.PropertiesToLoad.Add("memberOf");
        List<string> groupsList = new List<string>();

        SearchResult result = search.FindOne();
        if (result != null)
        {
            int groupCount = result.Properties["memberOf"].Count;

            for (int counter = 0; counter < groupCount; counter++)
            {
                string group = (string)result.Properties["memberOf"][counter];
                string[] groupData = group.Split(',');
                groupsList.Add(groupData[0].Replace("CN=", ""));
            }
        }
        return groupsList;
    }

    /// <summary>
    /// Tests if a user belongs to a specific group.
    /// </summary>
    /// <param name="loginName">Domain and username.</param>
    /// <param name="groupName">Tested group</param>
    /// <returns>Success/Failure</returns>
    public static bool UserExistsInGroup(string loginName, string groupName)
    {
        List<string> groupsList = GetADUserGroups(loginName);

        return groupsList.Contains(groupName);
    }

    public static bool UserExistsInAnyGroup(string loginName, string[] groupNames)
    {
        List<string> groupsList = GetADUserGroups(loginName);

        foreach (string group in groupNames)
        {
            if (groupsList.Contains(group))
                return true;
        }

        return false;
    }


    /// <summary>
    /// Tests if a user exists in active directory or not.
    /// </summary>
    /// <param name="loginName">Domain and username.</param>
    /// <returns>Success/Failure</returns>
    public static bool UserExists(string loginName)
    {
        string userName = ExtractUserName(loginName);
        DirectorySearcher search = new DirectorySearcher();
        search.Filter = String.Format("(SAMAccountName={0})", userName);
        search.PropertiesToLoad.Add("cn");
        SearchResult result = search.FindOne();


        return (result != null);
    }

    /// <summary>
    /// Extracts the username from the DOMAIN\USER formatted login.
    /// </summary>
    /// <param name="path">DOMAIN\USER formatted user</param>
    /// <returns>Username</returns>
    private static string ExtractUserName(string path)
    {
        string[] userPath = path.Split(new char[] { '\\' });
        return userPath[userPath.Length - 1];
    }

    public static string GetUserName()
    {
        //return HttpContext.Current.User.Identity.Name.Split('\\')[1];
        string userName = "";
        try
        {
            userName = HttpContext.Current.User.Identity.Name.Split('\\')[1];
        }
        catch
        {
            userName = "Unknown";
        }
        return userName;
    }
}