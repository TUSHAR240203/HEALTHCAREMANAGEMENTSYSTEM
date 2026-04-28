(function () {
  const root = document.documentElement;
  const storedTheme = localStorage.getItem('hms-theme');
  if (storedTheme) root.setAttribute('data-theme', storedTheme);

  document.querySelectorAll('[data-theme-toggle]').forEach(button => {
    button.addEventListener('click', () => {
      const next = root.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
      root.setAttribute('data-theme', next);
      localStorage.setItem('hms-theme', next);
      const icon = button.querySelector('i');
      if (icon) icon.className = next === 'dark' ? 'bi bi-sun' : 'bi bi-moon-stars';
    });
  });

  document.querySelectorAll('[data-sidebar-toggle]').forEach(button => {
    button.addEventListener('click', () => document.body.classList.toggle('sidebar-open'));
  });

  document.addEventListener('click', (event) => {
    if (!document.body.classList.contains('sidebar-open')) return;
    const sidebar = document.querySelector('.app-sidebar');
    const toggle = event.target.closest('[data-sidebar-toggle]');
    if (sidebar && !sidebar.contains(event.target) && !toggle) document.body.classList.remove('sidebar-open');
  });
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
