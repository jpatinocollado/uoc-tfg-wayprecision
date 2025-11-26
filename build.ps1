
# Ruta raíz del proyecto
$solutionPath = "D:\GIT\PERSONAL\uoc-tfg-wayprecision"

Write-Host "=== Limpieza de la solución ==="
dotnet clean $solutionPath

Write-Host "=== Restaurando paquetes ==="
dotnet restore $solutionPath

Write-Host "=== Compilando en modo Release ==="
dotnet build $solutionPath -c Release

Write-Host "=== Verificando DLL de WayPrecision.Domain ==="
$domainDll = Join-Path $solutionPath "WayPrecision.Domain\bin\Release\net9.0\WayPrecision.Domain.dll"

if (Test-Path $domainDll) {
    Write-Host "✅ Compilación exitosa. DLL encontrada en: $domainDll"
} else {
    Write-Host "❌ Error: No se generó la DLL. Revisa errores en la compilación."
}
