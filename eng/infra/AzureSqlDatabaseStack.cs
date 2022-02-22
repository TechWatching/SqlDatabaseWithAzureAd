using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Sql;
using Pulumi.AzureNative.Sql.Inputs;
using Pulumi.Command.Local;
using System;
using System.Net.Http;
using Deployment = Pulumi.Deployment;
using AD = Pulumi.AzureAD;

class AzureSqlDatabaseStack : Stack
{
    public AzureSqlDatabaseStack()
    {
        var config = new Config();
        var sqlAdAdminLogin = config.Require("sqlAdAdmin");
        var sqlAdAdminPassword = config.RequireSecret("sqlAdPassword");

        var sqlAdAdmin = new AD.User("sqlAdmin", new AD.UserArgs
        {
            UserPrincipalName = sqlAdAdminLogin,
            Password = sqlAdAdminPassword,
            DisplayName = "Global SQL Admin"
        });

        var resourceGroup = new ResourceGroup($"rg-sqlDbWithAzureAd-{Deployment.Instance.StackName}");

        var sqlServer = new Server($"sql-sqlDbWithAzureAd-{Deployment.Instance.StackName}", new ServerArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Administrators = new ServerExternalAdministratorArgs
            {
                Login = sqlAdAdmin.UserPrincipalName,
                Sid = sqlAdAdmin.Id,
                AzureADOnlyAuthentication = true,
                AdministratorType = AdministratorType.ActiveDirectory,
                PrincipalType = PrincipalType.User,
            },
        });

        var publicIp = Output.Create(new HttpClient().GetStringAsync("https://api.ipify.org"));

        var enableLocalMachine = new FirewallRule("AllowLocalMachine", new FirewallRuleArgs
        {
            ResourceGroupName = resourceGroup.Name,
            ServerName = sqlServer.Name,
            StartIpAddress = publicIp,
            EndIpAddress = publicIp
        });

        var database = new Database("sqldb-sqlDbWithAzureAd-Main", new DatabaseArgs
        {
            ResourceGroupName = resourceGroup.Name,
            ServerName = sqlServer.Name,
            Sku = new SkuArgs
            {
                Name = "Basic"
            }
        });

        var sqlDatabaseAuthorizedGroup = new AD.Group("SqlDbUsersGroup", new AD.GroupArgs
        {
            DisplayName = "SqlDbUsersGroup",
            SecurityEnabled = true,
            Owners = new InputList<string> { sqlAdAdmin.Id }
        });

        var authorizeAdGroup = new Command("AuthorizeAdGroup", new CommandArgs
        {
            Create = Output.Format($"sqlcmd -S {sqlServer.Name}.database.windows.net -d {database.Name} -U {sqlAdAdmin.UserPrincipalName} -P {sqlAdAdmin.Password} -G -l 30 -Q 'CREATE USER {sqlDatabaseAuthorizedGroup.DisplayName} FROM EXTERNAL PROVIDER; ALTER ROLE db_datareader ADD MEMBER {sqlDatabaseAuthorizedGroup.DisplayName}; ALTER ROLE db_datawriter ADD MEMBER {sqlDatabaseAuthorizedGroup.DisplayName};'"),
            Interpreter = new InputList<string>
            {
                "pwsh",
                "-c"
            }
        });

        SqlDatabaseConnectionString = Output.Format($"Server={sqlServer.Name}.database.windows.net; Authentication=Active Directory Default; Database={database.Name}");
    }

    [Output]
    public Output<string> SqlDatabaseConnectionString { get; set; }
}
