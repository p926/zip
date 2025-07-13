using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Controls_ErrorLabel : System.Web.UI.UserControl
{
    public string Text
    {
        get { return lblMessage.Text; }
        set
        {
            Message.Visible = (value != "");
            lblMessage.Text = value;
        }
    }
}