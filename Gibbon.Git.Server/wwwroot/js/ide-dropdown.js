document.addEventListener('DOMContentLoaded', function () {
    initIdeDropdown();
});

function initIdeDropdown() {
    const dropdownButtons = document.querySelectorAll('.ide-dropdown-button');
    
    dropdownButtons.forEach(button => {
        button.addEventListener('click', function(event) {
            event.stopPropagation();
            const dropdown = this.nextElementSibling;
            
            // Close all other dropdowns
            document.querySelectorAll('.ide-dropdown-menu').forEach(menu => {
                if (menu !== dropdown) {
                    menu.style.display = 'none';
                }
            });
            
            // Toggle current dropdown
            if (dropdown.style.display === 'block') {
                dropdown.style.display = 'none';
            } else {
                dropdown.style.display = 'block';
            }
        });
    });
    
    // Close dropdown when clicking outside
    document.addEventListener('click', function() {
        document.querySelectorAll('.ide-dropdown-menu').forEach(menu => {
            menu.style.display = 'none';
        });
    });
    
    // Prevent dropdown from closing when clicking inside it
    document.querySelectorAll('.ide-dropdown-menu').forEach(menu => {
        menu.addEventListener('click', function(event) {
            event.stopPropagation();
        });
    });
}
