param (
    [Parameter(Mandatory=$true)][string]$server,
    [Parameter(Mandatory=$true)][string]$database,
    [Parameter(Mandatory=$true)][string]$username,
    [Parameter(Mandatory=$true)][string]$password,
    [int]$keyLength = 256
 )

$key = $null

for ( $i = 1; $i -le $keyLength; $i++) {
    $key += ( -join ((48..57) + (65..90) + (97..122) | Get-Random | % {[char]$_}) )
}

$json = '{
    "DB": {
        "Server": "' + $server + '",
        "Database": "' + $database + '",
        "Username": "' + $username + '",
        "Password": "' + $password + '",
    },
    "JWT": {
        "Key": "' + $key + '",
    }
}'

$json | dotnet user-secrets set