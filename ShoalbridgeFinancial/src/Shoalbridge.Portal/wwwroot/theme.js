// Light and dark mode. The viewer's own choice wins, then the default-dark-mode
// Octopus feature flag, then the operating system setting.
(() => {
  const key = "theme";
  const root = document.documentElement;
  const system = matchMedia("(prefers-color-scheme: dark)");
  let flagDefault = null;

  function saved() {
    try { return localStorage.getItem(key); } catch { return null; }
  }

  function apply(theme) {
    root.dataset.theme = theme;
    const button = document.getElementById("theme-toggle");
    if (button) button.setAttribute("aria-pressed", String(theme === "dark"));
  }

  const preferred = () => saved() || flagDefault || (system.matches ? "dark" : "light");
  apply(preferred());

  system.addEventListener("change", () => apply(preferred()));

  fetch("/api/features")
    .then(res => res.ok ? res.json() : null)
    .then(flags => {
      if (flags?.defaultDarkMode) { flagDefault = "dark"; apply(preferred()); }
    })
    .catch(() => {});

  document.addEventListener("DOMContentLoaded", () => {
    const button = document.getElementById("theme-toggle");
    if (!button) return;
    apply(root.dataset.theme);
    button.addEventListener("click", () => {
      const next = root.dataset.theme === "dark" ? "light" : "dark";
      try { localStorage.setItem(key, next); } catch { }
      apply(next);
    });
  });
})();
