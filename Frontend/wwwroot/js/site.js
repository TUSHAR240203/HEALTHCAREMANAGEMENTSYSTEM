(function () {
  const root = document.documentElement;

  // Teal is the only supported theme. Clear any older saved powder/vampire/dark value
  // so existing browsers always return to the normal teal UI.
  root.setAttribute('data-theme', 'default');
  try { localStorage.setItem('hms-theme', 'default'); } catch (_) { }

  document.querySelectorAll('[data-sidebar-toggle]').forEach(button => {
    button.addEventListener('click', () => document.body.classList.toggle('sidebar-open'));
  });

  document.addEventListener('click', (event) => {
    if (!document.body.classList.contains('sidebar-open')) return;
    const sidebar = document.querySelector('.app-sidebar');
    const toggle = event.target.closest('[data-sidebar-toggle]');
    if (sidebar && !sidebar.contains(event.target) && !toggle) document.body.classList.remove('sidebar-open');
  });

  const reactiveNodes = document.querySelectorAll('.theme-stage .teal-blob, .theme-stage .teal-light');
  if (reactiveNodes.length) {
    window.addEventListener('mousemove', (event) => {
      const x = (event.clientX / window.innerWidth) - 0.5;
      const y = (event.clientY / window.innerHeight) - 0.5;
      reactiveNodes.forEach((node, index) => {
        const depth = (index % 4 + 1) * 5;
        node.style.transform = `translate3d(${x * depth}px, ${y * depth}px, 0)`;
      });
    }, { passive: true });
  }
})();

// Photo URL preview (paste a URL)
(function () {
  const input = document.querySelector('[data-photo-url-input]');
  if (!input) return;
  const preview = document.getElementById('photoPreview');
  const fallback = document.getElementById('photoFallback');
  const update = () => {
    const url = input.value.trim();
    if (!preview || !fallback) return;
    if (!url) {
      preview.classList.add('d-none');
      fallback.classList.remove('d-none');
      return;
    }
    preview.src = url;
    preview.onload = () => {
      preview.classList.remove('d-none');
      fallback.classList.add('d-none');
    };
    preview.onerror = () => {
      preview.classList.add('d-none');
      fallback.classList.remove('d-none');
    };
  };
  input.addEventListener('input', update);
  update();
})();

// Photo gallery file picker preview
(function () {
  const fileInput = document.getElementById('photoFileInput');
  if (!fileInput) return;
  const preview = document.getElementById('photoPreview');
  const fallback = document.getElementById('photoFallback');
  const urlInput = document.querySelector('[data-photo-url-input]');
  fileInput.addEventListener('change', () => {
    const file = fileInput.files && fileInput.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (e) => {
      if (!preview || !fallback) return;
      preview.src = e.target.result;
      preview.classList.remove('d-none');
      fallback.classList.add('d-none');
      // Clear URL input so the uploaded file takes priority
      if (urlInput) urlInput.value = '';
    };
    reader.readAsDataURL(file);
  });
})();

// Password visibility toggle
(function () {
  document.querySelectorAll('[data-password-toggle]').forEach(btn => {
    btn.addEventListener('click', () => {
      const targetId = btn.getAttribute('data-password-toggle');
      const input = document.getElementById(targetId);
      if (!input) return;
      const isText = input.type === 'text';
      input.type = isText ? 'password' : 'text';
      const icon = btn.querySelector('i');
      if (icon) icon.className = isText ? 'bi bi-eye' : 'bi bi-eye-slash';
    });
  });
})();

// Role-aware notifications using existing MVC/API services.
(function () {
  const center = document.querySelector('[data-notifications]');
  if (!center) return;

  const toggle = center.querySelector('[data-notification-toggle]');
  const menu = center.querySelector('[data-notification-menu]');
  const countNode = center.querySelector('[data-notification-count]');
  const listNode = center.querySelector('[data-notification-list]');
  const clearButton = center.querySelector('[data-notification-clear]');
  const subtitleNode = center.querySelector('[data-notification-subtitle]');
  const storageKey = 'hms-notification-read-ids';
  let latestItems = [];

  const getReadIds = () => {
    try { return new Set(JSON.parse(localStorage.getItem(storageKey) || '[]')); }
    catch { return new Set(); }
  };

  const setReadIds = (ids) => {
    localStorage.setItem(storageKey, JSON.stringify(Array.from(ids).slice(-250)));
  };

  const escapeHtml = (value) => String(value ?? '').replace(/[&<>'"]/g, (ch) => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;'
  }[ch]));

  const relativeTime = (value) => {
    if (!value) return 'Date unavailable';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return 'Date unavailable';

    const diffSeconds = Math.floor((Date.now() - date.getTime()) / 1000);
    const absoluteDate = date.toLocaleString([], {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });

    // Future dates, old leave reviews, and older appointment records should not look like fresh alerts.
    if (diffSeconds < 0) return absoluteDate;
    if (diffSeconds < 60) return 'Just now';

    const minutes = Math.floor(diffSeconds / 60);
    if (minutes < 60) return `${minutes} min ago`;

    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} hr ago`;

    return absoluteDate;
  };

  const ensureToastStack = () => {
    let stack = document.querySelector('.notification-toast-stack');
    if (!stack) {
      stack = document.createElement('div');
      stack.className = 'notification-toast-stack';
      document.body.appendChild(stack);
    }
    return stack;
  };

  const showToast = (item) => {
    const stack = ensureToastStack();
    const toast = document.createElement('a');
    toast.className = 'notification-toast';
    toast.href = item.url || item.Url || '#';
    toast.innerHTML = `
      <span class="notification-icon"><i class="bi ${escapeHtml(item.icon || item.Icon || 'bi-bell')}"></i></span>
      <span>
        <strong>${escapeHtml(item.title || item.Title)}</strong>
        <p>${escapeHtml(item.message || item.Message)}</p>
      </span>`;
    stack.appendChild(toast);
    setTimeout(() => toast.remove(), 5200);
  };

  const render = (items) => {
    const readIds = getReadIds();
    const unread = items.filter(item => !readIds.has(item.id || item.Id));

    if (countNode) {
      countNode.textContent = unread.length > 99 ? '99+' : String(unread.length);
      countNode.classList.toggle('d-none', unread.length === 0);
    }

    if (subtitleNode) {
      subtitleNode.textContent = unread.length ? `${unread.length} unread update${unread.length === 1 ? '' : 's'}` : 'All caught up';
    }

    if (!listNode) return;

    if (!items.length) {
      listNode.innerHTML = '<div class="notification-empty"><i class="bi bi-bell-slash"></i><span>No updates yet</span></div>';
      return;
    }

    listNode.innerHTML = items.map(item => `
      <a class="notification-item" href="${escapeHtml(item.url || item.Url || '#')}" data-notification-id="${escapeHtml(item.id || item.Id)}">
        <span class="notification-icon"><i class="bi ${escapeHtml(item.icon || item.Icon || 'bi-bell')}"></i></span>
        <span class="notification-body">
          <strong>${escapeHtml(item.title || item.Title)}</strong>
          <p>${escapeHtml(item.message || item.Message)}</p>
          <small>${escapeHtml(item.type || item.Type || 'Update')} • ${relativeTime(item.createdAtUtc || item.CreatedAtUtc)}</small>
        </span>
      </a>`).join('');
  };

  const fetchNotifications = async (announceNew = false) => {
    try {
      const response = await fetch('/Notifications/Summary', {
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        cache: 'no-store'
      });
      if (!response.ok) return;

      const data = await response.json();
      const items = Array.isArray(data.items) ? data.items : [];
      const readIds = getReadIds();

      if (announceNew) {
        items
          .filter(item => !readIds.has(item.id || item.Id) && !latestItems.some(old => (old.id || old.Id) === (item.id || item.Id)))
          .slice(0, 3)
          .forEach(showToast);
      }

      latestItems = items;
      render(items);
    } catch {
      // Polling should be silent if the network/API is temporarily unavailable.
    }
  };

  toggle?.addEventListener('click', () => {
    center.classList.toggle('open');
    if (center.classList.contains('open')) fetchNotifications(false);
  });

  document.addEventListener('click', (event) => {
    if (!menu || !center.classList.contains('open')) return;
    if (!center.contains(event.target)) center.classList.remove('open');
  });

  listNode?.addEventListener('click', (event) => {
    const item = event.target.closest('[data-notification-id]');
    if (!item) return;
    const readIds = getReadIds();
    readIds.add(item.getAttribute('data-notification-id'));
    setReadIds(readIds);
    render(latestItems);
  });

  clearButton?.addEventListener('click', () => {
    const readIds = getReadIds();
    latestItems.forEach(item => readIds.add(item.id || item.Id));
    setReadIds(readIds);
    render(latestItems);
  });

  fetchNotifications(false);
  setInterval(() => fetchNotifications(true), 30000);
})();
