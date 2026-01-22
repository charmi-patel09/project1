// ==================== Global Data Structure ====================
const globalLocations = {
    // Countries with their currencies and major cities
    countries: [
        // Africa
        { name: "South Africa", code: "ZA", currency: "ZAR", flag: "🇿🇦", cities: ["Johannesburg", "Cape Town", "Durban", "Pretoria"] },
        { name: "Nigeria", code: "NG", currency: "NGN", flag: "🇳🇬", cities: ["Lagos", "Abuja", "Kano", "Ibadan"] },
        { name: "Egypt", code: "EG", currency: "EGP", flag: "🇪🇬", cities: ["Cairo", "Alexandria", "Giza", "Luxor"] },
        { name: "Kenya", code: "KE", currency: "KES", flag: "🇰🇪", cities: ["Nairobi", "Mombasa", "Kisumu", "Nakuru"] },
        { name: "Morocco", code: "MA", currency: "MAD", flag: "🇲🇦", cities: ["Casablanca", "Rabat", "Marrakech", "Fes"] },
        { name: "Ghana", code: "GH", currency: "GHS", flag: "🇬🇭", cities: ["Accra", "Kumasi", "Tamale", "Takoradi"] },
        { name: "Ethiopia", code: "ET", currency: "ETB", flag: "🇪🇹", cities: ["Addis Ababa", "Dire Dawa", "Mekelle", "Gondar"] },
        { name: "Tanzania", code: "TZ", currency: "TZS", flag: "🇹🇿", cities: ["Dar es Salaam", "Dodoma", "Mwanza", "Arusha"] },

        // Asia
        { name: "China", code: "CN", currency: "CNY", flag: "🇨🇳", cities: ["Beijing", "Shanghai", "Guangzhou", "Shenzhen", "Chengdu", "Hong Kong"] },
        { name: "India", code: "IN", currency: "INR", flag: "🇮🇳", cities: ["Mumbai", "Delhi", "Bangalore", "Hyderabad", "Chennai", "Kolkata", "Pune", "Ahmedabad"] },
        { name: "Japan", code: "JP", currency: "JPY", flag: "🇯🇵", cities: ["Tokyo", "Osaka", "Kyoto", "Yokohama", "Nagoya", "Sapporo"] },
        { name: "South Korea", code: "KR", currency: "KRW", flag: "🇰🇷", cities: ["Seoul", "Busan", "Incheon", "Daegu", "Daejeon"] },
        { name: "Indonesia", code: "ID", currency: "IDR", flag: "🇮🇩", cities: ["Jakarta", "Surabaya", "Bandung", "Medan", "Bali"] },
        { name: "Thailand", code: "TH", currency: "THB", flag: "🇹🇭", cities: ["Bangkok", "Chiang Mai", "Phuket", "Pattaya", "Krabi"] },
        { name: "Vietnam", code: "VN", currency: "VND", flag: "🇻🇳", cities: ["Hanoi", "Ho Chi Minh City", "Da Nang", "Hue", "Nha Trang"] },
        { name: "Singapore", code: "SG", currency: "SGD", flag: "🇸🇬", cities: ["Singapore"] },
        { name: "Malaysia", code: "MY", currency: "MYR", flag: "🇲🇾", cities: ["Kuala Lumpur", "Penang", "Johor Bahru", "Malacca"] },
        { name: "Philippines", code: "PH", currency: "PHP", flag: "🇵🇭", cities: ["Manila", "Quezon City", "Davao", "Cebu"] },
        { name: "Pakistan", code: "PK", currency: "PKR", flag: "🇵🇰", cities: ["Karachi", "Lahore", "Islamabad", "Rawalpindi"] },
        { name: "Bangladesh", code: "BD", currency: "BDT", flag: "🇧🇩", cities: ["Dhaka", "Chittagong", "Khulna", "Sylhet"] },
        { name: "Saudi Arabia", code: "SA", currency: "SAR", flag: "🇸🇦", cities: ["Riyadh", "Jeddah", "Mecca", "Medina", "Dammam"] },
        { name: "United Arab Emirates", code: "AE", currency: "AED", flag: "🇦🇪", cities: ["Dubai", "Abu Dhabi", "Sharjah", "Ajman"] },
        { name: "Turkey", code: "TR", currency: "TRY", flag: "🇹🇷", cities: ["Istanbul", "Ankara", "Izmir", "Antalya", "Bursa"] },
        { name: "Israel", code: "IL", currency: "ILS", flag: "🇮🇱", cities: ["Tel Aviv", "Jerusalem", "Haifa", "Eilat"] },
        { name: "Iran", code: "IR", currency: "IRR", flag: "🇮🇷", cities: ["Tehran", "Mashhad", "Isfahan", "Shiraz"] },
        { name: "Iraq", code: "IQ", currency: "IQD", flag: "🇮🇶", cities: ["Baghdad", "Basra", "Mosul", "Erbil"] },

        // Europe
        { name: "United Kingdom", code: "GB", currency: "GBP", flag: "🇬🇧", cities: ["London", "Manchester", "Birmingham", "Edinburgh", "Glasgow", "Liverpool"] },
        { name: "Germany", code: "DE", currency: "EUR", flag: "🇩🇪", cities: ["Berlin", "Munich", "Frankfurt", "Hamburg", "Cologne", "Stuttgart"] },
        { name: "France", code: "FR", currency: "EUR", flag: "🇫🇷", cities: ["Paris", "Lyon", "Marseille", "Nice", "Toulouse", "Bordeaux"] },
        { name: "Italy", code: "IT", currency: "EUR", flag: "🇮🇹", cities: ["Rome", "Milan", "Venice", "Florence", "Naples", "Turin"] },
        { name: "Spain", code: "ES", currency: "EUR", flag: "🇪🇸", cities: ["Madrid", "Barcelona", "Valencia", "Seville", "Bilbao", "Malaga"] },
        { name: "Netherlands", code: "NL", currency: "EUR", flag: "🇳🇱", cities: ["Amsterdam", "Rotterdam", "The Hague", "Utrecht"] },
        { name: "Switzerland", code: "CH", currency: "CHF", flag: "🇨🇭", cities: ["Zurich", "Geneva", "Basel", "Bern", "Lausanne"] },
        { name: "Belgium", code: "BE", currency: "EUR", flag: "🇧🇪", cities: ["Brussels", "Antwerp", "Ghent", "Bruges"] },
        { name: "Austria", code: "AT", currency: "EUR", flag: "🇦🇹", cities: ["Vienna", "Salzburg", "Innsbruck", "Graz"] },
        { name: "Sweden", code: "SE", currency: "SEK", flag: "🇸🇪", cities: ["Stockholm", "Gothenburg", "Malmö", "Uppsala"] },
        { name: "Norway", code: "NO", currency: "NOK", flag: "🇳🇴", cities: ["Oslo", "Bergen", "Trondheim", "Stavanger"] },
        { name: "Denmark", code: "DK", currency: "DKK", flag: "🇩🇰", cities: ["Copenhagen", "Aarhus", "Odense", "Aalborg"] },
        { name: "Finland", code: "FI", currency: "EUR", flag: "🇫🇮", cities: ["Helsinki", "Espoo", "Tampere", "Turku"] },
        { name: "Poland", code: "PL", currency: "PLN", flag: "🇵🇱", cities: ["Warsaw", "Krakow", "Wroclaw", "Gdansk"] },
        { name: "Russia", code: "RU", currency: "RUB", flag: "🇷🇺", cities: ["Moscow", "St. Petersburg", "Novosibirsk", "Yekaterinburg"] },
        { name: "Czech Republic", code: "CZ", currency: "CZK", flag: "🇨🇿", cities: ["Prague", "Brno", "Ostrava", "Plzen"] },
        { name: "Portugal", code: "PT", currency: "EUR", flag: "🇵🇹", cities: ["Lisbon", "Porto", "Faro", "Coimbra"] },
        { name: "Greece", code: "GR", currency: "EUR", flag: "🇬🇷", cities: ["Athens", "Thessaloniki", "Patras", "Heraklion"] },
        { name: "Ireland", code: "IE", currency: "EUR", flag: "🇮🇪", cities: ["Dublin", "Cork", "Galway", "Limerick"] },
        { name: "Romania", code: "RO", currency: "RON", flag: "🇷🇴", cities: ["Bucharest", "Cluj-Napoca", "Timisoara", "Iasi"] },
        { name: "Hungary", code: "HU", currency: "HUF", flag: "🇭🇺", cities: ["Budapest", "Debrecen", "Szeged", "Miskolc"] },

        // North America
        { name: "United States", code: "US", currency: "USD", flag: "🇺🇸", cities: ["New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Francisco", "Miami", "Seattle", "Boston", "Las Vegas"] },
        { name: "Canada", code: "CA", currency: "CAD", flag: "🇨🇦", cities: ["Toronto", "Vancouver", "Montreal", "Calgary", "Ottawa", "Edmonton"] },
        { name: "Mexico", code: "MX", currency: "MXN", flag: "🇲🇽", cities: ["Mexico City", "Guadalajara", "Monterrey", "Cancun", "Tijuana"] },

        // South America
        { name: "Brazil", code: "BR", currency: "BRL", flag: "🇧🇷", cities: ["São Paulo", "Rio de Janeiro", "Brasília", "Salvador", "Fortaleza"] },
        { name: "Argentina", code: "AR", currency: "ARS", flag: "🇦🇷", cities: ["Buenos Aires", "Córdoba", "Rosario", "Mendoza"] },
        { name: "Chile", code: "CL", currency: "CLP", flag: "🇨🇱", cities: ["Santiago", "Valparaíso", "Concepción", "La Serena"] },
        { name: "Colombia", code: "CO", currency: "COP", flag: "🇨🇴", cities: ["Bogotá", "Medellín", "Cali", "Cartagena"] },
        { name: "Peru", code: "PE", currency: "PEN", flag: "🇵🇪", cities: ["Lima", "Cusco", "Arequipa", "Trujillo"] },
        { name: "Venezuela", code: "VE", currency: "VES", flag: "🇻🇪", cities: ["Caracas", "Maracaibo", "Valencia", "Barquisimeto"] },

        // Oceania
        { name: "Australia", code: "AU", currency: "AUD", flag: "🇦🇺", cities: ["Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Gold Coast"] },
        { name: "New Zealand", code: "NZ", currency: "NZD", flag: "🇳🇿", cities: ["Auckland", "Wellington", "Christchurch", "Queenstown"] },

        // Additional Countries
        { name: "Ukraine", code: "UA", currency: "UAH", flag: "🇺🇦", cities: ["Kyiv", "Kharkiv", "Odesa", "Lviv"] },
        { name: "Croatia", code: "HR", currency: "EUR", flag: "🇭🇷", cities: ["Zagreb", "Split", "Dubrovnik", "Rijeka"] },
        { name: "Bulgaria", code: "BG", currency: "BGN", flag: "🇧🇬", cities: ["Sofia", "Plovdiv", "Varna", "Burgas"] },
        { name: "Serbia", code: "RS", currency: "RSD", flag: "🇷🇸", cities: ["Belgrade", "Novi Sad", "Niš", "Kragujevac"] },
        { name: "Slovenia", code: "SI", currency: "EUR", flag: "🇸🇮", cities: ["Ljubljana", "Maribor", "Celje", "Kranj"] },
        { name: "Slovakia", code: "SK", currency: "EUR", flag: "🇸🇰", cities: ["Bratislava", "Košice", "Prešov", "Žilina"] },
        { name: "Lithuania", code: "LT", currency: "EUR", flag: "🇱🇹", cities: ["Vilnius", "Kaunas", "Klaipėda", "Šiauliai"] },
        { name: "Latvia", code: "LV", currency: "EUR", flag: "🇱🇻", cities: ["Riga", "Daugavpils", "Liepāja", "Jelgava"] },
        { name: "Estonia", code: "EE", currency: "EUR", flag: "🇪🇪", cities: ["Tallinn", "Tartu", "Narva", "Pärnu"] },
        { name: "Iceland", code: "IS", currency: "ISK", flag: "🇮🇸", cities: ["Reykjavik", "Akureyri", "Keflavik", "Hafnarfjordur"] },
        { name: "Luxembourg", code: "LU", currency: "EUR", flag: "🇱🇺", cities: ["Luxembourg City", "Esch-sur-Alzette", "Differdange"] },
        { name: "Malta", code: "MT", currency: "EUR", flag: "🇲🇹", cities: ["Valletta", "Sliema", "St. Julian's", "Mdina"] },
        { name: "Cyprus", code: "CY", currency: "EUR", flag: "🇨🇾", cities: ["Nicosia", "Limassol", "Larnaca", "Paphos"] },
        { name: "Qatar", code: "QA", currency: "QAR", flag: "🇶🇦", cities: ["Doha", "Al Wakrah", "Al Rayyan", "Umm Salal"] },
        { name: "Kuwait", code: "KW", currency: "KWD", flag: "🇰🇼", cities: ["Kuwait City", "Hawalli", "Salmiya", "Farwaniya"] },
        { name: "Bahrain", code: "BH", currency: "BHD", flag: "🇧🇭", cities: ["Manama", "Muharraq", "Riffa", "Hamad Town"] },
        { name: "Oman", code: "OM", currency: "OMR", flag: "🇴🇲", cities: ["Muscat", "Salalah", "Sohar", "Nizwa"] },
        { name: "Jordan", code: "JO", currency: "JOD", flag: "🇯🇴", cities: ["Amman", "Zarqa", "Irbid", "Petra"] },
        { name: "Lebanon", code: "LB", currency: "LBP", flag: "🇱🇧", cities: ["Beirut", "Tripoli", "Sidon", "Tyre"] },
        { name: "Sri Lanka", code: "LK", currency: "LKR", flag: "🇱🇰", cities: ["Colombo", "Kandy", "Galle", "Jaffna"] },
        { name: "Nepal", code: "NP", currency: "NPR", flag: "🇳🇵", cities: ["Kathmandu", "Pokhara", "Lalitpur", "Bhaktapur"] },
        { name: "Myanmar", code: "MM", currency: "MMK", flag: "🇲🇲", cities: ["Yangon", "Mandalay", "Naypyidaw", "Bagan"] },
        { name: "Cambodia", code: "KH", currency: "KHR", flag: "🇰🇭", cities: ["Phnom Penh", "Siem Reap", "Battambang", "Sihanoukville"] },
        { name: "Laos", code: "LA", currency: "LAK", flag: "🇱🇦", cities: ["Vientiane", "Luang Prabang", "Pakse", "Savannakhet"] },
        { name: "Mongolia", code: "MN", currency: "MNT", flag: "🇲🇳", cities: ["Ulaanbaatar", "Erdenet", "Darkhan", "Choibalsan"] },
        { name: "Kazakhstan", code: "KZ", currency: "KZT", flag: "🇰🇿", cities: ["Almaty", "Nur-Sultan", "Shymkent", "Karaganda"] },
        { name: "Uzbekistan", code: "UZ", currency: "UZS", flag: "🇺🇿", cities: ["Tashkent", "Samarkand", "Bukhara", "Khiva"] },
        { name: "Georgia", code: "GE", currency: "GEL", flag: "🇬🇪", cities: ["Tbilisi", "Batumi", "Kutaisi", "Rustavi"] },
        { name: "Armenia", code: "AM", currency: "AMD", flag: "🇦🇲", cities: ["Yerevan", "Gyumri", "Vanadzor", "Vagharshapat"] },
        { name: "Azerbaijan", code: "AZ", currency: "AZN", flag: "🇦🇿", cities: ["Baku", "Ganja", "Sumqayit", "Lankaran"] },
        { name: "Algeria", code: "DZ", currency: "DZD", flag: "🇩🇿", cities: ["Algiers", "Oran", "Constantine", "Annaba"] },
        { name: "Tunisia", code: "TN", currency: "TND", flag: "🇹🇳", cities: ["Tunis", "Sfax", "Sousse", "Kairouan"] },
        { name: "Libya", code: "LY", currency: "LYD", flag: "🇱🇾", cities: ["Tripoli", "Benghazi", "Misrata", "Bayda"] },
        { name: "Sudan", code: "SD", currency: "SDG", flag: "🇸🇩", cities: ["Khartoum", "Omdurman", "Port Sudan", "Kassala"] },
        { name: "Senegal", code: "SN", currency: "XOF", flag: "🇸🇳", cities: ["Dakar", "Touba", "Thiès", "Saint-Louis"] },
        { name: "Ivory Coast", code: "CI", currency: "XOF", flag: "🇨🇮", cities: ["Abidjan", "Yamoussoukro", "Bouaké", "Daloa"] },
        { name: "Cameroon", code: "CM", currency: "XAF", flag: "🇨🇲", cities: ["Douala", "Yaoundé", "Garoua", "Bamenda"] },
        { name: "Uganda", code: "UG", currency: "UGX", flag: "🇺🇬", cities: ["Kampala", "Gulu", "Lira", "Mbarara"] },
        { name: "Zimbabwe", code: "ZW", currency: "ZWL", flag: "🇿🇼", cities: ["Harare", "Bulawayo", "Chitungwiza", "Mutare"] },
        { name: "Zambia", code: "ZM", currency: "ZMW", flag: "🇿🇲", cities: ["Lusaka", "Kitwe", "Ndola", "Kabwe"] },
        { name: "Mozambique", code: "MZ", currency: "MZN", flag: "🇲🇿", cities: ["Maputo", "Matola", "Beira", "Nampula"] },
        { name: "Angola", code: "AO", currency: "AOA", flag: "🇦🇴", cities: ["Luanda", "Huambo", "Lobito", "Benguela"] },
        { name: "Botswana", code: "BW", currency: "BWP", flag: "🇧🇼", cities: ["Gaborone", "Francistown", "Molepolole", "Maun"] },
        { name: "Namibia", code: "NA", currency: "NAD", flag: "🇳🇦", cities: ["Windhoek", "Walvis Bay", "Swakopmund", "Rundu"] },
        { name: "Mauritius", code: "MU", currency: "MUR", flag: "🇲🇺", cities: ["Port Louis", "Beau Bassin", "Vacoas", "Curepipe"] },
        { name: "Jamaica", code: "JM", currency: "JMD", flag: "🇯🇲", cities: ["Kingston", "Montego Bay", "Spanish Town", "Portmore"] },
        { name: "Trinidad and Tobago", code: "TT", currency: "TTD", flag: "🇹🇹", cities: ["Port of Spain", "San Fernando", "Chaguanas", "Arima"] },
        { name: "Barbados", code: "BB", currency: "BBD", flag: "🇧🇧", cities: ["Bridgetown", "Speightstown", "Oistins", "Holetown"] },
        { name: "Bahamas", code: "BS", currency: "BSD", flag: "🇧🇸", cities: ["Nassau", "Freeport", "West End", "Coopers Town"] },
        { name: "Costa Rica", code: "CR", currency: "CRC", flag: "🇨🇷", cities: ["San José", "Limón", "Alajuela", "Heredia"] },
        { name: "Panama", code: "PA", currency: "PAB", flag: "🇵🇦", cities: ["Panama City", "Colón", "David", "La Chorrera"] },
        { name: "Guatemala", code: "GT", currency: "GTQ", flag: "🇬🇹", cities: ["Guatemala City", "Antigua", "Quetzaltenango", "Escuintla"] },
        { name: "Honduras", code: "HNL", currency: "HNL", flag: "🇭🇳", cities: ["Tegucigalpa", "San Pedro Sula", "Choloma", "La Ceiba"] },
        { name: "Nicaragua", code: "NI", currency: "NIO", flag: "🇳🇮", cities: ["Managua", "León", "Masaya", "Matagalpa"] },
        { name: "El Salvador", code: "SV", currency: "USD", flag: "🇸🇻", cities: ["San Salvador", "Santa Ana", "San Miguel", "Soyapango"] },
        { name: "Dominican Republic", code: "DO", currency: "DOP", flag: "🇩🇴", cities: ["Santo Domingo", "Santiago", "La Romana", "Punta Cana"] },
        { name: "Cuba", code: "CU", currency: "CUP", flag: "🇨🇺", cities: ["Havana", "Santiago de Cuba", "Camagüey", "Holguín"] },
        { name: "Haiti", code: "HT", currency: "HTG", flag: "🇭🇹", cities: ["Port-au-Prince", "Cap-Haïtien", "Gonaïves", "Les Cayes"] },
        { name: "Ecuador", code: "EC", currency: "USD", flag: "🇪🇨", cities: ["Quito", "Guayaquil", "Cuenca", "Galápagos"] },
        { name: "Bolivia", code: "BO", currency: "BOB", flag: "🇧🇴", cities: ["La Paz", "Santa Cruz", "Cochabamba", "Sucre"] },
        { name: "Paraguay", code: "PY", currency: "PYG", flag: "🇵🇾", cities: ["Asunción", "Ciudad del Este", "San Lorenzo", "Luque"] },
        { name: "Uruguay", code: "UY", currency: "UYU", flag: "🇺🇾", cities: ["Montevideo", "Salto", "Paysandú", "Punta del Este"] },
        { name: "Fiji", code: "FJ", currency: "FJD", flag: "🇫🇯", cities: ["Suva", "Nadi", "Lautoka", "Labasa"] },
        { name: "Papua New Guinea", code: "PG", currency: "PGK", flag: "🇵🇬", cities: ["Port Moresby", "Lae", "Arawa", "Mount Hagen"] },
        { name: "Albania", code: "AL", currency: "ALL", flag: "🇦🇱", cities: ["Tirana", "Durrës", "Vlorë", "Shkodër"] },
        { name: "North Macedonia", code: "MK", currency: "MKD", flag: "🇲🇰", cities: ["Skopje", "Bitola", "Kumanovo", "Prilep"] },
        { name: "Bosnia and Herzegovina", code: "BA", currency: "BAM", flag: "🇧🇦", cities: ["Sarajevo", "Banja Luka", "Tuzla", "Zenica"] },
        { name: "Montenegro", code: "ME", currency: "EUR", flag: "🇲🇪", cities: ["Podgorica", "Nikšić", "Pljevlja", "Bijelo Polje"] },
        { name: "Moldova", code: "MD", currency: "MDL", flag: "🇲🇩", cities: ["Chișinău", "Tiraspol", "Bălți", "Bender"] },
        { name: "Belarus", code: "BY", currency: "BYN", flag: "🇧🇾", cities: ["Minsk", "Gomel", "Mogilev", "Vitebsk"] },
    ]
};

// Currency symbols mapping
const currencySymbols = {
    USD: "$", EUR: "€", GBP: "£", JPY: "¥", CNY: "¥", INR: "₹", AUD: "A$", CAD: "C$",
    CHF: "Fr", SEK: "kr", NOK: "kr", DKK: "kr", RUB: "₽", BRL: "R$", ZAR: "R",
    MXN: "$", SGD: "S$", HKD: "HK$", NZD: "NZ$", KRW: "₩", TRY: "₺", PLN: "zł",
    THB: "฿", IDR: "Rp", MYR: "RM", PHP: "₱", CZK: "Kč", ILS: "₪", AED: "د.إ",
    SAR: "﷼", ARS: "$", CLP: "$", COP: "$", PEN: "S/", VES: "Bs", UAH: "₴",
    RON: "lei", HUF: "Ft", BGN: "лв", HRK: "kn", ISK: "kr", QAR: "﷼", KWD: "د.ك",
    BHD: "د.ب", OMR: "﷼", JOD: "د.ا", LBP: "ل.ل", EGP: "£", NGN: "₦", KES: "KSh",
    MAD: "د.م.", GHS: "₵", ETB: "Br", TZS: "TSh", PKR: "₨", BDT: "৳", VND: "₫",
    LKR: "Rs", NPR: "₨", MMK: "K", KHR: "៛", LAK: "₭", MNT: "₮", KZT: "₸",
    UZS: "so'm", GEL: "₾", AMD: "֏", AZN: "₼", DZD: "د.ج", TND: "د.ت", LYD: "ل.د",
    SDG: "ج.س.", XOF: "CFA", XAF: "FCFA", UGX: "USh", ZWL: "Z$", ZMW: "ZK",
    MZN: "MT", AOA: "Kz", BWP: "P", NAD: "N$", MUR: "₨", JMD: "J$", TTD: "TT$",
    BBD: "Bds$", BSD: "B$", CRC: "₡", PAB: "B/.", GTQ: "Q", HNL: "L", NIO: "C$",
    DOP: "RD$", CUP: "₱", HTG: "G", BOB: "Bs.", PYG: "₲", UYU: "$U", FJD: "FJ$",
    PGK: "K", ALL: "L", MKD: "ден", BAM: "KM", MDL: "L", BYN: "Br", IRR: "﷼",
    IQD: "ع.د", RSD: "дин"
};

// ==================== State Management ====================
let exchangeRates = {};
let lastUpdateTime = null;
let fromCurrency = null;
let toCurrency = null;

// ==================== DOM Elements ====================
const fromSelect = document.getElementById('from-select');
const toSelect = document.getElementById('to-select');
const fromAmount = document.getElementById('from-amount');
const toAmount = document.getElementById('to-amount');
const fromFlag = document.getElementById('from-flag');
const toFlag = document.getElementById('to-flag');
const fromInfo = document.getElementById('from-info');
const toInfo = document.getElementById('to-info');
const fromSymbol = document.getElementById('from-symbol');
const toSymbol = document.getElementById('to-symbol');
const swapBtn = document.getElementById('swap-btn');
const convertBtn = document.getElementById('convert-btn');
const rateValue = document.getElementById('rate-value');
const lastUpdated = document.getElementById('last-updated');
const errorMessage = document.getElementById('error-message');
const loadingOverlay = document.getElementById('loading-overlay');

// ==================== Initialization ====================
async function init() {
    // Localization is handled globally by site.js now.
    // specific page logic:

    populateSelects();
    attachEventListeners();
    setDefaultSelections();
    fetchExchangeRates();
}

// ==================== Populate Dropdowns ====================
function populateSelects() {
    const fragment = document.createDocumentFragment();

    // Add countries
    globalLocations.countries.forEach(country => {
        const option = document.createElement('option');
        option.value = JSON.stringify({
            type: 'country',
            name: country.name,
            code: country.code,
            currency: country.currency,
            flag: country.flag
        });
        option.textContent = `${country.flag} ${country.name} (${country.currency})`;
        fragment.appendChild(option.cloneNode(true));
    });

    // Add cities
    globalLocations.countries.forEach(country => {
        if (country.cities && country.cities.length > 0) {
            country.cities.forEach(city => {
                const option = document.createElement('option');
                option.value = JSON.stringify({
                    type: 'city',
                    name: city,
                    country: country.name,
                    code: country.code,
                    currency: country.currency,
                    flag: country.flag
                });
                option.textContent = `${country.flag} ${city}, ${country.name} (${country.currency})`;
                fragment.appendChild(option.cloneNode(true));
            });
        }
    });

    fromSelect.appendChild(fragment.cloneNode(true));
    toSelect.appendChild(fragment);
}

// ==================== Event Listeners ====================
function attachEventListeners() {
    fromSelect.addEventListener('change', handleFromChange);
    toSelect.addEventListener('change', handleToChange);
    fromAmount.addEventListener('input', handleAmountInput);
    swapBtn.addEventListener('click', handleSwap);
    convertBtn.addEventListener('click', handleConvert);
}

// ==================== Event Handlers ====================
function handleFromChange(e) {
    if (!e.target.value) return;

    const data = JSON.parse(e.target.value);
    fromCurrency = data.currency;

    fromFlag.textContent = data.flag;
    fromSymbol.textContent = currencySymbols[data.currency] || data.currency;

    const codeEl = fromInfo.querySelector('.currency-code');
    const nameEl = fromInfo.querySelector('.location-name');
    codeEl.textContent = data.currency;
    nameEl.textContent = data.type === 'city' ? `${data.name}, ${data.country}` : data.name;

    updateConversion();
}

function handleToChange(e) {
    if (!e.target.value) return;

    const data = JSON.parse(e.target.value);
    toCurrency = data.currency;

    toFlag.textContent = data.flag;
    toSymbol.textContent = currencySymbols[data.currency] || data.currency;

    const codeEl = toInfo.querySelector('.currency-code');
    const nameEl = toInfo.querySelector('.location-name');
    codeEl.textContent = data.currency;
    nameEl.textContent = data.type === 'city' ? `${data.name}, ${data.country}` : data.name;

    updateConversion();
}

function handleAmountInput() {
    updateConversion();
}

function handleSwap() {
    const fromValue = fromSelect.value;
    const toValue = toSelect.value;

    if (!fromValue || !toValue) {
        showError(window.i18n.t('SelectCurrencies'));
        return;
    }

    fromSelect.value = toValue;
    toSelect.value = fromValue;

    handleFromChange({ target: fromSelect });
    handleToChange({ target: toSelect });
}

async function handleConvert() {
    if (!fromCurrency || !toCurrency) {
        showError(window.i18n.t('SelectCurrencies'));
        return;
    }

    if (!fromAmount.value || parseFloat(fromAmount.value) <= 0) {
        showError(window.i18n.t('EnterAmount'));
        return;
    }

    await fetchExchangeRates();
    updateConversion();
}

// ==================== Currency Conversion ====================
function updateConversion() {
    if (!fromCurrency || !toCurrency || !fromAmount.value) {
        return;
    }

    const amount = parseFloat(fromAmount.value);
    if (isNaN(amount) || amount <= 0) {
        toAmount.value = '';
        return;
    }

    if (Object.keys(exchangeRates).length === 0) {
        // Soft error or just wait?
        showError(window.i18n.t('ExchangeRatesNotLoaded'));
        return;
    }

    // Convert from base currency (USD) to target
    const fromRate = exchangeRates[fromCurrency] || 1;
    const toRate = exchangeRates[toCurrency] || 1;

    // Convert: amount in fromCurrency -> USD -> toCurrency
    const amountInUSD = amount / fromRate;
    const convertedAmount = amountInUSD * toRate;

    toAmount.value = convertedAmount.toFixed(2);

    // Update exchange rate display
    const rate = toRate / fromRate;
    rateValue.textContent = `1 ${fromCurrency} = ${rate.toFixed(4)} ${toCurrency}`;

    hideError();
}

// ==================== API Integration ====================
async function fetchExchangeRates() {
    try {
        showLoading();

        // Using ExchangeRate-API (free tier)
        const response = await fetch('https://api.exchangerate-api.com/v4/latest/USD');

        if (!response.ok) {
            throw new Error(window.i18n.t('FetchError'));
        }

        const data = await response.json();
        exchangeRates = data.rates;
        lastUpdateTime = new Date();

        lastUpdated.textContent = `Last updated: ${lastUpdateTime.toLocaleString()}`;

        hideLoading();
        hideError();
    } catch (error) {
        hideLoading();
        showError(window.i18n.t('FetchError'));
        console.error('Exchange rate fetch error:', error);
    }
}

// ==================== Default Selections ====================
function setDefaultSelections() {
    // Set USD (United States) as default from
    const usOption = Array.from(fromSelect.options).find(opt => {
        if (!opt.value) return false;
        const data = JSON.parse(opt.value);
        return data.type === 'country' && data.code === 'US';
    });

    if (usOption) {
        fromSelect.value = usOption.value;
        handleFromChange({ target: fromSelect });
    }

    // Set EUR (Germany) as default to
    const eurOption = Array.from(toSelect.options).find(opt => {
        if (!opt.value) return false;
        const data = JSON.parse(opt.value);
        return data.type === 'country' && data.code === 'DE';
    });

    if (eurOption) {
        toSelect.value = eurOption.value;
        handleToChange({ target: toSelect });
    }
}

// ==================== UI Helpers ====================
function showLoading() {
    loadingOverlay.classList.add('show');
    convertBtn.classList.add('loading');
}

function hideLoading() {
    loadingOverlay.classList.remove('show');
    convertBtn.classList.remove('loading');
}

function showError(message) {
    errorMessage.textContent = message;
    errorMessage.classList.add('show');

    setTimeout(() => {
        hideError();
    }, 5000);
}

function hideError() {
    errorMessage.classList.remove('show');
}

// ==================== Initialize App ====================
document.addEventListener('DOMContentLoaded', init);
