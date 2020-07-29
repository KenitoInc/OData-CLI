param(
 [Parameter(Mandatory=$True)]
 [string]
 $subscriptionId,

 [string]
 $resourceGroupName = "ODATASAMPLERGROUP49",

 [string]
 $resourceGroupLocation = "Central US",

 [string]
 $deploymentName = "ODATASAMPLEDEPLOYMENT49",

 [string]
 $templateFilePath = "C:\armdeps\azuredeploy.json",

 [string]
 $parametersFilePath = "C:\armdeps\azuredeploy.parameters.json",

 [string]
 $projectFilePath = "C:\newcode\ODataApiService\EdmObjectsGenerator.zip"

)

#******************************************************************************
# Script body
# Execution begins here
#******************************************************************************
$ErrorActionPreference = "Stop"

# sign in
Write-Host "Logging in...";
Login-AzureRmAccount;

# select subscription
Write-Host "Selecting subscription '$subscriptionId'";
Select-AzureRmSubscription -SubscriptionID $subscriptionId;

#Create or check for existing resource group
$resourceGroup = Get-AzureRmResourceGroup -Name $resourceGroupName -ErrorAction SilentlyContinue
if(!$resourceGroup)
{
    Write-Host "Creating resource group '$resourceGroupName' in location '$resourceGroupLocation'";
    New-AzureRmResourceGroup -Name $resourceGroupName -Location $resourceGroupLocation
}
else{
    Write-Host "Using existing resource group '$resourceGroupName'";
}

# Start the deployment
Write-Host "Starting deployment...";
if(Test-Path $parametersFilePath) {
    $outputs = New-AzureRmResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile $templateFilePath -TemplateParameterFile $parametersFilePath;  
    foreach ($key in $outputs.Outputs.keys) {
        if ($key -eq "wName") {
            $AppName = $outputs.Outputs[$key].value;
            Write-Host "App Service Name '$AppName'";
            az webapp config appsettings set --resource-group $resourceGroupName --name $AppName --settings SCM_DO_BUILD_DURING_DEPLOYMENT=true
            az webapp deployment source config-zip -g $resourceGroupName -n $AppName --src $projectFilePath
        }
        else 
        {
            if($key -eq "ResourceId")
            {
                $AppURL = $outputs.Outputs[$key].value;            
            }        
        }
    }

    Write-Host "The SAMPLE ODATA URL IS: '$AppURL'"; 
} 
else {
    New-AzureRmResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile $templateFilePath;
}