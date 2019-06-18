---
services: virtual-network
platforms: dotnet
author: yaohaizh
---

# Getting started on managing client to virtual network VPN connection using C# #

          Azure Network sample for managing virtual network gateway.
           - Create a virtual network with subnets
           - Create virtual network gateway
           - Update virtual network gateway with Point-to-Site connection configuration
           - Generate and download VPN client configuration package. Now it can be used to create VPN connection to Azure.
           - Revoke a client certificate
         
           Please note: in order to run this sample, you need to have:
            - pre-generated root certificate and public key exported to $CERT_PATH file
               For more details please see https://docs.microsoft.com/en-us/azure/vpn-gateway/vpn-gateway-certificates-point-to-site for PowerShell instructions
               and https://docs.microsoft.com/en-us/azure/vpn-gateway/vpn-gateway-certificates-point-to-site-makecert for Makecert instructions.
            - client certificate generated for this root certificate installed on your machine.
               Please see: https://docs.microsoft.com/en-us/azure/vpn-gateway/point-to-site-how-to-vpn-client-install-azure-cert
            - thumbprint for client certificate saved to $CLIENT_CERT_THUMBPRINT


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/network-dotnet-manage-vpn-client-connection.git

    cd network-dotnet-manage-vpn-client-connection
  
    dotnet build
    
    bin\Debug\net452\ManageVpnGatewayPoint2SiteConnection.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.