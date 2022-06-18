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
    
    # Arrange
    $uri = "House/$([Guid]::NewGuid())"
    
    # Act
    $response = MakeWebRequest $uri "GET" $null

    #Assert
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
    $testName = "Patch_Should_Return_UnprocessableEntity_When_Trying_To_Remove_Room_By_Id_Not_Index_In_Array"

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

Function Patch_Should_Return_404_When_House_Not_Exists() {
    $testName = "Patch_Should_Return_404_When_House_Not_Exists"

    # Arrange
    $patch = "[{'op': 'replace', 'path': '/address/street', 'value' : 'New street name'}]";
    $houseId = [Guid]::NewGuid;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "Patch" $patch $true

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::NotFound) $response.StatusCode)
}

#endregion

#region Update_Should

Function Update_Should_Return_404_When_House_Not_Exists() {
    $testName = "Update_Should_Return_404_When_House_Not_Exists"

    # Arrange
    $body = @{
        Area  = 51;
        Color = "Azure";
        Name  = "Some New Name";
    }

    $houseId = [Guid]::NewGuid;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "PUT" $body $true

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::NotFound) $response.StatusCode)
}

Function Update_Should_Return_400_When_Request_Body_Invalid() {
    $testName = "Update_Should_Return_400_When_Request_Body_Invalid"

    # Arrange
    $body = @{
        Area  = 51;
        Color = $null;
        Name  = "Some New Name";
    }

    $house = [Houses]::House2
    $houseId = $house.Id;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "PUT" $body $true

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::BadRequest) $response.StatusCode)
}

Function Update_Should_Update_House_Successfully() {
    $testName = "Update_Should_Update_House_Successfully"

    # Arrange
    $body = @{
        Area  = 51;
        Color = "Orange";
        Name  = "Some New Name";
    }

    $house = [Houses]::House10
    $houseId = $house.Id;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "PUT" $body

    # Assert response
    if (([int][HttpStatusCode]::Accepted) -eq $response.StatusCode) {

        # Assert persisted entity after Update 
        $houseAfterUpdateResponse = MakeWebRequest $uri "GET" $null 

        if (([int][HttpStatusCode]::OK) -eq $houseAfterUpdateResponse.StatusCode) {

            $houseAfterUpdate = $houseAfterUpdateResponse | ConvertFrom-Json

            return LogOutcome $testName ( 
                Assert $body.Area $houseAfterUpdate.Area ||  
                Assert $body.Color $houseAfterUpdate.Color || 
                Assert $body.Name $houseAfterUpdate.Name )
        }
    }

    LogFail $testName
}

Function Delete_Should_Return_404_When_House_Not_Exists() {
    $testName = "Delete_Should_Return_404_When_House_Not_Exists"

    # Arrange
    $houseId = [Guid]::NewGuid;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "Delete"

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::NotFound) $response.StatusCode)
}

Function Delete_Should_Return_400_When_House_Empty_Guid_Passed() {
    $testName = "Delete_Should_Return_400_When_House_Empty_Guid_Passed"

    # Arrange
    $houseId = [Guid]::Empty;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "Delete"

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::BadRequest) $response.StatusCode)
}

Function Delete_Should_DeleteEntity() {
    $testName = "Delete_Should_DeleteEntity"

    # Arrange
    $house = [Houses]::House10
    $houseId = $house.Id;
    $uri = "House/$houseId"

    # Act
    $response = MakeWebRequest $uri "Delete"

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::Accepted) $response.StatusCode)
}

#endregion

#region Create_Should

Function Create_Should_CreateEntity() {
    $testName = "Create_Should_CreateEntity"

    # Arrange
    $body = @{
        Area  = 51;
        Color = "Orange";
        Name  = "Some New Name";
    }

    # Act
    $response = MakeWebRequest "House" "POST" $body

    # Assert response
    if (([int][HttpStatusCode]::Created) -eq $response.StatusCode) {

        $house = $response.Content | ConvertFrom-Json

        # Assert persisted entity after Update 
        $houseAfterUpdateResponse = MakeWebRequest "House/$($house.Id)" "GET" $null 

        if (([int][HttpStatusCode]::OK) -eq $houseAfterUpdateResponse.StatusCode) {

            $houseAfterUpdate = $houseAfterUpdateResponse | ConvertFrom-Json

            return LogOutcome $testName ( 
                Assert $body.Area $houseAfterUpdate.Area ||  
                Assert $body.Color $houseAfterUpdate.Color || 
                Assert $body.Name $houseAfterUpdate.Name )
        }
    }

    LogFail $testName
}

Function Create_Should_Return_400_When_Name_IsEmpty() {
    $testName = "Create_Should_Return_400_When_Name_IsEmpty"

    # Arrange
    $body = @{
        Area  = 51;
        Color = "Orange";
        Name  = "";
    }

    # Act
    $response = MakeWebRequest "House" "POST" $body

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::BadRequest) $response.StatusCode)
}

Function Create_Should_Return_400_When_Name_IsNull() {
    $testName = "Create_Should_Return_400_When_Name_IsNull"

    # Arrange
    $body = @{
        Area  = 51;
        Color = "Orange";
        Name  = $null;
    }

    # Act
    $response = MakeWebRequest "House" "POST" $body

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::BadRequest) $response.StatusCode)
}

Function Create_Should_Return_400_When_Color_IsEmpty() {
    $testName = "Create_Should_Return_400_When_Color_IsEmpty"

    # Arrange
    $body = @{
        Area  = 51;
        Color = "";
        Name  = "Some Name";
    }

    # Act
    $response = MakeWebRequest "House" "POST" $body

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::BadRequest) $response.StatusCode)
}

Function Create_Should_Return_400_When_Color_IsNull() {
    $testName = "Create_Should_Return_400_When_Color_IsNull"

    # Arrange
    $body = @{
        Area  = 51;
        Color = $null;
        Name  = "Some Name";
    }

    # Act
    $response = MakeWebRequest "House" "POST" $body

    # Assert response
    LogOutcome $testName ( 
        Assert ([int][HttpStatusCode]::BadRequest) $response.StatusCode)
}

#endregion