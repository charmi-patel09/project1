const TimeTracker = (function () {
    const STATE_KEY = 'timeTrackerState';
    const API_URL = '/TimeTracker';
    const NAV_KEY = 'isInternalNav';

    // No hardcoded task durations. Dynamic parsing only.

    let state = {
        startTime: null,
        accumulatedTime: 0,
        isRunning: false,
        taskName: '',
        targetMinutes: null
    };

    let timerInterval = null;
    let els = {};

    function init() {
        // Cache DOM elements
        els = {
            // Navbar
            navbarTimer: document.getElementById('navbarTimer'),
            timerDisplay: document.getElementById('timerDisplay'), // Navbar badge text
            timerIcon: document.getElementById('timerIcon'),
            dropdown: document.getElementById('timerDropdown'),
            taskInput: document.getElementById('timerTaskInput'),
            displayBig: document.getElementById('timerDisplayBig'), // Navbar dropdown big display
            status: document.getElementById('timerStatus'),
            btnStart: document.getElementById('btnTimerStart'),
            btnPause: document.getElementById('btnTimerPause'),
            btnStop: document.getElementById('btnTimerStop'),
            recentList: document.getElementById('recentTasksList'),

            // Time Tracker Page Specific
            pageDisplay: document.getElementById('pageTimerDisplay'),
            pageTask: document.getElementById('pageTaskName'),
            pageContainer: document.getElementById('pageActiveTaskContainer')
        };

        if (!els.navbarTimer) return;

        loadState();
        setupNavigationGuards();
        setupEventListeners();

        // Initial UI Update
        updateDisplay();
        updateControls();
        els.taskInput.value = state.taskName || '';

        // Check internal navigation flag
        localStorage.removeItem(NAV_KEY);

        if (state.isRunning) {
            startInterval();
        }

        fetchRecentTasks();

        // Sync across tabs
        window.addEventListener('storage', (e) => {
            if (e.key === STATE_KEY) {
                loadState();
                updateDisplay();
                updateControls();
                if (state.isRunning) startInterval();
                else if (timerInterval) clearInterval(timerInterval);

                // Update input if changed elsewhere
                if (els.taskInput && state.taskName !== els.taskInput.value) {
                    els.taskInput.value = state.taskName || '';
                }
            }
        });

        window.addEventListener('beforeunload', handleUnload);
    }

    function loadState() {
        const savedState = localStorage.getItem(STATE_KEY);
        if (savedState) {
            state = JSON.parse(savedState);
            if (typeof state.targetMinutes === 'undefined') state.targetMinutes = null;
        }
    }

    function setupEventListeners() {
        els.navbarTimer.addEventListener('click', toggleDropdown);
        els.btnStart.addEventListener('click', startTimer);
        els.btnPause.addEventListener('click', pauseTimer);
        els.btnStop.addEventListener('click', stopTimer);

        els.taskInput.addEventListener('input', (e) => {
            state.taskName = e.target.value;
            saveState();
        });

        document.addEventListener('click', (e) => {
            if (!els.navbarTimer.contains(e.target) && !els.dropdown.contains(e.target)) {
                els.dropdown.style.display = 'none';
            }
        });
    }

    function setupNavigationGuards() {
        document.body.addEventListener('click', (e) => {
            const link = e.target.closest('a');
            if (link && link.href && link.href.startsWith(window.location.origin) && !link.getAttribute('target')) {
                localStorage.setItem(NAV_KEY, 'true');
            }
        });
        document.body.addEventListener('submit', (e) => {
            if (e.target.action && e.target.action.startsWith(window.location.origin) && !e.target.target) {
                localStorage.setItem(NAV_KEY, 'true');
            }
        });
    }

    function handleUnload() {
        const isInternal = localStorage.getItem(NAV_KEY) === 'true';
        if (!isInternal && state.isRunning) {
            state.accumulatedTime += Date.now() - state.startTime;
            state.startTime = null;
            state.isRunning = false;
            saveState();
        }
    }

    function toggleDropdown() {
        els.dropdown.style.display = els.dropdown.style.display === 'none' ? 'block' : 'none';
    }

    function getElapsedTime() {
        if (!state.isRunning) return state.accumulatedTime;
        return state.accumulatedTime + (Date.now() - state.startTime);
    }

    function formatTime(ms) {
        const totalSeconds = Math.floor(ms / 1000);
        const h = Math.floor(totalSeconds / 3600);
        const m = Math.floor((totalSeconds % 3600) / 60);
        const s = totalSeconds % 60;
        return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
    }

    function updateDisplay() {
        const elapsed = getElapsedTime();
        const timeStr = formatTime(elapsed);

        // Update all displays
        if (els.timerDisplay) els.timerDisplay.textContent = timeStr;
        if (els.displayBig) els.displayBig.textContent = timeStr;
        if (els.pageDisplay) els.pageDisplay.textContent = timeStr;

        // Auto-Stop Check
        if (state.isRunning && state.targetMinutes) {
            const targetMs = state.targetMinutes * 60 * 1000;
            if (elapsed >= targetMs) {
                stopTimer(true);
            }
        }
    }

    function startInterval() {
        if (timerInterval) clearInterval(timerInterval);
        timerInterval = setInterval(updateDisplay, 1000);
    }

    // Helper to parse duration from task string
    function parseDuration(text) {
        if (!text) return null;
        let minutes = 0;
        let found = false;

        // Match hours
        const hMatch = text.match(/(\d+(?:\.\d+)?)\s*(?:h|hr|hours?)\b/i);
        if (hMatch) {
            minutes += parseFloat(hMatch[1]) * 60;
            found = true;
        }

        // Match minutes
        const mMatch = text.match(/(\d+(?:\.\d+)?)\s*(?:m|min|minutes?)\b/i);
        if (mMatch) {
            minutes += parseFloat(mMatch[1]);
            found = true;
        }

        if (!found) return null;
        return Math.floor(minutes);
    }

    function startTimer() {
        if (state.isRunning) return;

        if (!state.taskName.trim()) {
            if (els.taskInput.offsetParent !== null) { // Only focus if visible
                els.taskInput.focus();
                els.taskInput.classList.add('is-invalid');
            } else {
                alert('Please enter a task name in the timer menu first.');
            }
            return;
        }
        els.taskInput.classList.remove('is-invalid');

        const duration = parseDuration(state.taskName);
        state.targetMinutes = duration && duration > 0 ? duration : null;

        state.isRunning = true;
        state.startTime = Date.now();
        saveState();

        updateControls();
        startInterval();
    }

    function pauseTimer() {
        if (!state.isRunning) return;

        state.accumulatedTime += Date.now() - state.startTime;
        state.startTime = null;
        state.isRunning = false;
        saveState();

        updateControls();
        if (timerInterval) clearInterval(timerInterval);
        updateDisplay();
    }

    function stopTimer(isAuto = false) {
        pauseTimer();

        const entry = {
            taskName: state.taskName,
            durationSeconds: Math.floor(state.accumulatedTime / 1000),
            startTime: new Date(Date.now() - state.accumulatedTime).toISOString(),
            endTime: new Date().toISOString()
        };

        if (isAuto) {
            alert(`Task "${state.taskName}" completed! (Limit: ${state.targetMinutes} min)`);
        }

        // Reset
        state = {
            startTime: null,
            accumulatedTime: 0,
            isRunning: false,
            taskName: '',
            targetMinutes: null
        };
        saveState();

        updateControls();
        updateDisplay();
        els.taskInput.value = '';

        fetch(API_URL + '/SaveEntry', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(entry)
        }).then(res => {
            if (res.ok) {
                fetchRecentTasks();
                if (window.location.pathname.includes('/TimeTracker')) {
                    window.location.reload();
                }
            }
        });
    }

    function updateControls() {
        const running = state.isRunning;
        const hasTime = getElapsedTime() > 0;
        const taskName = state.taskName || 'No Active Task';

        // Navbar Controls
        if (running) {
            els.btnStart.style.display = 'none';
            els.btnPause.style.display = 'inline-block';
            els.timerIcon.className = 'fas fa-pause';
            els.timerIcon.style.color = 'orange';
            els.status.textContent = 'Running';
            els.navbarTimer.style.background = 'rgba(255, 193, 7, 0.2)';
        } else {
            els.btnStart.style.display = 'inline-block';
            els.btnPause.style.display = 'none';
            els.timerIcon.className = 'fas fa-play';
            els.timerIcon.style.color = '';
            els.timerIcon.classList.add('text-primary');
            els.status.textContent = hasTime ? 'Paused' : 'Stopped';
            els.navbarTimer.style.background = hasTime ? 'rgba(13, 110, 253, 0.1)' : 'transparent';
        }

        els.btnStop.disabled = !hasTime && !running;

        // Page Controls
        if (els.pageTask) els.pageTask.textContent = taskName;
        if (els.pageContainer) {
            // Show container if any activity
            els.pageContainer.style.display = (running || hasTime) ? 'block' : 'none';
        }
    }

    function saveState() {
        localStorage.setItem(STATE_KEY, JSON.stringify(state));
    }

    function fetchRecentTasks() {
        fetch(API_URL + '/GetRecentTasks')
            .then(res => res.json())
            .then(tasks => {
                const list = els.recentList;
                if (!tasks || tasks.length === 0) {
                    list.style.display = 'none';
                    return;
                }
                list.innerHTML = '';
                tasks.forEach(task => {
                    const item = document.createElement('button');
                    item.className = 'list-group-item list-group-item-action list-group-item-light small';
                    item.textContent = task;
                    item.onclick = () => {
                        state.taskName = task;
                        els.taskInput.value = task;
                        list.style.display = 'none';
                        saveState();
                    };
                    list.appendChild(item);
                });
                els.taskInput.onfocus = () => {
                    if (list.children.length > 0) list.style.display = 'block';
                };
                els.taskInput.onblur = () => {
                    setTimeout(() => list.style.display = 'none', 200);
                };
            })
            .catch(console.error);
    }

    return {
        init: init
    };
})();

document.addEventListener('DOMContentLoaded', TimeTracker.init);
