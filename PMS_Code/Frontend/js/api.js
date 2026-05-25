// api.js — PMS Frontend API Layer
// All calls go through this file. Handles auth token, base URL, errors.

const API_BASE = 'http://localhost:5000/api';

// ─── Auth helpers ───────────────────────────────────────────────────────────
function getToken() {
  const u = sessionStorage.getItem('pms_user');
  return u ? JSON.parse(u).token : null;
}

function getUser() {
  const u = sessionStorage.getItem('pms_user');
  return u ? JSON.parse(u) : null;
}

function requireAuth() {
  if (!getUser()) {
    window.location.href = 'index.html';
    return false;
  }
  return true;
}

function logout() {
  sessionStorage.removeItem('pms_user');
  window.location.href = 'index.html';
}

// ─── Core fetch wrapper ─────────────────────────────────────────────────────
async function apiFetch(path, options = {}) {
  const token = getToken();
  const headers = {
    'Content-Type': 'application/json',
    ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
    ...(options.headers || {})
  };

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });

  if (res.status === 401) {
    logout();
    throw new Error('Session expired');
  }

  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: 'Request failed' }));
    throw new Error(err.message || `HTTP ${res.status}`);
  }

  if (res.status === 204) return null;
  return res.json();
}

// ─── Stats ───────────────────────────────────────────────────────────────────
const statsApi = {
  getDashboard: () => apiFetch('/stats/dashboard'),
};

// ─── Users ───────────────────────────────────────────────────────────────────
const usersApi = {
  getAll: (params = {}) => apiFetch('/users?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/users/${id}`),
  create: (data) => apiFetch('/users', { method: 'POST', body: JSON.stringify(data) }),
  update: (id, data) => apiFetch(`/users/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id) => apiFetch(`/users/${id}`, { method: 'DELETE' }),
  updateStatus: (id, status) => apiFetch(`/users/${id}/status`, { method: 'PATCH', body: JSON.stringify(status) }),
};

// ─── Cases ───────────────────────────────────────────────────────────────────
const casesApi = {
  getAll: (params = {}) => apiFetch('/cases?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/cases/${id}`),
  create: (data) => apiFetch('/cases', { method: 'POST', body: JSON.stringify(data) }),
  update: (id, data) => apiFetch(`/cases/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id) => apiFetch(`/cases/${id}`, { method: 'DELETE' }),
};

// ─── Evidence ─────────────────────────────────────────────────────────────────
const evidenceApi = {
  getAll: (params = {}) => apiFetch('/evidence?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/evidence/${id}`),
  create: (data) => apiFetch('/evidence', { method: 'POST', body: JSON.stringify(data) }),
  updateStatus: (id, status) => apiFetch(`/evidence/${id}/status`, { method: 'PATCH', body: JSON.stringify(status) }),
  delete: (id) => apiFetch(`/evidence/${id}`, { method: 'DELETE' }),
};

// ─── Suspects ─────────────────────────────────────────────────────────────────
const suspectsApi = {
  getAll: (params = {}) => apiFetch('/suspects?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/suspects/${id}`),
  create: (data) => apiFetch('/suspects', { method: 'POST', body: JSON.stringify(data) }),
  updateStatus: (id, status) => apiFetch(`/suspects/${id}/status`, { method: 'PATCH', body: JSON.stringify(status) }),
  delete: (id) => apiFetch(`/suspects/${id}`, { method: 'DELETE' }),
};

// ─── Arrests ─────────────────────────────────────────────────────────────────
const arrestsApi = {
  getAll: (params = {}) => apiFetch('/arrests?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/arrests/${id}`),
  create: (data) => apiFetch('/arrests', { method: 'POST', body: JSON.stringify(data) }),
  updateStatus: (id, status) => apiFetch(`/arrests/${id}/status`, { method: 'PATCH', body: JSON.stringify(status) }),
  delete: (id) => apiFetch(`/arrests/${id}`, { method: 'DELETE' }),
};

// ─── Fines ───────────────────────────────────────────────────────────────────
const finesApi = {
  getAll: (params = {}) => apiFetch('/fines?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/fines/${id}`),
  create: (data) => apiFetch('/fines', { method: 'POST', body: JSON.stringify(data) }),
  updateStatus: (id, status) => apiFetch(`/fines/${id}/status`, { method: 'PATCH', body: JSON.stringify(status) }),
  getTodayStats: () => apiFetch('/fines/stats/today'),
};

// ─── Forensic Reports ─────────────────────────────────────────────────────────
const forensicApi = {
  getAll: (params = {}) => apiFetch('/forensic-reports?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/forensic-reports/${id}`),
  create: (data) => apiFetch('/forensic-reports', { method: 'POST', body: JSON.stringify(data) }),
  update: (id, data) => apiFetch(`/forensic-reports/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id) => apiFetch(`/forensic-reports/${id}`, { method: 'DELETE' }),
};

// ─── Citizen Reports ─────────────────────────────────────────────────────────
const citizenReportsApi = {
  getAll: (params = {}) => apiFetch('/citizen-reports?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/citizen-reports/${id}`),
  create: (data) => apiFetch('/citizen-reports', { method: 'POST', body: JSON.stringify(data) }),
  updateStatus: (id, status) => apiFetch(`/citizen-reports/${id}/status`, { method: 'PATCH', body: JSON.stringify(status) }),
};

// ─── UI helpers ──────────────────────────────────────────────────────────────

function showToast(msg, type = 'success') {
  const t = document.createElement('div');
  t.style.cssText = `
    position:fixed; bottom:24px; right:24px; z-index:9999;
    padding:14px 20px; border-radius:6px; font-size:13px; max-width:320px;
    font-family:'IBM Plex Sans',sans-serif; box-shadow:0 8px 24px rgba(0,0,0,0.4);
    animation: slideIn .25s ease; color:white;
    background:${type === 'success' ? 'rgba(34,197,94,0.9)' : type === 'error' ? 'rgba(239,68,68,0.9)' : 'rgba(59,130,246,0.9)'};
    border: 1px solid ${type === 'success' ? 'rgba(34,197,94,0.5)' : type === 'error' ? 'rgba(239,68,68,0.5)' : 'rgba(59,130,246,0.5)'};
  `;
  t.textContent = (type === 'success' ? '✓ ' : type === 'error' ? '⚠ ' : 'ℹ ') + msg;
  document.body.appendChild(t);
  setTimeout(() => t.remove(), 3500);
}

function formatDate(d) {
  if (!d) return '—';
  return new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function formatDateTime(d) {
  if (!d) return '—';
  return new Date(d).toLocaleString('en-GB', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
}

function statusBadge(status) {
  const map = {
    'Active': 'status-active', 'Open': 'status-open', 'Urgent': 'status-urgent',
    'Pending': 'status-pending', 'Closed': 'status-closed', 'Processing': 'status-pending',
    'Submitted': 'status-active', 'In Progress': 'status-active', 'Unpaid': 'status-urgent',
    'Paid': 'status-active', 'Detained': 'status-pending', 'At Large': 'status-urgent',
    'Released': 'status-closed', 'Identified': 'status-pending',
  };
  const cls = map[status] || 'status-pending';
  return `<span class="status ${cls}">${status}</span>`;
}

function priorityBadge(p) {
  const map = { 'High': 'priority-high', 'Medium': 'priority-med', 'Low': 'priority-low' };
  return `<span class="priority ${map[p] || 'priority-low'}">${p?.toUpperCase() || 'LOW'}</span>`;
}

// Fill user info in sidebar from session
function initUserInfo() {
  const user = getUser();
  if (!user) return;
  const nameEl = document.querySelector('.user-name');
  const badgeEl = document.querySelector('.user-badge-num');
  if (nameEl) nameEl.textContent = user.fullName || user.FullName || 'User';
  const initials = (user.fullName || user.FullName || 'U').split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase();
  const avatarEl = document.querySelector('.user-avatar');
  if (avatarEl) avatarEl.textContent = initials;

  // Citizen dashboard: replace hero "Welcome, X 👋" with real name
  const heroEl = document.querySelector('.hero-title');
  if (heroEl) {
    const firstName = (user.fullName || user.FullName || 'there').split(' ')[0];
    heroEl.textContent = `Welcome, ${firstName} 👋`;
  }
}

// ─── Departments (FR_09) ──────────────────────────────────────────────────────
const departmentsApi = {
  getAll: (params = {}) => apiFetch('/departments?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/departments/${id}`),
  create: (data) => apiFetch('/departments', { method: 'POST', body: JSON.stringify(data) }),
  update: (id, data) => apiFetch(`/departments/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id) => apiFetch(`/departments/${id}`, { method: 'DELETE' }),
};

// ─── Settings ─────────────────────────────────────────────────────────────────
const settingsApi = {
  getAll: () => apiFetch('/settings'),
  updateAll: (settings) => apiFetch('/settings', { method: 'PUT', body: JSON.stringify({ settings }) }),
};

// ─── Notifications (FR_42 / FR_44) ────────────────────────────────────────────
const notificationsApi = {
  getMine: (params = {}) => apiFetch('/notifications?' + new URLSearchParams(params)),
  markRead: (id) => apiFetch(`/notifications/${id}/read`, { method: 'PATCH' }),
  markAllRead: (params = {}) => apiFetch('/notifications/mark-all-read?' + new URLSearchParams(params), { method: 'POST' }),
  create: (data) => apiFetch('/notifications', { method: 'POST', body: JSON.stringify(data) }),
  delete: (id) => apiFetch(`/notifications/${id}`, { method: 'DELETE' }),
};

// ─── Extended Users API (UC39, UC42) ──────────────────────────────────────────
const usersExtApi = {
  resetPassword: (id, newPassword) => apiFetch(`/users/${id}/reset-password`, {
    method: 'POST', body: JSON.stringify({ newPassword })
  }),
  getActiveCases: (id) => apiFetch(`/users/${id}/active-cases`),
};

// ─── Extended Cases API (UC14, FR_10) ─────────────────────────────────────────
const casesExtApi = {
  assign: (id, data) => apiFetch(`/cases/${id}/assign`, { method: 'PATCH', body: JSON.stringify(data) }),
  requestClose: (id, reason) => apiFetch(`/cases/${id}/request-close`, {
    method: 'POST', body: JSON.stringify({ reason })
  }),
  approveClose: (id) => apiFetch(`/cases/${id}/approve-close`, { method: 'POST' }),
};

// ─── Witnesses ────────────────────────────────────────────────────────────────
const witnessesApi = {
  getAll: (params = {}) => apiFetch('/witnesses?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/witnesses/${id}`),
  create: (data) => apiFetch('/witnesses', { method: 'POST', body: JSON.stringify(data) }),
  update: (id, data) => apiFetch(`/witnesses/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  delete: (id) => apiFetch(`/witnesses/${id}`, { method: 'DELETE' }),
};

// ─── Patrols ─────────────────────────────────────────────────────────────────
const patrolsApi = {
  getAll: (params = {}) => apiFetch('/patrols?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/patrols/${id}`),
  create: (data) => apiFetch('/patrols', { method: 'POST', body: JSON.stringify(data) }),
  updateStatus: (id, status) => apiFetch(`/patrols/${id}/status`, { method: 'PATCH', body: JSON.stringify(status) }),
  delete: (id) => apiFetch(`/patrols/${id}`, { method: 'DELETE' }),
};

// ─── Missing Persons ──────────────────────────────────────────────────────────
const missingPersonsApi = {
  getAll: (params = {}) => apiFetch('/missing-persons?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/missing-persons/${id}`),
  create: (data) => apiFetch('/missing-persons', { method: 'POST', body: JSON.stringify(data) }),
  updateStatus: (id, status) => apiFetch(`/missing-persons/${id}/status`, { method: 'PATCH', body: JSON.stringify(status) }),
};

// ─── Driving Licenses ─────────────────────────────────────────────────────────
const licensesApi = {
  getAll: (params = {}) => apiFetch('/driving-licenses?' + new URLSearchParams(params)),
  getById: (id) => apiFetch(`/driving-licenses/${id}`),
  create: (data) => apiFetch('/driving-licenses', { method: 'POST', body: JSON.stringify(data) }),
  flag: (id, data) => apiFetch(`/driving-licenses/${id}/flag`, { method: 'PATCH', body: JSON.stringify(data) }),
};

// ─── Audit Logs ───────────────────────────────────────────────────────────────
const auditLogsApi = {
  getAll: (params = {}) => apiFetch('/audit-logs?' + new URLSearchParams(params)),
};

// ─── Change Password ──────────────────────────────────────────────────────────
const authExtApi = {
  changePassword: (data) => apiFetch('/auth/change-password', { method: 'POST', body: JSON.stringify(data) }),
};
