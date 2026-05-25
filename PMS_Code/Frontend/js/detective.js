// detective.js — Detective dashboard — fully functional

document.addEventListener('DOMContentLoaded', async () => {
  if (!requireAuth()) return;
  initUserInfo();
  await loadDashboardStats();
  startClock();
});

// ─── DASHBOARD ────────────────────────────────────────────────────────────────
async function loadDashboardStats() {
  try {
    const [cases, suspects, evidence] = await Promise.all([
      casesApi.getAll(), suspectsApi.getAll(), evidenceApi.getAll()
    ]);
    const el = id => document.getElementById(id);
    if (el('stat-active-inv'))    el('stat-active-inv').textContent    = cases.filter(c => c.status !== 'Closed').length;
    if (el('stat-total-suspects'))el('stat-total-suspects').textContent= suspects.length;
    if (el('stat-evidence-items'))el('stat-evidence-items').textContent= evidence.length;
    if (el('stat-closed'))        el('stat-closed').textContent        = cases.filter(c => c.status === 'Closed').length;

    // Dashboard recent cases
    const list = document.getElementById('dashboard-inv-list');
    if (list) {
      const active = cases.filter(c => c.status !== 'Closed').slice(0, 3);
      list.innerHTML = active.length
        ? active.map(c => `
            <div style="padding:14px 0;border-bottom:1px solid var(--border-2);">
              <div style="display:flex;justify-content:space-between;align-items:center;">
                <div>
                  <div class="mono" style="font-size:11px;color:var(--text-dim);">${c.caseNumber}</div>
                  <div style="font-size:14px;font-weight:500;margin:3px 0;">${c.crimeType} — ${c.location||'Unknown'}</div>
                  <div style="font-size:12px;color:var(--text-dim);">${formatDate(c.filedDate)}</div>
                </div>
                <div style="display:flex;flex-direction:column;gap:6px;align-items:flex-end;">
                  ${statusBadge(c.status)} ${priorityBadge(c.priority)}
                </div>
              </div>
              <div style="display:flex;gap:8px;margin-top:10px;">
                <button class="btn btn-primary btn-sm" onclick="openStatusModal(${c.id},'${c.caseNumber}','${c.status}')">Update Status</button>
                <button class="btn btn-secondary btn-sm" onclick="openSuspectModal('${c.caseNumber}',${c.id})">Add Suspect</button>
              </div>
            </div>`).join('')
        : '<div style="color:var(--text-dim);font-size:13px;padding:12px 0;">No active investigations.</div>';
    }
  } catch { /* keep static */ }
}

// ─── INVESTIGATIONS ───────────────────────────────────────────────────────────
async function loadInvestigations() {
  const tbody = document.getElementById('investigations-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const [cases, suspects] = await Promise.all([casesApi.getAll(), suspectsApi.getAll()]);
    tbody.innerHTML = cases.length
      ? cases.map(c => {
          const susp = suspects.filter(s => s.caseId === c.id).length;
          return `
            <tr>
              <td class="mono">${c.caseNumber}</td>
              <td>${c.crimeType}</td>
              <td>${susp}</td>
              <td>${c.witnessCount || 0}</td>
              <td>${statusBadge(c.status)}</td>
              <td>${priorityBadge(c.priority)}</td>
              <td style="display:flex;gap:6px;">
                <button class="btn btn-primary btn-sm" onclick="openStatusModal(${c.id},'${c.caseNumber}','${c.status}')">Status</button>
                <button class="btn btn-secondary btn-sm" onclick="openSuspectModal('${c.caseNumber}',${c.id})">+ Suspect</button>
              </td>
            </tr>`;
        }).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No investigations found.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

// Update case status
function openStatusModal(caseId, caseNumber, currentStatus) {
  document.getElementById('det-status-case-id').value       = caseId;
  document.getElementById('det-status-case-label').textContent = caseNumber;
  document.getElementById('det-status-select').value        = currentStatus;
  document.getElementById('det-status-modal').style.display = 'flex';
}
function closeStatusModal() { document.getElementById('det-status-modal').style.display = 'none'; }

async function saveStatus() {
  const id     = document.getElementById('det-status-case-id').value;
  const status = document.getElementById('det-status-select').value;
  try {
    await casesApi.update(id, { status });
    showToast('Case status updated to ' + status + '!');
    closeStatusModal();
    await loadInvestigations();
    await loadDashboardStats();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── SUSPECTS ─────────────────────────────────────────────────────────────────
async function loadSuspects() {
  const tbody = document.getElementById('suspects-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const suspects = await suspectsApi.getAll();
    tbody.innerHTML = suspects.length
      ? suspects.map(s => `
          <tr>
            <td>${s.fullName}${s.age ? ', ' + s.age : ''}</td>
            <td>${s.gender || '—'}</td>
            <td>${s.physicalDescription || '—'}</td>
            <td class="mono">${s.caseNumber}</td>
            <td>${s.charge}</td>
            <td>${statusBadge(s.status)}</td>
            <td>
              <select class="btn btn-secondary btn-sm" style="padding:4px 8px;font-size:11px;" onchange="updateSuspectStatus(${s.id},this.value)">
                <option value="">Status...</option>
                <option value="Identified">Identified</option>
                <option value="Detained">Detained</option>
                <option value="Arrested">Arrested</option>
                <option value="Released">Released</option>
                <option value="At Large">At Large</option>
              </select>
            </td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No suspects in database.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

function openSuspectModal(caseNumber = '', caseId = 0) {
  if (document.getElementById('susp-case-num')) document.getElementById('susp-case-num').value = caseNumber;
  if (document.getElementById('susp-case-id'))  document.getElementById('susp-case-id').value  = caseId;
  document.getElementById('suspect-modal').style.display = 'flex';
  loadSuspectCaseDropdown(caseId);
}
function closeSuspectModal() { document.getElementById('suspect-modal').style.display = 'none'; }

async function loadSuspectCaseDropdown(selectedId = 0) {
  const sel = document.getElementById('susp-case-select');
  if (!sel) return;
  try {
    const cases = await casesApi.getAll();
    sel.innerHTML = '<option value="">Select case...</option>' +
      cases.map(c => `<option value="${c.id}" data-num="${c.caseNumber}" ${c.id == selectedId ? 'selected' : ''}>${c.caseNumber} — ${c.crimeType}</option>`).join('');
    sel.onchange = () => {
      const opt = sel.options[sel.selectedIndex];
      if (document.getElementById('susp-case-id'))  document.getElementById('susp-case-id').value  = opt.value;
      if (document.getElementById('susp-case-num')) document.getElementById('susp-case-num').value = opt.dataset.num || '';
    };
  } catch {}
}

async function addSuspect() {
  const user = getUser();
  const data = {
    fullName:            document.getElementById('susp-name')?.value.trim() || '',
    age:                 parseInt(document.getElementById('susp-age')?.value) || 0,
    gender:              document.getElementById('susp-gender')?.value || '',
    physicalDescription: document.getElementById('susp-desc')?.value.trim() || '',
    caseNumber:          document.getElementById('susp-case-num')?.value || '',
    caseId:              parseInt(document.getElementById('susp-case-id')?.value) || 0,
    charge:              document.getElementById('susp-charge')?.value.trim() || '',
    addedByBadge:        user?.badgeNumber || ''
  };
  if (!data.fullName || !data.charge) { showToast('Name and charge are required.', 'error'); return; }
  if (!data.caseId) { showToast('Please select a case.', 'error'); return; }
  try {
    await suspectsApi.create(data);
    showToast('Suspect added to database!');
    closeSuspectModal();
    ['susp-name','susp-age','susp-desc','susp-charge'].forEach(id => { const el=document.getElementById(id); if(el) el.value=''; });
    await loadSuspects();
    await loadDashboardStats();
  } catch (err) { showToast(err.message, 'error'); }
}

async function updateSuspectStatus(id, status) {
  if (!status) return;
  try {
    await suspectsApi.updateStatus(id, status);
    showToast('Suspect status updated.');
    await loadSuspects();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── EVIDENCE ─────────────────────────────────────────────────────────────────
async function loadEvidence() {
  const tbody = document.getElementById('det-evidence-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
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
            <td>${statusBadge(e.status)}</td>
          </tr>`).join('')
      : '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">No evidence items.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

// ─── REPORTS ─────────────────────────────────────────────────────────────────
async function loadReports() {
  const tbody = document.getElementById('inv-reports-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const cases = await casesApi.getAll();
    tbody.innerHTML = cases.length
      ? cases.map(c => `
          <tr>
            <td class="mono">${c.caseNumber}</td>
            <td>${c.crimeType}</td>
            <td>${c.description?.substring(0,60)||'—'}...</td>
            <td>${formatDate(c.filedDate)}</td>
            <td>${statusBadge(c.status)}</td>
          </tr>`).join('')
      : '<tr><td colspan="5" style="text-align:center;color:var(--text-dim);padding:20px;">No reports found.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

// ─── PAGE NAVIGATION ─────────────────────────────────────────────────────────
function showPage(id, el) {
  document.querySelectorAll('[id^="page-"]').forEach(p => p.style.display = 'none');
  const page = document.getElementById('page-' + id);
  if (page) page.style.display = 'block';
  if (el) { document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active')); el.classList.add('active'); }
  const t = { dashboard:'Detective Dashboard', cases:'My Investigations', suspects:'Suspects Database', evidence:'Evidence Management', reports:'Investigation Reports', witnesses:'Witness Management' };
  const titleEl = document.querySelector('.topbar-title');
  if (titleEl) titleEl.textContent = t[id] || '';
  if (id === 'dashboard') loadDashboardStats();
  if (id === 'cases')     loadInvestigations();
  if (id === 'suspects')  loadSuspects();
  if (id === 'evidence')  loadEvidence();
  if (id === 'reports')   loadReports();
  if (id === 'witnesses') loadWitnesses();
}

function startClock() {
  const update = () => { const el=document.getElementById('clock'); if(el) el.textContent=new Date().toLocaleTimeString('en-GB',{hour:'2-digit',minute:'2-digit',second:'2-digit'}); };
  setInterval(update, 1000); update();
}

// ─── WITNESSES (UC13 / FR_21) ─────────────────────────────────────────────────
let _allWitnesses = [];

async function loadWitnesses() {
  const tbody = document.getElementById('witnesses-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    _allWitnesses = await witnessesApi.getAll();
    renderWitnesses(_allWitnesses);
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="8" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

function filterWitnesses() {
  const caseFilter = document.getElementById('wit-filter-case')?.value?.toLowerCase() || '';
  const hideConf = document.getElementById('wit-hide-confidential')?.checked;
  const filtered = _allWitnesses.filter(w => {
    if (caseFilter && !w.caseNumber?.toLowerCase().includes(caseFilter)) return false;
    if (hideConf && w.isConfidential) return false;
    return true;
  });
  renderWitnesses(filtered);
}

function renderWitnesses(list) {
  const tbody = document.getElementById('witnesses-tbody');
  if (!tbody) return;
  tbody.innerHTML = list.length
    ? list.map(w => `
        <tr>
          <td class="mono">${w.witnessNumber}</td>
          <td class="mono">${w.caseNumber}</td>
          <td>${w.fullName}</td>
          <td style="font-size:12px;color:var(--text-dim);">${w.contactInfo || '—'}</td>
          <td style="max-width:200px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;">${w.testimony || '—'}</td>
          <td>${w.isConfidential ? '<span class="status status-urgent">Yes</span>' : '<span class="status status-active">No</span>'}</td>
          <td>${formatDate(w.addedAt)}</td>
          <td><button class="btn btn-danger btn-sm" onclick="deleteWitness(${w.id},'${w.witnessNumber}')">Remove</button></td>
        </tr>`).join('')
    : '<tr><td colspan="8" style="text-align:center;color:var(--text-dim);padding:20px;">No witnesses found.</td></tr>';
}

async function openWitnessModal() {
  await loadWitnessCaseDropdown();
  const modal = document.getElementById('witness-modal');
  if (modal) { modal.style.display = 'flex'; }
  ['wit-name','wit-contact','wit-testimony'].forEach(id => { const el=document.getElementById(id); if(el) el.value=''; });
  const conf = document.getElementById('wit-confidential'); if(conf) conf.checked = false;
}

function closeWitnessModal() {
  const modal = document.getElementById('witness-modal');
  if (modal) modal.style.display = 'none';
}

async function loadWitnessCaseDropdown() {
  const sel = document.getElementById('wit-case-id');
  if (!sel) return;
  try {
    const cases = await casesApi.getAll();
    sel.innerHTML = '<option value="">Select case...</option>' + cases.map(c =>
      `<option value="${c.id}" data-num="${c.caseNumber}">${c.caseNumber} — ${c.crimeType}</option>`
    ).join('');
  } catch {}
}

async function saveWitness() {
  const sel = document.getElementById('wit-case-id');
  const caseId = parseInt(sel?.value);
  const caseNum = sel?.options[sel.selectedIndex]?.dataset?.num || '';
  const name = document.getElementById('wit-name')?.value?.trim();
  if (!caseId || !name) { showToast('Case and witness name are required.', 'error'); return; }
  const user = getUser();
  try {
    await witnessesApi.create({
      caseId, caseNumber: caseNum, fullName: name,
      contactInfo: document.getElementById('wit-contact')?.value?.trim() || '',
      testimony: document.getElementById('wit-testimony')?.value?.trim() || '',
      isConfidential: document.getElementById('wit-confidential')?.checked || false,
      addedByBadge: user?.badgeNumber || ''
    });
    showToast('Witness added successfully.');
    closeWitnessModal();
    loadWitnesses();
  } catch (err) { showToast(err.message, 'error'); }
}

async function deleteWitness(id, num) {
  if (!confirm(`Remove witness ${num}? This action cannot be undone.`)) return;
  try {
    await witnessesApi.delete(id);
    showToast('Witness removed.');
    loadWitnesses();
  } catch (err) { showToast(err.message, 'error'); }
}
