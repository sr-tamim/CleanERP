// Custom JavaScript for enhanced GoldenFiberERP Swagger UI module navigation

(function() {
    'use strict';

    // Wait for Swagger UI to load
    function waitForSwaggerUI() {
        if (typeof window.ui !== 'undefined' && window.ui.specSelectors) {
            initializeModuleEnhancements();
        } else {
            setTimeout(waitForSwaggerUI, 100);
        }
    }

    function initializeModuleEnhancements() {
        // Add module indicator to the header
        addModuleIndicator();
        
        // Enhance definition selector
        enhanceDefinitionSelector();
        
        // Add keyboard shortcuts
        addKeyboardShortcuts();
        
        // Add keyboard shortcuts help
        addKeyboardShortcutsHelp();
    }

    function addModuleIndicator() {
        const topbar = document.querySelector('.swagger-ui .topbar');
        if (topbar && !document.querySelector('.module-indicator')) {
            const indicator = document.createElement('div');
            indicator.className = 'module-indicator';
            indicator.innerHTML = `
                <div class="module-info">
                    <h3>🏢 GoldenFiberERP</h3>
                    <p>Enterprise Resource Planning System</p>
                </div>
            `;
            topbar.appendChild(indicator);
        }
    }

    function enhanceDefinitionSelector() {
        const selector = document.querySelector('.swagger-ui .topbar .download-url-wrapper select');
        if (selector) {
            // Add change event listener to show module description
            selector.addEventListener('change', function() {
                const selectedModule = this.value.split('/').pop().replace('.json', '');
                showModuleDescription(selectedModule);
            });

            // Show initial module description
            const initialModule = selector.value.split('/').pop().replace('.json', '');
            showModuleDescription(initialModule);
        }
    }

    function showModuleDescription(module) {
        // Remove existing description
        const existingDesc = document.querySelector('.module-description');
        if (existingDesc) {
            existingDesc.remove();
        }

        // Get module description
        const descriptions = {
            'v1': 'Complete API overview with all modules and endpoints',
            'inventory': 'Product management, stock control, and inventory operations',
            'auth': 'User authentication, authorization, and security management',
            'sales': 'Order processing, customer management, and sales tracking',
            'manufacturing': 'Production planning, work orders, and manufacturing processes',
            'financial': 'Accounting, billing, payments, and financial reporting',
            'system': '🏥 Health checks, system monitoring, and administrative functions - Default launch module for system status'
        };

        const description = descriptions[module] || 'API documentation';
        
        // Add description below the topbar
        const infoSection = document.querySelector('.swagger-ui .info');
        if (infoSection) {
            const descDiv = document.createElement('div');
            descDiv.className = 'module-description';
            descDiv.innerHTML = `
                <div class="module-desc-content">
                    <h4>📘 Current Module: ${module.charAt(0).toUpperCase() + module.slice(1)}</h4>
                    <p>${description}</p>
                </div>
            `;
            infoSection.parentNode.insertBefore(descDiv, infoSection.nextSibling);
        }
    }

    function addKeyboardShortcuts() {
        document.addEventListener('keydown', function(e) {
            // Alt + M to focus on module selector
            if (e.altKey && e.key === 'm') {
                e.preventDefault();
                const selector = document.querySelector('.swagger-ui .topbar .download-url-wrapper select');
                if (selector) {
                    selector.focus();
                }
            }
            
            // Alt + O for overview (v1)
            if (e.altKey && e.key === 'o') {
                e.preventDefault();
                switchToModule('v1');
            }
            
            // Alt + I for inventory
            if (e.altKey && e.key === 'i') {
                e.preventDefault();
                switchToModule('inventory');
            }
            
            // Alt + A for auth
            if (e.altKey && e.key === 'a') {
                e.preventDefault();
                switchToModule('auth');
            }
        });
    }

    function switchToModule(moduleName) {
        const selector = document.querySelector('.swagger-ui .topbar .download-url-wrapper select');
        if (selector) {
            // Find the option that contains the module name
            for (let option of selector.options) {
                if (option.value.includes(moduleName)) {
                    selector.value = option.value;
                    selector.dispatchEvent(new Event('change'));
                    break;
                }
            }
        }
    }

    function addKeyboardShortcutsHelp() {
        // Only add if not already present
        if (document.querySelector('.keyboard-shortcuts')) return;
        
        const helpDiv = document.createElement('div');
        helpDiv.className = 'keyboard-shortcuts';
        helpDiv.innerHTML = `
            <h5>⌨️ Keyboard Shortcuts</h5>
            <ul>
                <li><kbd>Alt</kbd> + <kbd>M</kbd> Focus module selector</li>
                <li><kbd>Alt</kbd> + <kbd>O</kbd> API Overview</li>
                <li><kbd>Alt</kbd> + <kbd>I</kbd> Inventory Module</li>
                <li><kbd>Alt</kbd> + <kbd>A</kbd> Authentication Module</li>
            </ul>
        `;
        document.body.appendChild(helpDiv);
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', waitForSwaggerUI);
    } else {
        waitForSwaggerUI();
    }

})();
