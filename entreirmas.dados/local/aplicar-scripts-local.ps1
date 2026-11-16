# Aplica, em ordem de versao, os scripts de owner/ no MySQL LOCAL usando o cliente mysql.
# Serve para quem nao tem o Flyway instalado; nos ambientes (dsv, tst, hmg) use o Flyway com conf/<ambiente>.conf.
# A senha e pedida pelo proprio mysql (-p) e nao fica gravada.
#
# Uso:  .\local\aplicar-scripts-local.ps1 [-Usuario root] [-Banco entreirmas] [-APartirDe 1.0.6]
#       -APartirDe aplica so as versoes iguais ou posteriores (banco que ja tem as anteriores).

param(
    [string]$Usuario = "root",
    [string]$Banco = "entreirmas",
    [string]$Mysql = "mysql",
    [string]$APartirDe = "0.0"
)

$ErrorActionPreference = "Stop"
$raiz = Split-Path -Parent $PSScriptRoot

$versao = { [version](($_.BaseName -split "__")[0].TrimStart("V")) }
$scripts = @(Get-ChildItem -Path (Join-Path $raiz "owner") -Recurse -Filter "V*.sql" |
    Where-Object { (& $versao) -ge [version]$APartirDe } |
    Sort-Object $versao)

if ($scripts.Count -eq 0) {
    Write-Host "Nenhum script encontrado em owner/."
    exit 0
}

Write-Host "Scripts que serao aplicados em '$Banco':"
$scripts | ForEach-Object { Write-Host "  $($_.Name)" }

# Junta os scripts em um unico lote para pedir a senha uma vez so.
$lote = Join-Path ([System.IO.Path]::GetTempPath()) "entreirmas-lote.sql"
$scripts | ForEach-Object { Get-Content $_.FullName -Raw -Encoding UTF8 } | Set-Content $lote -Encoding UTF8

try {
    Get-Content $lote -Raw -Encoding UTF8 | & $Mysql -u $Usuario -p --default-character-set=utf8mb4 $Banco
    if ($LASTEXITCODE -ne 0) { throw "O mysql terminou com erro ($LASTEXITCODE)." }
    Write-Host "Scripts aplicados."
}
finally {
    Remove-Item $lote -ErrorAction SilentlyContinue
}
