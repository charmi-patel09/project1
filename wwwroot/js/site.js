// SYNDASH ADMIN DASHBOARD - JAVASCRIPT
// ============================================

document.addEventListener('DOMContentLoaded', function () {
    // 1. Sidebar Toggle & Generic UI
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.querySelector('.sidebar');
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', () => {
            sidebar.classList.toggle('active');
            document.body.classList.toggle('sidebar-open');
        });

        // Close on overlay click
        document.addEventListener('click', (event) => {
            if (window.innerWidth <= 768 &&
                !sidebar.contains(event.target) &&
                !sidebarToggle.contains(event.target) &&
                document.body.classList.contains('sidebar-open')) {

                sidebar.classList.remove('active');
                document.body.classList.remove('sidebar-open');
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

    // 4. Auto-Fit Sidebar Text Logic (New)
    fitSidebarText();
    // Re-run on resize
    window.addEventListener('resize', fitSidebarText);
});


/* Topbar Utility Mobile Toggle */
document.addEventListener('DOMContentLoaded', function () {
    const utilToggle = document.getElementById('mobileUtilToggle');
    const utilContainer = document.getElementById('utilityContainer');

    if (utilToggle && utilContainer) {
        utilToggle.addEventListener('click', (e) => {
            e.stopPropagation();
            utilContainer.classList.toggle('show');
        });

        document.addEventListener('click', (e) => {
            if (!utilContainer.contains(e.target) && !utilToggle.contains(e.target)) {
                utilContainer.classList.remove('show');
            }
        });
    }
});

/* Helper Function: Fit Text to Container width */
function fitSidebarText() {
    const el = document.querySelector('.sidebar-user .user-info h4');
    if (!el) return;

    // Reset to base size first to recalculate correctly
    let fontSize = 0.95; // rem
    el.style.fontSize = fontSize + 'rem';

    // Convert current width/scrollWidth logic
    // We loop down until scrollWidth <= offsetWidth
    // Or safety break at 10px (approx 0.6rem)

    // Get numeric pixel width
    const getOverflow = () => el.scrollWidth > el.offsetWidth;

    let safety = 20; // max iterations
    while (getOverflow() && safety > 0) {
        fontSize -= 0.05; // decrement 0.05rem steps
        if (fontSize < 0.6) break; // Minimum readable size limit
        el.style.fontSize = fontSize + 'rem';
        safety--;
    }
}
