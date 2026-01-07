import re

# Read the file
with open(r'd:\Antigravity\JsonCrudApp\Views\Home\Dashboard.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Define the new function
new_function = '''        async function initFiscal() {
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
        }'''

# Pattern to match the old function (with flexible whitespace)
pattern = r'        async function initFiscal\(\) \{.*?        \}'

# Replace using DOTALL flag to match across lines
new_content = re.sub(pattern, new_function, content, flags=re.DOTALL)

# Write back
with open(r'd:\Antigravity\JsonCrudApp\Views\Home\Dashboard.cshtml', 'w', encoding='utf-8') as f:
    f.write(new_content)

print("✅ Successfully updated initFiscal function in Dashboard.cshtml!")
