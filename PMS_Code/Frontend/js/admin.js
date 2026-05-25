// admin.js — Administrator dashboard — fully functional

document.addEventListener('DOMContentLoaded', async () => {
  if (!requireAuth()) return;
  const user = getUser();
  if (user?.role !== 'Admin') { window.location.href = 'index.html'; return; }
  initUserInfo();
  await loadDashboard();
  startClock();
});

// ─── DASHBOARD ────────────────────────────────────────────────────────────────
async function loadDashboard() {
  try {
    const [stats, recentCases, allUsers] = await Promise.all([
      statsApi.getDashboard(), casesApi.getAll(), usersApi.getAll()
    ]);
    const el = id => document.getElementById(id);

    // Stats
    const totalUsers  = stats.totalUsers  ?? 0;
    const activeCases = stats.activeCases ?? 0;
    const closedCases = stats.closedCases ?? 0;
    const totalCases  = activeCases + closedCases;
    const resolveRate = totalCases ? Math.round((closedCases / totalCases) * 100) : 0;
    const totalStaff  = allUsers.filter(u => u.status === 'Active').length;

    if (el('stat-total-users'))    el('stat-total-users').textContent    = totalUsers;
    if (el('stat-active-cases'))   el('stat-active-cases').textContent   = activeCases;
    if (el('stat-closed-cases'))   el('stat-closed-cases').textContent   = closedCases;
    if (el('stat-total-staff'))    el('stat-total-staff').textContent    = totalStaff;
    if (el('stat-resolve-rate'))   el('stat-resolve-rate').textContent   = resolveRate + '% resolve rate';
    if (el('stat-users-change'))   el('stat-users-change').textContent   = totalUsers + ' total active';
    if (el('stat-cases-change'))   el('stat-cases-change').textContent   = activeCases + ' open right now';
    if (el('stat-staff-change'))   el('stat-staff-change').textContent   = totalStaff + ' active members';

    // Recent cases table
    const tbody = el('recent-cases-tbody');
    if (tbody) {
      tbody.innerHTML = recentCases.length
        ? recentCases.slice(0,6).map(c => `
            <tr>
              <td class="mono">${c.caseNumber}</td><td>${c.crimeType}</td>
              <td>${c.reportingOfficerBadge||'—'}</td>
              <td>${statusBadge(c.status)}</td><td>${priorityBadge(c.priority)}</td>
            </tr>`).join('')
        : '<tr><td colspan="5" style="text-align:center;color:var(--text-dim);padding:16px;">No cases yet.</td></tr>';
    }

    // System Activity — recent audit logs
    try {
      const auditData = await auditLogsApi.getAll({ pageSize: 5 });
      const logs = auditData.logs || [];
      const activityEl = document.querySelector('.activity-item')?.parentElement;
      if (activityEl && logs.length) {
        const colors = { LOGIN:'var(--green)', CREATE:'var(--accent)', UPDATE:'var(--gold)', DELETE:'var(--red)', ACCOUNT_LOCKED:'var(--red)', STATUS_CHANGE:'var(--purple)' };
        activityEl.innerHTML = logs.map(l => {
          const color = colors[l.actionType] || 'var(--text-dim)';
          const time  = new Date(l.timestamp).toLocaleTimeString('en-GB',{hour:'2-digit',minute:'2-digit'});
          return `<div class="activity-item"><div class="activity-dot" style="background:${color}"></div><div class="activity-content"><div class="activity-text">${l.description}</div><div class="activity-time">${time} · ${l.userBadge}</div></div></div>`;
        }).join('');
      }
    } catch { /* keep static activity */ }

  } catch (e) { console.error('Dashboard load error:', e); }
}

// ─── USERS ────────────────────────────────────────────────────────────────────
async function loadUsers(filterRole = '') {
  const tbody = document.getElementById('users-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const params = filterRole ? { role: filterRole } : {};
    const users = await usersApi.getAll(params);
    tbody.innerHTML = users.length
      ? users.map(u => `
          <tr>
            <td>${u.fullName}</td>
            <td class="mono">${u.badgeNumber}</td>
            <td>${u.role}</td>
            <td>${u.department}</td>
            <td>${statusBadge(u.status)}</td>
            <td class="text-dim" style="font-size:12px;">${u.lastLogin ? formatDateTime(u.lastLogin) : 'Never'}</td>
            <td style="display:flex;gap:6px;flex-wrap:wrap;">
              <button class="btn btn-secondary btn-sm" onclick='openEditUser(${u.id},"${u.fullName}","${u.email}","${u.role}","${u.department}","${u.status}")'>Edit</button>
              <button class="btn btn-secondary btn-sm" onclick='openResetPasswordModal(${u.id},"${u.fullName}","${u.badgeNumber}")'>Reset Pwd</button>
              ${u.status === 'Active'
                ? `<button class="btn btn-danger btn-sm" onclick='confirmDeactivateUser(${u.id},"${u.fullName}","${u.badgeNumber}")'>Deactivate</button>`
                : u.status === 'Pending'
                  ? `<button class="btn btn-success btn-sm" onclick="updateUserStatus(${u.id},'Active')">Approve</button>
                     <button class="btn btn-danger btn-sm" onclick="deleteUser(${u.id})">Reject</button>`
                  : `<button class="btn btn-success btn-sm" onclick="updateUserStatus(${u.id},'Active')">Reactivate</button>`
              }
            </td>
          </tr>`).join('')
      : '<tr><td colspan="7" style="text-align:center;color:var(--text-dim);padding:20px;">No users found.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="7" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

async function createUser() {
  const data = {
    fullName:    document.getElementById('new-name').value.trim(),
    badgeNumber: document.getElementById('new-badge').value.trim(),
    email:       document.getElementById('new-email').value.trim(),
    role:        document.getElementById('new-role').value,
    department:  document.getElementById('new-dept').value,
    password:    document.getElementById('new-password').value.trim() || 'Change@123',
  };
  if (!data.fullName || !data.badgeNumber || !data.role) { showToast('Name, Badge and Role are required.', 'error'); return; }
  try {
    await usersApi.create(data);
    showToast('User created successfully!');
    document.getElementById('new-user-form').style.display = 'none';
    ['new-name','new-badge','new-email','new-password'].forEach(id => document.getElementById(id).value = '');
    await loadUsers();
  } catch (err) { showToast(err.message, 'error'); }
}

function openEditUser(id, fullName, email, role, department, status) {
  document.getElementById('edit-user-id').value    = id;
  document.getElementById('edit-fullname').value   = fullName;
  document.getElementById('edit-email').value      = email;
  document.getElementById('edit-role').value       = role;
  document.getElementById('edit-department').value = department;
  document.getElementById('edit-status').value     = status;
  document.getElementById('edit-user-modal').style.display = 'flex';
}
function closeEditModal() { document.getElementById('edit-user-modal').style.display = 'none'; }

async function saveEditUser() {
  const id   = document.getElementById('edit-user-id').value;
  const data = {
    fullName:   document.getElementById('edit-fullname').value.trim(),
    email:      document.getElementById('edit-email').value.trim(),
    role:       document.getElementById('edit-role').value,
    department: document.getElementById('edit-department').value,
    status:     document.getElementById('edit-status').value,
  };
  try {
    await usersApi.update(id, data);
    showToast('User updated!');
    closeEditModal();
    await loadUsers();
  } catch (err) { showToast(err.message, 'error'); }
}

async function updateUserStatus(id, status) {
  try {
    await usersApi.updateStatus(id, status);
    showToast(`User ${status === 'Active' ? 'activated' : 'deactivated'}.`);
    await loadUsers();
  } catch (err) { showToast(err.message, 'error'); }
}

async function deleteUser(id) {
  if (!confirm('Delete this user permanently?')) return;
  try {
    await usersApi.delete(id);
    showToast('User deleted.');
    await loadUsers();
  } catch (err) { showToast(err.message, 'error'); }
}

function filterUsers() {
  const role = document.getElementById('filter-role').value;
  loadUsers(role);
}

// ─── CASES ────────────────────────────────────────────────────────────────────
async function loadCases() {
  const tbody = document.getElementById('cases-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const cases = await casesApi.getAll();

    // Update page-level stats
    const el = id => document.getElementById(id);
    if (el('cases-stat-active'))      el('cases-stat-active').textContent      = cases.filter(c => c.status === 'Open' || c.status === 'Active').length;
    if (el('cases-stat-investigation'))el('cases-stat-investigation').textContent = cases.filter(c => c.status === 'Active').length;
    if (el('cases-stat-closed'))      el('cases-stat-closed').textContent      = cases.filter(c => c.status === 'Closed').length;
    if (el('cases-stat-urgent'))      el('cases-stat-urgent').textContent      = cases.filter(c => c.priority === 'Urgent' || c.status === 'Urgent').length;

    tbody.innerHTML = cases.length
      ? cases.map(c => `
          <tr>
            <td class="mono">${c.caseNumber}</td>
            <td>${c.crimeType}</td>
            <td>${formatDate(c.filedDate)}</td>
            <td>${c.location||'—'}</td>
            <td>${c.reportingOfficerBadge||'—'}</td>
            <td>${statusBadge(c.status)}</td>
            <td>${priorityBadge(c.priority)}</td>
            <td>
              <select class="btn btn-secondary btn-sm" style="padding:4px 8px;font-size:11px;" onchange="updateCaseStatus(${c.id},this.value)">
                <option value="">Status...</option>
                <option value="Open">Open</option>
                <option value="Active">Active</option>
                <option value="Urgent">Urgent</option>
                <option value="Pending">Pending</option>
                <option value="Closed">Closed</option>
              </select>
            </td>
          </tr>`).join('')
      : '<tr><td colspan="8" style="text-align:center;color:var(--text-dim);padding:20px;">No cases found.</td></tr>';
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="8" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

async function updateCaseStatus(id, status) {
  if (!status) return;
  try {
    await casesApi.update(id, { status });
    showToast('Case status updated to ' + status + '.');
    await loadCases();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── ASSIGNMENTS ──────────────────────────────────────────────────────────────
async function loadAssignments() {
  const tbody = document.getElementById('assignments-tbody');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';
  try {
    const [users, cases] = await Promise.all([usersApi.getAll(), casesApi.getAll()]);
    const staff = users.filter(u => ['Officer','Detective','Traffic','Forensic'].includes(u.role) && u.status === 'Active');

    tbody.innerHTML = staff.length
      ? staff.map(u => {
          const cnt = cases.filter(c => c.assignedOfficerId === u.id || c.assignedDetectiveId === u.id).length;
          return `
            <tr>
              <td>${u.fullName}</td>
              <td>${u.role}</td>
              <td>${cnt} case${cnt!==1?'s':''}</td>
              <td>${u.department}</td>
              <td>${statusBadge(u.status)}</td>
              <td><button class="btn btn-secondary btn-sm" onclick="openAssignModal(${u.id},'${u.fullName}','${u.role}')">Assign Case</button></td>
            </tr>`;
        }).join('')
      : '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">No staff members found.</td></tr>';

    // fill case dropdown
    const sel = document.getElementById('assign-case-select');
    if (sel) {
      const open = cases.filter(c => c.status !== 'Closed');
      sel.innerHTML = '<option value="">Select case...</option>' +
        open.map(c => `<option value="${c.id}">${c.caseNumber} — ${c.crimeType}</option>`).join('');
    }
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

let _assignUserId = null;
let _assignUserRole = null;

function openAssignModal(userId, name, role) {
  _assignUserId   = userId;
  _assignUserRole = role;
  document.getElementById('assign-user-name').textContent = name + ' (' + role + ')';
  document.getElementById('assign-modal').style.display   = 'flex';
}
function closeAssignModal() { document.getElementById('assign-modal').style.display = 'none'; }

async function saveAssignment() {
  const caseId = parseInt(document.getElementById('assign-case-select').value);
  if (!caseId) { showToast('Please select a case.', 'error'); return; }
  const isDetective = _assignUserRole === 'Detective';
  const updateData  = isDetective ? { assignedDetectiveId: _assignUserId } : { assignedOfficerId: _assignUserId };
  try {
    await casesApi.update(caseId, updateData);
    showToast('Case assigned successfully!');
    closeAssignModal();
    await loadAssignments();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── DEPARTMENTS ──────────────────────────────────────────────────────────────
async function loadDepartments() {
  const grid = document.getElementById('departments-grid');
  if (!grid) return;
  try {
    const users = await usersApi.getAll();
    const depts = [
      { name:'Criminal Investigation', color:'var(--accent-2)', roles:['Detective'] },
      { name:'Patrol Unit',            color:'var(--green)',    roles:['Officer'] },
      { name:'Traffic Division',       color:'var(--red)',      roles:['Traffic'] },
      { name:'Forensics Lab',          color:'var(--purple)',   roles:['Forensic'] },
      { name:'Administration',         color:'var(--gold)',     roles:['Admin'] },
    ];
    grid.innerHTML = depts.map(d => {
      const staff = users.filter(u => d.roles.includes(u.role) && u.status === 'Active');
      return `
        <div class="card">
          <div class="card-header"><div class="card-title">${d.name}</div></div>
          <div class="card-body" style="padding:16px 20px;">
            <div style="font-size:28px;font-family:Rajdhani;font-weight:700;color:${d.color};">${staff.length}</div>
            <div class="text-dim" style="font-size:12px;">Active Staff Members</div>
            <div style="margin-top:12px;"><span class="status status-active">Operational</span></div>
            <div style="margin-top:12px;font-size:12px;color:var(--text-dim);">
              ${staff.slice(0,3).map(u=>`<div style="padding:2px 0;">${u.fullName} <span class="mono" style="font-size:10px;">${u.badgeNumber}</span></div>`).join('')}
              ${staff.length>3?`<div style="color:var(--text-muted);font-size:11px;">+${staff.length-3} more</div>`:''}
            </div>
          </div>
        </div>`;
    }).join('');
  } catch {
    grid.innerHTML = '<p style="color:var(--text-dim);padding:20px;">Could not load departments.</p>';
  }
}

// ─── ANALYTICS ────────────────────────────────────────────────────────────────
async function loadAnalytics() {
  try {
    const [stats, cases, arrests] = await Promise.all([
      statsApi.getDashboard(), casesApi.getAll(), arrestsApi.getAll()
    ]);
    const el = id => document.getElementById(id);
    const total   = cases.length;
    const closed  = cases.filter(c => c.status === 'Closed').length;
    const resolve = total ? Math.round((closed/total)*100) : 0;

    if (el('analytics-resolve'))  el('analytics-resolve').textContent  = resolve + '%';
    if (el('analytics-total'))    el('analytics-total').textContent    = total;
    if (el('analytics-arrests'))  el('analytics-arrests').textContent  = arrests.length;
    if (el('analytics-evidence')) el('analytics-evidence').textContent = stats.totalEvidenceItems ?? 0;

    // Chart by month
    const chartWrap = el('analytics-chart');
    if (chartWrap) {
      const months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
      const counts = Array(12).fill(0);
      cases.forEach(c => { counts[new Date(c.filedDate).getMonth()]++; });
      const max = Math.max(...counts, 1);
      chartWrap.innerHTML = months.map((m,i) => `
        <div class="chart-col">
          <div class="chart-bar" style="height:${Math.max(Math.round((counts[i]/max)*100),3)}%;width:100%;"></div>
          <div class="chart-label">${m}<br><span style="color:var(--accent-2);font-size:9px;">${counts[i]}</span></div>
        </div>`).join('');
    }
  } catch { /* keep static */ }
}

// ─── PAGE NAVIGATION ─────────────────────────────────────────────────────────
function showPage(id) {
  document.querySelectorAll('[id^="page-"]').forEach(p => p.style.display = 'none');
  const page = document.getElementById('page-' + id);
  if (page) page.style.display = 'block';
  document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
  if (event?.currentTarget) event.currentTarget.classList.add('active');
  const titles = {
    dashboard:'Administrator Dashboard', users:'User Management',
    departments:'Departments', assignments:'Assignments',
    cases:'All Cases', audit:'Audit Log',
    analytics:'Analytics & Reports', settings:'Settings'
  };
  const titleEl = document.querySelector('.topbar-title');
  if (titleEl) titleEl.textContent = titles[id] || 'Admin';

  if (id === 'dashboard')   loadDashboard();
  if (id === 'users')       loadUsers();
  if (id === 'cases')       loadCases();
  if (id === 'assignments') loadAssignments();
  if (id === 'departments') loadDepartments();
  if (id === 'analytics')   loadAnalytics();
}

function startClock() {
  const update = () => { const el = document.getElementById('clock'); if(el) el.textContent = new Date().toLocaleTimeString('en-GB',{hour:'2-digit',minute:'2-digit',second:'2-digit'}); };
  setInterval(update, 1000); update();
}



// Export Analytics data as a CSV file that opens in Excel
async function exportAnalyticsReport() {
  try {
    const [cases, users, arrests, evidence, fines] = await Promise.all([
      casesApi.getAll().catch(() => []),
      usersApi.getAll().catch(() => []),
      arrestsApi.getAll().catch(() => []),
      evidenceApi.getAll().catch(() => []),
      finesApi.getAll().catch(() => []),
    ]);

    const totalCases = cases.length;
    const closedCases = cases.filter(c => c.status === 'Closed').length;
    const activeCases = cases.filter(c => c.status !== 'Closed').length;
    const resolveRate = totalCases ? Math.round((closedCases / totalCases) * 100) + '%' : '0%';
    const paidFines = fines.filter(f => f.status === 'Paid');
    const collected = paidFines.reduce((sum, f) => sum + Number(f.amount || f.fineAmount || 0), 0);

    const rows = [
      ['Metric', 'Value'],
      ['Total Users', users.length],
      ['Active Staff', users.filter(u => u.status === 'Active' && u.role !== 'Citizen').length],
      ['Total Cases', totalCases],
      ['Active Cases', activeCases],
      ['Closed Cases', closedCases],
      ['Case Resolve Rate', resolveRate],
      ['Total Arrests', arrests.length],
      ['Evidence Items', evidence.length],
      ['Total Fines', fines.length],
      ['Paid Fines', paidFines.length],
      ['Collected Amount', collected],
      [],
      ['Cases'],
      ['Case Number', 'Crime Type', 'Status', 'Priority', 'Date Filed', 'Officer Badge'],
      ...cases.map(c => [c.caseNumber, c.crimeType, c.status, c.priority, c.filedDate || c.dateFiled || '', c.reportingOfficerBadge || '']),
      [],
      ['Users'],
      ['Badge', 'Full Name', 'Role', 'Department', 'Status'],
      ...users.map(u => [u.badgeNumber, u.fullName, u.role, u.department, u.status]),
    ];

    const csv = rows.map(row => row.map(value => {
      const text = String(value ?? '');
      return '"' + text.replace(/"/g, '""') + '"';
    }).join(',')).join('\n');

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'pms_analytics_report.csv';
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
    showToast('Analytics report exported. Open the CSV file with Excel.');
  } catch (err) {
    showToast(err.message || 'Export failed.', 'error');
  }
}

// ─── AUDIT LOG (FR_45 / UC40) ─────────────────────────────────────────────────
let _auditPage = 1;

async function loadAuditLog(page = 1) {
  _auditPage = page;
  const tbody = document.getElementById('audit-tbody');
  const pagination = document.getElementById('audit-pagination');
  if (!tbody) return;
  tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">Loading...</td></tr>';

  const params = { page, pageSize: 50 };
  const badge  = document.getElementById('al-badge')?.value?.trim();
  const action = document.getElementById('al-action')?.value;
  const entity = document.getElementById('al-entity')?.value;
  const from   = document.getElementById('al-from')?.value;
  const to     = document.getElementById('al-to')?.value;
  if (badge)  params.userBadge  = badge;
  if (action) params.actionType = action;
  if (entity) params.entityType = entity;
  if (from)   params.from       = from;
  if (to)     params.to         = to;

  try {
    const data = await auditLogsApi.getAll(params);
    const logs = data.logs || [];
    const total = data.total || 0;

    if (!logs.length) {
      tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-dim);padding:20px;">No audit records found.</td></tr>';
      if (pagination) pagination.innerHTML = '';
      return;
    }

    tbody.innerHTML = logs.map(l => {
      const isAlert = ['ACCOUNT_LOCKED','LOCKOUT_ATTEMPT'].includes(l.actionType);
      const actionColor = isAlert ? 'color:var(--red);font-weight:500;' : l.actionType === 'LOGIN' ? 'color:var(--green);' : '';
      const ts = new Date(l.timestamp).toLocaleString('en-GB', { day:'2-digit', month:'short', year:'numeric', hour:'2-digit', minute:'2-digit', second:'2-digit' });
      return `<tr>
        <td class="mono" style="font-size:11px;white-space:nowrap;">${ts}</td>
        <td class="mono">${l.userBadge}</td>
        <td><span style="font-size:11px;padding:2px 6px;border-radius:3px;background:rgba(255,255,255,0.05);">${l.userRole}</span></td>
        <td style="${actionColor}">${l.actionType}</td>
        <td style="font-size:12px;color:var(--text-dim);">${l.entityType}${l.entityId && l.entityId !== 'pending' ? ' #' + l.entityId : ''}</td>
        <td style="font-size:12px;max-width:260px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;" title="${l.description}">${l.description}</td>
      </tr>`;
    }).join('');

    // Pagination
    const totalPages = Math.ceil(total / 50);
    if (pagination) {
      pagination.innerHTML = `
        <span>${total} total records · Page ${page} of ${totalPages}</span>
        <div style="display:flex;gap:8px;">
          ${page > 1 ? `<button class="btn btn-secondary btn-sm" onclick="loadAuditLog(${page - 1})">← Prev</button>` : ''}
          ${page < totalPages ? `<button class="btn btn-secondary btn-sm" onclick="loadAuditLog(${page + 1})">Next →</button>` : ''}
        </div>`;
    }
  } catch (err) {
    tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;color:var(--red);padding:20px;">${err.message}</td></tr>`;
  }
}

function clearAuditFilters() {
  ['al-badge','al-from','al-to'].forEach(id => { const el=document.getElementById(id); if(el) el.value=''; });
  ['al-action','al-entity'].forEach(id => { const el=document.getElementById(id); if(el) el.selectedIndex=0; });
  loadAuditLog(1);
}

// ─── SETTINGS (Save Changes fully functional) ─────────────────────────────────
async function loadSettings() {
  try {
    const settings = await settingsApi.getAll();
    const setVal = (id, key) => {
      const el = document.getElementById(id);
      if (el && settings[key]) el.value = settings[key].value;
    };
    setVal('set-system-name',     'system_name');
    setVal('set-department-name', 'department_name');
    setVal('set-timezone',        'timezone');
    setVal('set-session-timeout', 'session_timeout_min');
    setVal('set-max-attempts',    'max_login_attempts');
    setVal('set-password-min',    'password_min_length');
  } catch (err) {
    showToast('Failed to load settings: ' + err.message, 'error');
  }
}

async function saveSettings() {
  const getVal = id => document.getElementById(id)?.value?.trim() || '';
  const payload = {
    system_name:         getVal('set-system-name'),
    department_name:     getVal('set-department-name'),
    timezone:            getVal('set-timezone'),
    session_timeout_min: getVal('set-session-timeout'),
    max_login_attempts:  getVal('set-max-attempts'),
    password_min_length: getVal('set-password-min'),
  };
  // Basic validation
  const timeout = parseInt(payload.session_timeout_min);
  if (isNaN(timeout) || timeout < 5 || timeout > 120) {
    showToast('Session timeout must be between 5 and 120 minutes.', 'error'); return;
  }
  const attempts = parseInt(payload.max_login_attempts);
  if (isNaN(attempts) || attempts < 3 || attempts > 10) {
    showToast('Max login attempts must be between 3 and 10.', 'error'); return;
  }
  const pwLen = parseInt(payload.password_min_length);
  if (isNaN(pwLen) || pwLen < 6 || pwLen > 32) {
    showToast('Password min length must be between 6 and 32.', 'error'); return;
  }
  try {
    await settingsApi.updateAll(payload);
    showToast('Settings saved successfully.');
  } catch (err) {
    showToast(err.message, 'error');
  }
}

// ─── DEPARTMENTS CRUD (FR_09) ─────────────────────────────────────────────────
async function loadDepartments() {
  const grid = document.getElementById('departments-grid');
  if (!grid) return;
  try {
    const depts = await departmentsApi.getAll();
    const colorByCode = {
      'CID':'var(--accent-2)', 'PAT':'var(--green)', 'TRF':'var(--red)',
      'FOR':'var(--purple)',   'ADM':'var(--gold)'
    };
    // Add a "+ New Department" card at the end
    const cardsHtml = depts.map(d => {
      const color = colorByCode[d.code] || 'var(--accent-2)';
      return `
        <div class="card">
          <div class="card-header" style="display:flex;justify-content:space-between;align-items:center;">
            <div class="card-title">${d.name}</div>
            <span class="mono" style="font-size:10px;color:var(--text-dim);">${d.code}</span>
          </div>
          <div class="card-body" style="padding:16px 20px;">
            <div style="font-size:28px;font-family:Rajdhani;font-weight:700;color:${color};">${d.staffCount}</div>
            <div class="text-dim" style="font-size:12px;">Active Staff Members</div>
            <div style="margin-top:8px;font-size:11px;color:var(--text-dim);">${d.description || 'No description'}</div>
            <div style="margin-top:12px;"><span class="status ${d.status === 'Active' ? 'status-active' : 'status-closed'}">${d.status}</span></div>
            <div style="margin-top:14px;display:flex;gap:6px;">
              <button class="btn btn-secondary btn-sm" onclick='openEditDept(${d.id},${JSON.stringify(d.name)},${JSON.stringify(d.code)},${JSON.stringify(d.description||"")},${JSON.stringify(d.headBadge||"")},${JSON.stringify(d.status)})'>Edit</button>
              <button class="btn btn-danger btn-sm" onclick="deleteDept(${d.id})">Delete</button>
            </div>
          </div>
        </div>`;
    }).join('');
    grid.innerHTML = cardsHtml + `
      <div class="card" style="border-style:dashed;cursor:pointer;" onclick="openNewDept()">
        <div class="card-body" style="padding:30px 20px;text-align:center;color:var(--text-dim);">
          <div style="font-size:24px;margin-bottom:8px;">+</div>
          <div style="font-size:13px;">New Department</div>
        </div>
      </div>`;
  } catch (err) {
    grid.innerHTML = `<div style="color:var(--red);padding:20px;">${err.message}</div>`;
  }
}

function openNewDept() {
  document.getElementById('dept-id').value          = '';
  document.getElementById('dept-modal-title').textContent = 'New Department';
  document.getElementById('dept-name').value        = '';
  document.getElementById('dept-code').value        = '';
  document.getElementById('dept-description').value = '';
  document.getElementById('dept-head').value        = '';
  document.getElementById('dept-status-row').style.display = 'none';
  document.getElementById('dept-modal').style.display = 'flex';
}

function openEditDept(id, name, code, desc, head, status) {
  document.getElementById('dept-id').value          = id;
  document.getElementById('dept-modal-title').textContent = 'Edit Department';
  document.getElementById('dept-name').value        = name;
  document.getElementById('dept-code').value        = code;
  document.getElementById('dept-description').value = desc;
  document.getElementById('dept-head').value        = head;
  document.getElementById('dept-status').value      = status;
  document.getElementById('dept-status-row').style.display = 'block';
  document.getElementById('dept-modal').style.display = 'flex';
}

function closeDeptModal() { document.getElementById('dept-modal').style.display = 'none'; }

async function saveDept() {
  const id   = document.getElementById('dept-id').value;
  const data = {
    name:        document.getElementById('dept-name').value.trim(),
    code:        document.getElementById('dept-code').value.trim().toUpperCase(),
    description: document.getElementById('dept-description').value.trim(),
    headBadge:   document.getElementById('dept-head').value.trim(),
    status:      document.getElementById('dept-status').value || 'Active'
  };
  if (!data.name || !data.code) { showToast('Name and Code are required.', 'error'); return; }
  try {
    if (id) await departmentsApi.update(id, data);
    else    await departmentsApi.create(data);
    showToast('Department ' + (id ? 'updated' : 'created') + '.');
    closeDeptModal();
    await loadDepartments();
  } catch (err) { showToast(err.message, 'error'); }
}

async function deleteDept(id) {
  if (!confirm('Delete this department? Staff must be reassigned first.')) return;
  try {
    await departmentsApi.delete(id);
    showToast('Department deleted.');
    await loadDepartments();
  } catch (err) { showToast(err.message, 'error'); }
}

// ─── ADMIN RESET PASSWORD (UC39) ──────────────────────────────────────────────
function openResetPasswordModal(userId, userName, badge) {
  document.getElementById('rp-user-id').value     = userId;
  document.getElementById('rp-user-label').textContent = `${userName} (${badge})`;
  document.getElementById('rp-new').value         = '';
  document.getElementById('rp-confirm').value     = '';
  document.getElementById('rp-error').style.display = 'none';
  document.getElementById('reset-pwd-modal').style.display = 'flex';
}
function closeResetPasswordModal() { document.getElementById('reset-pwd-modal').style.display = 'none'; }

async function submitResetPassword() {
  const id      = document.getElementById('rp-user-id').value;
  const newPwd  = document.getElementById('rp-new').value;
  const confirm = document.getElementById('rp-confirm').value;
  const errEl   = document.getElementById('rp-error');
  errEl.style.display = 'none';
  if (newPwd.length < 8) { errEl.textContent = 'Password must be at least 8 characters.'; errEl.style.display = 'block'; return; }
  if (newPwd !== confirm) { errEl.textContent = 'Passwords do not match.'; errEl.style.display = 'block'; return; }
  try {
    await usersExtApi.resetPassword(id, newPwd);
    showToast('Password reset successfully. The user has been notified.');
    closeResetPasswordModal();
  } catch (err) {
    errEl.textContent = err.message;
    errEl.style.display = 'block';
  }
}

// ─── DEACTIVATION WITH WARNING (UC42) ─────────────────────────────────────────
async function confirmDeactivateUser(id, fullName, badge) {
  try {
    const data = await usersExtApi.getActiveCases(id);
    let msg;
    if (data.count > 0) {
      const list = data.cases.slice(0, 5).map(c => `  • ${c.caseNumber} — ${c.crimeType} (${c.status})`).join('\n');
      const more = data.count > 5 ? `\n  ...and ${data.count - 5} more` : '';
      msg = `⚠ Warning: ${fullName} (${badge}) has ${data.count} active case${data.count > 1 ? 's' : ''}:\n\n${list}${more}\n\nDeactivating will leave these cases without an assignee. Continue?`;
    } else {
      msg = `Deactivate ${fullName} (${badge})?`;
    }
    if (!confirm(msg)) return;
    await usersApi.updateStatus(id, 'Inactive');
    showToast('User deactivated.');
    await loadUsers();
  } catch (err) {
    showToast(err.message, 'error');
  }
}
