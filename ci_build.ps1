dotnet restore
dotnet build --no-restore

if (${env:APPVEYOR_REPO_TAG} -eq $true)
{
	dotnet pack --no-restore `
	  -o artifacts_nuget `
	  --configuration Release
}
else
{
	dotnet pack --no-restore `
	  -o artifacts_nuget `
	  --version-suffix=${env:APPVEYOR_REPO_BRANCH}${env:APPVEYOR_BUILD_NUMBER} `
	  --configuration Release
}

ForEach ($proj in (Get-ChildItem -Path test -Recurse -Filter '*.csproj'))
{
	dotnet test --no-restore --no-build $proj.FullName
}
