Write-Host 'Creating Certificates for Self-Signed Testing'

$certsDir = Join-Path $PSScriptRoot 'certs'
if (!(Test-Path $certsDir)) {
    New-Item -ItemType Directory -Path $certsDir | Out-Null
    Write-Host "Created directory: $certsDir"
}

$rootCert = New-SelfSignedCertificate -Type Custom -KeySpec Signature `
    -Subject 'CN=MedicalRootCA' `
    -FriendlyName 'Medical Root CA' `
    -KeyExportPolicy Exportable `
    -HashAlgorithm sha256 -KeyLength 4096 `
    -CertStoreLocation 'cert:\CurrentUser\My' `
    -KeyUsageProperty Sign `
    -KeyUsage CertSign `
    -NotAfter (Get-Date).AddYears(5)

# GrpcService cert: Server Auth EKU, SANs localhost + grpcservice (container hostname)
$serverCert = New-SelfSignedCertificate -Type Custom -KeySpec KeyExchange `
    -Subject 'CN=grpcservice' `
    -FriendlyName 'Medical GrpcService Cert' `
    -DnsName 'localhost','grpcservice' `
    -HashAlgorithm sha256 -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(2) `
    -CertStoreLocation 'cert:\CurrentUser\My' `
    -Signer $rootCert `
    -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.1')

# Client cert: Server Auth + Client Auth EKU (Kestrel needs Server Auth)
# SANs: localhost + medicalapp-client (container hostname)
$clientCert = New-SelfSignedCertificate -Type Custom -KeySpec KeyExchange `
    -Subject 'CN=localhost' `
    -FriendlyName 'Medical Client Cert' `
    -DnsName 'localhost','medicalapp-client' `
    -HashAlgorithm sha256 -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(2) `
    -CertStoreLocation 'cert:\CurrentUser\My' `
    -Signer $rootCert `
    -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.1')

$password = ConvertTo-SecureString -String 'P@ssw0rd!' -Force -AsPlainText

$serverPfx = Join-Path $certsDir 'server.pfx'
$clientPfx = Join-Path $certsDir 'client.pfx'

Export-PfxCertificate -Cert $serverCert -FilePath $serverPfx -Password $password | Out-Null
Export-PfxCertificate -Cert $clientCert -FilePath $clientPfx -Password $password | Out-Null

Write-Host "server.pfx -> $serverPfx (Server Auth, SANs: localhost, grpcservice)"
Write-Host "client.pfx -> $clientPfx (Server Auth + Client Auth, SANs: localhost, medicalapp-client)"
