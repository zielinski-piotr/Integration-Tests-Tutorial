function Authenticate([string] $userName, [string] $password) {
    $body = @{
        Email    = $userName
        Password = $password
    }

    $loginResponse = Invoke-WebRequest "$($baseUrl)/auth" -Body ($body | ConvertTo-Json) -Method "POST" -ContentType "application/json"
    
    if ($loginResponse.StatusCode -ne [HttpStatusCode]::OK -or $null -eq $loginResponse.Content) {
        LogError "Could not login to the service"
    }

    $responseObject = $loginResponse.Content | ConvertFrom-Json
    
    $token = $responseObject.AccessToken

    return $token
}

function LogError([string] $message) {
    Write-Error -Message $message
}

function LogSuccess([string] $testName) {
    Write-Host "$testName Passed" -ForegroundColor Green
}

function LogFail([string] $testName) {
    Write-Host "$testName Failed" -ForegroundColor Red
}

function LogOutcome([string] $testName, [bool] $outcome) {
    if ($outcome -eq $true) {
        LogSuccess $testName
    }
    else {
        LogFail $testName
    }
}

function LogAssertionFail([string] $message) {
    Write-Host $message -ForegroundColor Red
}

function MakeWebRequest {
    param (
        [Parameter(Mandatory=$true)]
        $uri,
        [Parameter(Mandatory=$true)]
        $method,
        [Parameter(Mandatory=$false)]
        $body,
        [Parameter(Mandatory=$false)]
        $bodyIsJSON
    )

    if($null -eq $bodyIsJSON) {
        $bodyIsJSON = $false
    }

    $secureToken = ConvertTo-SecureString -String $token -AsPlainText

    if ($null -ne $body) {
        $response = Invoke-WebRequest "$baseUrl/$uri" -Body ($bodyIsJSON ? $body : ($body | ConvertTo-Json)) -Method $method -Authentication Bearer -Token $secureToken -ContentType "application/json" -SkipHttpErrorCheck 
    }
    else {
        $response = Invoke-WebRequest "$baseUrl/$uri" -Method $method -Authentication Bearer -Token $secureToken -ContentType "application/json" -SkipHttpErrorCheck 
    }

    return $response
}

function Assert([object] $expectedValue, [object] $actualValue) {
    if ($expectedValue -ne $actualValue) {
        LogAssertionFail $"Actual Value: '$($actualValue)' differs from Expected Value: '$($expectedValue)'"

        return $false
    }

    return $true
}

Function Last([System.Collections.IEnumerable] $enumerable) {
    return [Linq.Enumerable]::Last($enumerable)
}