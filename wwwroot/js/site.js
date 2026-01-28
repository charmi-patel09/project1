// SYNDASH ADMIN DASHBOARD - JAVASCRIPT
// ============================================

document.addEventListener('DOMContentLoaded', function () {
    // 1. Sidebar Toggle & Generic UI
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.sidebar');
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', () => sidebar.classList.toggle('active'));
        document.addEventListener('click', (event) => {
            if (window.innerWidth <= 768 && !sidebar.contains(event.target) && !sidebarToggle.contains(event.target)) {
                sidebar.classList.remove('active');
            }
        });
    }

    // 2. Active Link Highlighting
    const currentPath = window.location.pathname;
    document.querySelectorAll('.nav-link').forEach(link => {
        const linkPath = link.getAttribute('href');
        if (linkPath && currentPath.includes(linkPath) && linkPath !== '/') link.classList.add('active');
        else if (linkPath === '/' && currentPath === '/') link.classList.add('active');
    });
    // 3. Initialize Bootstrap Tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })
});

