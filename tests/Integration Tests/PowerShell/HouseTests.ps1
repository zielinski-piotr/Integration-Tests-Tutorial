#region GetById_Should
function GetById_Should_Return_Valid_Entity() {

    $testName = "GetById_Should_Return_Valid_Entity"

    # Arrange
    $house = [Houses]::House1
    $houseId = $house.Id
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "GET" $null

    # Assert

    if (([int][HttpStatusCode]::OK) -eq $response.StatusCode) {

        $house = $response.Content | ConvertFrom-Json

        LogOutcome $testName ( 
            Assert $house.Id $house.Id ||
            Assert $house.Address.Street $house.Address.Street
        )
    }
    else {
        LogFail $testName
    }
}

function GetById_Should_Return_404_When_Entity_NotFound() {
    $testName = "GetById_Should_Return_404_When_Entity_NotFound"
    $uri = "House/$([Guid]::NewGuid())"
    
    $response = MakeWebRequest $uri "GET" $null

    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::NotFound) $response.StatusCode
    )
}

#endregion

#region Patch_Should

Function Patch_Should_Update_House_Successfully() {
    $testName = "Patch_Should_Update_House_Successfully"

    # Arrange
    $newStreetName = "New street name";
    $patch = "[{'op': 'replace', 'path': '/address/street', 'value' : '$newStreetName'}]";
    $house = [Houses]::House8
    $houseId = $house.Id;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "Patch" $patch $true

    # Assert response
    if (([int][HttpStatusCode]::Accepted) -eq $response.StatusCode) {

        # Assert persisted entity after Patch 
        $houseAfterPatchResponse = MakeWebRequest $uri "GET" $null

        if (([int][HttpStatusCode]::OK) -eq $houseAfterPatchResponse.StatusCode) {

            $houseAfterPatch = $houseAfterPatchResponse | ConvertFrom-Json

            return LogOutcome $testName ( 
                Assert $newStreetName $houseAfterPatch.Address.Street )
        }
    }

    LogFail $testName
}

Function Patch_Should_Return_UnprocessableEntity_When_Trying_To_Patch_Not_Existing_Property([string] $operation) {
    $testName = "Patch_Should_Return_UnprocessableEntity_When_Trying_To_Patch_Not_Existing_Property - Operation: '$operation'"

    # Arrange
    $patch = "[{'op': '$operation', 'path': '/address/street/length', 'value' : 'length'}]";
    $house = [Houses]::House8
    $houseId = $house.Id;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "Patch" $patch $true

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::UnprocessableEntity) $response.StatusCode )
    
}

Function Patch_Should_Add_Room_To_House_Successfully() {
    $testName = "Patch_Should_Add_Room_To_House_Successfully"

    # Arrange
    $patch = "[{'op': 'add', 'path': '/rooms/-', 'value' : {'name' : 'Room1', 'color' : 'Red', 'area' : '12.2' }}]"
    $house = [Houses]::House1
    $houseId = $house.Id;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "Patch" $patch $true

    # Assert response
    if (([int][HttpStatusCode]::Accepted) -eq $response.StatusCode) {

        # Assert persisted entity after Patch 
        $houseAfterPatchResponse = MakeWebRequest $uri "GET" $null

        if (([int][HttpStatusCode]::OK) -eq $houseAfterPatchResponse.StatusCode) {

            $houseAfterPatch = $houseAfterPatchResponse | ConvertFrom-Json

            return LogOutcome $testName ( 
                Assert 1 $houseAfterPatch.Rooms.Count )
        }
    }

    LogFail $testName
}

Function Patch_Should_Replace_Rooms_In_House_Successfully() {
    $testName = "Patch_Should_Replace_Rooms_In_House_Successfully"

    # Arrange
    $patch = "[{'op': 'replace', 'path': '/rooms', 'value' : [{'name' : 'Room1', 'color' : 'Red', 'area' : '12.2' }, {'name' : 'Room2', 'color' : 'Yellow', 'area' : '24.2' }]}]"
    $house = [Houses]::House3
    $houseId = $house.Id;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "Patch" $patch $true

    # Assert response
    if (([int][HttpStatusCode]::Accepted) -eq $response.StatusCode) {

        # Assert persisted entity after Patch 
        $houseAfterPatchResponse = MakeWebRequest $uri "GET" $null

        if (([int][HttpStatusCode]::OK) -eq $houseAfterPatchResponse.StatusCode) {

            $houseAfterPatch = $houseAfterPatchResponse | ConvertFrom-Json

            return LogOutcome $testName ( 
                Assert 2 $houseAfterPatch.Rooms.Count )
        }
    }

    LogFail $testName
}

Function Patch_Should_Return_UnprocessableEntity_When_Trying_To_Remove_Room_By_Id_Not_Index_In_Array() {
    # Arrange
    $house = [Houses]::House2
    $lastRoom = Last $house.Rooms
    $patch = "[{'op': 'remove', 'path': '/rooms/$($lastRoom.Id)' }]";
    $house = [Houses]::House2
    $houseId = $house.Id;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "Patch" $patch $true

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::UnprocessableEntity) $response.StatusCode )
}

#endregion