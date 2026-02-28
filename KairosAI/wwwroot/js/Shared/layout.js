
    (function () {
        'use strict';

    /* ─── Remove no-transition after 2 rAF frames ─────────────────────── */
    requestAnimationFrame(function () {
        requestAnimationFrame(function () {
            document.documentElement.classList.remove('no-transition');
        });
                });

    /* ─── Icons ─────────────────────────────────────────────────────────── */
    lucide.createIcons();

    /* ─── Staggered nav animations ───────────────────────────────────────
       Each .nav-item-anim has --i as inline style; we read it here
       to compute an animation-delay, keeping the CSS clean.
    ─────────────────────────────────────────────────────────────────────── */
    document.querySelectorAll('.nav-item-anim').forEach(function (el) {
                    var i = parseFloat(el.style.getPropertyValue('--i') || '0');
    el.style.animationDelay = (60 + i * 42) + 'ms';
                });

    /* ─── Alpha progress bar ─────────────────────────────────────────────── */
    setTimeout(function () {
                    var bar = document.getElementById('alphaBar');
    if (!bar) return;
    bar.style.width = bar.dataset.target + '%';
    setTimeout(function () {bar.classList.add('shimmer'); }, 1350);
                }, 550);

    /* ─── Theme toggle — with ripple burst ───────────────────────────────── */
    window.toggleTheme = function (btn) {
                    if (btn) {
                        var r = document.createElement('span');
    r.className = 'theme-ripple';
    r.style.left = '50%';
    r.style.top  = '50%';
    btn.appendChild(r);
    setTimeout(function () { if (r.parentNode) r.parentNode.removeChild(r); }, 520);
                    }
    var html = document.documentElement;
    var next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
    html.setAttribute('data-theme', next);
    html.setAttribute('color-scheme', next);
    localStorage.setItem('kairos-theme', next);
                };

    /* ─── Ripple on nav links ────────────────────────────────────────────── */
    window.addRipple = function (e, el) {
                    var rect   = el.getBoundingClientRect();
    var size   = Math.max(rect.width, rect.height);
    var x      = e.clientX - rect.left  - size / 2;
    var y      = e.clientY - rect.top   - size / 2;
    var ripple = document.createElement('span');
    ripple.className  = 'ripple';
    ripple.style.cssText =
    'width:' + size + 'px;height:' + size + 'px;' +
    'left:'  + x    + 'px;top:'    + y    + 'px;';
    el.appendChild(ripple);
    setTimeout(function () { if (ripple.parentNode) ripple.parentNode.removeChild(ripple); }, 520);
                };

    /* ─── Sidebar (mobile) ───────────────────────────────────────────────── */
    window.openSidebar = function () {
        document.getElementById('sidebar').classList.add('open');
    document.getElementById('mobile-overlay').classList.add('open');
    document.getElementById('sidebar').removeAttribute('aria-hidden');
                };
    window.closeSidebar = function () {
        document.getElementById('sidebar').classList.remove('open');
    document.getElementById('mobile-overlay').classList.remove('open');
    document.getElementById('sidebar').setAttribute('aria-hidden', 'true');
                };

    /* ─── History accordion ──────────────────────────────────────────────── */
    var historyOpen = false;
    window.toggleHistory = function () {
        historyOpen = !historyOpen;
    var panel   = document.getElementById('historyPanel');
    var chevron = document.getElementById('historyChevron');
    var toggle  = document.getElementById('historyToggle');
    panel.style.maxHeight   = historyOpen ? panel.scrollHeight + 'px' : '0px';
    chevron.style.transform = historyOpen ? 'rotate(180deg)' : 'rotate(0deg)';
    toggle.setAttribute('aria-expanded', String(historyOpen));
                };

    /* ─── Active nav detection ───────────────────────────────────────────── */
    var path = window.location.pathname.toLowerCase();

    document.querySelectorAll('a.nav-link').forEach(function (link) {
                    var href = link.getAttribute('href');
    if (!href) return;
    var h = href.toLowerCase();
                    // Exact root match OR prefix match — but never match the accordion toggle
                    // (the toggle is a <button>, not an <a>, so querySelectorAll('a.nav-link') already
                    //  excludes it — this guard is belt-and-suspenders for any future <a> accordions)
            if (h === '#' || h === '') return;
            if ((h === '/' && path === '/') || (h !== '/' && path.startsWith(h))) {
                link.classList.add('active');
                    }
                });

            // history-item active state: derive from route, don't rely on hardcoded class
            // Remove all hardcoded .active from history items first, then re-apply by URL match
            document.querySelectorAll('.history-item').forEach(function (item) {
                item.classList.remove('active');
            var href = item.getAttribute('href');
            if (!href || href === '#') return;
            var h = href.toLowerCase();
            if (path === h || (h !== '/' && h !== '#' && path.startsWith(h))) {
                item.classList.add('active');
                    }
                });

            })();
 