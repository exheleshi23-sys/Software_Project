// traffic.js — Traffic Officer dashboard — database-driven and cleaned

function asArray(data, key) {
  if (Array.isArray(data)) return data;
  if (data && Array.isArray(data[key])) return data[key];
  if (data && Array.isArray(data.items)) return data.items;
  return [];
}

function safe(v) {
  return String(v ?? '—').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}

function money(v) {
  const n = Number(v || 0);
  return '€' + n.toLocaleString();
}

function isSameDay(dateValue, dateObj) {
  if (!dateValue) return false;
  const d = new Date(dateValue);
  return d.getFullYear() === dateObj.getFullYear()
    && d.getMonth() === dateObj.getMonth()
    && d.getDate() === dateObj.getDate();
}

function getFineDate(f) {
  return f.issuedAt || f.dateIssued || f.createdAt || f.date || f.dateTime;
}

function getCaseDate(c) {
  return c.incidentDate || c.dateFiled || c.createdAt || c.date;
}

document.addEventListener('DOMContentLoaded', async () => {
  if (!requireAuth()) return;
  initUserInfo();

  const user = getUser();
  const ioEl = document.getElementById('fine-issuing-officer');
  if (ioEl && user) ioEl.value = `${user.fullName || user.FullName || 'Officer'} (${user.badgeNumber || ''})`;

  await loadDashboardStats();
  startClock();
});

// ─── DASHBOARD ────────────────────────────────────────────────────────────────
async function loadDashboardStats() {
  const user = getUser();
  const officerBadge = user?.badgeNumber || user?.BadgeNumber || '';

  try {
    const [finesData, casesData] = await Promise.all([
      finesApi.getAll(officerBadge ? { officerBadge } : {}),
      casesApi.getAll()
    ]);

    const allFines = asArray(finesData, 'fines');
    const allCases = asArray(casesData, 'cases');
    const today = new Date();
    const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);

    const todayFines = allFines.filter(f => isSameDay(getFineDate(f), today));
    const monthFines = allFines.filter(f => new Date(getFineDate(f)) >= monthStart);
    const paidMonthFines = monthFines.filter(f => String(f.status || '').toLowerCase() === 'paid');
    const collected = paidMonthFines.reduce((sum, f) => sum + Number(f.amount || 0), 0);
    const paymentRate = monthFines.length ? Math.round((paidMonthFines.length / monthFines.length) * 100) : 0;
    const accidents = allCases.filter(c => String(c.crimeType || c.type || '').toLowerCase().includes('traffic accident'));
    const accidentsToday = accidents.filter(c => isSameDay(getCaseDate(c), today));

    setText('stat-fines-today', todayFines.length);
    setText('stat-accidents-today', accidentsToday.length);
    setText('stat-fines-month', monthFines.length);
    setText('stat-collected', money(collected));
    setText('history-fines-month', monthFines.length);
    setText('history-total-collected', money(collected));
    setText('history-payment-rate', paymentRate + '%');

    const badge = document.getElementById('today-fines-badge');
    if (badge) {
      if (todayFines.length > 0) {
        badge.textContent = `Today: ${todayFines.length}`;
        badge.style.display = 'inline-flex';
      } else {
        badge.textContent = '';
        badge.style.display = 'none';
      }
    }

    renderRecentViolations(allFines);
    renderAccidentPreview(accidents);
  } catch (err) {
    renderRecentViolations([]);
    renderAccidentPreview([]);
    showToast(err.message || 'Could not load traffic dashboard data.', 'error');
  }
}

function setText(id, value) {
  const el = document.getElementById(id);
  if (el) el.textContent = value;
}

function renderRecentViolations(fines) {
  const list = document.getElementById('recent-violations-list');
  if (!list) return;

  const sorted = [...fines].sort((a, b) => new Date(getFineDate(b) || 0) - new Date(getFineDate(a) || 0)).slice(0, 5);
  if (!sorted.length) {
    list.innerHTML = '<div style="color:var(--text-dim);font-size:13px;padding:12px 0;">No recent violations issued.</div>';
    return;
  }

  list.innerHTML = sorted.map(f => `
    <div class="violation-card">
      <div class="plate-badge">${safe(f.licensePlate)}</div>
      <div style="flex:1;">
        <div style="font-size:14px;font-weight:500;">${safe(f.violationType)}</div>
        <div style="font-size:12px;color:var(--text-dim);margin-top:3px;">${safe(f.location)} · ${formatDateTime(getFineDate(f))}</div>
      </div>
      <div style="text-align:right;">
        <div class="fine-amount">${money(f.amount)}</div>
        <div style="margin-top:4px;display:inline-block;">${statusBadge(f.status || 'Unpaid')}</div>
      </div>
    </div>`).join('');
}

function renderAccidentPreview(accidents) {
  const list = document.getElementById('accident-cases-preview');
  if (!list) return;

  const sorted = [...accidents].sort((a, b) => new Date(getCaseDate(b) || 0) - new Date(getCaseDate(a) || 0)).slice(0, 4);
  if (!sorted.length) {
    list.innerHTML = '<div style="color:var(--text-dim);font-size:12px;padding:12px 0;text-align:center;">No accident cases found.</div>';
    return;
  }

  list.innerHTML = sorted.map(c => `
    <div class="activity-item">
      <div class="activity-dot" style="background:${String(c.status || '').toLowerCase() === 'closed' ? 'var(--text-dim)' : 'var(--gold)'}"></div>
      <div class="activity-content">
        <div class="activity-text"><strong>${safe(c.caseNumber)}</strong> — ${safe(c.description || c.crimeType || 'Traffic Accident')}</div>
        <div class="activity-time">${formatDateTime(getCaseDate(c))} · ${safe(c.status || 'Open')}</div>
      </div>
    </div>`).join('');
}

// ─── ISSUE FINE ───────────────────────────────────────────────────────────────
async function issueQuickFine() {
  const plateEl = document.getElementById('quick-fine-plate');
  const amountEl = document.getElementById('quick-fine-amount');
  const violationEl = document.getElementById('quick-fine-violation');

  const fullPlate = document.getElementById('fine-plate');
  const fullAmount = document.getElementById('fine-amount');
  const fullViolation = document.getElementById('fine-violation');

  if (fullPlate && plateEl) fullPlate.value = plateEl.value;
  if (fullAmount && amountEl) fullAmount.value = amountEl.value;
  if (fullViolation && violationEl) fullViolation.value = violationEl.value;

  await issueFine();

  if (plateEl) plateEl.value = '';
  if (amountEl) amountEl.value = '';
}

async function issueFine() {
  const user = getUser();
  const plate = document.getElementById('fine-plate')?.value?.trim().toUpperCase();
  const amount = parseFloat(document.getElementById('fine-amount')?.value);

  if (!plate || !amount) {
    showToast('Plate and amount are required.', 'error');
    return;
  }

  const dateValue = document.getElementById('fine-date')?.value;
  const data = {
    licensePlate: plate,
    vehicleType: document.getElementById('fine-vehicle-type')?.value || 'Car',
    violationType: document.getElementById('fine-violation')?.value || '',
    amount,
    location: document.getElementById('fine-location')?.value?.trim() || '',
    issuingOfficerBadge: user?.badgeNumber || '',
    issuedAt: dateValue ? new Date(dateValue).toISOString() : undefined,
    notes: document.getElementById('fine-notes')?.value || null
  };

  try {
    const fine = await finesApi.create(data);
    showToast('Fine ' + (fine.fineNumber || '') + ' issued to ' + plate + '!');
    ['fine-plate', 'fine-amount', 'fine-location', 'fine-notes', 'fine-date'].forEach(id => {
      const el = document.getElementById(id);
      if (el) el.value = '';
    });
    await loadDashboardStats();
    if (document.getElementById('fines-history-tbody')) await loadFinesHistory();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// ─── FINES HISTORY ────────────────────────────────────────────────────────────
async function loadFinesHistory() {
  const user = getUser();
  const tbody = document.getElementById('fines-history-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';

  try {
    const data = await finesApi.getAll(user?.badgeNumber ? { officerBadge: user.badgeNumber } : {});
    const fines = asArray(data, 'fines');
    const today = new Date();
    const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);
    const monthFines = fines.filter(f => new Date(getFineDate(f)) >= monthStart);
    const paidMonthFines = monthFines.filter(f => String(f.status || '').toLowerCase() === 'paid');
    const collected = paidMonthFines.reduce((sum, f) => sum + Number(f.amount || 0), 0);
    const paymentRate = monthFines.length ? Math.round((paidMonthFines.length / monthFines.length) * 100) : 0;

    setText('history-fines-month', monthFines.length);
    setText('history-total-collected', money(collected));
    setText('history-payment-rate', paymentRate + '%');

    tbody.innerHTML = fines.length
      ? fines.map(f => `
          <tr>
            <td class="mono">${safe(f.fineNumber)}</td>
            <td class="mono" style="letter-spacing:2px;">${safe(f.licensePlate)}</td>
            <td>${safe(f.violationType)}</td>
            <td style="color:${Number(f.amount || 0) > 200 ? 'var(--red)' : 'var(--gold)'};">${money(f.amount)}</td>
            <td>${formatDateTime(getFineDate(f))}</td>
            <td>${safe(f.location)}</td>
            <td>${statusBadge(f.status || 'Unpaid')}</td>
            <td>
              ${String(f.status || '').toLowerCase() === 'unpaid'
                ? `<button class="btn btn-secondary btn-sm" onclick="updateFineStatus(${f.id},'Paid')">Mark Paid</button>`
                : `<span style="color:var(--text-dim);font-size:12px;">${safe(f.status)}</span>`}
            </td>
          </tr>`).join('')
      : '<tr><td colspan="8" style="text-align:center;color:var(--text-dim);padding:20px;">No fines issued yet.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="8" style="text-align:center;color:var(--red);padding:20px;">${safe(err.message)}</td></tr>`;
  }
}

async function updateFineStatus(id, status) {
  try {
    await finesApi.updateStatus(id, status);
    showToast('Fine marked as ' + status + '.');
    await loadFinesHistory();
    await loadDashboardStats();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// ─── VEHICLE LOOKUP ───────────────────────────────────────────────────────────
async function searchVehicle() {
  const plate = document.getElementById('vehicle-search')?.value?.trim().toUpperCase();
  if (!plate) {
    showToast('Enter a license plate.', 'error');
    return;
  }

  try {
    const data = await finesApi.getAll({ licensePlate: plate });
    const fines = asArray(data, 'fines');
    const resultEl = document.getElementById('vehicle-result');
    const titleEl = document.getElementById('vehicle-result-title');
    if (resultEl) resultEl.style.display = 'block';
    if (titleEl) titleEl.textContent = 'Results for: ' + plate;

    const setVal = (id, value) => {
      const el = document.getElementById(id);
      if (el) el.value = value || '—';
    };
    setVal('vr-owner', '—');
    setVal('vr-national-id', '—');
    setVal('vr-license-num', '—');
    setVal('vr-license-status', '—');

    const linkedCitizenId = fines.find(f => f.citizenUserId)?.citizenUserId;
    if (linkedCitizenId) {
      try {
        const owner = await usersApi.getById(linkedCitizenId);
        setVal('vr-owner', owner?.fullName || owner?.FullName || '—');
      } catch {}
      try {
        const licenseData = await licensesApi.getAll({ citizenUserId: linkedCitizenId });
        const licenses = asArray(licenseData, 'licenses');
        if (licenses.length) {
          const lic = licenses[0];
          setVal('vr-national-id', lic.holderNationalId || '—');
          setVal('vr-license-num', lic.licenseNumber || '—');
          setVal('vr-license-status', lic.status || 'Active');
        }
      } catch {}
    }

    const tbody = document.getElementById('vehicle-fines-tbody');
    if (tbody) {
      tbody.innerHTML = fines.length
        ? fines.map(f => `
            <tr>
              <td class="mono">${safe(f.fineNumber)}</td>
              <td>${safe(f.violationType)}</td>
              <td style="color:${String(f.status || '').toLowerCase() === 'paid' ? 'var(--green)' : 'var(--red)'};">${money(f.amount)}</td>
              <td>${formatDate(getFineDate(f))}</td>
              <td>${statusBadge(f.status || 'Unpaid')}</td>
            </tr>`).join('')
        : '<tr><td colspan="5" style="text-align:center;color:var(--text-dim);padding:12px;">No fines on record.</td></tr>';
    }
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// ─── ACCIDENTS ────────────────────────────────────────────────────────────────
async function loadAccidents() {
  const tbody = document.getElementById('accidents-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';

  try {
    const data = await casesApi.getAll();
    const cases = asArray(data, 'cases');
    const accidents = cases.filter(c => String(c.crimeType || c.type || '').toLowerCase().includes('traffic accident'));
    renderAccidentPreview(accidents);

    tbody.innerHTML = accidents.length
      ? accidents.map(c => `
          <tr>
            <td class="mono">${safe(c.caseNumber)}</td>
            <td>${safe(c.location)}</td>
            <td>${safe(c.witnessCount || 0)} vehicles</td>
            <td>${formatDateTime(getCaseDate(c))}</td>
            <td>${statusBadge(c.status || 'Open')}</td>
            <td><button class="btn btn-secondary btn-sm" onclick="openStatusModal(${c.id},'${safe(c.caseNumber)}','${safe(c.status || 'Open')}')">Update</button></td>
          </tr>`).join('')
      : '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">No accidents reported.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:var(--red);padding:20px;">${safe(err.message)}</td></tr>`;
  }
}

async function fileAccidentReport() {
  const user = getUser();
  const dateVal = document.getElementById('acc-datetime')?.value;
  const data = {
    crimeType: 'Traffic Accident',
    description: document.getElementById('acc-desc')?.value.trim() || '',
    location: document.getElementById('acc-location')?.value.trim() || '',
    district: document.getElementById('acc-district')?.value || 'District 1',
    incidentDate: dateVal ? new Date(dateVal).toISOString() : new Date().toISOString(),
    reportingOfficerBadge: user?.badgeNumber || '',
    witnessCount: parseInt(document.getElementById('acc-vehicles')?.value) || 0,
    suspectsIdentified: false,
    priority: 'Medium'
  };

  if (!data.description || !data.location) {
    showToast('Description and location are required.', 'error');
    return;
  }

  try {
    const c = await casesApi.create(data);
    showToast('Accident report ' + (c.caseNumber || '') + ' filed!');
    ['acc-desc', 'acc-location', 'acc-vehicles', 'acc-datetime'].forEach(id => {
      const el = document.getElementById(id);
      if (el) el.value = '';
    });
    await loadAccidents();
    await loadDashboardStats();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

function openStatusModal(caseId, caseNumber, currentStatus) {
  document.getElementById('acc-status-case-id').value = caseId;
  document.getElementById('acc-status-case-label').textContent = caseNumber;
  document.getElementById('acc-status-select').value = currentStatus;
  document.getElementById('acc-status-modal').style.display = 'flex';
}

function closeStatusModal() {
  const modal = document.getElementById('acc-status-modal');
  if (modal) modal.style.display = 'none';
}

async function saveAccidentStatus() {
  const id = document.getElementById('acc-status-case-id').value;
  const status = document.getElementById('acc-status-select').value;
  try {
    await casesApi.update(id, { status });
    showToast('Accident status updated to ' + status + '.');
    closeStatusModal();
    await loadAccidents();
    await loadDashboardStats();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// ─── PAGE NAVIGATION ─────────────────────────────────────────────────────────
function showPage(id, el) {
  document.querySelectorAll('[id^="page-"]').forEach(p => p.style.display = 'none');
  const page = document.getElementById('page-' + id);
  if (page) page.style.display = 'block';
  if (el) {
    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
    el.classList.add('active');
  }

  const titles = {
    dashboard: 'Traffic Officer Dashboard',
    fines: 'Issue Fine',
    accidents: 'Accident Reports',
    vehicles: 'Vehicle Lookup',
    history: 'Fines History',
    licenses: 'Driving Licenses'
  };
  const titleEl = document.querySelector('.topbar-title');
  if (titleEl) titleEl.textContent = titles[id] || '';

  if (id === 'dashboard') loadDashboardStats();
  if (id === 'history') loadFinesHistory();
  if (id === 'accidents') loadAccidents();
  if (id === 'licenses') loadLicenses();
}

function startClock() {
  const update = () => {
    const el = document.getElementById('clock');
    if (el) el.textContent = new Date().toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  };
  setInterval(update, 1000);
  update();
}

// ─── DRIVING LICENSES (FR_36 / UC27) ─────────────────────────────────────────
async function loadLicenses() {
  const tbody = document.getElementById('licenses-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Use the search above to find a license.</td></tr>';
}

async function searchLicenses() {
  const tbody = document.getElementById('licenses-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Searching...</td></tr>';

  const params = {};
  const n = document.getElementById('lic-search-number')?.value?.trim();
  const name = document.getElementById('lic-search-name')?.value?.trim();
  const nid = document.getElementById('lic-search-nid')?.value?.trim();
  const status = document.getElementById('lic-search-status')?.value;
  if (n) params.licenseNumber = n;
  if (name) params.holderName = name;
  if (nid) params.nationalId = nid;
  if (status) params.status = status;

  try {
    const data = await licensesApi.getAll(params);
    const licenses = asArray(data, 'licenses');
    tbody.innerHTML = licenses.length
      ? licenses.map(l => {
          const isExpired = new Date(l.expiryDate) < new Date();
          const statusClass = l.status === 'Valid' ? 'status-active' : l.status === 'Suspended' || l.status === 'Revoked' ? 'status-urgent' : 'status-closed';
          return `<tr>
            <td class="mono">${safe(l.licenseNumber)}</td>
            <td>${safe(l.holderName)}</td>
            <td class="mono">${safe(l.holderNationalId)}</td>
            <td><span style="font-family:'IBM Plex Mono',monospace;font-size:12px;">${safe(l.category)}</span></td>
            <td style="${isExpired ? 'color:var(--red);' : ''}">${formatDate(l.expiryDate)}</td>
            <td><span class="status ${statusClass}">${safe(l.status)}</span></td>
            <td><button class="btn btn-danger btn-sm" onclick="openFlagModal(${l.id},'${safe(l.licenseNumber)}')">Flag/Suspend</button></td>
          </tr>`;
        }).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No license record found.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${safe(err.message)}</td></tr>`;
  }
}

function openAddLicenseModal() {
  const modal = document.getElementById('add-license-modal');
  if (modal) modal.style.display = 'flex';
  ['lic-number', 'lic-holder', 'lic-nid', 'lic-issue', 'lic-expiry'].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.value = '';
  });
  const cat = document.getElementById('lic-cat');
  if (cat) cat.value = '';
}

function closeAddLicenseModal() {
  const modal = document.getElementById('add-license-modal');
  if (modal) modal.style.display = 'none';
}

async function saveLicense() {
  const num = document.getElementById('lic-number')?.value?.trim();
  const holder = document.getElementById('lic-holder')?.value?.trim();
  const nid = document.getElementById('lic-nid')?.value?.trim();
  const cat = document.getElementById('lic-cat')?.value;
  const issue = document.getElementById('lic-issue')?.value;
  const expiry = document.getElementById('lic-expiry')?.value;

  if (!num || !holder || !nid || !cat || !issue || !expiry) {
    showToast('All fields are required.', 'error');
    return;
  }

  try {
    await licensesApi.create({ licenseNumber: num, holderName: holder, holderNationalId: nid, category: cat, issueDate: issue, expiryDate: expiry });
    showToast('License registered successfully.');
    closeAddLicenseModal();
    searchLicenses();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

function openFlagModal(id, num) {
  const modal = document.getElementById('flag-license-modal');
  if (modal) modal.style.display = 'flex';
  const numEl = document.getElementById('flag-license-num');
  if (numEl) numEl.textContent = num;
  const idEl = document.getElementById('flag-license-id');
  if (idEl) idEl.value = id;
  const reason = document.getElementById('flag-reason');
  if (reason) reason.value = '';
}

function closeFlagModal() {
  const modal = document.getElementById('flag-license-modal');
  if (modal) modal.style.display = 'none';
}

async function saveFlagLicense() {
  const id = document.getElementById('flag-license-id')?.value;
  const status = document.getElementById('flag-status')?.value;
  const reason = document.getElementById('flag-reason')?.value?.trim();
  if (!reason) {
    showToast('Please enter a reason.', 'error');
    return;
  }

  const user = getUser();
  try {
    await licensesApi.flag(id, { status, reason, officerBadge: user?.badgeNumber || '' });
    showToast(`License status updated to ${status}.`);
    closeFlagModal();
    searchLicenses();
  } catch (err) {
    showToast(err.message, 'error');
  }
}
