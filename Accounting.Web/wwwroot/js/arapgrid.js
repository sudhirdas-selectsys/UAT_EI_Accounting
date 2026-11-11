$(function () {
    BindSearchPanelClickEvents();
    BindGridEvents();
});

function BindChipText() {
    $('.custom-multi-selection .dropdown-item input').on('change', function () {
        const $dropdown = $(this).closest('.custom-multi-selection');
        const $button = $dropdown.find('button.form-select');
        const $defaultText = $button.find('.default-text');
        const value = $(this).val();
        const isChecked = $(this).prop('checked');
        const $countChip = $button.find('.chip-count');

        // Get checked values
        const $checkedInputs = $dropdown.find('.dropdown-item input:checked');
        const checkedValues = $checkedInputs.map(function () { return $(this).val(); }).get();

        // Remove all chips before adding new ones (clean rebuild)
        $button.find('.chip').remove();

        if (checkedValues.length === 0) {
            $defaultText.show();
            $button.attr('title', 'Select');
            return;
        }

        $defaultText.hide();

        // Add the first selected chip with name
        const firstValue = checkedValues[0];
        $button.append(
            '<div class="chip text-truncate d-inline-flex align-items-center me-1" ' +
            'data-value="' + firstValue + '" style="max-width: 100px;" title="' + firstValue + '">' +
            '<span class="chip-text text-truncate">' + firstValue + '</span>' +
            '<span class="remove-chip ms-2" style="cursor:pointer;">&times;</span>' +
            '</div>'
        );

        // Add count chip if more than one selected
        if (checkedValues.length > 1) {
            const count = checkedValues.length;
            $button.append(
                '<div class="chip pe-2 d-inline-flex align-items-center" ' +
                'data-count="' + count + '" title="' + count + ' selected" style="cursor: default;">+' + count + '</div>'
            );
        }

        // Update title attribute
        $button.attr('title', 'Selected (' + checkedValues.length + '): ' + checkedValues.join(', '));
    });

    // On chip remove
    $(document).on('click', '.remove-chip', function (e) {
        e.stopPropagation();
        const $chip = $(this).closest('.chip');
        const value = $chip.data('value');
        const $dropdown = $(this).closest('.custom-multi-selection');
        const $button = $dropdown.find('button.form-select');
        const $checkbox = $dropdown.find('.dropdown-item input[value="' + value + '"]');

        $checkbox.prop('checked', false).trigger('change');
    });
}