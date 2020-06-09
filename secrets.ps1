param (
    [Parameter(Mandatory=$true)][string]$dbServer,
    [Parameter(Mandatory=$true)][string]$dbDatabase,
    [Parameter(Mandatory=$true)][string]$dbUsername,
    [Parameter(Mandatory=$true)][string]$dbPassword,
    [Parameter(Mandatory=$true)][string]$smtpHost,
    [Parameter(Mandatory=$true)][string]$smtpPort,
    [Parameter(Mandatory=$true)][string]$smtpEmail,
    [Parameter(Mandatory=$true)][string]$smtpPassword,
    [int]$keyLength = 256
 )

$key = $null

for ( $i = 1; $i -le $keyLength; $i++) {
    $key += ( -join ((48..57) + (65..90) + (97..122) | Get-Random | % {[char]$_}) )
}

$json = '{
    "DB": {
        "Server": "' + $dbServer + '",
        "Database": "' + $dbDatabase + '",
        "Username": "' + $dbUsername + '",
        "Password": "' + $dbPassword + '",
    },
    "JWT": {
        "Key": "' + $key + '",
    },
    "SMTP": {
        "Host": "' + $smtpHost + '",
        "Port": "' + $smtpPort + '",
        "Email": "' + $smtpEmail + '",
        "Password": "' + $smtpPassword + '",
    }
}'

$json | dotnet user-secrets set