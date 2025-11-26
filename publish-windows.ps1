
# Ruta del proyecto
$projectPath = "D:\GIT\PERSONAL\uoc-tfg-wayprecision\WayPrecision"
$outputPath = "D:\GIT\PERSONAL\uoc-tfg-wayprecision\publish\windows"

Write-Host "=== Publicando aplicación MAUI empaquetada (MSIX) ==="

dotnet publish "D:\GIT\PERSONAL\uoc-tfg-wayprecision\WayPrecision\WayPrecision.csproj" `
    -c Release `
    -f net9.0-windows10.0.19041.0 `
    -o "D:\GIT\PERSONAL\uoc-tfg-wayprecision\publish\windows" `
    /p:WindowsPackageType=MSIX

Write-Host "✅ Publicación completada en: $outputPath"
