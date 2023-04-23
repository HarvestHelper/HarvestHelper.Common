# HarvestHelper.Common

Common libraries used in HarvestHelper

## How to create and publish my package
```powershell
$version="1.0.14"
$owner="HarvestHelper" 
$gh_pat="[PAT HERE]"

dotnet pack src\HarvestHelper.Common\ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/HarvestHelper.Common -o ..\packages

dotnet nuget push ..\packages\HarvestHelper.Common.$version.nupkg --api-key $gh_pat --source "github" 
```
