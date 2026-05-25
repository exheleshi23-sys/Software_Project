const map = L.map('map').setView([41.3275, 19.8187], 14);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
maxZoom: 19,
attribution: '© OpenStreetMap contributors'
}).addTo(map);

function getRiskColor(risk) {
switch(risk) {
case 'High': return '#ff3333';
case 'Medium': return '#ff9900';
case 'Low': return '#33cc33';
default: return '#999999';
}
}

async function loadThermalBlueprint() {
try {
const response = await fetch('http://localhost:5020/api/CrimeMap/thermal-blueprint');
const data = await response.json();

// We will loop multiple times to create a massive information spread
// Simulating multiple reports throughout Tirana
for (let i = 0; i < 5; i++) {
data.forEach((item, index) => {
// Generate completely random spreads across the municipality bounds
const randomLat = 41.3275 + (Math.random() - 0.5) * 0.045;
const randomLng = 19.8187 + (Math.random() - 0.5) * 0.055;
const coords = [randomLat, randomLng];

// Vary the stats slightly so the popup text looks unique
const pseudoCrimes = item.totalCrimes + Math.floor((Math.random() - 0.5) * 150);
const pseudoLights = Math.min(10, Math.max(1, (item.avgLighting + (Math.random() - 0.5) * 2))).toFixed(2);
const pseudoPatrols = Math.min(10, Math.max(1, (item.avgPatrols + (Math.random() - 0.5) * 2))).toFixed(2);

const circle = L.circle(coords, {
color: getRiskColor(item.riskAssessment),
fillColor: getRiskColor(item.riskAssessment),
fillOpacity: 0.4,
radius: 280 // Slightly smaller circles looks much sharper when dense
}).addTo(map);

circle.bindPopup(`
<div style="font-family: Arial, sans-serif; line-height: 1.4;">
<strong style="font-size: 14px;">${item.location} (Zone Plot ${i+1}-${index+1})</strong><br/>
<hr style="margin: 5px 0; border: 0; border-top: 1px solid #eee;"/>
<b>Risk Level:</b> <span style="color:${getRiskColor(item.riskAssessment)}; font-weight:bold;">${item.riskAssessment}</span><br/>
<b>Incidents Count:</b> ${pseudoCrimes}<br/>
<b>Avg Street Lights:</b> ${pseudoLights}/10<br/>
<b>Avg Patrols:</b> ${pseudoPatrols}/10
</div>
`);
});
}

} catch (error) {
console.error("Frontend-to-API link error:", error);
}
}

loadThermalBlueprint();