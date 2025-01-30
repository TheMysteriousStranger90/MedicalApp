Write-Host "Creating Certificates for Self-Signed Testing"

# Create root certificate
$rootCert = New-SelfSignedCertificate -Type Custom -KeySpec Signature `
    -Subject "CN=MedicalRootCA" `
    -FriendlyName "Medical Root CA" `
    -KeyExportPolicy Exportable `
    -HashAlgorithm sha256 -KeyLength 4096 `
    -CertStoreLocation "cert:\CurrentUser\My" `
    -KeyUsageProperty Sign `
    -KeyUsage CertSign `
    -NotAfter (Get-Date).AddYears(5)

# Create server certificate
$serverCert = New-SelfSignedCertificate -Type Custom -KeySpec Signature `
    -Subject "CN=localhost" `
    -FriendlyName "Medical Server Cert" `
    -DnsName "localhost" `
    -HashAlgorithm sha256 -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(2) `
    -CertStoreLocation "cert:\CurrentUser\My" `
    -Signer $rootCert `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1")

# Create client certificate
$clientCert = New-SelfSignedCertificate -Type Custom -KeySpec Signature `
    -Subject "CN=MedicalClient" `
    -FriendlyName "Medical Client Cert" `
    -HashAlgorithm sha256 -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(2) `
    -CertStoreLocation "cert:\CurrentUser\My" `
    -Signer $rootCert `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2")

$password = ConvertTo-SecureString -String "P@ssw0rd!" -Force -AsPlainText

Export-PfxCertificate -Cert $serverCert -FilePath server.pfx -Password $password
Export-PfxCertificate -Cert $clientCert -FilePath client.pfx -Password $password