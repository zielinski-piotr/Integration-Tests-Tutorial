using namespace System.Net;
using namespace Seeding;
using namespace Domain;

Param (
    [Parameter(Mandatory = $true)][string]$baseAddress,
    [Parameter(Mandatory = $true)][string]$adminUserName,
    [Parameter(Mandatory = $true)][string]$adminPassword
)

$ErrorActionPreference = "Stop"

$path = (Get-Location).Path

Add-Type -AssemblyName .\Domain.dll
Add-Type -AssemblyName .\Seeding.dll

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
