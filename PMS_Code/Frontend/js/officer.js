// officer.js — Police Officer dashboard — fully functional

document.addEventListener('DOMContentLoaded', async () => {
  if (!requireAuth()) return;
  initUserInfo();
  await loadDashboardStats();
  startClock();
});

// ─── DASHBOARD ────────────────────────────────────────────────────────────────
async function loadDashboardStats() {
  const user = getUser();
  try {
    const [allCases, evidence, arrests] = await Promise.all([
      casesApi.getAll(),
      evidenceApi.getAll(),
      arrestsApi.getAll({ officerBadge: user?.badgeNumber })
    ]);

    // All cases (officer sees all, not just assigned — since assignment is by id which may be 0)
    const el = id => document.getElementById(id);
    if (el('dash-cases'))    el('dash-cases').textContent    = allCases.filter(c => c.status !== 'Closed').length;
    if (el('dash-reports'))  el('dash-reports').textContent  = allCases.length;
    if (el('dash-evidence')) el('dash-evidence').textContent = evidence.length;
    if (el('dash-arrests'))  el('dash-arrests').textContent  = arrests.length;

    // Active cases cards on dashboard
    renderDashboardCases(allCases.filter(c => c.status !== 'Closed').slice(0, 3));

    // Nav badge
    const navBadge = document.getElementById('cases-nav-badge');
    if (navBadge) navBadge.textContent = allCases.filter(c => c.status !== 'Closed').length;

    // Recent activity from audit logs (officer's own actions)
    renderRecentActivity(user?.badgeNumber);
  } catch { /* keep static */ }
}

async function renderRecentActivity(badge) {
  const container = document.getElementById('officer-recent-activity');
  if (!container || !badge) return;
  try {
    const data = await auditLogsApi.getAll({ userBadge: badge, pageSize: 5 });
    const logs = data.logs || [];
    if (!logs.length) {
      container.innerHTML = '<div style="color:var(--text-dim);font-size:12px;padding:12px 0;text-align:center;">No recent activity yet. Your actions will appear here.</div>';
      return;
    }
    const colors = { LOGIN:'var(--green)', CREATE:'var(--accent)', UPDATE:'var(--gold)', DELETE:'var(--red)', STATUS_CHANGE:'var(--purple)', PASSWORD_CHANGE:'var(--accent-2)' };
    container.innerHTML = logs.map(l => {
      const color = colors[l.actionType] || 'var(--text-dim)';
      const time  = relativeTime(l.timestamp);
      return `<div class="activity-item">
        <div class="activity-dot" style="background:${color}"></div>
        <div class="activity-content">
          <div class="activity-text">${l.description}</div>
          <div class="activity-time">${time}</div>
        </div>
      </div>`;
    }).join('');
  } catch {
    container.innerHTML = '<div style="color:var(--text-dim);font-size:12px;padding:12px 0;text-align:center;">Unable to load activity.</div>';
  }
}

// Format relative time (e.g. "2 hrs ago")
function relativeTime(d) {
  if (!d) return '';
  const diff = (Date.now() - new Date(d).getTime()) / 1000;
  if (diff < 60) return 'Just now';
  if (diff < 3600) return Math.floor(diff/60) + ' min ago';
  if (diff < 86400) return Math.floor(diff/3600) + ' hr' + (Math.floor(diff/3600)!==1?'s':'') + ' ago';
  if (diff < 86400*2) return 'Yesterday';
  if (diff < 86400*7) return Math.floor(diff/86400) + ' days ago';
  return new Date(d).toLocaleDateString('en-GB', { day:'2-digit', month:'short' });
}

function renderDashboardCases(cases) {
  const container = document.getElementById('dashboard-cases-list');
  if (!container) return;
  if (!cases.length) {
    container.innerHTML = '<div style="color:var(--text-dim);font-size:13px;padding:12px 0;">No active cases assigned to you.</div>';
    return;
  }
  container.innerHTML = cases.map(c => `
    <div class="incident-card ${c.status === 'Urgent' ? 'urgent' : c.status === 'Closed' ? 'closed' : ''}">
      <div style="display:flex;justify-content:space-between;align-items:flex-start;">
        <div>
          <div class="incident-id">${c.caseNumber} · ${c.crimeType}</div>
          <div class="incident-title">${c.description?.substring(0,60) || '—'}...</div>
          <div class="incident-meta">
            <span>📅 ${formatDate(c.filedDate)}</span>
            <span>📍 ${c.district || c.location || '—'}</span>
          </div>
        </div>
        <div style="display:flex;flex-direction:column;gap:6px;align-items:flex-end;">
          ${statusBadge(c.status)}
          ${priorityBadge(c.priority)}
        </div>
      </div>
      <div style="margin-top:12px;display:flex;gap:8px;">
        <button class="btn btn-primary btn-sm" onclick="openUpdateStatusModal(${c.id},'${c.caseNumber}','${c.status}')">Update Status</button>
        <button class="btn btn-secondary btn-sm" onclick="showPage('my-cases',null)">View All</button>
        <button class="btn btn-gold btn-sm" onclick="openLogEvidenceModal('${c.caseNumber}',${c.id})">Add Evidence</button>
      </div>
    </div>`).join('');
}

// ─── MY CASES ─────────────────────────────────────────────────────────────────
async function loadMyCases() {
  const tbody = document.getElementById('my-cases-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const cases = await casesApi.getAll();
    tbody.innerHTML = cases.length
      ? cases.map(c => `
          <tr>
            <td class="mono">${c.caseNumber}</td>
            <td>${c.crimeType}</td>
            <td>${c.description?.substring(0,45) || '—'}...</td>
            <td>${formatDate(c.filedDate)}</td>
            <td>${statusBadge(c.status)}</td>
            <td>${priorityBadge(c.priority)}</td>
            <td style="display:flex;gap:6px;">
              <button class="btn btn-primary btn-sm" onclick="openUpdateStatusModal(${c.id},'${c.caseNumber}','${c.status}')">Status</button>
              <button class="btn btn-gold btn-sm" onclick="openLogEvidenceModal('${c.caseNumber}',${c.id})">Evidence</button>
            </td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No cases found.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

// Update case status modal
function openUpdateStatusModal(caseId, caseNumber, currentStatus) {
  document.getElementById('status-case-id').value      = caseId;
  document.getElementById('status-case-label').textContent = caseNumber;
  document.getElementById('status-select').value       = currentStatus;
  document.getElementById('status-modal').style.display = 'flex';
}
function closeStatusModal() { document.getElementById('status-modal').style.display = 'none'; }

async function saveStatus() {
  const id     = document.getElementById('status-case-id').value;
  const status = document.getElementById('status-select').value;
  const notes  = document.getElementById('status-notes').value;
  try {
    await casesApi.update(id, { status, description: notes || undefined });
    showToast('Case status updated to ' + status + '!');
    closeStatusModal();
    await loadMyCases();
    await loadDashboardStats();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── NEW INCIDENT REPORT ──────────────────────────────────────────────────────
async function submitIncidentReport() {
  const user = getUser();
  const dateVal = document.getElementById('incident-date').value;
  const data = {
    crimeType:             document.getElementById('crime-type').value,
    incidentDate:          dateVal ? new Date(dateVal).toISOString() : new Date().toISOString(),
    location:              document.getElementById('incident-location').value.trim(),
    district:              document.getElementById('incident-district').value,
    description:           document.getElementById('incident-desc').value.trim(),
    reportingOfficerBadge: user?.badgeNumber || '',
    witnessCount:          parseInt(document.getElementById('witness-count')?.value) || 0,
    suspectsIdentified:    document.getElementById('suspects-identified')?.value !== 'No',
    priority:              document.getElementById('incident-priority')?.value || 'Low',
  };

  if (!data.description || !data.location) {
    showToast('Location and description are required.', 'error'); return;
  }

  try {
    const c = await casesApi.create(data);
    showToast('Incident report ' + c.caseNumber + ' submitted!');
    // Clear form
    ['incident-location','incident-desc','witness-count'].forEach(id => {
      const el = document.getElementById(id); if (el) el.value = '';
    });
    showPage('my-cases', null);
    await loadMyCases();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── EVIDENCE ─────────────────────────────────────────────────────────────────
async function loadEvidence() {
  const tbody = document.getElementById('evidence-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const items = await evidenceApi.getAll();
    tbody.innerHTML = items.length
      ? items.map(e => `
          <tr>
            <td class="mono">${e.evidenceNumber}</td>
            <td class="mono">${e.caseNumber}</td>
            <td>${e.type}</td>
            <td>${e.description}</td>
            <td>${e.chainOfCustody}</td>
            <td>${formatDate(e.collectedAt)}</td>
            <td>${statusBadge(e.status)}</td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No evidence logged.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

function openLogEvidenceModal(caseNumber = '', caseId = 0) {
  if (document.getElementById('evd-case-num')) document.getElementById('evd-case-num').value = caseNumber;
  if (document.getElementById('evd-case-id'))  document.getElementById('evd-case-id').value  = caseId;
  document.getElementById('evidence-modal').style.display = 'flex';
  // Populate case dropdown
  loadCaseDropdown();
}
function closeEvidenceModal() { document.getElementById('evidence-modal').style.display = 'none'; }

async function loadCaseDropdown() {
  const sel = document.getElementById('evd-case-select');
  if (!sel) return;
  try {
    const cases = await casesApi.getAll();
    sel.innerHTML = '<option value="">Select case...</option>' +
      cases.map(c => `<option value="${c.id}" data-num="${c.caseNumber}">${c.caseNumber} — ${c.crimeType}</option>`).join('');
    sel.onchange = () => {
      const opt = sel.options[sel.selectedIndex];
      if (document.getElementById('evd-case-id'))  document.getElementById('evd-case-id').value  = opt.value;
      if (document.getElementById('evd-case-num')) document.getElementById('evd-case-num').value = opt.dataset.num || '';
    };
  } catch {}
}

async function logEvidence() {
  const user = getUser();
  const data = {
    caseNumber:      document.getElementById('evd-case-num')?.value || '',
    caseId:          parseInt(document.getElementById('evd-case-id')?.value) || 0,
    type:            document.getElementById('evd-type')?.value || '',
    description:     document.getElementById('evd-desc')?.value.trim() || '',
    collectedBy:     user?.badgeNumber || '',
    storageLocation: document.getElementById('evd-storage')?.value || ''
  };

  if (!data.description || !data.type) {
    showToast('Type and description are required.', 'error'); return;
  }
  if (!data.caseId) {
    showToast('Please select a case.', 'error'); return;
  }

  try {
    const evd = await evidenceApi.create(data);
    showToast('Evidence ' + evd.evidenceNumber + ' logged!');
    closeEvidenceModal();
    // Clear fields
    ['evd-desc','evd-storage'].forEach(id => { const el = document.getElementById(id); if(el) el.value=''; });
    await loadEvidence();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── ARRESTS ─────────────────────────────────────────────────────────────────
async function loadArrests() {
  const user = getUser();
  const tbody = document.getElementById('arrests-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const items = await arrestsApi.getAll({ officerBadge: user?.badgeNumber });
    tbody.innerHTML = items.length
      ? items.map(a => `
          <tr>
            <td class="mono">${a.arrestNumber}</td>
            <td>${a.suspectName}</td>
            <td class="mono">${a.caseNumber}</td>
            <td>${a.charge}</td>
            <td>${formatDateTime(a.arrestedAt)}</td>
            <td>${a.arrestLocation}</td>
            <td>${statusBadge(a.status)}</td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No arrests recorded.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

function openArrestModal() {
  document.getElementById('arrest-modal').style.display = 'flex';
  loadArrestCaseDropdown();
}
function closeArrestModal() { document.getElementById('arrest-modal').style.display = 'none'; }

async function loadArrestCaseDropdown() {
  const sel = document.getElementById('arr-case-select');
  if (!sel) return;
  try {
    const cases = await casesApi.getAll();
    sel.innerHTML = '<option value="">Select case...</option>' +
      cases.map(c => `<option value="${c.id}" data-num="${c.caseNumber}">${c.caseNumber} — ${c.crimeType}</option>`).join('');
    sel.onchange = () => {
      const opt = sel.options[sel.selectedIndex];
      if (document.getElementById('arr-case-id'))  document.getElementById('arr-case-id').value  = opt.value;
      if (document.getElementById('arr-case-num')) document.getElementById('arr-case-num').value = opt.dataset.num || '';
    };
  } catch {}
}

async function recordArrest() {
  const user = getUser();
  const data = {
    suspectName:           document.getElementById('arr-suspect')?.value.trim() || '',
    caseNumber:            document.getElementById('arr-case-num')?.value || '',
    caseId:                parseInt(document.getElementById('arr-case-id')?.value) || 0,
    charge:                document.getElementById('arr-charge')?.value.trim() || '',
    arrestLocation:        document.getElementById('arr-location')?.value.trim() || '',
    arrestingOfficerBadge: user?.badgeNumber || ''
  };

  if (!data.suspectName || !data.charge || !data.arrestLocation) {
    showToast('Suspect name, charge and location are required.', 'error'); return;
  }
  if (!data.caseId) {
    showToast('Please select a case.', 'error'); return;
  }

  try {
    const arr = await arrestsApi.create(data);
    showToast('Arrest record ' + arr.arrestNumber + ' created!');
    closeArrestModal();
    ['arr-suspect','arr-charge','arr-location'].forEach(id => { const el=document.getElementById(id); if(el) el.value=''; });
    await loadArrests();
    await loadDashboardStats();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── PAGE NAVIGATION ─────────────────────────────────────────────────────────
function showPage(id, el) {
  document.querySelectorAll('[id^="page-"]').forEach(p => p.style.display = 'none');
  const page = document.getElementById('page-' + id);
  if (page) page.style.display = 'block';
  if (el) { document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active')); el.classList.add('active'); }
  const titles = { dashboard:'Officer Dashboard', 'my-cases':'My Cases', 'new-report':'New Incident Report', evidence:'Evidence Management', arrests:'Arrest Records', patrols:'Patrol Management' };
  const titleEl = document.querySelector('.topbar-title');
  if (titleEl) titleEl.textContent = titles[id] || '';

  if (id === 'dashboard')  loadDashboardStats();
  if (id === 'my-cases')   loadMyCases();
  if (id === 'evidence')   loadEvidence();
  if (id === 'arrests')    loadArrests();
  if (id === 'patrols')    loadPatrols();
}

function startClock() {
  const update = () => { const el=document.getElementById('clock'); if(el) el.textContent=new Date().toLocaleTimeString('en-GB',{hour:'2-digit',minute:'2-digit',second:'2-digit'}); };
  setInterval(update, 1000); update();
}

// ─── PATROLS (FR_15 / UC8) ────────────────────────────────────────────────────
async function loadPatrols() {
  const tbody = document.getElementById('patrols-tbody');
  if (!tbody) return;
  const user = getUser();
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const patrols = await patrolsApi.getAll({ officerBadge: user?.badgeNumber });
    tbody.innerHTML = patrols.length
      ? patrols.map(p => `
          <tr>
            <td class="mono">${p.patrolNumber}</td>
            <td>${p.route}</td>
            <td>${p.area}</td>
            <td>${formatDateTime(p.startTime)}</td>
            <td>${formatDateTime(p.endTime)}</td>
            <td>${statusBadge(p.status)}</td>
            <td style="display:flex;gap:6px;">
              ${p.status === 'Scheduled' ? `<button class="btn btn-primary btn-sm" onclick="updatePatrolStatus(${p.id},'Active')">Start</button>` : ''}
              ${p.status === 'Active' ? `<button class="btn btn-gold btn-sm" onclick="updatePatrolStatus(${p.id},'Completed')">Complete</button>` : ''}
              ${p.status === 'Scheduled' ? `<button class="btn btn-sm" onclick="updatePatrolStatus(${p.id},'Cancelled')">Cancel</button>` : ''}
            </td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No patrols found.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

function openPatrolModal() {
  const modal = document.getElementById('patrol-modal');
  if (modal) modal.style.display = 'flex';
  ['pat-route','pat-area','pat-notes'].forEach(id => { const el=document.getElementById(id); if(el) el.value=''; });
}

function closePatrolModal() {
  const modal = document.getElementById('patrol-modal');
  if (modal) modal.style.display = 'none';
}

async function savePatrol() {
  const route = document.getElementById('pat-route')?.value?.trim();
  const area  = document.getElementById('pat-area')?.value?.trim();
  const start = document.getElementById('pat-start')?.value;
  const end   = document.getElementById('pat-end')?.value;
  if (!route || !area || !start || !end) { showToast('Route, area, start and end time are required.', 'error'); return; }
  if (new Date(end) <= new Date(start)) { showToast('End time must be after start time.', 'error'); return; }
  const user = getUser();
  try {
    await patrolsApi.create({
      officerBadge: user?.badgeNumber || '',
      officerName: user?.fullName || '',
      route, area,
      startTime: new Date(start).toISOString(),
      endTime: new Date(end).toISOString(),
      notes: document.getElementById('pat-notes')?.value?.trim() || ''
    });
    showToast('Patrol scheduled successfully.');
    closePatrolModal();
    loadPatrols();
  } catch (err) { showToast(err.message, 'error'); }
}

async function updatePatrolStatus(id, status) {
  try {
    await patrolsApi.updateStatus(id, status);
    showToast(`Patrol marked as ${status}.`);
    loadPatrols();
  } catch (err) { showToast(err.message, 'error'); }
}
