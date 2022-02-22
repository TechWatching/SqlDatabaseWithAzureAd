# Sample code for the article "How to provision an Azure SQL Database with Active Directory authentication"

## What is it?

This repository contains the code used in this [blog article](https://www.techwatching.dev/posts/sqldatabase-active-directory-authent) that talks about provisioning an Azure SQL Database with Active Directory authentication configured, using [Pulumi](https://www.pulumi.com/).

This code is a Pulumi program that can be executed from the Pulumi CLI. When you execute it, it will provision the following Azure resources:
- a resource group
- an Active Directory user
- an Active Directory group
- an Azure SQL Server
- an Azure SQL Database

The SQL Server provisioned will be configured to use Azure AD authentication with the created AD user as its admin. The AD group will have access to the database with `db_reader` and `db_writer` roles.

I suggest you to read [the article](https://www.techwatching.dev/posts/sqldatabase-active-directory-authent) before using this code. And if you are not familiar with Pulumi you should check their [documentation](https://www.pulumi.com/docs/) or [learning pathways](https://www.pulumi.com/learn/) too.

## How to use it ?

### Code organization

The infrastructure code is located in the `eng/infra/`folder. If you wan to open it in Visual Studio there is a solution file `Infra.sln` located at the root of the repository but you can open the code in the IDE of your choice.

### Prerequisites

You can check [Pulumi documentation](https://www.pulumi.com/docs/get-started/azure/begin/) to set up your environment. You will have to install pulumi and azure cli on your machine.

You will need an Azure subscription where the Pulumi program will create Azure resources.

You can use any [backend](https://www.pulumi.com/docs/intro/concepts/state/) for your Pulumi program (to store the state and encrypt secrets) but I suggest you to use the default backend: the Pulumi Service. It's free for individuals, you will just need to create an account on Pulumi website. If you prefer to use an Azure Blob Storage backend with an Azure Key Vault as the encryption provider you can check [this article](https://www.techwatching.dev/posts/pulumi-azure-backend).

Before executing the program you need to modify the `Pulumi.dev.yaml` configuration file to set the admin email and password you want for the Azure AD user that will be created and be the SQL Server administrator. You can do that by removing the 2nd and 3rd lines in the file and by executing the following commands:

```pwsh
pulumi config set SqlDatabaseWithAzureAd:sqlAdAdmin changethisemail@yourtenant.com
pulumi config --secret SqlDatabaseWithAzureAd:sqlAdPassword yoursecurePassword1234$
```

### Execute the Pulumi program

- clone this repository
- open a terminal in the `eng/infra` folder
- log on to your Azure account
- log on to your Pulumi backend
- run this command `pulumi up`



