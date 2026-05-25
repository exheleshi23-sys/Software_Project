// citizen.js — Citizen portal — fully functional

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
    const [fines, reports] = await Promise.all([
      finesApi.getAll({ citizenUserId: user?.id }),
      citizenReportsApi.getAll({ citizenUserId: user?.id })
    ]);
    const el = id => document.getElementById(id);
    const unpaid = fines.filter(f => f.status === 'Unpaid');
    if (el('stat-reports'))      el('stat-reports').textContent      = reports.length;
    if (el('stat-fines-count'))  el('stat-fines-count').textContent  = fines.length;
    if (el('stat-unpaid-count')) el('stat-unpaid-count').textContent = unpaid.length;
    if (el('stat-resolved'))     el('stat-resolved').textContent     = reports.filter(r => r.status === 'Resolved').length;

    // Show alert for unpaid fines
    const alert = document.getElementById('fine-alert');
    if (alert) {
      alert.style.display = unpaid.length > 0 ? 'flex' : 'none';
      const msg = alert.querySelector('#fine-alert-msg');
      if (msg) msg.textContent = 'You have ' + unpaid.length + ' unpaid fine' + (unpaid.length > 1 ? 's' : '') + '. Click to pay.';
    }

    // Recent reports on dashboard
    const list = document.getElementById('dashboard-reports-list');
    if (list) {
      list.innerHTML = reports.slice(0,3).map(r => `
        <div style="padding:12px 0;border-bottom:1px solid var(--border-2);">
          <div style="display:flex;justify-content:space-between;">
            <div>
              <div class="mono" style="font-size:11px;color:var(--text-dim);">${r.reportNumber}</div>
              <div style="font-size:13px;font-weight:500;margin:3px 0;">${r.incidentType}</div>
              <div style="font-size:12px;color:var(--text-dim);">${formatDate(r.submittedAt)} · ${r.location}</div>
            </div>
            ${statusBadge(r.status)}
          </div>
        </div>`).join('') || '<div style="color:var(--text-dim);font-size:13px;padding:12px 0;">No reports submitted yet.</div>';
    }
  } catch { /* keep static */ }
}

// ─── MY REPORTS ───────────────────────────────────────────────────────────────
async function loadMyReports() {
  const user  = getUser();
  const tbody = document.getElementById('my-reports-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const reports = await citizenReportsApi.getAll({ citizenUserId: user?.id });
    tbody.innerHTML = reports.length
      ? reports.map(r => `
          <tr>
            <td class="mono">${r.reportNumber}</td>
            <td>${r.incidentType}</td>
            <td>${r.description?.substring(0,45)||'—'}...</td>
            <td>${r.location}</td>
            <td>${formatDate(r.submittedAt)}</td>
            <td>${statusBadge(r.status)}</td>
            <td>${r.officialNotes ? `<span style="font-size:12px;color:var(--text-dim);">${r.officialNotes.substring(0,40)}</span>` : '—'}</td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No reports submitted yet.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

// ─── MY FINES ─────────────────────────────────────────────────────────────────
async function loadMyFines() {
  const user  = getUser();
  const tbody = document.getElementById('my-fines-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const fines = await finesApi.getAll({ citizenUserId: user?.id });
    tbody.innerHTML = fines.length
      ? fines.map(f => `
          <tr>
            <td class="mono">${f.fineNumber}</td>
            <td class="mono" style="letter-spacing:2px;">${f.licensePlate}</td>
            <td>${f.violationType}</td>
            <td style="color:${f.status==='Paid'?'var(--green)':'var(--red)'};">€${f.amount}</td>
            <td>${formatDate(f.issuedAt)}</td>
            <td>${statusBadge(f.status)}</td>
            <td>${f.status==='Unpaid'
              ? `<button class="btn btn-primary btn-sm" onclick="payFine(${f.id})">Pay Now</button>`
              : '—'}</td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No fines on your record.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

async function payFine(id) {
  try {
    await finesApi.updateStatus(id, 'Paid');
    showToast('Fine paid successfully!');
    await loadMyFines();
    await loadDashboardStats();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── SUBMIT INCIDENT REPORT ───────────────────────────────────────────────────
async function submitIncidentReport() {
  const user = getUser();
  const dateVal = document.getElementById('cit-date')?.value;
  const data = {
    citizenUserId: user?.id || 0,
    incidentType:  document.getElementById('cit-incident-type')?.value || '',
    description:   document.getElementById('cit-desc')?.value.trim() || '',
    location:      document.getElementById('cit-location')?.value.trim() || '',
    incidentDate:  dateVal ? new Date(dateVal).toISOString() : new Date().toISOString()
  };
  if (!data.description || !data.location || !data.incidentType) {
    showToast('Please fill in all required fields.', 'error'); return;
  }
  try {
    const r = await citizenReportsApi.create(data);
    showToast('Report ' + r.reportNumber + ' submitted! We will review it shortly.');
    ['cit-desc','cit-location','cit-date'].forEach(id => { const el=document.getElementById(id); if(el) el.value=''; });
    showPage('my-reports', null);
    await loadMyReports();
    await loadDashboardStats();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── SUBMIT COMPLAINT ─────────────────────────────────────────────────────────
async function submitComplaint() {
  const user = getUser();
  const data = {
    citizenUserId: user?.id || 0,
    incidentType:  'Complaint — ' + (document.getElementById('comp-type')?.value || 'General'),
    description:   document.getElementById('comp-desc')?.value.trim() || '',
    location:      document.getElementById('comp-location')?.value.trim() || 'Police Department',
    incidentDate:  new Date().toISOString()
  };
  if (!data.description) { showToast('Please describe your complaint.', 'error'); return; }
  try {
    const r = await citizenReportsApi.create(data);
    showToast('Complaint ' + r.reportNumber + ' submitted. Reference: ' + r.reportNumber);
    document.getElementById('comp-desc').value = '';
    await loadDashboardStats();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── PAGE NAVIGATION ─────────────────────────────────────────────────────────
function showPage(id, el) {
  document.querySelectorAll('[id^="page-"]').forEach(p => p.style.display = 'none');
  const page = document.getElementById('page-' + id);
  if (page) page.style.display = 'block';
  if (el) { document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active')); el.classList.add('active'); }
  const t = { home:'Citizen Portal', report:'Report Incident', complaint:'Submit Complaint', 'my-reports':'My Reports', fines:'My Fines', notifications:'Notifications' };
  const titleEl = document.querySelector('.topbar-title');
  if (titleEl) titleEl.textContent = t[id] || '';
  if (id === 'home')          loadDashboardStats();
  if (id === 'my-reports')    loadMyReports();
  if (id === 'fines')         loadMyFines();
  if (id === 'notifications') loadCitizenNotifications();
}

// Notifications page: combines fines + reports updates + system notifications
async function loadCitizenNotifications() {
  const container = document.getElementById('citizen-notifications-list');
  if (!container) return;
  container.innerHTML = '<div style="color:var(--text-dim);padding:20px;text-align:center;font-size:12px;">Loading...</div>';
  const user = getUser();
  try {
    const [fines, reports, notifData] = await Promise.all([
      finesApi.getAll({ citizenUserId: user?.id }).catch(()=>[]),
      citizenReportsApi.getAll({ citizenUserId: user?.id }).catch(()=>[]),
      notificationsApi.getMine({ badge: user?.badgeNumber, role: 'Citizen' }).catch(()=>({notifications:[]}))
    ]);

    const items = [];

    // Unpaid fines as alert notifications
    fines.filter(f => f.status === 'Unpaid').forEach(f => {
      items.push({
        ts: new Date(f.issuedAt),
        color: 'var(--red)',
        bg: 'rgba(239,68,68,0.04)',
        title: 'Fine Notice',
        text: `Traffic fine ${f.fineNumber} (€${f.amount}) has been issued${f.licensePlate ? ' for vehicle ' + f.licensePlate : ''}. Status: Unpaid.`,
        read: false
      });
    });

    // Paid fines
    fines.filter(f => f.status === 'Paid').forEach(f => {
      items.push({
        ts: new Date(f.issuedAt),
        color: 'var(--green)',
        bg: 'transparent',
        title: 'Fine Paid',
        text: `Fine ${f.fineNumber} (€${f.amount}) has been paid. Thank you.`,
        read: true
      });
    });

    // Citizen reports — status updates
    reports.forEach(r => {
      const colorByStatus = { 'Pending':'var(--gold)', 'In Progress':'var(--accent)', 'Resolved':'var(--green)', 'Closed':'var(--text-dim)' };
      items.push({
        ts: new Date(r.submittedAt),
        color: colorByStatus[r.status] || 'var(--accent)',
        bg: 'transparent',
        title: r.status === 'Resolved' || r.status === 'Closed' ? 'Case Closed' : 'Report Update',
        text: `Your report ${r.reportNumber} (${r.incidentType}) is currently: ${r.status}.`,
        read: r.status === 'Resolved' || r.status === 'Closed'
      });
    });

    // System notifications from API
    (notifData.notifications || []).forEach(n => {
      const colorByType = { 'Info':'var(--accent-2)', 'Warning':'var(--gold)', 'Alert':'var(--red)', 'Success':'var(--green)' };
      items.push({
        ts: new Date(n.createdAt),
        color: colorByType[n.type] || 'var(--accent-2)',
        bg: n.isRead ? 'transparent' : 'rgba(59,130,246,0.04)',
        title: n.title,
        text: n.message,
        read: n.isRead
      });
    });

    // Sort newest first
    items.sort((a,b) => b.ts - a.ts);

    if (!items.length) {
      container.innerHTML = '<div style="color:var(--text-dim);padding:30px;text-align:center;font-size:13px;">You have no notifications yet.</div>';
      return;
    }

    container.innerHTML = items.map(n => `
      <div class="activity-item" style="background:${n.bg};">
        <div class="activity-dot" style="background:${n.color}"></div>
        <div class="activity-content">
          <div class="activity-text"><strong>${n.title}:</strong> ${n.text}</div>
          <div class="activity-time">${formatDateTime(n.ts)} · ${n.read ? 'Read' : 'Unread'}</div>
        </div>
      </div>`).join('');
  } catch {
    container.innerHTML = '<div style="color:var(--text-dim);padding:20px;text-align:center;font-size:12px;">Unable to load notifications.</div>';
  }
}

async function markAllCitizenNotificationsRead() {
  const user = getUser();
  try {
    await notificationsApi.markAllRead({ badge: user?.badgeNumber, role: 'Citizen' });
    showToast('All notifications marked as read.');
    loadCitizenNotifications();
    if (typeof refreshNotifications === 'function') refreshNotifications();
  } catch (err) {
    showToast(err.message, 'error');
  }
}

function startClock() {
  const update = () => { const el=document.getElementById('clock'); if(el) el.textContent=new Date().toLocaleTimeString('en-GB',{hour:'2-digit',minute:'2-digit',second:'2-digit'}); };
  setInterval(update, 1000); update();
}

// ─── MISSING PERSONS (FR_39 / UC32) ──────────────────────────────────────────
async function loadMissingReports() {
  const tbody = document.getElementById('missing-reports-tbody');
  if (!tbody) return;
  const user = getUser();
  try {
    const reports = await missingPersonsApi.getAll({ citizenUserId: user?.id });
    tbody.innerHTML = reports.length
      ? reports.map(r => {
          const statusClass = r.status === 'Active' ? 'status-urgent' : r.status === 'Located' ? 'status-active' : 'status-closed';
          return `<tr>
            <td class="mono">${r.reportNumber}</td>
            <td>${r.missingPersonName}</td>
            <td>${formatDate(r.lastSeenDate)}</td>
            <td>${r.lastKnownLocation}</td>
            <td><span class="status ${statusClass}">${r.status}</span></td>
            <td>${formatDate(r.submittedAt)}</td>
          </tr>`;
        }).join('')
      : '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">You have not filed any missing person reports.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

async function submitMissingReport() {
  const name     = document.getElementById('mp-name')?.value?.trim();
  const desc     = document.getElementById('mp-desc')?.value?.trim();
  const location = document.getElementById('mp-location')?.value?.trim();
  const date     = document.getElementById('mp-date')?.value;
  const contact  = document.getElementById('mp-contact')?.value?.trim();
  if (!name || !desc || !location || !date || !contact) {
    showToast('Please fill in all required fields.', 'error'); return;
  }
  const user = getUser();
  const dob = document.getElementById('mp-dob')?.value;
  try {
    const r = await missingPersonsApi.create({
      citizenUserId: user?.id || 0,
      missingPersonName: name,
      dateOfBirth: dob || null,
      physicalDescription: desc,
      lastKnownLocation: location,
      lastSeenDate: new Date(date).toISOString(),
      contactInfo: contact,
      additionalNotes: document.getElementById('mp-notes')?.value?.trim() || ''
    });
    showToast(`Missing person report filed. Reference: ${r.reportNumber}`);
    ['mp-name','mp-dob','mp-desc','mp-location','mp-date','mp-contact','mp-notes'].forEach(id => {
      const el = document.getElementById(id); if(el) el.value = '';
    });
    loadMissingReports();
  } catch (err) { showToast(err.message, 'error'); }
}
