<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PushCommerce.aspx.cs" Inherits="Sitecore.HabitatHome.Foundation.CoveoIndexing.PushCommerce" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Push Sitecore Commerce Sellable Items to Coveo Cloud</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="PushButton" Text="Push" OnClick="HandlePushButtonClick" runat="server"/>
            <h2>Logs</h2>
            <pre ID="LogPanel" runat="server"></pre>
        </div>
    </form>
</body>
</html>
