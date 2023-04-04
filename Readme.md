# HarvestHelper.Common

Common libraries used in HarvestHelper

## How to creat and publish my package
```powershell
$version="1.0.4"
$owner="HarvestHelper" 

dotnet pack src\HarvestHelper.Common\ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/HarvestHelper.Common -o ..\packages
```
