# PowerShell script to update all project files to use centralized package management
# This script removes Version attributes from PackageReference elements

$projectFiles = Get-ChildItem -Path "." -Recurse -Filter "*.csproj"

foreach ($project in $projectFiles) {
    Write-Host "Processing: $($project.FullName)"
    
    $content = Get-Content $project.FullName -Raw
    
    # Remove Version attributes from PackageReference elements
    $updatedContent = $content -replace '(<PackageReference\s+Include="[^"]+")(\s+Version="[^"]+")(\s*/?>)', '$1$3'
    
    # Only update the file if changes were made
    if ($content -ne $updatedContent) {
        Set-Content -Path $project.FullName -Value $updatedContent -NoNewline
        Write-Host "Updated: $($project.Name)" -ForegroundColor Green
    } else {
        Write-Host "No changes needed: $($project.Name)" -ForegroundColor Yellow
    }
}

Write-Host "All project files have been processed!" -ForegroundColor Cyan