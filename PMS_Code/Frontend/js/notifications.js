// notifications.js — Shared notification bell handler for all dashboards
// Wires up the .topbar-btn (bell icon) to show a dropdown with notifications from API.

(function () {
  let _open = false;
  let _list = [];

  // Initialize after DOM and after api.js is loaded
  document.addEventListener('DOMContentLoaded', () => {
    setTimeout(initNotifications, 100); // small delay so other init runs first
  });

  function initNotifications() {
    const bell = document.querySelector('.topbar-btn');
    if (!bell) return;

    // Make it look clickable
    bell.style.cursor = 'pointer';
    bell.title = 'Notifications';

    // Build dropdown panel (hidden by default)
    const panel = document.createElement('div');
    panel.id = 'notif-panel';
    panel.style.cssText = `
      position:absolute; top:48px; right:0; width:340px; max-height:420px; overflow-y:auto;
      background:var(--navy-2,#0f1e3a); border:1px solid var(--border,rgba(255,255,255,0.1));
      border-radius:8px; box-shadow:0 10px 30px rgba(0,0,0,0.5);
      z-index:1500; display:none; padding:0;
    `;
    panel.innerHTML = `
      <div style="padding:12px 16px;border-bottom:1px solid var(--border-2,rgba(255,255,255,0.06));display:flex;justify-content:space-between;align-items:center;">
        <div style="font-family:'Rajdhani',sans-serif;font-size:14px;font-weight:600;">Notifications</div>
        <button id="notif-mark-all" style="background:none;border:none;color:var(--accent-2,#3b82f6);font-size:11px;cursor:pointer;">Mark all read</button>
      </div>
      <div id="notif-list" style="padding:4px 0;">
        <div style="padding:20px;text-align:center;color:var(--text-dim,#94a3b8);font-size:12px;">Loading...</div>
      </div>
    `;

    // Insert panel as sibling of bell, positioned relative to topbar-right
    const topRight = bell.parentElement;
    if (topRight) {
      topRight.style.position = 'relative';
      topRight.appendChild(panel);
    }

    bell.addEventListener('click', toggleNotifications);
    document.addEventListener('click', (e) => {
      if (!bell.contains(e.target) && !panel.contains(e.target)) {
        panel.style.display = 'none';
        _open = false;
      }
    });

    document.getElementById('notif-mark-all').addEventListener('click', async (e) => {
      e.stopPropagation();
      await markAllRead();
    });

    // Initial load and poll every 30s
    loadNotifications();
    setInterval(loadNotifications, 30000);
  }

  async function loadNotifications() {
    if (typeof notificationsApi === 'undefined') return;
    try {
      const user = getUser?.();
      const params = {};
      if (user?.badgeNumber) params.badge = user.badgeNumber;
      if (user?.role) params.role = user.role;
      const data = await notificationsApi.getMine(params);
      _list = data.notifications || [];
      updateDot(data.unreadCount || 0);
      renderList();
    } catch {
      // Silent — don't disrupt page on API failure
    }
  }

  function updateDot(unreadCount) {
    const dot = document.querySelector('.topbar-btn .notif-dot');
    if (!dot) return;
    if (unreadCount > 0) {
      dot.style.display = 'block';
      dot.setAttribute('data-count', unreadCount);
    } else {
      dot.style.display = 'none';
    }
  }

  function renderList() {
    const listEl = document.getElementById('notif-list');
    if (!listEl) return;
    if (!_list.length) {
      listEl.innerHTML = '<div style="padding:20px;text-align:center;color:var(--text-dim,#94a3b8);font-size:12px;">No notifications.</div>';
      return;
    }
    const typeColors = {
      Info: 'var(--accent-2,#3b82f6)',
      Warning: 'var(--gold,#f59e0b)',
      Alert: 'var(--red,#ef4444)',
      Success: 'var(--green,#22c55e)',
    };
    listEl.innerHTML = _list.map(n => {
      const color = typeColors[n.type] || typeColors.Info;
      const time = new Date(n.createdAt).toLocaleString('en-GB', { day:'2-digit', month:'short', hour:'2-digit', minute:'2-digit' });
      const bg = n.isRead ? 'transparent' : 'rgba(59,130,246,0.04)';
      return `
        <div data-id="${n.id}" class="notif-row" style="padding:10px 16px;border-bottom:1px solid var(--border-2,rgba(255,255,255,0.04));background:${bg};cursor:pointer;">
          <div style="display:flex;gap:10px;align-items:flex-start;">
            <div style="width:8px;height:8px;border-radius:50%;background:${color};margin-top:5px;flex-shrink:0;"></div>
            <div style="flex:1;min-width:0;">
              <div style="font-size:12px;font-weight:${n.isRead ? '400' : '600'};color:var(--text,#e2e8f0);">${escapeHtml(n.title)}</div>
              <div style="font-size:11px;color:var(--text-dim,#94a3b8);margin-top:2px;">${escapeHtml(n.message)}</div>
              <div style="font-size:10px;color:var(--text-muted,#64748b);margin-top:4px;font-family:'IBM Plex Mono',monospace;">${time}</div>
            </div>
          </div>
        </div>`;
    }).join('');

    listEl.querySelectorAll('.notif-row').forEach(row => {
      row.addEventListener('click', async () => {
        const id = parseInt(row.dataset.id);
        const n = _list.find(x => x.id === id);
        if (n && !n.isRead) {
          try {
            await notificationsApi.markRead(id);
            n.isRead = true;
            renderList();
            updateDot(_list.filter(x => !x.isRead).length);
          } catch {}
        }
      });
    });
  }

  async function markAllRead() {
    try {
      const user = getUser?.();
      const params = {};
      if (user?.badgeNumber) params.badge = user.badgeNumber;
      if (user?.role) params.role = user.role;
      await notificationsApi.markAllRead(params);
      _list.forEach(n => n.isRead = true);
      renderList();
      updateDot(0);
    } catch {}
  }

  function toggleNotifications(e) {
    e.stopPropagation();
    const panel = document.getElementById('notif-panel');
    if (!panel) return;
    _open = !_open;
    panel.style.display = _open ? 'block' : 'none';
    if (_open) loadNotifications();
  }

  function escapeHtml(s) {
    if (!s) return '';
    return String(s).replace(/[&<>"']/g, c => ({
      '&':'&amp;', '<':'&lt;', '>':'&gt;', '"':'&quot;', "'":'&#39;'
    }[c]));
  }

  // Expose for other modules
  window.refreshNotifications = loadNotifications;
})();
