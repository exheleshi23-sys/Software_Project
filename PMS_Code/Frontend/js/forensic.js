// forensic.js — Forensic Expert dashboard — fully functional

document.addEventListener('DOMContentLoaded', async () => {
  if (!requireAuth()) return;
  initUserInfo();
  await loadDashboardStats();
  startClock();
});

// ─── DASHBOARD ────────────────────────────────────────────────────────────────
async function loadDashboardStats() {
  try {
    const [evidence, reports] = await Promise.all([
      evidenceApi.getAll(), forensicApi.getAll()
    ]);
    const el = id => document.getElementById(id);
    const queue       = evidence.filter(e => e.status === 'Logged');
    const inProgress  = reports.filter(r => r.status === 'In Progress' || r.status === 'Confirming');
    const submitted   = reports.filter(r => r.status === 'Submitted' || r.status === 'Archived');

    if (el('stat-queue'))       el('stat-queue').textContent       = queue.length;
    if (el('stat-in-progress')) el('stat-in-progress').textContent = inProgress.length;
    if (el('stat-submitted'))   el('stat-submitted').textContent   = submitted.length;
    if (el('stat-custody'))     el('stat-custody').textContent     = evidence.length;
    if (el('stat-total-evd'))   el('stat-total-evd').textContent   = evidence.length;

    renderActiveAnalyses(inProgress);
    renderEvidenceQueuePreview(queue);
    renderRecentReportsPreview(reports.filter(r => r.status === 'Submitted' || r.status === 'Archived'));
  } catch { /* keep static */ }
}

// Active analyses cards (replaces hardcoded HTML)
function renderActiveAnalyses(reports) {
  const container = document.getElementById('active-analyses-list');
  if (!container) return;
  if (!reports.length) {
    container.innerHTML = '<div class="card"><div class="card-body" style="text-align:center;color:var(--text-dim);padding:30px 20px;">No active analyses. Visit Evidence Queue to start a new analysis.</div></div>';
    return;
  }
  // Color map by analysis type
  const typeStyles = {
    'DNA':           { color:'var(--red)',      bg:'rgba(239,68,68,0.1)',  icon:'🧬', label:'DNA ANALYSIS' },
    'Fingerprints':  { color:'var(--accent-2)', bg:'rgba(59,130,246,0.1)', icon:'🔍', label:'FINGERPRINT ANALYSIS' },
    'Ballistics':    { color:'var(--gold)',     bg:'rgba(245,158,11,0.1)', icon:'🔫', label:'BALLISTICS / WEAPONS' },
    'Chemical':      { color:'var(--purple)',   bg:'rgba(167,139,250,0.1)',icon:'⚗️', label:'CHEMICAL ANALYSIS' },
    'Fiber':         { color:'var(--accent)',   bg:'rgba(34,197,94,0.1)',  icon:'🧵', label:'FIBER ANALYSIS' },
    'Digital':       { color:'var(--accent-2)', bg:'rgba(59,130,246,0.1)', icon:'💾', label:'DIGITAL FORENSICS' },
    'Toxicology':    { color:'var(--gold)',     bg:'rgba(245,158,11,0.1)', icon:'☣️', label:'TOXICOLOGY' },
  };
  container.innerHTML = reports.slice(0, 5).map(r => {
    const style = typeStyles[r.analysisType] || typeStyles['DNA'];
    const progress = r.progressPercent || 0;
    const received = r.submittedAt ? formatDate(r.submittedAt) : '—';
    return `
      <div class="analysis-card">
        <div class="analysis-type" style="background:${style.bg};color:${style.color};border:1px solid ${style.color}33;">
          ${style.icon} ${style.label}
        </div>
        <div style="font-size:14px;font-weight:500;">${r.evidenceNumber || 'Evidence'} — ${r.caseNumber}</div>
        <div style="font-size:12px;color:var(--text-dim);margin-top:4px;">Report ${r.reportNumber} · Received ${received}</div>
        <div style="margin-top:12px;font-size:12px;color:var(--text-dim);">Analysis Progress</div>
        <div class="progress-bar"><div class="progress-fill" style="width:${progress}%;background:${style.color};"></div></div>
        <div style="font-size:11px;color:var(--text-dim);margin-top:4px;font-family:'IBM Plex Mono',monospace;">${progress}% · ${r.status}</div>
        <div style="margin-top:12px;display:flex;gap:8px;">
          <button class="btn btn-primary btn-sm" onclick="openUpdateReportModal(${r.id},'${r.reportNumber}',${progress},'${r.status}','${(r.findings||'').replace(/'/g,"\\'")}')">Update Progress</button>
        </div>
      </div>`;
  }).join('');
}

function renderEvidenceQueuePreview(items) {
  const container = document.getElementById('evidence-queue-preview');
  if (!container) return;
  if (!items.length) {
    container.innerHTML = '<div style="color:var(--text-dim);padding:12px 0;font-size:12px;text-align:center;">Queue is empty.</div>';
    return;
  }
  const colors = ['var(--red)', 'var(--gold)', 'var(--accent)'];
  container.innerHTML = items.slice(0, 4).map((e, i) => `
    <div class="activity-item">
      <div class="activity-dot" style="background:${colors[i % colors.length]}"></div>
      <div class="activity-content">
        <div class="activity-text"><strong>${e.evidenceNumber}</strong> — ${(e.description || e.type || '').substring(0,40)}</div>
        <div class="activity-time">${formatDate(e.collectedAt)} · ${e.type || 'Evidence'}</div>
      </div>
    </div>`).join('');
}

function renderRecentReportsPreview(reports) {
  const container = document.getElementById('recent-reports-preview');
  if (!container) return;
  if (!reports.length) {
    container.innerHTML = '<div style="color:var(--text-dim);padding:12px 0;font-size:12px;text-align:center;">No reports yet.</div>';
    return;
  }
  container.innerHTML = reports.slice(0, 3).map(r => `
    <div class="activity-item">
      <div class="activity-dot" style="background:var(--green)"></div>
      <div class="activity-content">
        <div class="activity-text"><strong>${r.reportNumber}</strong> — ${r.analysisType} report</div>
        <div class="activity-time">${r.submittedAt ? 'Submitted ' + formatDate(r.submittedAt) : 'Pending'}</div>
      </div>
    </div>`).join('');
}

// ─── EVIDENCE QUEUE ───────────────────────────────────────────────────────────
async function loadEvidenceQueue() {
  const tbody = document.getElementById('queue-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const items = await evidenceApi.getAll({ status: 'Logged' });
    tbody.innerHTML = items.length
      ? items.map(e => `
          <tr>
            <td class="mono">${e.evidenceNumber}</td>
            <td class="mono">${e.caseNumber}</td>
            <td>${e.type}</td>
            <td>${e.description}</td>
            <td>${formatDate(e.collectedAt)}</td>
            <td>
              <button class="btn btn-primary btn-sm" onclick="openNewReportModal('${e.evidenceNumber}',${e.caseId},'${e.caseNumber}')">Begin Analysis</button>
            </td>
          </tr>`).join('')
      : '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">Queue is empty — no evidence awaiting analysis.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

// ─── ANALYSIS TASKS ───────────────────────────────────────────────────────────
async function loadAnalysisTasks() {
  const user = getUser();
  const tbody = document.getElementById('analysis-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const reports = await forensicApi.getAll({ analystBadge: user?.badgeNumber });
    const active  = reports.filter(r => r.status === 'In Progress' || r.status === 'Confirming');
    tbody.innerHTML = active.length
      ? active.map(r => `
          <tr>
            <td class="mono">${r.reportNumber}</td>
            <td class="mono">${r.evidenceNumber}</td>
            <td class="mono">${r.caseNumber}</td>
            <td>${r.analysisType}</td>
            <td>
              <div style="display:flex;align-items:center;gap:8px;">
                <div style="height:6px;background:rgba(255,255,255,0.06);border-radius:3px;width:80px;overflow:hidden;">
                  <div style="height:100%;background:var(--accent);border-radius:3px;width:${r.progressPercent}%;"></div>
                </div>
                <span style="font-size:11px;font-family:'IBM Plex Mono',monospace;">${r.progressPercent}%</span>
              </div>
            </td>
            <td>${statusBadge(r.status)}</td>
            <td>
              <button class="btn btn-secondary btn-sm" onclick="openUpdateReportModal(${r.id},'${r.reportNumber}',${r.progressPercent},'${r.status}','${r.findings||''}')">Update</button>
            </td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No active analyses.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

// ─── FORENSIC REPORTS ─────────────────────────────────────────────────────────
async function loadForensicReports() {
  const tbody = document.getElementById('reports-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const reports = await forensicApi.getAll();
    tbody.innerHTML = reports.length
      ? reports.map(r => `
          <tr>
            <td class="mono">${r.reportNumber}</td>
            <td class="mono">${r.caseNumber}</td>
            <td>${r.analysisType}</td>
            <td>${r.findings?.substring(0,50)||'—'}${r.findings?.length>50?'...':''}</td>
            <td>${r.submittedAt ? formatDate(r.submittedAt) : '—'}</td>
            <td>${statusBadge(r.status)}</td>
          </tr>`).join('')
      : '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">No reports submitted.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

// ─── CHAIN OF CUSTODY ─────────────────────────────────────────────────────────
async function loadCustody() {
  const tbody = document.getElementById('custody-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const items = await evidenceApi.getAll();
    tbody.innerHTML = items.length
      ? items.map(e => `
          <tr>
            <td class="mono">${e.evidenceNumber}</td>
            <td class="mono">${e.caseNumber}</td>
            <td>${e.collectedBy}</td>
            <td>${formatDate(e.collectedAt)}</td>
            <td>${e.chainOfCustody}</td>
            <td>${e.storageLocation||'—'}</td>
            <td>${statusBadge('Active')}</td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No items in chain of custody.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

// ─── NEW REPORT MODAL ─────────────────────────────────────────────────────────
function openNewReportModal(evidenceNumber = '', caseId = 0, caseNumber = '') {
  if (document.getElementById('rpt-evidence-num')) document.getElementById('rpt-evidence-num').value = evidenceNumber;
  if (document.getElementById('rpt-case-id'))      document.getElementById('rpt-case-id').value      = caseId;
  if (document.getElementById('rpt-case-num'))     document.getElementById('rpt-case-num').value     = caseNumber;
  document.getElementById('new-report-modal').style.display = 'flex';
  loadReportCaseDropdown(caseId);
}
function closeNewReportModal() { document.getElementById('new-report-modal').style.display = 'none'; }

async function loadReportCaseDropdown(selectedId = 0) {
  const sel = document.getElementById('rpt-case-select');
  if (!sel) return;
  try {
    const cases = await casesApi.getAll();
    sel.innerHTML = '<option value="">Select case...</option>' +
      cases.map(c => `<option value="${c.id}" data-num="${c.caseNumber}" ${c.id == selectedId?'selected':''}>${c.caseNumber} — ${c.crimeType}</option>`).join('');
    sel.onchange = () => {
      const opt = sel.options[sel.selectedIndex];
      if (document.getElementById('rpt-case-id'))  document.getElementById('rpt-case-id').value  = opt.value;
      if (document.getElementById('rpt-case-num')) document.getElementById('rpt-case-num').value = opt.dataset.num || '';
    };
  } catch {}
}

async function createForensicReport() {
  const user = getUser();
  const data = {
    caseNumber:   document.getElementById('rpt-case-num')?.value || '',
    caseId:       parseInt(document.getElementById('rpt-case-id')?.value) || 0,
    evidenceNumber:document.getElementById('rpt-evidence-num')?.value || '',
    analysisType: document.getElementById('rpt-type')?.value || '',
    findings:     document.getElementById('rpt-findings')?.value.trim() || '',
    analystBadge: user?.badgeNumber || '',
    dueDate:      document.getElementById('rpt-due')?.value ? new Date(document.getElementById('rpt-due').value).toISOString() : null,
  };
  if (!data.analysisType || !data.findings) { showToast('Analysis type and initial findings are required.', 'error'); return; }
  if (!data.caseId) { showToast('Please select a case.', 'error'); return; }
  try {
    const r = await forensicApi.create(data);
    showToast('Report ' + r.reportNumber + ' created!');
    closeNewReportModal();
    ['rpt-evidence-num','rpt-findings','rpt-due'].forEach(id => { const el=document.getElementById(id); if(el) el.value=''; });
    await loadAnalysisTasks();
    await loadDashboardStats();
    // Mark evidence as Processing
    await evidenceApi.updateStatus(0, 'Processing').catch(()=>{});
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── UPDATE REPORT MODAL ──────────────────────────────────────────────────────
let _updateReportId = null;

function openUpdateReportModal(id, reportNumber, progress, status, findings) {
  _updateReportId = id;
  document.getElementById('upd-report-label').textContent = reportNumber;
  document.getElementById('upd-progress').value           = progress;
  document.getElementById('upd-progress-display').textContent = progress + '%';
  document.getElementById('upd-status').value             = status;
  document.getElementById('upd-findings').value           = findings;
  document.getElementById('update-report-modal').style.display = 'flex';
}
function closeUpdateReportModal() { document.getElementById('update-report-modal').style.display = 'none'; }

async function saveReportUpdate() {
  const data = {
    progressPercent: parseInt(document.getElementById('upd-progress').value),
    findings:        document.getElementById('upd-findings').value.trim(),
    status:          document.getElementById('upd-status').value,
  };
  try {
    await forensicApi.update(_updateReportId, data);
    showToast('Report updated!');
    closeUpdateReportModal();
    await loadAnalysisTasks();
    await loadForensicReports();
    await loadDashboardStats();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── PAGE NAVIGATION ─────────────────────────────────────────────────────────
function showPage(id, el) {
  document.querySelectorAll('[id^="page-"]').forEach(p => p.style.display = 'none');
  const page = document.getElementById('page-' + id);
  if (page) page.style.display = 'block';
  if (el) { document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active')); el.classList.add('active'); }
  const t = { dashboard:'Forensics Lab Dashboard', 'evidence-queue':'Evidence Queue', analysis:'Analysis Tasks', reports:'Forensic Reports', custody:'Chain of Custody' };
  const titleEl = document.querySelector('.topbar-title');
  if (titleEl) titleEl.textContent = t[id] || '';
  if (id === 'dashboard')      loadDashboardStats();
  if (id === 'evidence-queue') loadEvidenceQueue();
  if (id === 'analysis')       loadAnalysisTasks();
  if (id === 'reports')        loadForensicReports();
  if (id === 'custody')        loadCustody();
}

function startClock() {
  const update = () => { const el=document.getElementById('clock'); if(el) el.textContent=new Date().toLocaleTimeString('en-GB',{hour:'2-digit',minute:'2-digit',second:'2-digit'}); };
  setInterval(update, 1000); update();
}
