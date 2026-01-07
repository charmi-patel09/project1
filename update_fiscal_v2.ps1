# PowerShell script to update Dashboard.cshtml
$filePath = "d:\Antigravity\JsonCrudApp\Views\Home\Dashboard.cshtml"

# Read all lines
$lines = Get-Content $filePath

# Find the start and end of the initFiscal function
$startLine = -1
$endLine = -1
$braceCount = 0
$inFunction = $false

for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '^\s+async function initFiscal') {
        $startLine = $i
        $inFunction = $true
        $braceCount = 0
    }
    
    if ($inFunction) {
        # Count opening braces
        $braceCount += ($lines[$i] -split '\{').Count - 1
        # Count closing braces
        $braceCount -= ($lines[$i] -split '\}').Count - 1
        
        # When braces balance out, we've found the end
        if ($braceCount -eq 0 -and $i -gt $startLine) {
            $endLine = $i
            break
        }
    }
}

Write-Host "Found function from line $($startLine + 1) to line $($endLine + 1)"

# New function lines
$newFunction = @'
        async function initFiscal() {
            try {
                const rateRes = await fetch('https://open.er-api.com/v6/latest/USD');
                const rateData = await rateRes.json();
                baseCurrencyRates = rateData.rates;
                
                const f = document.getElementById('fromCurrency');
                const t = document.getElementById('toCurrency');
                
                // Clear existing options
                f.innerHTML = '';
                t.innerHTML = '';
                
                // Add countries and cities with flags
                globalLocations.forEach(location => {
                    // Add country option
                    const countryOption = new Option(
                        `${location.flag} ${location.name} (${location.currency})`,
                        location.currency
                    );
                    f.add(countryOption.cloneNode(true));
                    t.add(countryOption.cloneNode(true));
                    
                    // Add city options (mapped to country currency)
                    if (location.cities && location.cities.length > 0) {
                        location.cities.forEach(city => {
                            const cityOption = new Option(
                                `${location.flag} ${city}, ${location.name} (${location.currency})`,
                                location.currency
                            );
                            f.add(cityOption.cloneNode(true));
                            t.add(cityOption.cloneNode(true));
                        });
                    }
                });
                
                // Set defaults
                f.value = 'USD';
                t.value = 'INR';

            } catch (e) { 
                console.error("Fiscal link error", e);
                const display = document.getElementById('currencyResult');
                display.innerHTML = '<span style="color: #ef4444;">Network Error: Unable to fetch exchange rates. Please check your connection.</span>';
            }
        }
'@

# Build new content
$newLines = @()
$newLines += $lines[0..($startLine - 1)]
$newLines += $newFunction -split "`r`n"
$newLines += $lines[($endLine + 1)..($lines.Count - 1)]

# Write back
$newLines | Set-Content $filePath -Encoding UTF8

Write-Host "✅ Successfully updated initFiscal function!"
Write-Host "   Old function: lines $($startLine + 1) to $($endLine + 1)"
Write-Host "   New function: enhanced with global locations"
