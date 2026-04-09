/* Sphinx-specific theme enhancements */
(function() {
    'use strict';

    // Wrap tables in content area for responsive scrolling
    document.addEventListener('DOMContentLoaded', function() {
        var tables = document.querySelectorAll('.cms table');
        for (var i = 0; i < tables.length; i++) {
            var table = tables[i];
            if (!table.parentElement.classList.contains('overflow-x-auto')) {
                var wrapper = document.createElement('div');
                wrapper.className = 'overflow-x-auto';
                table.parentNode.insertBefore(wrapper, table);
                wrapper.appendChild(table);
            }
        }
    });
})();
