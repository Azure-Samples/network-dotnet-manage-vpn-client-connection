// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;

namespace ManageVpnGatewayPoint2SiteConnection
{
    public class Program
    {
        private static ResourceIdentifier? _resourceGroupId = null;

        /**
         * Azure Network sample for managing virtual network gateway.
         *  - Create a virtual network with subnets
         *  - Create virtual network gateway
         *  - Update virtual network gateway with Point-to-Site connection configuration
         *  - Generate and download VPN client configuration package. Now it can be used to create VPN connection to Azure.
         *  - Revoke a client certificate
         *
         *  Please note: in order to run this sample, you need to have:
         *   - pre-generated root certificate and public key exported to $CERT_PATH file
         *      For more details please see https://docs.microsoft.com/en-us/azure/vpn-gateway/vpn-gateway-certificates-point-to-site for PowerShell instructions
         *      and https://docs.microsoft.com/en-us/azure/vpn-gateway/vpn-gateway-certificates-point-to-site-makecert for Makecert instructions.
         *   - client certificate generated for this root certificate installed on your machine.
         *      Please see: https://docs.microsoft.com/en-us/azure/vpn-gateway/point-to-site-how-to-vpn-client-install-azure-cert
         *   - thumbprint for client certificate saved to $CLIENT_CERT_THUMBPRINT
         */
        public static async Task RunSample(ArmClient client)
        {
            string rgName = Utilities.CreateRandomName("NetworkSampleRG");
            string vnetName = Utilities.CreateRandomName("vnet");
            string pipName = Utilities.CreateRandomName("pip");
            string vpnGatewayName = Utilities.CreateRandomName("vngw");
            string certPath = Environment.GetEnvironmentVariable("CERT_PATH");
            string clientCertThumbprint = Environment.GetEnvironmentVariable("CLIENT_CERT_THUMBPRINT");

            try
            {
                // Get default subscription
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();

                // Create a resource group in the EastUS region
                Utilities.Log($"Creating resource group...");
                ArmOperation<ResourceGroupResource> rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS));
                ResourceGroupResource resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;
                Utilities.Log("Created a resource group with name: " + resourceGroup.Data.Name);

                //============================================================
                // Create virtual network with address spaces 192.168.0.0/16 and 10.254.0.0/16 and 3 subnets
                Utilities.Log("Creating virtual network...");
                VirtualNetworkData vnetInput = new VirtualNetworkData()
                {
                    Location = resourceGroup.Data.Location,
                    AddressPrefixes = { "192.168.0.0/16", "10.254.0.0/16" },
                    Subnets =
                    {
                        new SubnetData() { AddressPrefix = "192.168.200.0/24", Name = "GatewaySubnet" },
                        new SubnetData() { AddressPrefix = "192.168.1.0/24", Name = "FrontEnd" },
                        new SubnetData() { AddressPrefix = "10.254.1.0/24", Name = "BackEnd" }
                    },
                };
                var vnetLro = await resourceGroup.GetVirtualNetworks().CreateOrUpdateAsync(WaitUntil.Completed, vnetName, vnetInput);
                VirtualNetworkResource vnet = vnetLro.Value;
                Utilities.Log($"Created a virtual network: {vnet.Data.Name}");

                //    .WithAddressSpace("192.168.0.0/16")
                //    .WithAddressSpace("10.254.0.0/16")
                //    .WithSubnet("GatewaySubnet", "192.168.200.0/24")
                //    .WithSubnet("FrontEnd", "192.168.1.0/24")
                //    .WithSubnet("BackEnd", "10.254.1.0/24")

                //============================================================
                // Create public ip for virtual network gateway
                var pip = await Utilities.CreatePublicIP(resourceGroup, pipName);

                // Create VPN gateway
                Utilities.Log("Creating virtual network gateway...");
                VirtualNetworkGatewayData vpnGatewayInput = new VirtualNetworkGatewayData()
                {
                    Location = resourceGroup.Data.Location,
                    Sku = new VirtualNetworkGatewaySku()
                    {
                        Name = VirtualNetworkGatewaySkuName.Basic,
                        Tier = VirtualNetworkGatewaySkuTier.Basic
                    },
                    Tags = { { "key", "value" } },
                    EnableBgp = false,
                    GatewayType = VirtualNetworkGatewayType.Vpn,
                    VpnType = VpnType.RouteBased,
                    IPConfigurations =
                    {
                        new VirtualNetworkGatewayIPConfiguration()
                        {
                            Name = Utilities.CreateRandomName("config"),
                            PrivateIPAllocationMethod = NetworkIPAllocationMethod.Dynamic,
                            PublicIPAddressId  = pip.Data.Id,
                            SubnetId = vnet.Data.Subnets.First(item => item.Name == "GatewaySubnet").Id,
                        }
                    }
                };
                var vpnGatewayLro = await resourceGroup.GetVirtualNetworkGateways().CreateOrUpdateAsync(WaitUntil.Completed, vpnGatewayName, vpnGatewayInput);
                VirtualNetworkGatewayResource vpnGateway = vpnGatewayLro.Value;
                Utilities.Log($"Created virtual network gateway: {vpnGateway.Data.Name}");

                //IVirtualNetworkGateway vngw1 = azure.VirtualNetworkGateways.Define(vpnGatewayName)
                //    .WithRegion(region)
                //    .WithExistingResourceGroup(rgName)
                //    .WithExistingNetwork(network)
                //    .WithRouteBasedVpn()
                //    .WithSku(VirtualNetworkGatewaySkuName.VpnGw1)
                //    .Create();

                //============================================================
                // Update virtual network gateway with Point-to-Site connection configuration
                Utilities.Log("Creating Point-to-Site configuration...");
                vngw1.Update()
                    .DefinePointToSiteConfiguration()
                    .WithAddressPool("172.16.201.0/24")
                    .WithAzureCertificateFromFile("p2scert.cer", new FileInfo(certPath))
                    .Attach()
                    .Apply();
                Utilities.Log("Created Point-to-Site configuration");

                //============================================================
                // Generate and download VPN client configuration package. Now it can be used to create VPN connection to Azure.
                Utilities.Log("Generating VPN profile...");
                String profile = vngw1.GenerateVpnProfile();
                Utilities.Log(String.Format("Profile generation is done. Please download client package at: %s", profile));

                // At this point vpn client package can be downloaded from provided link. Unzip it and run the configuration corresponding to your OS.
                // For Windows machine, VPN client .exe can be run. For non-Windows, please use configuration from downloaded VpnSettings.xml

                //============================================================
                // Revoke a client certificate. After this command, you will no longer available to connect with the corresponding client certificate.
                Utilities.Log("Revoking client certificate...");
                vngw1.Update().UpdatePointToSiteConfiguration()
                    .WithRevokedCertificate("p2sclientcert.cer", clientCertThumbprint)
                    .Parent()
                    .Apply();
                Utilities.Log("Revoked client certificate");
            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Utilities.Log($"Deleting Resource Group...");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Utilities.Log($"Deleted Resource Group: {_resourceGroupId.Name}");
                    }
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception ex)
                {
                    Utilities.Log(ex);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                await RunSample(client);
            }
            catch (Exception ex)
            {
                Utilities.Log(ex);
            }
        }
    }
}