using namespace System.Net;
using namespace Seeding;
using namespace Domain;

# To run the PowerShell Integration tests, copy Domain.dll and Seeding.dll from src\WebApi\bin\Debug\net6.0 to bin\ directory
# Then execute with command: .\RunTests.ps1 "https://localhost:44364" "test@grpc.test" "p@sSw0rd123"

Param (
    [Parameter(Mandatory = $true)][string]$baseAddress,
    [Parameter(Mandatory = $true)][string]$adminUserName,
    [Parameter(Mandatory = $true)][string]$adminPassword
)

$ErrorActionPreference = "Stop"

$path = (Get-Location).Path

Add-Type -AssemblyName .\Bin\Domain.dll
Add-Type -AssemblyName .\Bin\Seeding.dll

. "$($path)\GlobalScripts.ps1"
. "$($path)\GlobalVariables.ps1"
. "$($path)\HouseTests.ps1"

$global:baseUrl = $baseAddress
$global:token = Authenticate $adminUserName $adminPassword

$ErrorActionPreference = "Continue"

GetById_Should_Return_Valid_Entity
GetById_Should_Return_404_When_Entity_NotFound

Patch_Should_Update_House_Successfully
Patch_Should_Return_UnprocessableEntity_When_Trying_To_Patch_Not_Existing_Property "add"
Patch_Should_Return_UnprocessableEntity_When_Trying_To_Patch_Not_Existing_Property "remove"
Patch_Should_Return_UnprocessableEntity_When_Trying_To_Patch_Not_Existing_Property "replace"
Patch_Should_Add_Room_To_House_Successfully
Patch_Should_Replace_Rooms_In_House_Successfully
Patch_Should_Return_UnprocessableEntity_When_Trying_To_Remove_Room_By_Id_Not_Index_In_Array
Patch_Should_Return_404_When_House_Not_Exists

Update_Should_Return_404_When_House_Not_Exists
Update_Should_Return_400_When_Request_Body_Invalid
Update_Should_Update_House_Successfully

Delete_Should_Return_404_When_House_Not_Exists
Delete_Should_Return_400_When_House_Empty_Guid_Passed
Delete_Should_DeleteEntity

Create_Should_CreateEntity
Create_Should_Return_400_When_Name_IsEmpty
Create_Should_Return_400_When_Name_IsNull
Create_Should_Return_400_When_Color_IsEmpty
Create_Should_Return_400_When_Color_IsNull